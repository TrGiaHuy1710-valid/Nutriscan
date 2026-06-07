from dataclasses import dataclass
import os
from pathlib import Path
from typing import FrozenSet


@dataclass(frozen=True)
class OcrServiceSettings:
    upload_folder: Path
    allowed_extensions: FrozenSet[str]
    max_file_size: int
    host: str
    port: int
    debug: bool

    @staticmethod
    def from_env() -> "OcrServiceSettings":
        service_root = Path(__file__).resolve().parents[1]
        repo_root = service_root.parents[1]
        default_uploads = repo_root / "storage" / "uploads" / "ocr-service"

        extensions = os.getenv("NUTRISCAN_ALLOWED_EXTENSIONS", "png,jpg,jpeg,gif,bmp,webp")

        return OcrServiceSettings(
            upload_folder=Path(os.getenv("NUTRISCAN_UPLOAD_FOLDER", default_uploads)),
            allowed_extensions=frozenset(
                item.strip().lower() for item in extensions.split(",") if item.strip()
            ),
            max_file_size=int(os.getenv("NUTRISCAN_MAX_FILE_SIZE", str(16 * 1024 * 1024))),
            host=os.getenv("NUTRISCAN_OCR_HOST", "0.0.0.0"),
            port=int(os.getenv("NUTRISCAN_OCR_PORT", "5000")),
            debug=os.getenv("NUTRISCAN_OCR_DEBUG", "false").lower() == "true",
        )


@dataclass(frozen=True)
class OcrAnalyzeResult:
    raw_text: str
    nutrition: dict
    detected_foods: list[dict] | None = None
    confidence: float = 0.0
    source: str = "local_ocr"
    fallback_used: bool = False
    error_code: str | None = None
    message: str = ""
    debug_info: dict | None = None

    def to_internal_response(self, request_id: str) -> dict:
        return {
            "request_id": request_id,
            "raw_text": self.raw_text,
            "nutrition": self.nutrition,
            "detected_foods": self.detected_foods or [],
            "confidence": self.confidence,
            "source": self.source,
            "fallback_used": self.fallback_used,
            "error_code": self.error_code,
            "message": self.message,
            "debug_info": self.debug_info or {},
        }
