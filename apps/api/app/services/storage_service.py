import uuid
from pathlib import Path

import aiofiles
from fastapi import HTTPException, UploadFile

from app.core.config import settings


ALLOWED_EXTENSIONS = {".jpg", ".jpeg", ".png", ".webp"}
ALLOWED_CONTENT_TYPES = {"image/jpeg", "image/png", "image/webp"}


async def save_upload_file(file: UploadFile) -> str:
    suffix = Path(file.filename or "").suffix.lower()

    if suffix not in ALLOWED_EXTENSIONS or file.content_type not in ALLOWED_CONTENT_TYPES:
        raise HTTPException(status_code=400, detail="Unsupported image type")

    content = await file.read()
    max_bytes = settings.MAX_UPLOAD_MB * 1024 * 1024

    if len(content) > max_bytes:
        raise HTTPException(status_code=400, detail="File too large")

    output_dir = settings.upload_path
    output_dir.mkdir(parents=True, exist_ok=True)
    output_path = output_dir / f"{uuid.uuid4()}{suffix}"

    async with aiofiles.open(output_path, "wb") as output_file:
        await output_file.write(content)

    return str(output_path)
