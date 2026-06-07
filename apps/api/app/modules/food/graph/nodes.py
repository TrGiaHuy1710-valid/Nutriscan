from app.modules.food.graph.state import FoodItemState, FoodSessionState
from app.integrations.providers.provider_factory import get_llm_provider


async def analyze_image_node(state: FoodSessionState) -> dict:
    provider = get_llm_provider()
    result = await provider.analyze_food_image(state["image_path"])

    return {
        "detected_foods": result.get("detected_foods", []),
        "status": "image_analyzed",
    }


async def extract_user_info_node(state: FoodSessionState) -> dict:
    provider = get_llm_provider()
    result = await provider.extract_food_info_from_text(state["user_message"])
    old_items = state.get("confirmed_foods") or state.get("detected_foods") or []
    merged_foods = merge_food_items(old_items=old_items, new_items=result.get("foods", []))

    return {
        "confirmed_foods": merged_foods,
        "people_count": result.get("people_count") or state.get("people_count"),
    }


def check_missing_info_node(state: FoodSessionState) -> dict:
    missing: list[str] = []
    foods = state.get("confirmed_foods") or state.get("detected_foods") or []

    if not foods:
        missing.append("foods")

    if not state.get("people_count"):
        missing.append("people_count")

    for food in foods:
        name = food.get("name", "unknown_food")
        grams = food.get("grams")
        unit = food.get("unit")

        if grams is None and not unit:
            missing.append(f"amount_for_{name}")

    if missing:
        return {
            "missing_fields": missing,
            "status": "need_more_info",
        }

    return {
        "missing_fields": [],
        "status": "ready",
    }


def ask_user_node(state: FoodSessionState) -> dict:
    missing = state.get("missing_fields", [])
    foods = state.get("confirmed_foods") or state.get("detected_foods") or []
    food_names = [food.get("name") for food in foods if food.get("name")]
    amount_missing = [
        field.replace("amount_for_", "", 1)
        for field in missing
        if field.startswith("amount_for_")
    ]

    if "foods" in missing:
        question = (
            "Bạn cho mình biết trong ảnh có những món gì, mỗi món khoảng bao nhiêu "
            "gram hoặc khẩu phần, và có bao nhiêu người ăn nhé?"
        )
    else:
        parts: list[str] = []
        if food_names:
            parts.append(f"Mình thấy có thể có: {', '.join(food_names)}.")
        if amount_missing:
            parts.append(
                "Bạn xác nhận giúp "
                f"{', '.join(amount_missing)} khoảng bao nhiêu gram hoặc bao nhiêu phần."
            )
        if "people_count" in missing:
            parts.append("Bữa này có bao nhiêu người ăn?")
        question = " ".join(parts) or (
            "Bạn cho mình biết mỗi món khoảng bao nhiêu gram hoặc khẩu phần nhé?"
        )

    return {
        "assistant_message": question,
        "status": "need_more_info",
    }


def finalize_result_node(state: FoodSessionState) -> dict:
    foods = state.get("confirmed_foods") or state.get("detected_foods") or []
    people_count = state.get("people_count")
    final_foods: list[FoodItemState] = []

    for food in foods:
        grams = food.get("grams")
        if grams is None and food.get("estimated_grams") is not None:
            grams = food.get("estimated_grams")

        grams_per_person = None
        if grams is not None and people_count:
            grams_per_person = round(float(grams) / int(people_count), 2)

        final_foods.append(
            {
                "name": food.get("name", ""),
                "grams": grams,
                "unit": food.get("unit") or ("g" if grams is not None else None),
                "grams_per_person": grams_per_person,
            }
        )

    return {
        "confirmed_foods": final_foods,
        "status": "completed",
        "assistant_message": "Mình đã tổng hợp xong thông tin bữa ăn.",
    }


def merge_food_items(
    old_items: list[FoodItemState],
    new_items: list[dict],
) -> list[FoodItemState]:
    merged: dict[str, FoodItemState] = {}
    order: list[str] = []

    for item in old_items:
        name = str(item.get("name", "")).strip()
        if not name:
            continue
        key = _food_key(name)
        merged[key] = dict(item)
        order.append(key)

    for item in new_items:
        name = str(item.get("name", "")).strip()
        if not name:
            continue
        key = _food_key(name)
        if key not in merged:
            merged[key] = {"name": name}
            order.append(key)

        merged[key]["name"] = name
        if "grams" in item:
            merged[key]["grams"] = item.get("grams")
        if "unit" in item:
            merged[key]["unit"] = item.get("unit")

    return [merged[key] for key in order]


def _food_key(name: str) -> str:
    return " ".join(name.lower().strip().split())
