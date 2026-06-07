from fastapi import HTTPException


class ErrorCode:
    IMAGE_TOO_LARGE = "IMAGE_TOO_LARGE"
    UNSUPPORTED_IMAGE_TYPE = "UNSUPPORTED_IMAGE_TYPE"
    OCR_SERVICE_UNAVAILABLE = "OCR_SERVICE_UNAVAILABLE"
    GEMINI_API_ERROR = "GEMINI_API_ERROR"
    LOW_CONFIDENCE_RESULT = "LOW_CONFIDENCE_RESULT"
    SESSION_NOT_FOUND = "SESSION_NOT_FOUND"
    INVALID_CONFIRMATION_PAYLOAD = "INVALID_CONFIRMATION_PAYLOAD"
    NUTRITION_NOT_FOUND = "NUTRITION_NOT_FOUND"
    INTERNAL_ERROR = "INTERNAL_ERROR"


class AppError(Exception):
    message = "Application error"
    status_code = 500
    error_code = ErrorCode.INTERNAL_ERROR

    def __init__(self, message: str | None = None, error_code: str | None = None) -> None:
        super().__init__(message or self.message)
        self.message = message or self.message
        self.error_code = error_code or self.error_code


class MissingAPIKeyError(AppError):
    message = "Gemini API key is not configured"
    status_code = 500
    error_code = ErrorCode.GEMINI_API_ERROR


class ProviderError(AppError):
    message = "LLM provider failed"
    status_code = 502
    error_code = ErrorCode.GEMINI_API_ERROR


class InvalidLLMJSONError(AppError):
    message = "LLM returned invalid JSON"
    status_code = 502
    error_code = ErrorCode.GEMINI_API_ERROR


class UnsupportedProviderError(AppError):
    status_code = 500
    error_code = ErrorCode.INTERNAL_ERROR


class OcrServiceUnavailableError(AppError):
    message = "OCR service is unavailable"
    status_code = 503
    error_code = ErrorCode.OCR_SERVICE_UNAVAILABLE


class SessionNotFoundError(AppError):
    message = "Session not found"
    status_code = 404
    error_code = ErrorCode.SESSION_NOT_FOUND


class InvalidConfirmationPayloadError(AppError):
    message = "Invalid confirmation payload"
    status_code = 422
    error_code = ErrorCode.INVALID_CONFIRMATION_PAYLOAD


def app_error_to_http(error: AppError) -> HTTPException:
    return HTTPException(status_code=error.status_code, detail=error.message)
