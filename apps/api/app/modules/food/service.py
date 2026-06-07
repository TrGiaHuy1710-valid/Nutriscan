import uuid
from datetime import datetime, timezone

from fastapi import HTTPException, UploadFile

from app.core.errors import AppError, ErrorCode, OcrServiceUnavailableError, app_error_to_http
from app.integrations.ocr_client import foods_from_ocr_response, ocr_service_client
from app.modules.food.graph.state import FoodSessionState
from app.modules.food.graph.workflow import run_image_workflow
from app.modules.food.repository import food_repository
from app.modules.food.schemas import (
    AnalyzeFoodResponse,
    ConfirmedFoodInput,
    DetectedFood,
    FlowStatus,
    FoodConfirmRequest,
    FoodConfirmResponse,
    FoodFeedbackRequest,
    FoodFeedbackResponse,
    FoodValidationFlag,
    FoodValidationNutrition,
    FoodValidationRequest,
    FoodValidationResponse,
    MealDetailResponse,
    MealHistoryItem,
    MealHistoryResponse,
    ResultSource,
    SessionResponse,
)
from app.modules.food.session_service import food_session_service
from app.modules.food.nutrition_service import nutrition_service
from app.services.storage_service import save_upload_file


async def analyze_food_image(
    image: UploadFile,
    session_id: str | None = None,
    request_id: str | None = None,
    debug: bool = False,
) -> AnalyzeFoodResponse:
    current_request_id = request_id or str(uuid.uuid4())
    current_session_id = session_id or str(uuid.uuid4())
    image_path = await save_upload_file(image)

    state: FoodSessionState = food_session_service.get(current_session_id) or {"session_id": current_session_id}
    state.update(
        {
            "session_id": current_session_id,
            "request_id": current_request_id,
            "image_path": image_path,
            "status": "image_uploaded",
        }
    )

    try:
        ocr_result = await ocr_service_client.analyze_food_image(
            image_path=image_path,
            request_id=current_request_id,
            debug=debug,
        )
        detected_foods = foods_from_ocr_response(ocr_result)
        confidence = float(ocr_result.get("confidence") or _average_confidence(detected_foods))
        source = ResultSource(ocr_result.get("source") or "local_vision")

        state.update(
            {
                "status": "need_more_info",
                "detected_foods": [food.model_dump(mode="json") for food in detected_foods],
                "missing_fields": ["grams", "people_count"],
                "assistant_message": _build_assistant_message(detected_foods),
                "confidence": confidence,
                "source": source.value,
            }
        )
        food_session_service.save(current_session_id, state)

        return AnalyzeFoodResponse(
            session_id=current_session_id,
            request_id=current_request_id,
            status="need_more_info",
            detected_foods=detected_foods,
            missing_fields=["grams", "people_count"],
            assistant_message=state["assistant_message"],
            confidence=confidence,
            source=source,
            message="Image analyzed by OCR service.",
        )
    except OcrServiceUnavailableError:
        return await _analyze_with_legacy_graph(
            state=state,
            session_id=current_session_id,
            request_id=current_request_id,
        )


async def _analyze_with_legacy_graph(
    state: FoodSessionState,
    session_id: str,
    request_id: str,
) -> AnalyzeFoodResponse:
    try:
        result = await run_image_workflow(state)
    except AppError as exc:
        if exc.error_code == ErrorCode.GEMINI_API_ERROR:
            return _failed_analyze_response(session_id, request_id, exc.error_code, exc.message)
        raise app_error_to_http(exc) from exc

    result["session_id"] = session_id
    result["request_id"] = request_id
    food_session_service.save(session_id, result)

    detected_foods = [
        _candidate_to_detected_food(item)
        for item in result.get("detected_foods", [])
    ]

    return AnalyzeFoodResponse(
        session_id=session_id,
        request_id=request_id,
        status=result.get("status", "need_more_info"),
        detected_foods=detected_foods,
        missing_fields=result.get("missing_fields", []),
        assistant_message=result.get("assistant_message", ""),
        confidence=_average_confidence(detected_foods),
        source=ResultSource.gemini if detected_foods else None,
        message="OCR service unavailable; used legacy AI graph fallback.",
    )


def confirm_food(request: FoodConfirmRequest) -> FoodConfirmResponse:
    state = food_session_service.get(request.session_id)
    if state is None:
        raise HTTPException(status_code=404, detail="Session not found")

    current_request_id = request.request_id or state.get("request_id") or str(uuid.uuid4())
    items, summary = nutrition_service.calculate(request.foods, request.people_count)
    meal_id = str(uuid.uuid4())
    created_at = datetime.now(timezone.utc).isoformat()

    meal = {
        "meal_id": meal_id,
        "session_id": request.session_id,
        "request_id": current_request_id,
        "created_at": created_at,
        "people_count": request.people_count,
        "items": [item.model_dump(mode="json") for item in items],
        "summary": summary.model_dump(mode="json"),
        "notes": request.notes,
    }
    food_repository.save_meal(meal)

    state.update(
        {
            "status": FlowStatus.completed.value,
            "confirmed_foods": [food.model_dump(mode="json") for food in request.foods],
            "people_count": request.people_count,
            "meal_id": meal_id,
            "nutrition_summary": summary.model_dump(mode="json"),
        }
    )
    food_session_service.save(request.session_id, state)

    return FoodConfirmResponse(
        meal_id=meal_id,
        session_id=request.session_id,
        request_id=current_request_id,
        people_count=request.people_count,
        items=items,
        summary=summary,
    )


