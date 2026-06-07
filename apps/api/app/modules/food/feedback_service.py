from app.modules.food.repository import food_repository
from app.modules.food.schemas import FoodFeedbackRequest


class FeedbackService:
    def save_feedback(self, session_id: str, request: FoodFeedbackRequest, request_id: str) -> None:
        food_repository.save_feedback(
            session_id,
            {
                "session_id": session_id,
                "request_id": request_id,
                "corrected_foods": [food.model_dump() for food in request.corrected_foods],
                "notes": request.notes,
                "rating": request.rating,
            },
        )


feedback_service = FeedbackService()
