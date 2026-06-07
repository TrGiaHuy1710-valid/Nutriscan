from app.modules.food.normalization_service import normalization_service
from app.modules.food.schemas import ConfirmedFoodInput, FoodNutritionItem, NutritionSummary


NUTRITION_PER_100G: dict[str, dict[str, float]] = {
    "com trang": {"calories": 130, "protein": 2.7, "carbs": 28.2, "fat": 0.3},
    "rice": {"calories": 130, "protein": 2.7, "carbs": 28.2, "fat": 0.3},
    "ga": {"calories": 165, "protein": 31, "carbs": 0, "fat": 3.6},
    "chicken": {"calories": 165, "protein": 31, "carbs": 0, "fat": 3.6},
    "trung": {"calories": 155, "protein": 13, "carbs": 1.1, "fat": 11},
    "egg": {"calories": 155, "protein": 13, "carbs": 1.1, "fat": 11},
    "thit bo": {"calories": 250, "protein": 26, "carbs": 0, "fat": 15},
    "beef": {"calories": 250, "protein": 26, "carbs": 0, "fat": 15},
}


class NutritionService:
    def calculate(self, foods: list[ConfirmedFoodInput], people_count: int) -> tuple[list[FoodNutritionItem], NutritionSummary]:
        items: list[FoodNutritionItem] = []

        for food in foods:
            normalized_name = normalization_service.normalize_name(food.normalized_name or food.name)
            grams = food.grams or _grams_from_serving(food.serving_size)
            base = NUTRITION_PER_100G.get(normalized_name)
            needs_manual_review = base is None
            base = base or {"calories": 100, "protein": 3, "carbs": 15, "fat": 3}
            factor = grams / 100

            calories = round(base["calories"] * factor, 2)
            protein = round(base["protein"] * factor, 2)
            carbs = round(base["carbs"] * factor, 2)
            fat = round(base["fat"] * factor, 2)

            items.append(
                FoodNutritionItem(
                    name=food.name,
                    normalized_name=normalized_name,
                    grams=grams,
                    calories=calories,
                    protein=protein,
                    carbs=carbs,
                    fat=fat,
                    calories_per_person=round(calories / people_count, 2),
                    protein_per_person=round(protein / people_count, 2),
                    carbs_per_person=round(carbs / people_count, 2),
                    fat_per_person=round(fat / people_count, 2),
                    estimated=True,
                    needs_manual_review=needs_manual_review,
                )
            )

        summary = NutritionSummary(
            calories=round(sum(item.calories for item in items), 2),
            protein=round(sum(item.protein for item in items), 2),
            carbs=round(sum(item.carbs for item in items), 2),
            fat=round(sum(item.fat for item in items), 2),
            calories_per_person=round(sum(item.calories_per_person for item in items), 2),
            protein_per_person=round(sum(item.protein_per_person for item in items), 2),
            carbs_per_person=round(sum(item.carbs_per_person for item in items), 2),
            fat_per_person=round(sum(item.fat_per_person for item in items), 2),
            estimated=True,
            needs_manual_review=any(item.needs_manual_review for item in items),
        )
        return items, summary


def _grams_from_serving(serving_size: str | None) -> float:
    if not serving_size:
        return 100

    digits = "".join(ch for ch in serving_size if ch.isdigit() or ch == ".")
    return float(digits) if digits else 100


nutrition_service = NutritionService()
