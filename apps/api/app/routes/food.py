from fastapi import APIRouter, File, Form, UploadFile

from app.schemas.food import (
    AnalyzeFoodResponse,
    FoodValidationFlag,
    FoodValidationRequest,
    FoodValidationResponse,
    FoodValidationNutrition,
    SessionResponse,
)
from app.services.food_service import analyze_food_image, get_food_session


router = APIRouter(prefix="/food", tags=["food"])


@router.post("/analyze", response_model=AnalyzeFoodResponse)
async def analyze_food(
    image: UploadFile = File(...),
    session_id: str | None = Form(default=None),
) -> AnalyzeFoodResponse:
    return await analyze_food_image(image=image, session_id=session_id)


@router.get("/sessions/{session_id}", response_model=SessionResponse)
async def read_food_session(session_id: str) -> SessionResponse:
    return get_food_session(session_id)


@router.post("/validate", response_model=FoodValidationResponse)
async def validate_food_label(payload: FoodValidationRequest) -> FoodValidationResponse:
    nutrition = payload.ocrNutrition
    calories = int(round(float(nutrition.calories or 0)))
    carbs = float(nutrition.carbs if nutrition.carbs is not None else nutrition.carb or 0)
    protein = float(nutrition.protein or 0)
    fat = float(nutrition.fat or 0)
    sugar = float(nutrition.sugar) if nutrition.sugar is not None else None
    sodium = float(nutrition.sodium) if nutrition.sodium is not None else None

    text = (payload.rawText or "").lower()
    has_label_terms = any(term in text for term in ["nutrition", "calories", "protein", "fat", "carb", "năng lượng", "chất đạm"])
    nutrient_count = sum(1 for value in [calories, carbs, protein, fat] if value > 0)
    confidence = min(0.95, 0.35 + (0.15 * nutrient_count) + (0.2 if has_label_terms else 0))

    flags: list[FoodValidationFlag] = []
    alternatives: list[str] = []

    if sugar is not None and sugar >= 10:
        flags.append(FoodValidationFlag(
            type="high_sugar",
            severity="medium",
            message="Sugar is high; review serving size before adding it to the day."
        ))
        alternatives.append("Lower sugar option")

    if calories >= 500:
        flags.append(FoodValidationFlag(
            type="high_calorie",
            severity="medium",
            message="Calories are high for a single serving."
        ))
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
