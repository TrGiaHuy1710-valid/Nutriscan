import time
from pathlib import Path

import httpx

from app.core.config import settings
from app.core.errors import OcrServiceUnavailableError
from app.modules.food.schemas import DetectedFood, ResultSource


class OcrServiceClient:
    def __init__(self, base_url: str | None = None) -> None:
        self._base_url = (base_url or getattr(settings, "OCR_SERVICE_BASE_URL", "http://localhost:5000")).rstrip("/")

    async def analyze_food_image(self, image_path: str, request_id: str, debug: bool = False) -> dict:
        start = time.perf_counter()
        path = Path(image_path)

        try:
            async with httpx.AsyncClient(timeout=15) as client:
                with path.open("rb") as file_handle:
                    response = await client.post(
                        f"{self._base_url}/internal/v1/ocr/analyze-food-image",
                        data={"request_id": request_id, "debug": str(debug).lower()},
                        files={"image": (path.name, file_handle, "application/octet-stream")},
                    )
            response.raise_for_status()
            data = response.json()
            data["processing_time_ms"] = round((time.perf_counter() - start) * 1000, 2)
            return data
        except Exception as exc:
            raise OcrServiceUnavailableError("OCR service is unavailable") from exc


def foods_from_ocr_response(data: dict) -> list[DetectedFood]:
    foods = data.get("detected_foods") or []
    return [
        DetectedFood(
            name=item.get("name", "unknown_food"),
            normalized_name=item.get("normalized_name"),
            confidence=float(item.get("confidence") or 0),
            source=ResultSource(item.get("source") or data.get("source") or "local_vision"),
            bounding_box=item.get("bounding_box"),
            raw_text=item.get("raw_text") or data.get("raw_text"),
            reasoning=item.get("reasoning"),
            estimated_grams=item.get("estimated_grams"),
            unit=item.get("unit"),
            visible_evidence=item.get("visible_evidence"),
        )
        for item in foods
    ]


ocr_service_client = OcrServiceClient()
