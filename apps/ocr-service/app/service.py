from pathlib import Path

from .ocr_engine import analyze_food_label
from .schemas import OcrAnalyzeResult


class FoodLabelOcrService:
    def analyze(self, image_path: Path) -> OcrAnalyzeResult:
        raw_text, nutrition = analyze_food_label(image_path)
        return OcrAnalyzeResult(raw_text=raw_text, nutrition=nutrition)
