from functools import lru_cache
from pathlib import Path

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    APP_NAME: str = "Food AI Backend"
    APP_ENV: str = "dev"
    API_PREFIX: str = "/api/v1"

    LLM_PROVIDER: str = "gemini"
    GEMINI_API_KEY: str | None = None
    GEMINI_MODEL: str = "gemini-2.5-flash"
    OCR_SERVICE_BASE_URL: str = "http://localhost:5000"

    UPLOAD_DIR: str = "../../storage/uploads/api"
    MAX_UPLOAD_MB: int = 10

    SESSION_BACKEND: str = "memory"
    REDIS_URL: str = "redis://localhost:6379/0"

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    @property
    def upload_path(self) -> Path:
        return Path(self.UPLOAD_DIR)


@lru_cache
def get_settings() -> Settings:
    return Settings()


settings = get_settings()
