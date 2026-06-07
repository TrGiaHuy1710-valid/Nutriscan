from copy import deepcopy

from app.modules.food.graph.state import FoodSessionState


_SESSION_STORE: dict[str, FoodSessionState] = {}


def get_session(session_id: str) -> FoodSessionState | None:
    state = _SESSION_STORE.get(session_id)
    return deepcopy(state) if state is not None else None


def save_session(session_id: str, state: FoodSessionState) -> None:
    _SESSION_STORE[session_id] = deepcopy(state)


def delete_session(session_id: str) -> None:
    _SESSION_STORE.pop(session_id, None)


def clear_sessions() -> None:
    _SESSION_STORE.clear()


# TODO: Replace this MVP in-memory store with Redis for multi-process production use.
