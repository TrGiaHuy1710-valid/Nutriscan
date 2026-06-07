from copy import deepcopy

from app.modules.food.models import FeedbackRecord, MealRecord


class FoodRepository:
    def __init__(self) -> None:
        self._meals: dict[str, MealRecord] = {}
        self._feedback: dict[str, list[FeedbackRecord]] = {}

    def save_meal(self, meal: MealRecord) -> None:
        self._meals[meal["meal_id"]] = deepcopy(meal)

    def get_meal(self, meal_id: str) -> MealRecord | None:
        meal = self._meals.get(meal_id)
        return deepcopy(meal) if meal else None

    def list_meals(self) -> list[MealRecord]:
        return [deepcopy(meal) for meal in self._meals.values()]

    def save_feedback(self, session_id: str, feedback: FeedbackRecord) -> None:
        self._feedback.setdefault(session_id, []).append(deepcopy(feedback))


food_repository = FoodRepository()
