from pathlib import Path

from .fallback_policy import should_use_gemini
from .gemini_adapter import GeminiFoodVisionAdapter, foods_from_gemini_response
from .image_preprocessor import prepare_image
from .ocr_engine import analyze_food_label
from .schemas import OcrAnalyzeResult
from .vision_engine import detect_food_candidates


class FoodLabelOcrService:
    def __init__(self, gemini_adapter: GeminiFoodVisionAdapter | None = None) -> None:
        self._gemini_adapter = gemini_adapter or GeminiFoodVisionAdapter()

    def analyze(self, image_path: Path) -> OcrAnalyzeResult:
        raw_text, nutrition = analyze_food_label(image_path)
        detected_foods, confidence = detect_food_candidates(image_path, raw_text)
        return OcrAnalyzeResult(
            raw_text=raw_text,
            nutrition=nutrition,
            detected_foods=detected_foods,
            confidence=confidence,
            source="local_ocr",
            message="Food label OCR completed.",
        )

    def analyze_food_image(self, image_path: Path, request_id: str, debug: bool = False) -> OcrAnalyzeResult:
        prepared_path = prepare_image(image_path)
        result = self.analyze(prepared_path)

        if should_use_gemini(result.confidence, len(result.detected_foods or [])) and self._gemini_adapter.is_configured:
            try:
                gemini_response = self._gemini_adapter.analyze_image(prepared_path)
                gemini_foods = foods_from_gemini_response(gemini_response)
                if gemini_foods:
                    return OcrAnalyzeResult(
                        raw_text=result.raw_text,
                        nutrition=result.nutrition,
                        detected_foods=gemini_foods,
                        confidence=max(result.confidence, _average_confidence(gemini_foods)),
                        source="gemini",
                        fallback_used=True,
                        message="OCR confidence was low; Gemini fallback used.",
                        debug_info={"request_id": request_id} if debug else None,
                    )
            except Exception:
                if debug:
                    return OcrAnalyzeResult(
                        raw_text=result.raw_text,
                        nutrition=result.nutrition,
                        detected_foods=result.detected_foods,
                        confidence=result.confidence,
                        source=result.source,
                        fallback_used=False,
                        error_code="GEMINI_FALLBACK_FAILED",
                        message="Gemini fallback failed; returning local OCR result.",
                        debug_info={"request_id": request_id},
                    )

        return OcrAnalyzeResult(
            raw_text=result.raw_text,
            nutrition=result.nutrition,
            detected_foods=result.detected_foods,
            confidence=result.confidence,
            source=result.source,
            fallback_used=False,
            message=result.message,
            debug_info={"request_id": request_id} if debug else None,
        )


def _average_confidence(foods: list[dict]) -> float:
    if not foods:
        return 0.0
    return round(sum(float(food.get("confidence") or 0) for food in foods) / len(foods), 3)
