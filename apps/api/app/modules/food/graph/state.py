from typing import Literal, TypedDict


class FoodItemState(TypedDict, total=False):
    name: str
    confidence: float | None
    estimated_grams: float | None
    grams: float | None
    unit: str | None
    grams_per_person: float | None
    visible_evidence: str | None


class FoodSessionState(TypedDict, total=False):
    session_id: str
    image_path: str
    user_message: str

    detected_foods: list[FoodItemState]
    confirmed_foods: list[FoodItemState]

    people_count: int | None
    missing_fields: list[str]

    status: Literal[
        "image_uploaded",
        "image_analyzed",
        "need_more_info",
        "ready",
        "completed",
        "error",
    ]

    assistant_message: str
    error_message: str | None
