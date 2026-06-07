import re
import unicodedata


class FoodNormalizationService:
    def normalize_name(self, name: str) -> str:
        normalized = unicodedata.normalize("NFKC", name or "").strip().lower()
        normalized = re.sub(r"\s+", " ", normalized)
        return normalized


normalization_service = FoodNormalizationService()
