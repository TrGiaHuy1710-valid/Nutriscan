from fastapi import APIRouter

from app.modules.food.schemas import MealDetailResponse, MealHistoryResponse
from app.modules.food.service import get_meal_detail, list_meal_history


router = APIRouter(prefix="/meals", tags=["meals"])


@router.get("/history", response_model=MealHistoryResponse)
async def read_meal_history() -> MealHistoryResponse:
    return list_meal_history()


@router.get("/{meal_id}", response_model=MealDetailResponse)
async def read_meal_detail(meal_id: str) -> MealDetailResponse:
    return get_meal_detail(meal_id)
