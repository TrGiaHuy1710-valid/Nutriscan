import mimetypes
from pathlib import Path
from typing import Any

from app.core.config import settings
from app.core.errors import MissingAPIKeyError, ProviderError
from app.integrations.providers.base import BaseLLMProvider
from app.utils.json_utils import parse_llm_json


IMAGE_ANALYSIS_PROMPT = """You are a food image analysis assistant.

Analyze the uploaded food image and return ONLY valid JSON.

Schema:
{
  "detected_foods": [
    {
      "name": "Vietnamese food name",
      "confidence": 0.0,
      "estimated_grams": null,
      "unit": null,
      "visible_evidence": "short visual reason"
    }
  ],
  "uncertainty": "short note",
  "need_user_confirmation": true
}

Rules:
- Do not guess exact grams if the image lacks a clear scale.
- If the dish is unclear, use a low confidence score.
- Prefer Vietnamese food names if possible.
- Do not include markdown.
- Do not include explanations outside JSON.
"""

TEXT_EXTRACTION_PROMPT = """You extract structured meal information from Vietnamese user text.

Return ONLY valid JSON.

Schema:
{
  "foods": [
    {
      "name": "food name",
      "grams": 300,
      "unit": "g"
    }
  ],
  "people_count": 2
}

Rules:
- If grams are not provided, set grams to null.
- If the user gives a portion unit like "1 bat", "2 mieng", "1 dia", keep it in unit.
- Normalize "gram", "grams", "g" to unit = "g".
- If people count is not mentioned, set people_count to null.
- Do not include markdown.
"""


class GeminiProvider(BaseLLMProvider):
    def __init__(self) -> None:
        if not settings.GEMINI_API_KEY:
            raise MissingAPIKeyError()

        try:
            from google import genai
        except ImportError as exc:
            raise ProviderError("LLM provider failed") from exc

        self._client = genai.Client(api_key=settings.GEMINI_API_KEY)
        self._model = settings.GEMINI_MODEL

    async def analyze_food_image(self, image_path: str) -> dict[str, Any]:
        try:
            from google.genai import types

            path = Path(image_path)
            mime_type = mimetypes.guess_type(path.name)[0] or "image/jpeg"
            image_part = types.Part.from_bytes(
                data=path.read_bytes(),
                mime_type=mime_type,
            )
            response = await self._client.aio.models.generate_content(
                model=self._model,
                contents=[IMAGE_ANALYSIS_PROMPT, image_part],
                config=types.GenerateContentConfig(
                    response_mime_type="application/json",
                ),
            )
        except Exception as exc:
            raise ProviderError() from exc

        return parse_llm_json(_response_text(response))

    async def extract_food_info_from_text(self, message: str) -> dict[str, Any]:
        prompt = f"{TEXT_EXTRACTION_PROMPT}\n\nUser text:\n{message}"

        try:
            from google.genai import types

            response = await self._client.aio.models.generate_content(
                model=self._model,
                contents=prompt,
                config=types.GenerateContentConfig(
                    response_mime_type="application/json",
                ),
            )
        except Exception as exc:
            raise ProviderError() from exc

        return parse_llm_json(_response_text(response))


def _response_text(response: Any) -> str:
    text = getattr(response, "text", None)
    if isinstance(text, str) and text.strip():
        return text

    raise ProviderError()
