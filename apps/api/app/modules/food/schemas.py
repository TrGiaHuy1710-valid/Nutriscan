from enum import Enum
from typing import Literal

from pydantic import BaseModel, Field, model_validator


class FlowStatus(str, Enum):
    pending = "pending"
    need_more_info = "need_more_info"
    confirmed = "confirmed"
    completed = "completed"
    failed = "failed"


class ResultSource(str, Enum):
    local_ocr = "local_ocr"
    local_vision = "local_vision"
    gemini = "gemini"
    user_corrected = "user_corrected"


class ServiceErrorResponse(BaseModel):
    request_id: str
    error_code: str
    message: str


class DetectedFood(BaseModel):
    name: str
    normalized_name: str | None = None
    confidence: float = Field(default=0.0, ge=0.0, le=1.0)
    source: ResultSource = ResultSource.local_vision
    bounding_box: dict | None = None
    raw_text: str | None = None
    reasoning: str | None = None
    estimated_grams: float | None = None
    unit: str | None = None
    visible_evidence: str | None = None


class FoodAnalyzeRequest(BaseModel):
    session_id: str | None = None
    request_id: str | None = None
    debug: bool = False


class FoodAnalyzeResponse(BaseModel):
    session_id: str
    request_id: str
    status: FlowStatus
    detected_foods: list[DetectedFood] = Field(default_factory=list)
    required_fields: list[str] = Field(default_factory=lambda: ["grams", "people_count", "serving_size"])
    missing_fields: list[str] = Field(default_factory=list)
    assistant_message: str = ""
    confidence: float = Field(default=0.0, ge=0.0, le=1.0)
    source: ResultSource | None = None
    error_code: str | None = None
    message: str = ""


class ConfirmedFoodInput(BaseModel):
    name: str
    normalized_name: str | None = None
    grams: float | None = Field(default=None, gt=0)
    serving_size: str | None = None
    unit: str | None = None
    notes: str | None = None

    @model_validator(mode="after")
    def require_amount(self) -> "ConfirmedFoodInput":
        if self.grams is None and not self.serving_size:
            raise ValueError("Either grams or serving_size is required")
        return self


class FoodConfirmRequest(BaseModel):
    session_id: str
    request_id: str | None = None
    foods: list[ConfirmedFoodInput]
    people_count: int = Field(gt=0)
    notes: str | None = None


class FoodNutritionItem(BaseModel):
    name: str
    normalized_name: str
    grams: float
    calories: float
    protein: float
    carbs: float
    fat: float
    calories_per_person: float
    protein_per_person: float
    carbs_per_person: float
    fat_per_person: float
    estimated: bool = True
    needs_manual_review: bool = False


class NutritionSummary(BaseModel):
    calories: float = 0
    protein: float = 0
    carbs: float = 0
    fat: float = 0
    calories_per_person: float = 0
    protein_per_person: float = 0
    carbs_per_person: float = 0
    fat_per_person: float = 0
    estimated: bool = True
    needs_manual_review: bool = False


class FoodConfirmResponse(BaseModel):
    meal_id: str
    session_id: str
    request_id: str
    status: FlowStatus = FlowStatus.completed
    people_count: int
    items: list[FoodNutritionItem]
    summary: NutritionSummary
    message: str = "Meal nutrition calculated."


class FoodFeedbackRequest(BaseModel):
    request_id: str | None = None
    corrected_foods: list[ConfirmedFoodInput] = Field(default_factory=list)
    notes: str | None = None
    rating: int | None = Field(default=None, ge=1, le=5)


class FoodFeedbackResponse(BaseModel):
    session_id: str
    request_id: str
    status: Literal["saved"]
    message: str


class MealHistoryItem(BaseModel):
    meal_id: str
    session_id: str
    created_at: str
    people_count: int
    items: list[FoodNutritionItem]
    summary: NutritionSummary
    notes: str | None = None


class MealHistoryResponse(BaseModel):
    meals: list[MealHistoryItem]


class MealDetailResponse(MealHistoryItem):
    pass


class FoodValidationNutrition(BaseModel):
    calories: int = 0
    protein: float = 0
    carbs: float = 0
    fat: float = 0
    sugar: float | None = None
    sodium: float | None = None


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


class AnalyzeFoodResponse(FoodAnalyzeResponse):
    status: Literal["need_more_info", "completed", "error", "failed"]


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
