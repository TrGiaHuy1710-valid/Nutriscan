from fastapi import HTTPException, UploadFile

from app.core.errors import AppError, app_error_to_http
from app.modules.food.graph.state import FoodSessionState
from app.modules.food.graph.workflow import run_chat_workflow
from app.modules.food.service import (
    analyze_food_image as analyze_food_image_from_module,
    get_food_session as get_food_session_from_module,
)
from app.schemas.food import AnalyzeFoodResponse, ChatFoodResponse, SessionResponse
from app.services.session_store import get_session, save_session


async def analyze_food_image(
    image: UploadFile,
    session_id: str | None = None,
) -> AnalyzeFoodResponse:
    return await analyze_food_image_from_module(image=image, session_id=session_id)


async def continue_food_chat(session_id: str, message: str) -> ChatFoodResponse:
    state = get_session(session_id)
    if state is None:
        raise HTTPException(status_code=404, detail="Session not found")

    state.update(
        {
            "session_id": session_id,
            "user_message": message,
        }
    )

    try:
        result = await run_chat_workflow(state)
    except AppError as exc:
        raise app_error_to_http(exc) from exc

    result["session_id"] = session_id
    save_session(session_id, result)

    foods = result.get("confirmed_foods", [])
    summary = None
    if result.get("status") == "completed":
        summary = f"Bữa ăn đã được tổng hợp xong cho {result.get('people_count')} người."

    return ChatFoodResponse(
        session_id=session_id,
        status=result.get("status", "need_more_info"),
        confirmed_foods=[] if result.get("status") == "completed" else foods,
        foods=foods if result.get("status") == "completed" else [],
        people_count=result.get("people_count"),
        missing_fields=result.get("missing_fields", []),
        assistant_message=result.get("assistant_message", ""),
        summary=summary,
    )


def get_food_session(session_id: str) -> SessionResponse:
    return get_food_session_from_module(session_id)
