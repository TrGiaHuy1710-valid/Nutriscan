import pytest
from httpx import ASGITransport, AsyncClient

from app.main import app
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


@pytest.mark.anyio
async def test_health(client: AsyncClient) -> None:
    response = await client.get("/health")

    assert response.status_code == 200
    assert response.json() == {
        "status": "ok",
        "service": "Food AI Backend",
    }


@pytest.mark.anyio
async def test_unsupported_file_type(client: AsyncClient) -> None:
    response = await client.post(
        "/api/v1/food/analyze",
        files={"image": ("meal.txt", b"not an image", "text/plain")},
    )

    assert response.status_code == 400
    assert response.json()["detail"] == "Unsupported image type"


@pytest.mark.anyio
async def test_session_not_found(client: AsyncClient) -> None:
    response = await client.get("/api/v1/food/sessions/missing-session")

    assert response.status_code == 404
    assert response.json()["detail"] == "Session not found"


@pytest.mark.anyio
async def test_chat_completes_with_mocked_provider(
    client: AsyncClient,
    monkeypatch,
) -> None:
    class FakeProvider:
        async def analyze_food_image(self, image_path: str) -> dict:
            return {"detected_foods": []}

        async def extract_food_info_from_text(self, message: str) -> dict:
            return {
                "foods": [
                    {
                        "name": "com trang",
                        "grams": 300,
                        "unit": "g",
                    }
                ],
                "people_count": 2,
            }

    monkeypatch.setattr(
        "app.modules.food.graph.nodes.get_llm_provider",
        lambda: FakeProvider(),
    )
    save_session(
        "session-1",
        {
            "session_id": "session-1",
            "detected_foods": [{"name": "com trang"}],
            "status": "need_more_info",
        },
    )

    response = await client.post(
        "/api/v1/food/chat",
        json={
            "session_id": "session-1",
            "message": "Co com trang 300g, 2 nguoi an",
        },
    )

    assert response.status_code == 200
    body = response.json()
    assert body["status"] == "completed"
    assert body["people_count"] == 2
    assert body["foods"][0]["grams_per_person"] == 150
