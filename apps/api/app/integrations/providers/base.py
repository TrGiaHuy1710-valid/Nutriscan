from abc import ABC, abstractmethod
from typing import Any


class BaseLLMProvider(ABC):
    @abstractmethod
    async def analyze_food_image(self, image_path: str) -> dict[str, Any]:
        raise NotImplementedError

    @abstractmethod
    async def extract_food_info_from_text(self, message: str) -> dict[str, Any]:
        raise NotImplementedError
