# SKILL.md — Backend Food Image Analysis Service

## 1. Goal

Build a backend service that allows a user to upload a food image, then uses an LLM vision provider to detect possible dishes and starts a short conversational flow to collect missing information:

- What dishes are in the image?
- How many grams or portions for each dish?
- How many people will eat?
- Return a structured JSON result that can later be used for nutrition, calories, meal sharing, or logging.

The service should be implemented as a Python backend using:

- FastAPI for HTTP API
- LangGraph for stateful workflow orchestration
- Gemini API as the default vision provider
- Optional provider abstraction for OpenAI, Claude, or other LLM providers
- Pydantic for request/response schemas
- Redis or in-memory store for session state in MVP
- Local file storage for image uploads in MVP

Do not build a frontend. Only build the backend service.

---

## 2. Product Behavior

### Main User Flow

```text
User uploads food image
→ Backend saves image
→ Gemini Vision analyzes image
→ Backend returns detected food candidates
→ Backend asks user to confirm dishes, grams, and number of people
→ User replies with text
→ Backend extracts structured food information
→ Backend checks if required information is complete
→ If incomplete: ask follow-up question
→ If complete: return final structured result
```

### Important Design Principle

Do not pretend that the AI can accurately estimate grams from an image.

The image model may suggest possible dishes, but the final grams should come from the user or be explicitly marked as uncertain.

For example:

```json
{
  "name": "cơm trắng",
  "confidence": 0.82,
  "estimated_grams": null,
  "needs_user_confirmation": true
}
```

---

## 3. Tech Stack

Use the following stack:

```text
Python 3.11+
FastAPI
Uvicorn
LangGraph
Pydantic v2
google-genai
python-dotenv
python-multipart
Redis optional
```

### Recommended `requirements.txt`

```txt
fastapi
uvicorn[standard]
python-multipart
pydantic
pydantic-settings
python-dotenv
langgraph
google-genai
redis
aiofiles
```

---

## 4. Project Structure

Create this folder structure:

```text
food-ai-backend/
│
├── app/
│   ├── main.py
│   │
│   ├── api/
│   │   ├── __init__.py
│   │   ├── food.py
│   │   └── chat.py
│   │
│   ├── core/
│   │   ├── __init__.py
│   │   ├── config.py
│   │   └── errors.py
│   │
│   ├── graph/
│   │   ├── __init__.py
│   │   ├── state.py
│   │   ├── nodes.py
│   │   └── workflow.py
│   │
│   ├── providers/
│   │   ├── __init__.py
│   │   ├── base.py
│   │   ├── gemini_provider.py
│   │   └── provider_factory.py
│   │
│   ├── schemas/
│   │   ├── __init__.py
│   │   ├── food.py
│   │   └── chat.py
│   │
│   ├── services/
│   │   ├── __init__.py
│   │   ├── storage_service.py
│   │   └── session_store.py
│   │
│   └── utils/
│       ├── __init__.py
│       └── json_utils.py
│
├── uploads/
├── tests/
│   ├── test_food_api.py
│   └── test_graph.py
│
├── .env.example
├── requirements.txt
├── README.md
└── SKILL.md
```

---

## 5. Environment Variables

Create `.env.example`:

```env
APP_NAME=Food AI Backend
APP_ENV=dev
API_PREFIX=/api/v1

LLM_PROVIDER=gemini
GEMINI_API_KEY=replace_with_your_key
GEMINI_MODEL=gemini-2.5-flash

UPLOAD_DIR=uploads
MAX_UPLOAD_MB=10

SESSION_BACKEND=memory
REDIS_URL=redis://localhost:6379/0
```

Implementation rule:

- If `.env` is missing, the app should still start but provider calls must fail with a clear error message.
- Never hard-code API keys.
- Never print API keys in logs.

---

## 6. API Endpoints

### 6.1 Health Check

```http
GET /health
```

Response:

```json
{
  "status": "ok",
  "service": "Food AI Backend"
}
```

---

### 6.2 Upload and Analyze Food Image

```http
POST /api/v1/food/analyze
Content-Type: multipart/form-data
```

Request fields:

```text
image: file, required
session_id: string, optional
```

Behavior:

1. Validate image extension and content type.
2. Save image to local upload folder.
3. Create or reuse session.
4. Run LangGraph workflow from image analysis step.
5. Return detected food candidates and a question asking the user to confirm missing information.

Response example:

