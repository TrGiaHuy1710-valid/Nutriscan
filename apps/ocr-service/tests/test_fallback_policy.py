from app.fallback_policy import should_use_gemini


def test_should_use_gemini_when_no_food_detected() -> None:
    assert should_use_gemini(confidence=0.9, detected_count=0) is True


def test_should_use_gemini_when_confidence_is_low() -> None:
    assert should_use_gemini(confidence=0.4, detected_count=1) is True


def test_should_not_use_gemini_when_local_result_is_confident() -> None:
    assert should_use_gemini(confidence=0.8, detected_count=1) is False
