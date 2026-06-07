from fastapi import HTTPException


class AppError(Exception):
    message = "Application error"
    status_code = 500

    def __init__(self, message: str | None = None) -> None:
        super().__init__(message or self.message)
        self.message = message or self.message


class MissingAPIKeyError(AppError):
    message = "Gemini API key is not configured"
    status_code = 500


class ProviderError(AppError):
    message = "LLM provider failed"
    status_code = 502


class InvalidLLMJSONError(AppError):
    message = "LLM returned invalid JSON"
    status_code = 502


class UnsupportedProviderError(AppError):
    status_code = 500


def app_error_to_http(error: AppError) -> HTTPException:
    return HTTPException(status_code=error.status_code, detail=error.message)
