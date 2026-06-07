# NutriScan API

FastAPI service for food validation and AI-backed food analysis.

## Layout

```text
app/
  main.py
  core/                  config and errors
  routes/                request handlers
  services/              business logic
  schemas/               request/response models
  modules/food/graph/    food workflow graph
  integrations/providers external LLM adapters
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

## Test

```powershell
.\venv\Scripts\python.exe -m pytest -q
```
