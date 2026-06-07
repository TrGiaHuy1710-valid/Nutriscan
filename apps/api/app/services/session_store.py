from copy import deepcopy

from app.modules.food.graph.state import FoodSessionState
from app.modules.food.session_service import food_session_service


def get_session(session_id: str) -> FoodSessionState | None:
    state = food_session_service.get(session_id)
    return deepcopy(state) if state is not None else None


def save_session(session_id: str, state: FoodSessionState) -> None:
    food_session_service.save(session_id, deepcopy(state))


def delete_session(session_id: str) -> None:
    food_session_service.delete(session_id)


def clear_sessions() -> None:
    food_session_service.clear()


# TODO: Replace this MVP in-memory store with Redis for multi-process production use.