def save_food_feedback(session_id: str, request: FoodFeedbackRequest) -> FoodFeedbackResponse:
    state = food_session_service.get(session_id)
    if state is None:
        raise HTTPException(status_code=404, detail="Session not found")

    food_repository.save_feedback(
        session_id,
        {
            "session_id": session_id,
            "request_id": request.request_id or state.get("request_id") or str(uuid.uuid4()),
            "created_at": datetime.now(timezone.utc).isoformat(),
            "corrected_foods": [food.model_dump(mode="json") for food in request.corrected_foods],
            "notes": request.notes,
            "rating": request.rating,
        },
    )
    return FoodFeedbackResponse(
        session_id=session_id,
        request_id=request.request_id or state.get("request_id") or "",
        status="saved",
        message="Feedback saved.",
    )


def list_meal_history() -> MealHistoryResponse:
    meals = [
        _meal_to_history_item(meal)
        for meal in sorted(food_repository.list_meals(), key=lambda item: item["created_at"], reverse=True)
    ]
    return MealHistoryResponse(meals=meals)


def get_meal_detail(meal_id: str) -> MealDetailResponse:
    meal = food_repository.get_meal(meal_id)
    if meal is None:
        raise HTTPException(status_code=404, detail="Meal not found")
    return MealDetailResponse(**_meal_to_history_item(meal).model_dump(mode="json"))


def get_food_session(session_id: str) -> SessionResponse:
    state = food_session_service.get(session_id)
    if state is None:
        raise HTTPException(status_code=404, detail="Session not found")
    return SessionResponse(session_id=session_id, state=dict(state))


def validate_food_label(payload: FoodValidationRequest) -> FoodValidationResponse:
    nutrition = payload.ocrNutrition
    calories = int(round(float(nutrition.calories or 0)))
    carbs = float(nutrition.carbs if nutrition.carbs is not None else nutrition.carb or 0)
    protein = float(nutrition.protein or 0)
    fat = float(nutrition.fat or 0)
    sugar = float(nutrition.sugar) if nutrition.sugar is not None else None
    sodium = float(nutrition.sodium) if nutrition.sodium is not None else None

    text = (payload.rawText or "").lower()
    has_label_terms = any(
        term in text
        for term in [
            "nutrition",
            "calories",
            "protein",
            "fat",
            "carb",
            "nang luong",
            "chat dam",
            "nÄƒng lÆ°á»£ng",
            "cháº¥t Ä‘áº¡m",
        ]
    )
    nutrient_count = sum(1 for value in [calories, carbs, protein, fat] if value > 0)
    confidence = min(0.95, 0.35 + (0.15 * nutrient_count) + (0.2 if has_label_terms else 0))

    flags: list[FoodValidationFlag] = []
    alternatives: list[str] = []

    if sugar is not None and sugar >= 10:
        flags.append(
            FoodValidationFlag(
                type="high_sugar",
                severity="medium",
                message="Sugar is high; review serving size before adding it to the day.",
            )
        )
        alternatives.append("Lower sugar option")

    if calories >= 500:
        flags.append(
            FoodValidationFlag(
                type="high_calorie",
                severity="medium",
                message="Calories are high for a single serving.",
            )
        )
        alternatives.append("Use a smaller serving")

    normalized_name = payload.fileName.rsplit(".", 1)[0] if payload.fileName else "Scanned food label"

    return FoodValidationResponse(
        isFoodLabel=has_label_terms or nutrient_count > 0,
        confidence=confidence,
        normalizedName=normalized_name,
        nutrition=FoodValidationNutrition(
            calories=calories,
            protein=protein,
            carbs=carbs,
            fat=fat,
            sugar=sugar,
            sodium=sodium,
        ),
        flags=flags,
        alternatives=alternatives,
    )


def _candidate_to_detected_food(item: dict) -> DetectedFood:
    return DetectedFood(
        name=item.get("name", "unknown_food"),
        normalized_name=item.get("normalized_name"),
        confidence=float(item.get("confidence") or 0),
        source=ResultSource.gemini,
        estimated_grams=item.get("estimated_grams"),
        unit=item.get("unit"),
        visible_evidence=item.get("visible_evidence"),
    )


def _average_confidence(foods: list[DetectedFood]) -> float:
    if not foods:
        return 0.0
    return round(sum(food.confidence for food in foods) / len(foods), 3)


def _build_assistant_message(foods: list[DetectedFood]) -> str:
    if not foods:
        return "Khong nhan dien duoc mon an ro rang. Hay nhap ten mon, so gram va so nguoi an."
    names = ", ".join(food.name for food in foods)
    return f"Toi da nhan dien: {names}. Hay xac nhan so gram va so nguoi an."


def _failed_analyze_response(session_id: str, request_id: str, error_code: str, message: str) -> AnalyzeFoodResponse:
    food_session_service.save(
        session_id,
        {
            "session_id": session_id,
            "request_id": request_id,
            "status": "failed",
            "error_code": error_code,
            "message": message,
        },
    )
    return AnalyzeFoodResponse(
        session_id=session_id,
        request_id=request_id,
        status="failed",
        detected_foods=[],
        missing_fields=[],
        assistant_message="Khong the phan tich anh luc nay.",
        confidence=0,
        error_code=error_code,
        message=message,
    )


def _meal_to_history_item(meal: dict) -> MealHistoryItem:
    return MealHistoryItem(
        meal_id=meal["meal_id"],
        session_id=meal["session_id"],
        created_at=meal["created_at"],
        people_count=meal["people_count"],
        items=meal["items"],
        summary=meal["summary"],
        notes=meal.get("notes"),
    )