```json
{
  "session_id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "need_more_info",
  "detected_foods": [
    {
      "name": "cơm trắng",
      "confidence": 0.82,
      "estimated_grams": null,
      "unit": null,
      "visible_evidence": "Một phần cơm màu trắng trên đĩa"
    },
    {
      "name": "thịt kho",
      "confidence": 0.71,
      "estimated_grams": null,
      "unit": null,
      "visible_evidence": "Miếng thịt màu nâu cạnh phần cơm"
    }
  ],
  "missing_fields": [
    "people_count",
    "amount_for_cơm trắng",
    "amount_for_thịt kho"
  ],
  "assistant_message": "Mình thấy có thể có cơm trắng và thịt kho. Bạn xác nhận giúp có những món gì, mỗi món khoảng bao nhiêu gram hoặc khẩu phần, và có bao nhiêu người ăn?"
}
```

---

### 6.3 Continue Food Conversation

```http
POST /api/v1/food/chat
Content-Type: application/json
```

Request:

```json
{
  "session_id": "550e8400-e29b-41d4-a716-446655440000",
  "message": "Có cơm trắng 300g, thịt kho 200g, canh rau 1 bát, 2 người ăn"
}
```

Behavior:

1. Load session state.
2. Extract food names, grams/units, and people count from user message.
3. Merge extracted info into current session state.
4. Run `check_missing_info`.
5. If still missing info, return a follow-up question.
6. If complete, return final structured result.

Incomplete response example:

```json
{
  "session_id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "need_more_info",
  "confirmed_foods": [
    {
      "name": "cơm trắng",
      "grams": 300,
      "unit": "g"
    },
    {
      "name": "thịt kho",
      "grams": null,
      "unit": null
    }
  ],
  "people_count": 2,
  "missing_fields": [
    "amount_for_thịt kho"
  ],
  "assistant_message": "Bạn cho mình biết thịt kho khoảng bao nhiêu gram hoặc bao nhiêu miếng/phần nhé?"
}
```

Complete response example:

```json
{
  "session_id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "completed",
  "people_count": 2,
  "foods": [
    {
      "name": "cơm trắng",
      "grams": 300,
      "unit": "g",
      "grams_per_person": 150
    },
    {
      "name": "thịt kho",
      "grams": 200,
      "unit": "g",
      "grams_per_person": 100
    },
    {
      "name": "canh rau",
      "grams": null,
      "unit": "1 bát",
      "grams_per_person": null
    }
  ],
  "summary": "Bữa ăn gồm 3 món, chia cho 2 người. Một số món dùng đơn vị khẩu phần nên chưa thể tính gram/người chính xác."
}
```

---

### 6.4 Get Session

```http
GET /api/v1/food/sessions/{session_id}
```

Return current session state.

---

## 7. Data Schemas

Create Pydantic schemas in `app/schemas/food.py`.

### FoodCandidate

```python
from pydantic import BaseModel, Field
from typing import Optional


class FoodCandidate(BaseModel):
    name: str
    confidence: float = Field(ge=0.0, le=1.0)
    estimated_grams: Optional[float] = None
    unit: Optional[str] = None
    visible_evidence: Optional[str] = None
```

### ConfirmedFoodItem

```python
class ConfirmedFoodItem(BaseModel):
    name: str
    grams: Optional[float] = None
    unit: Optional[str] = None
    grams_per_person: Optional[float] = None
```

### AnalyzeFoodResponse

```python
from typing import List, Literal


class AnalyzeFoodResponse(BaseModel):
    session_id: str
    status: Literal["need_more_info", "completed", "error"]
    detected_foods: List[FoodCandidate] = []
    missing_fields: List[str] = []
    assistant_message: str
```

### ChatFoodRequest

```python
class ChatFoodRequest(BaseModel):
    session_id: str
    message: str
```

### ChatFoodResponse

```python
class ChatFoodResponse(BaseModel):
    session_id: str
    status: Literal["need_more_info", "completed", "error"]
    confirmed_foods: List[ConfirmedFoodItem] = []
    people_count: Optional[int] = None
    missing_fields: List[str] = []
    assistant_message: str
```

---

## 8. LangGraph State

Create `app/graph/state.py`.

Use a TypedDict state:

