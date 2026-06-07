import pytest
from httpx import ASGITransport, AsyncClient

from app.main import app
from app.modules.food.repository import food_repository
from app.services.session_store import clear_sessions, save_session


@pytest.fixture
def anyio_backend() -> str:
    return "asyncio"


@pytest.fixture
async def client() -> AsyncClient:
    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as test_client:
        yield test_client


def setup_function() -> None:
    clear_sessions()
    food_repository._meals.clear()
    food_repository._feedback.clear()


@pytest.mark.anyio
async def test_analyze_food_uses_ocr_service_contract(
    client: AsyncClient,
    monkeypatch,
) -> None:
    async def fake_analyze_food_image(image_path: str, request_id: str, debug: bool = False) -> dict:
        return {
            "request_id": request_id,
            "detected_foods": [
                {
                    "name": "rice",
                    "normalized_name": "rice",
                    "confidence": 0.82,
                    "source": "local_ocr",
                }
            ],
            "confidence": 0.82,
            "source": "local_ocr",
        }

    monkeypatch.setattr(
        "app.modules.food.service.ocr_service_client.analyze_food_image",
        fake_analyze_food_image,
    )

    response = await client.post(
        "/api/v1/food/analyze",
        files={"image": ("meal.png", b"fake image bytes", "image/png")},
        data={"request_id": "request-analyze"},
    )

    assert response.status_code == 200
    body = response.json()
    assert body["request_id"] == "request-analyze"
    assert body["status"] == "need_more_info"
    assert body["detected_foods"][0]["name"] == "rice"


@pytest.mark.anyio
async def test_confirm_food_calculates_nutrition_and_creates_history(client: AsyncClient) -> None:
    save_session(
        "session-confirm",
        {
            "session_id": "session-confirm",
            "request_id": "request-confirm",
            "detected_foods": [{"name": "rice"}],
            "status": "need_more_info",
        },
    )

    response = await client.post(
        "/api/v1/food/confirm",
        json={
            "session_id": "session-confirm",
            "request_id": "request-confirm",
            "foods": [{"name": "rice", "grams": 200}],
            "people_count": 2,
        },
    )

    assert response.status_code == 200
    body = response.json()
    assert body["status"] == "completed"
    assert body["summary"]["calories"] == 260
    assert body["summary"]["calories_per_person"] == 130

    history_response = await client.get("/api/v1/meals/history")
    assert history_response.status_code == 200
    assert history_response.json()["meals"][0]["meal_id"] == body["meal_id"]


@pytest.mark.anyio
async def test_confirm_food_requires_existing_session(client: AsyncClient) -> None:
    response = await client.post(
        "/api/v1/food/confirm",
        json={
            "session_id": "missing-session",
            "foods": [{"name": "rice", "grams": 100}],
            "people_count": 1,
        },
    )

    assert response.status_code == 404
    assert response.json()["detail"] == "Session not found"


@pytest.mark.anyio
async def test_food_feedback_is_saved_for_existing_session(client: AsyncClient) -> None:
    save_session("session-feedback", {"session_id": "session-feedback", "request_id": "request-feedback"})

    response = await client.post(
        "/api/v1/food/session-feedback/feedback",
        json={
            "request_id": "request-feedback",
            "corrected_foods": [{"name": "chicken", "grams": 150}],
            "rating": 4,
        },
    )

    assert response.status_code == 200
    assert response.json()["status"] == "saved"


@pytest.mark.anyio
async def test_food_validation_uses_service_contract(client: AsyncClient) -> None:
    response = await client.post(
        "/api/v1/food/validate",
        json={
            "rawText": "Nutrition calories protein fat carb",
            "fileName": "label.png",
            "ocrNutrition": {
                "calories": 520,
                "protein": 15,
                "carbs": 45,
                "fat": 20,
                "sugar": 12,
            },
        },
    )

    assert response.status_code == 200
    body = response.json()
    assert body["isFoodLabel"] is True
    assert body["nutrition"]["calories"] == 520
    assert {flag["type"] for flag in body["flags"]} == {"high_sugar", "high_calorie"}
