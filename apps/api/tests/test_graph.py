from app.modules.food.graph.nodes import ask_user_node, check_missing_info_node, finalize_result_node


def test_check_missing_info_node_requires_people_count() -> None:
    state = {
        "confirmed_foods": [
            {
                "name": "com trang",
                "grams": 300,
                "unit": "g",
            }
        ]
    }

    result = check_missing_info_node(state)

    assert result["status"] == "need_more_info"
    assert "people_count" in result["missing_fields"]


def test_finalize_result_node_calculates_grams_per_person() -> None:
    state = {
        "confirmed_foods": [
            {
                "name": "com trang",
                "grams": 300,
                "unit": "g",
            }
        ],
        "people_count": 2,
    }

    result = finalize_result_node(state)

    assert result["status"] == "completed"
    assert result["confirmed_foods"][0]["grams_per_person"] == 150


def test_ask_user_node_returns_vietnamese_question() -> None:
    state = {
        "detected_foods": [{"name": "com trang"}],
        "missing_fields": ["people_count", "amount_for_com trang"],
    }

    result = ask_user_node(state)

    assert result["status"] == "need_more_info"
    assert "bao nhieu" in _strip_vietnamese_tone(result["assistant_message"]).lower()


def _strip_vietnamese_tone(value: str) -> str:
    replacements = {
        "á": "a",
        "à": "a",
        "ả": "a",
        "ã": "a",
        "ạ": "a",
        "ă": "a",
        "ắ": "a",
        "ằ": "a",
        "ẳ": "a",
        "ẵ": "a",
        "ặ": "a",
        "â": "a",
        "ấ": "a",
        "ầ": "a",
        "ẩ": "a",
        "ẫ": "a",
        "ậ": "a",
        "é": "e",
        "è": "e",
        "ẻ": "e",
        "ẽ": "e",
        "ẹ": "e",
        "ê": "e",
        "ế": "e",
        "ề": "e",
        "ể": "e",
        "ễ": "e",
        "ệ": "e",
        "í": "i",
        "ì": "i",
        "ỉ": "i",
        "ĩ": "i",
        "ị": "i",
        "ó": "o",
        "ò": "o",
        "ỏ": "o",
        "õ": "o",
        "ọ": "o",
        "ô": "o",
        "ố": "o",
        "ồ": "o",
        "ổ": "o",
        "ỗ": "o",
        "ộ": "o",
        "ơ": "o",
        "ớ": "o",
        "ờ": "o",
        "ở": "o",
        "ỡ": "o",
        "ợ": "o",
        "ú": "u",
        "ù": "u",
        "ủ": "u",
        "ũ": "u",
        "ụ": "u",
        "ư": "u",
        "ứ": "u",
        "ừ": "u",
        "ử": "u",
        "ữ": "u",
        "ự": "u",
        "ý": "y",
        "ỳ": "y",
        "ỷ": "y",
        "ỹ": "y",
        "ỵ": "y",
        "đ": "d",
    }
    return "".join(replacements.get(char, char) for char in value)