```python
from typing import TypedDict, Optional, List, Literal


class FoodItemState(TypedDict, total=False):
    name: str
    confidence: Optional[float]
    estimated_grams: Optional[float]
    grams: Optional[float]
    unit: Optional[str]
    visible_evidence: Optional[str]


class FoodSessionState(TypedDict, total=False):
    session_id: str
    image_path: str
    user_message: str

    detected_foods: List[FoodItemState]
    confirmed_foods: List[FoodItemState]

    people_count: Optional[int]
    missing_fields: List[str]

    status: Literal[
        "image_uploaded",
        "image_analyzed",
        "need_more_info",
        "ready",
        "completed",
        "error"
    ]

    assistant_message: str
    error_message: Optional[str]
```

---

## 9. LangGraph Nodes

Create `app/graph/nodes.py`.

### 9.1 `analyze_image_node`

Input state:

```python
{
  "image_path": "...",
  "session_id": "..."
}
```

Behavior:

- Use provider factory to get selected vision provider.
- Call `provider.analyze_food_image(image_path)`.
- Parse returned JSON.
- Update state with:
  - `detected_foods`
  - `status = "image_analyzed"`

Pseudo-code:

```python
async def analyze_image_node(state: FoodSessionState) -> dict:
    provider = get_llm_provider()
    result = await provider.analyze_food_image(state["image_path"])

    return {
        "detected_foods": result.get("detected_foods", []),
        "status": "image_analyzed"
    }
```

---

### 9.2 `extract_user_info_node`

Input state:

```python
{
  "user_message": "...",
  "confirmed_foods": [...]
}
```

Behavior:

- Use LLM text extraction or a deterministic parser.
- Extract:
  - food names
  - grams
  - units
  - people_count
- Merge with previous `confirmed_foods`.

The LLM extraction output must be valid JSON:

```json
{
  "foods": [
    {
      "name": "cơm trắng",
      "grams": 300,
      "unit": "g"
    }
  ],
  "people_count": 2
}
```

Pseudo-code:

```python
async def extract_user_info_node(state: FoodSessionState) -> dict:
    provider = get_llm_provider()
    result = await provider.extract_food_info_from_text(state["user_message"])

    merged_foods = merge_food_items(
        old_items=state.get("confirmed_foods") or state.get("detected_foods") or [],
        new_items=result.get("foods", [])
    )

    return {
        "confirmed_foods": merged_foods,
        "people_count": result.get("people_count") or state.get("people_count")
    }
```

---

### 9.3 `check_missing_info_node`

Required information:

- At least one food item
- `people_count`
- For each confirmed food:
  - either `grams` or `unit`

Pseudo-code:

```python
def check_missing_info_node(state: FoodSessionState) -> dict:
    missing = []

    foods = state.get("confirmed_foods") or state.get("detected_foods") or []

    if not foods:
        missing.append("foods")

    if not state.get("people_count"):
        missing.append("people_count")

    for food in foods:
        name = food.get("name", "unknown_food")
        grams = food.get("grams") or food.get("estimated_grams")
        unit = food.get("unit")

        if grams is None and not unit:
            missing.append(f"amount_for_{name}")

    if missing:
        return {
            "missing_fields": missing,
            "status": "need_more_info"
        }

    return {
        "missing_fields": [],
        "status": "ready"
    }
```

---

### 9.4 `ask_user_node`

Generate a natural Vietnamese follow-up question.

Rules:

- Be short.
- Ask only missing information.
- Do not ask for information already known.
- Mention detected foods if available.
- Use Vietnamese.

Example:

```python
def ask_user_node(state: FoodSessionState) -> dict:
    missing = state.get("missing_fields", [])
    foods = state.get("confirmed_foods") or state.get("detected_foods") or []

    food_names = [f.get("name") for f in foods if f.get("name")]

    if "people_count" in missing:
        question = "Bữa này có bao nhiêu người ăn?"
    elif food_names:
        question = (
            f"Mình thấy có thể có: {', '.join(food_names)}. "
            "Bạn xác nhận giúp mỗi món khoảng bao nhiêu gram hoặc bao nhiêu phần nhé?"
        )
    else:
        question = "Bạn cho mình biết trong ảnh có những món gì, mỗi món khoảng bao nhiêu gram và có bao nhiêu người ăn nhé?"

    return {
        "assistant_message": question,
        "status": "need_more_info"
    }
```

---

### 9.5 `finalize_result_node`

Behavior:

- Calculate `grams_per_person` if `grams` and `people_count` are available.
- Return final completed state.
- Do not calculate calories yet.
- Do not provide medical or dieting advice.

Pseudo-code:

