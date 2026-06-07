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
