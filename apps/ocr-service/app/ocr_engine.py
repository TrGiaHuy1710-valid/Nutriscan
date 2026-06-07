from pathlib import Path

from PIL import Image

from .ocr_core.ocr_service import extract_text
from .ocr_core.nutrition_parser import extract_nutrition


def analyze_food_label(image_path: Path) -> tuple[str, dict]:
    if not image_path.exists():
        raise FileNotFoundError(f"Image file not found: {image_path}")

    try:
        image = Image.open(image_path)
        image.verify()
    except Exception as exc:
        raise ValueError(f"Invalid image file: {exc}") from exc

    raw_text = extract_text(str(image_path))
    nutrition = extract_nutrition(raw_text)
    return raw_text, nutrition