```python
def finalize_result_node(state: FoodSessionState) -> dict:
    foods = state.get("confirmed_foods") or state.get("detected_foods") or []
    people_count = state.get("people_count")

    final_foods = []

    for food in foods:
        grams = food.get("grams") or food.get("estimated_grams")
        grams_per_person = None

        if grams is not None and people_count:
            grams_per_person = round(float(grams) / int(people_count), 2)

        final_foods.append({
            "name": food.get("name"),
            "grams": grams,
            "unit": food.get("unit") or ("g" if grams is not None else None),
            "grams_per_person": grams_per_person
        })

    return {
        "confirmed_foods": final_foods,
        "status": "completed",
        "assistant_message": "Mình đã tổng hợp xong thông tin bữa ăn."
    }
```

---

## 10. LangGraph Workflow

Create `app/graph/workflow.py`.

Graph should support two entry modes:

### Mode A: Image Upload

```text
START
→ analyze_image_node
→ check_missing_info_node
→ route_by_status
  → ask_user_node if need_more_info
  → finalize_result_node if ready
→ END
```

### Mode B: Chat Continue

```text
START
→ extract_user_info_node
→ check_missing_info_node
→ route_by_status
  → ask_user_node if need_more_info
  → finalize_result_node if ready
→ END
```

Implementation expectation:

```python
from langgraph.graph import StateGraph, START, END
from app.modules.food.graph.state import FoodSessionState
from app.modules.food.graph.nodes import (
    analyze_image_node,
    extract_user_info_node,
    check_missing_info_node,
    ask_user_node,
    finalize_result_node,
)


def route_by_status(state: FoodSessionState) -> str:
    if state.get("status") == "ready":
        return "finalize_result"
    return "ask_user"


def build_image_graph():
    graph = StateGraph(FoodSessionState)

    graph.add_node("analyze_image", analyze_image_node)
    graph.add_node("check_missing_info", check_missing_info_node)
    graph.add_node("ask_user", ask_user_node)
    graph.add_node("finalize_result", finalize_result_node)

    graph.add_edge(START, "analyze_image")
    graph.add_edge("analyze_image", "check_missing_info")
    graph.add_conditional_edges(
        "check_missing_info",
        route_by_status,
        {
            "ask_user": "ask_user",
            "finalize_result": "finalize_result",
        }
    )
    graph.add_edge("ask_user", END)
    graph.add_edge("finalize_result", END)

    return graph.compile()


def build_chat_graph():
    graph = StateGraph(FoodSessionState)

    graph.add_node("extract_user_info", extract_user_info_node)
    graph.add_node("check_missing_info", check_missing_info_node)
    graph.add_node("ask_user", ask_user_node)
    graph.add_node("finalize_result", finalize_result_node)

    graph.add_edge(START, "extract_user_info")
    graph.add_edge("extract_user_info", "check_missing_info")
    graph.add_conditional_edges(
        "check_missing_info",
        route_by_status,
        {
            "ask_user": "ask_user",
            "finalize_result": "finalize_result",
        }
    )
    graph.add_edge("ask_user", END)
    graph.add_edge("finalize_result", END)

    return graph.compile()
```

---

## 11. Provider Abstraction

Create `app/providers/base.py`.

```python
from abc import ABC, abstractmethod


class BaseLLMProvider(ABC):
    @abstractmethod
    async def analyze_food_image(self, image_path: str) -> dict:
        pass

    @abstractmethod
    async def extract_food_info_from_text(self, message: str) -> dict:
        pass
```

---

## 12. Gemini Provider

Create `app/providers/gemini_provider.py`.

Use `google-genai`.

### Required behavior

- Read API key from settings.
- Analyze image using Gemini multimodal input.
- Force JSON output.
- Return parsed Python dict.
- Handle model errors cleanly.

### Image Analysis Prompt

```text
You are a food image analysis assistant.

Analyze the uploaded food image and return ONLY valid JSON.

Schema:
{
  "detected_foods": [
    {
      "name": "Vietnamese food name",
      "confidence": 0.0,
      "estimated_grams": null,
      "unit": null,
      "visible_evidence": "short visual reason"
    }
  ],
  "uncertainty": "short note",
  "need_user_confirmation": true
}

Rules:
- Do not guess exact grams if the image lacks a clear scale.
- If the dish is unclear, use a low confidence score.
- Prefer Vietnamese food names if possible.
- Do not include markdown.
- Do not include explanations outside JSON.
```

### Text Extraction Prompt

