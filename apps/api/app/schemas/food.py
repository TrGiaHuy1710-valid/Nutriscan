from typing import Literal

from pydantic import BaseModel, Field


class FoodCandidate(BaseModel):
    name: str
    confidence: float = Field(default=0.0, ge=0.0, le=1.0)
    estimated_grams: float | None = None
    unit: str | None = None
    visible_evidence: str | None = None


class ConfirmedFoodItem(BaseModel):
    name: str
    grams: float | None = None
    unit: str | None = None
    grams_per_person: float | None = None


class AnalyzeFoodResponse(BaseModel):
    session_id: str
    status: Literal["need_more_info", "completed", "error"]
    detected_foods: list[FoodCandidate] = Field(default_factory=list)
    missing_fields: list[str] = Field(default_factory=list)
    assistant_message: str


class ChatFoodRequest(BaseModel):
    session_id: str
    message: str


class ChatFoodResponse(BaseModel):
    session_id: str
    status: Literal["need_more_info", "completed", "error"]
    confirmed_foods: list[ConfirmedFoodItem] = Field(default_factory=list)
    people_count: int | None = None
    missing_fields: list[str] = Field(default_factory=list)
    assistant_message: str
    foods: list[ConfirmedFoodItem] = Field(default_factory=list)
    summary: str | None = None


class SessionResponse(BaseModel):
    session_id: str
    state: dict


class OcrNutrition(BaseModel):
    calories: int | float | None = None
    protein: float | None = None
    carb: float | None = None
    carbs: float | None = None
    fat: float | None = None
    sugar: float | None = None
    sodium: float | None = None


class FoodValidationUserContext(BaseModel):
    goalType: str | None = None
    dailyCalorieTarget: float | None = None
    allergies: list[str] = Field(default_factory=list)
    dietTags: list[str] = Field(default_factory=list)


class FoodValidationRequest(BaseModel):
    rawText: str = ""
    fileName: str = ""
    ocrNutrition: OcrNutrition = Field(default_factory=OcrNutrition)
    userContext: FoodValidationUserContext = Field(default_factory=FoodValidationUserContext)


class FoodValidationNutrition(BaseModel):
    calories: int = 0
    protein: float = 0
    carbs: float = 0
    fat: float = 0
    sugar: float | None = None
    sodium: float | None = None


class FoodValidationFlag(BaseModel):
    type: str
    severity: Literal["low", "medium", "high"] = "low"
    message: str


class FoodValidationResponse(BaseModel):
    isFoodLabel: bool = True
    confidence: float = Field(default=0.0, ge=0.0, le=1.0)
    normalizedName: str
    brand: str | None = None
    servingSize: str | None = None
    nutrition: FoodValidationNutrition
    flags: list[FoodValidationFlag] = Field(default_factory=list)
    alternatives: list[str] = Field(default_factory=list)
