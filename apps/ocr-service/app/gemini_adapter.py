import json
import os
from pathlib import Path
from urllib import request


class GeminiFoodVisionAdapter:
    def __init__(self) -> None:
        self._api_key = os.getenv("GEMINI_API_KEY")
        self._model = os.getenv("GEMINI_MODEL", "gemini-2.5-flash")

    @property
    def is_configured(self) -> bool:
        return bool(self._api_key)

    def analyze_image(self, image_path: Path) -> dict:
        if not self._api_key:
            raise RuntimeError("Gemini API key is not configured")

        # Placeholder adapter boundary. The API service owns the mature Gemini graph today;
        # this boundary keeps OCR service extensible without copying secrets into code.
        endpoint = f"https://generativelanguage.googleapis.com/v1beta/models/{self._model}:generateContent?key={self._api_key}"
        payload = {
            "contents": [
                {
                    "parts": [
                        {
                            "text": (
                                "Identify food items in this image and return compact JSON with "
                                "detected_foods: [{name, normalized_name, confidence}]."
                            )
                        }
                    ]
                }
            ]
        }
        data = json.dumps(payload).encode("utf-8")
        http_request = request.Request(
            endpoint,
            data=data,
            headers={"Content-Type": "application/json"},
            method="POST",
        )
        with request.urlopen(http_request, timeout=20) as response:
            return json.loads(response.read().decode("utf-8"))


def foods_from_gemini_response(response: dict) -> list[dict]:
    candidates = response.get("detected_foods")
    if isinstance(candidates, list):
        return candidates
    return []
