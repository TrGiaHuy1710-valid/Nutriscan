# Service Optimization Report

## Current Issues Found

- Food validation business rules were implemented directly in `apps/api/app/routes/food.py`.
- The old session store and new module session service were separate.
- API food analysis did not have a clear OCR service adapter boundary.
- OCR service exposed only the legacy label route, not an internal food-image contract.
- Nutrition confirmation, feedback, and meal history had no explicit API contract.

## New Service Boundaries

- API routes are thin and call `apps/api/app/modules/food/service.py`.
- Food module now owns schemas, service use cases, repository, session service, nutrition service, feedback service, and normalization.
- API calls OCR through `apps/api/app/integrations/ocr_client.py`.
- OCR service owns preprocessing, local OCR/candidate extraction, fallback policy, and optional Gemini adapter.

## Files Edited Or Added

- `apps/api/app/core/config.py`
- `apps/api/app/core/errors.py`
- `apps/api/app/routes/food.py`
- `apps/api/app/routes/meals.py`
- `apps/api/app/main.py`
- `apps/api/app/schemas/food.py`
- `apps/api/app/services/food_service.py`
- `apps/api/app/services/session_store.py`
- `apps/api/app/modules/food/service.py`
- `apps/api/app/modules/food/session_service.py`
- `apps/api/tests/test_food_service_contract.py`
- `apps/ocr-service/app/routes.py`
- `apps/ocr-service/app/service.py`
- `apps/ocr-service/app/schemas.py`
- `apps/ocr-service/app/fallback_policy.py`
- `apps/ocr-service/app/image_preprocessor.py`
- `apps/ocr-service/app/vision_engine.py`
- `apps/ocr-service/app/gemini_adapter.py`
- `apps/ocr-service/tests/test_fallback_policy.py`
- `apps/ocr-service/requirements.txt`
- `README.md`
- `apps/api/README.md`
- `apps/ocr-service/README.md`
- `docs/SERVICE_ARCHITECTURE.md`
- `docs/API_CONTRACT.md`
- `docs/AI_FALLBACK_POLICY.md`
- `docs/NUTRITION_LOGIC.md`
- `docs/OBSERVABILITY.md`
- `docs/SERVICE_OPTIMIZATION_REPORT.md`

## Standardized API Contract

- Food analysis returns `session_id`, `request_id`, `status`, `detected_foods`, `missing_fields`, `confidence`, `source`, `error_code`, and `message`.
- Confirmation returns meal nutrition and stores a meal record.
- Validation keeps the existing ASP.NET contract.
- Feedback and meal history are now explicit endpoints.

## Run Commands

API:

```powershell
cd apps/api
.\venv\Scripts\python.exe -m uvicorn app.main:app --host 127.0.0.1 --port 8000
```

OCR service:

```powershell
cd apps/ocr-service
.\venv\Scripts\python.exe -m app.main
```

Frontend:

```powershell
.\.dotnet\dotnet.exe run --project apps/frontend/NutriScan/NutriScan.csproj --urls http://127.0.0.1:5099
```

## Verification

- `python -m compileall apps/api/app apps/ocr-service/app`: passed.
- `cd apps/api; .\venv\Scripts\python.exe -m pytest -q`: passed, 12 tests.
- `cd apps/ocr-service; .\venv\Scripts\python.exe -m pytest -q`: passed, 3 tests, 2 deprecation warnings from pytesseract.
- `.\.dotnet\dotnet.exe build apps/frontend/NutriScan/NutriScan.csproj --no-restore --no-incremental`: failed because running process `NutriScan (10620)` locked Debug binaries.
- `.\.dotnet\dotnet.exe build apps/frontend/NutriScan/NutriScan.csproj -c Release --no-restore --no-incremental`: passed.

## Remaining Issues

- OCR Gemini adapter is a boundary implementation and should be hardened before production use.
- Nutrition data is still a small local table and should be replaced by a real nutrition database.
- Meal/session repositories are in-memory and should move to durable storage for multi-process deployment.
- PaddleOCR is optional and currently absent in the local OCR environment.
- Stop the currently running Debug frontend process before rebuilding `bin/Debug`.

## Manual Review Candidates

- Existing cache/generated folders such as `__pycache__`, `.pytest_cache`, `bin`, and `obj` should remain ignored and can be cleaned manually.
- Nested `NutriScanAI/.git` was previously detected and intentionally left untouched.
