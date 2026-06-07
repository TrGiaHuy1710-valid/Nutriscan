from app.core.config import settings
from app.core.errors import UnsupportedProviderError
from app.integrations.providers.base import BaseLLMProvider
from app.integrations.providers.gemini_provider import GeminiProvider


def get_llm_provider() -> BaseLLMProvider:
    if settings.LLM_PROVIDER == "gemini":
        return GeminiProvider()

    # TODO: Add OpenAIProvider
    # TODO: Add ClaudeProvider
    raise UnsupportedProviderError(f"Unsupported LLM_PROVIDER: {settings.LLM_PROVIDER}")
