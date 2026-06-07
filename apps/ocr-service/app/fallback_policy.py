def should_use_gemini(confidence: float, detected_count: int, threshold: float = 0.65) -> bool:
    return detected_count == 0 or confidence < threshold
