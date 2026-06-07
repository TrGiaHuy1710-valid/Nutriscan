import json
import re
from typing import Any

from app.core.errors import InvalidLLMJSONError


def parse_llm_json(text: str) -> dict[str, Any]:
    cleaned = text.strip()

    if cleaned.startswith("```"):
        cleaned = re.sub(r"^```json\s*", "", cleaned, flags=re.IGNORECASE)
        cleaned = re.sub(r"^```\s*", "", cleaned)
        cleaned = re.sub(r"\s*```$", "", cleaned)

    try:
        parsed = json.loads(cleaned)
    except json.JSONDecodeError as exc:
        raise InvalidLLMJSONError() from exc

    if not isinstance(parsed, dict):
        raise InvalidLLMJSONError()

    return parsed
