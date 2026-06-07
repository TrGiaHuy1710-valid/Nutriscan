from copy import deepcopy


class FoodSessionService:
    def __init__(self) -> None:
        self._sessions: dict[str, dict] = {}

    def get(self, session_id: str) -> dict | None:
        session = self._sessions.get(session_id)
        return deepcopy(session) if session is not None else None

    def save(self, session_id: str, state: dict) -> None:
        self._sessions[session_id] = deepcopy(state)

    def delete(self, session_id: str) -> None:
        self._sessions.pop(session_id, None)

    def clear(self) -> None:
        self._sessions.clear()


food_session_service = FoodSessionService()
