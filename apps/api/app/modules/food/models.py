from typing import TypedDict


class MealRecord(TypedDict, total=False):
    meal_id: str
    session_id: str
    request_id: str
    created_at: str
    people_count: int
    items: list[dict]
    summary: dict
    notes: str | None


class FeedbackRecord(TypedDict, total=False):
    session_id: str
    request_id: str
    corrected_foods: list[dict]
    notes: str | None
    rating: int | None