```text
You extract structured meal information from Vietnamese user text.

Return ONLY valid JSON.

Schema:
{
  "foods": [
    {
      "name": "food name",
      "grams": 300,
      "unit": "g"
    }
  ],
  "people_count": 2
}

Rules:
- If grams are not provided, set grams to null.
- If the user gives a portion unit like "1 bát", "2 miếng", "1 đĩa", keep it in unit.
- Normalize "gram", "grams", "g" to unit = "g".
- If people count is not mentioned, set people_count to null.
- Do not include markdown.
```

---

## 13. Provider Factory

Create `app/providers/provider_factory.py`.

```python
from app.core.config import settings
from app.integrations.providers.gemini_provider import GeminiProvider


def get_llm_provider():
    if settings.LLM_PROVIDER == "gemini":
        return GeminiProvider()

    raise ValueError(f"Unsupported LLM_PROVIDER: {settings.LLM_PROVIDER}")
```

Leave TODO comments for future providers:

```python
# TODO: Add OpenAIProvider
# TODO: Add ClaudeProvider
```

---

## 14. Storage Service

Create `app/services/storage_service.py`.

Requirements:

- Save uploaded images to `UPLOAD_DIR`.
- Generate unique filenames.
- Validate allowed image types:
  - jpg
  - jpeg
  - png
  - webp
- Reject file size larger than `MAX_UPLOAD_MB`.
- Return local file path.

Pseudo-code:

```python
import uuid
from pathlib import Path
from fastapi import UploadFile, HTTPException


ALLOWED_EXTENSIONS = {".jpg", ".jpeg", ".png", ".webp"}


async def save_upload_file(file: UploadFile) -> str:
    suffix = Path(file.filename or "").suffix.lower()

    if suffix not in ALLOWED_EXTENSIONS:
        raise HTTPException(status_code=400, detail="Unsupported image type")

    filename = f"{uuid.uuid4()}{suffix}"
    output_path = Path(settings.UPLOAD_DIR) / filename

    content = await file.read()

    max_bytes = settings.MAX_UPLOAD_MB * 1024 * 1024
    if len(content) > max_bytes:
        raise HTTPException(status_code=400, detail="File too large")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_bytes(content)

    return str(output_path)
```

---

## 15. Session Store

Create `app/services/session_store.py`.

MVP can use in-memory dictionary.

```python
from typing import Dict
from app.modules.food.graph.state import FoodSessionState

_SESSION_STORE: Dict[str, FoodSessionState] = {}


def get_session(session_id: str) -> FoodSessionState | None:
    return _SESSION_STORE.get(session_id)


def save_session(session_id: str, state: FoodSessionState) -> None:
    _SESSION_STORE[session_id] = state


def delete_session(session_id: str) -> None:
    _SESSION_STORE.pop(session_id, None)
```

Important:

- In-memory storage is fine for MVP.
- Add TODO for Redis implementation later.
- Do not use in-memory storage as production storage.

---

## 16. API Implementation Details

### `app/main.py`

Requirements:

- Create FastAPI app.
- Add CORS middleware.
- Include routers.
- Add `/health`.

Example:

```python
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.routes import food, chat
from app.core.config import settings


app = FastAPI(title=settings.APP_NAME)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(food.router, prefix=settings.API_PREFIX)
app.include_router(chat.router, prefix=settings.API_PREFIX)


@app.get("/health")
async def health():
    return {
        "status": "ok",
        "service": settings.APP_NAME
    }
```

---

## 17. Error Handling

Implement clear errors:

| Case | HTTP Status | Message |
|---|---:|---|
| Missing image | 422 | FastAPI validation |
| Unsupported file type | 400 | Unsupported image type |
| File too large | 400 | File too large |
| Missing session | 404 | Session not found |
| Missing API key | 500 | Gemini API key is not configured |
| Provider failure | 502 | LLM provider failed |
| Invalid LLM JSON | 502 | LLM returned invalid JSON |

Do not expose raw provider stack traces to the client.

---

## 18. JSON Parsing Utility

Create `app/utils/json_utils.py`.

Requirements:

- Parse pure JSON.
- Also handle accidental markdown fences from LLM.
- Raise clear error if invalid.

Pseudo-code:

