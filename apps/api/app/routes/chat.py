from fastapi import APIRouter

from app.schemas.chat import ChatFoodRequest, ChatFoodResponse
from app.services.food_service import continue_food_chat


router = APIRouter(prefix="/food", tags=["food"])


@router.post("/chat", response_model=ChatFoodResponse)
async def chat_food(request: ChatFoodRequest) -> ChatFoodResponse:
    return await continue_food_chat(
        session_id=request.session_id,
        message=request.message,
    )
