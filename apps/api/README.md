# NutriScan API

FastAPI service for food validation and AI-backed food analysis.

## Layout

```text
app/
  main.py
  core/                  config and errors
  routes/                thin request handlers
  modules/food/          schemas, service layer, repository, nutrition, sessions
  modules/food/graph/    legacy AI conversation workflow
  services/              compatibility wrappers and storage helpers
  schemas/               compatibility re-exports
  integrations/          OCR client and external LLM adapters
tests/
requirements.txt
.env.example
```

## Setup

```powershell
python -m venv venv
.\venv\Scripts\python.exe -m pip install -r requirements.txt
```

## Run

```powershell
.\venv\Scripts\python.exe -m uvicorn app.main:app --host 127.0.0.1 --port 8000
```

## Main Endpoints

- `POST /api/v1/food/analyze`
- `GET /api/v1/food/sessions/{session_id}`
- `POST /api/v1/food/confirm`
- `POST /api/v1/food/{session_id}/feedback`
- `POST /api/v1/food/validate`
- `GET /api/v1/meals/history`
- `GET /api/v1/meals/{meal_id}`

## Test

```powershell
.\venv\Scripts\python.exe -m pytest -q
```