```python
import json
import re


def parse_llm_json(text: str) -> dict:
    cleaned = text.strip()

    if cleaned.startswith("```"):
        cleaned = re.sub(r"^```json\s*", "", cleaned)
        cleaned = re.sub(r"^```\s*", "", cleaned)
        cleaned = re.sub(r"\s*```$", "", cleaned)

    try:
        return json.loads(cleaned)
    except json.JSONDecodeError as exc:
        raise ValueError(f"Invalid JSON from LLM: {exc}") from exc
```

---

## 19. README Requirements

Create `README.md` with:

### Setup

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
```

For Windows:

```bash
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
copy .env.example .env
```

### Run

```bash
uvicorn app.main:app --reload
```

### Test API

Use Swagger:

```text
http://localhost:8000/docs
```

### Example curl

```bash
curl -X POST "http://localhost:8000/api/v1/food/analyze" \
  -F "image=@sample_food.jpg"
```

```bash
curl -X POST "http://localhost:8000/api/v1/food/chat" \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "paste-session-id-here",
    "message": "Có cơm trắng 300g, thịt kho 200g, 2 người ăn"
  }'
```

---

## 20. Tests

Create minimal tests:

### `tests/test_graph.py`

Test:

- `check_missing_info_node` returns `need_more_info` when people count is missing.
- `finalize_result_node` calculates grams per person.
- `ask_user_node` returns a Vietnamese question.

### `tests/test_food_api.py`

Test:

- `/health` returns ok.
- unsupported file type is rejected.
- missing session returns 404.

Mock provider calls. Do not call real Gemini API in tests.

---

## 21. Acceptance Criteria

The implementation is complete when:

1. `uvicorn app.main:app --reload` starts successfully.
2. `/health` returns status ok.
3. `/api/v1/food/analyze` accepts an image upload.
4. The backend saves the uploaded image.
5. Gemini provider analyzes the image and returns food candidates.
6. The API returns a `session_id`.
7. The API asks for missing grams and people count.
8. `/api/v1/food/chat` accepts user text and updates the same session.
9. If info is missing, the API asks a follow-up question.
10. If info is complete, the API returns:
    - foods
    - grams
    - units
    - people_count
    - grams_per_person
    - status = `completed`
11. API keys are never hard-coded.
12. Provider errors are handled safely.
13. Tests can run without real external API calls.

---

## 22. Future Extensions

Add later, not required in MVP:

- Nutrition and calories calculation
- USDA or Vietnamese nutrition database
- PostgreSQL persistent session history
- Redis session backend
- User authentication
- Image storage with S3 or Cloudinary
- Multi-provider fallback:
  - Gemini
  - OpenAI
  - Claude
- Mobile app integration
- Meal history dashboard
- Async background cleanup for uploaded images

---

## 23. Coding Style

Follow these rules:

- Write clean, typed Python code.
- Use Pydantic models for API input/output.
- Keep provider-specific code inside `providers/`.
- Keep workflow logic inside `graph/`.
- Keep API routers thin.
- Do not put business logic directly in route handlers.
- Add comments only where helpful.
- Prefer small functions.
- Return consistent JSON responses.
- Avoid global mutable state except MVP session store.
- Make it easy to replace in-memory session store with Redis later.

---

## 24. Safety and UX Rules

- Do not provide medical or dieting advice.
- Do not claim calorie or gram estimates are exact.
- If image recognition is uncertain, ask the user to confirm.
- If the image is unclear, say so and ask the user to describe the food.
- Do not store unnecessary personal information.
- Do not log uploaded image contents or sensitive user data.
- Do not expose API keys or raw provider errors.

---

## 25. Final Expected Example

Image upload response:

```json
{
  "session_id": "abc123",
  "status": "need_more_info",
  "detected_foods": [
    {
      "name": "cơm trắng",
      "confidence": 0.82,
      "estimated_grams": null,
      "unit": null,
      "visible_evidence": "Phần cơm màu trắng trên đĩa"
    }
  ],
  "missing_fields": [
    "people_count",
    "amount_for_cơm trắng"
  ],
  "assistant_message": "Mình thấy có thể có cơm trắng. Bạn xác nhận giúp có những món gì, mỗi món khoảng bao nhiêu gram hoặc khẩu phần, và có bao nhiêu người ăn?"
}
```

Chat completion response:

```json
{
  "session_id": "abc123",
  "status": "completed",
  "people_count": 2,
  "foods": [
    {
      "name": "cơm trắng",
      "grams": 300,
      "unit": "g",
      "grams_per_person": 150
    }
  ],
  "summary": "Bữa ăn đã được tổng hợp xong cho 2 người."
}
```
