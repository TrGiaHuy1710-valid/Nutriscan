from pathlib import Path


def detect_food_candidates(image_path: Path, raw_text: str = "") -> tuple[list[dict], float]:
    detected_foods: list[dict] = []
    lowered_text = raw_text.lower()

    candidates = {
        "rice": ["rice", "com", "cÆ¡m"],
        "chicken": ["chicken", "ga", "gÃ "],
        "egg": ["egg", "trung", "trá»©ng"],
        "beef": ["beef", "bo", "bÃ²"],
    }

    for normalized_name, tokens in candidates.items():
        if any(token in lowered_text for token in tokens):
            detected_foods.append(
                {
                    "name": normalized_name,
                    "normalized_name": normalized_name,
                    "confidence": 0.7,
                    "source": "local_ocr",
                    "raw_text": raw_text,
                }
            )

    confidence = 0.7 if detected_foods else 0.0
    return detected_foods, confidence
