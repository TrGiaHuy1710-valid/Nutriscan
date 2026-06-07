from fastapi import APIRouter, File, Form, UploadFile

from app.modules.food.schemas import (
    AnalyzeFoodResponse,
    FoodConfirmRequest,
    FoodConfirmResponse,
    FoodFeedbackRequest,
    FoodFeedbackResponse,
    FoodValidationRequest,
    FoodValidationResponse,
    SessionResponse,
)
from app.modules.food.service import (
    analyze_food_image,
    confirm_food,
    get_food_session,
    save_food_feedback,
    validate_food_label,
)


router = APIRouter(prefix="/food", tags=["food"])


@router.post("/analyze", response_model=AnalyzeFoodResponse)
async def analyze_food(
    image: UploadFile = File(...),
    session_id: str | None = Form(default=None),
    request_id: str | None = Form(default=None),
    debug: bool = Form(default=False),
) -> AnalyzeFoodResponse:
    return await analyze_food_image(
        image=image,
        session_id=session_id,
        request_id=request_id,
        debug=debug,
    )


@router.get("/sessions/{session_id}", response_model=SessionResponse)
async def read_food_session(session_id: str) -> SessionResponse:
    return get_food_session(session_id)


@router.get("/session/{session_id}", response_model=SessionResponse)
async def read_food_session_alias(session_id: str) -> SessionResponse:
    return get_food_session(session_id)


@router.post("/confirm", response_model=FoodConfirmResponse)
async def confirm_food_selection(request: FoodConfirmRequest) -> FoodConfirmResponse:
    return confirm_food(request)


@router.post("/{session_id}/feedback", response_model=FoodFeedbackResponse)
async def add_food_feedback(
    session_id: str,
    request: FoodFeedbackRequest,
) -> FoodFeedbackResponse:
    return save_food_feedback(session_id, request)


@router.post("/validate", response_model=FoodValidationResponse)
async def validate_food_label_endpoint(payload: FoodValidationRequest) -> FoodValidationResponse:
    return validate_food_label(payload)
