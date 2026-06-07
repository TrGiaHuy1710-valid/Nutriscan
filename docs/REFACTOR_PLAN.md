# NutriScan2 Refactor Plan

## Current Repository Analysis

### Frontend

- `NutriScanAI/NutriScan/` is the ASP.NET Core MVC application and current frontend host.
- Razor views live in `NutriScanAI/NutriScan/Views/`.
- Static frontend assets live in `NutriScanAI/NutriScan/wwwroot/`.
- Recent frontend API helper lives in `NutriScanAI/NutriScan/wwwroot/js/api/nutriscan-api.js`.

### Backend API

- `NutriScanAI/NutriScan/Controllers/` contains ASP.NET controllers.
- `NutriScanAI/NutriScan/Services/` contains business services and integrations.
- `NutriScanAI/NutriScan/Data/` contains EF Core database models, DbContext, and current database service.
- `NutriScanAI/NutriScan/DTOs/` contains scan response DTOs.
- This API currently acts as the BFF/API gateway for frontend scan flow.

### OCR/AI

- `NutriScanAI/app.py` is the legacy Flask OCR HTTP service.
- `NutriScanAI/ocr/` contains OCR extraction and nutrition parsing.
- `NutriScanAI/nutriscan_ocr/` appears to be a newer modular OCR package.
- `Backend/FoodValidate-service/` is the FastAPI FoodValidate/AI backend.
- `Backend/FoodValidate-service/app/api/` contains routes.
- `Backend/FoodValidate-service/app/services/` contains service logic.
- `Backend/FoodValidate-service/app/schemas/` contains Pydantic schemas.
- `Backend/FoodValidate-service/app/providers/` contains LLM provider adapters.
- `Backend/FoodValidate-service/app/graph/` contains graph workflow logic.

### Harness

- `nutriscan-harness/` contains agent instructions, prompts, product docs, stories, validation docs, backlog, decisions, and trace template.
- Target location after refactor: `harness/`.

### Docs

- Root `docs/` is being created for repo-level refactor docs.
- `nutriscan-harness/docs/` contains product and implementation guidance.
- `NutriScanAI/docs/` contains OCR/application docs.
- Target behavior: keep repo-level docs in `docs/`, preserve harness docs under `harness/docs/`, and move app-specific docs close to their app where useful.

### Runtime, Storage, Generated, Cache

- Runtime traces: `traces/`, `nutriscan-harness/traces/`.
- Runtime uploads: `Backend/FoodValidate-service/uploads/`, `NutriScanAI/uploads/`.
- Generated/cache: `__pycache__/`, `.pytest_cache/`, `bin/`, `obj/`, `venv/`, `.dotnet/`.
- Build/runtime files should not be committed and should be ignored.

### Git Roots

- Root `.git` exists at `NutriScan2/.git`.
- Nested `.git` exists at `NutriScanAI/.git`.
- Per user instruction, nested `.git` will be reported and not deleted automatically.

## Target Structure

```text
NutriScan2/
├── apps/
│   ├── api/
│   │   └── README.md
│   ├── ocr-service/
│   │   ├── app/
│   │   ├── tests/
│   │   ├── requirements.txt
│   │   └── README.md
│   └── frontend/
│       └── NutriScan/
├── docs/
├── harness/
├── scripts/
├── storage/
│   ├── uploads/
│   ├── outputs/
│   └── traces/
├── archive/
├── .gitignore
└── README.md
```

## Refactor Strategy

This refactor will be conservative:

- Keep source code intact when moving files.
- Do not delete source code.
- Move uncertain legacy files to `archive/legacy/`.
- Move generated/cache/runtime files only after path verification.
- Prefer wrappers/compatibility files where moving a service would otherwise require broad logic changes.
- Keep the ASP.NET MVC/BFF project runnable after it is moved under `apps/frontend/NutriScan`.
- Keep the FastAPI FoodValidate service runnable after it is moved under `apps/api`.
- Convert the legacy Flask OCR service into `apps/ocr-service/app` with routes/service/schema/engine modules.

## Phase 1: Clean Generated And Runtime Files

Actions:

- Create `storage/uploads`, `storage/outputs`, `storage/traces`.
- Move root runtime traces from `traces/` to `storage/traces/`.
- Move upload images from `Backend/FoodValidate-service/uploads/` and `NutriScanAI/uploads/` to `storage/uploads/legacy-*`.
- Remove generated/cache directories only after verifying they are under the workspace:
  - `__pycache__/`
  - `.pytest_cache/`
  - `bin/`
  - `obj/`
- Keep `.dotnet/` and `venv/` physically present for local verification, but ignore them in `.gitignore`.

Checks:

- `Test-Path` for moved storage directories.
- No source code deleted.

## Phase 2: Create New App Structure

Actions:

- Create `apps/api`, `apps/ocr-service`, `apps/frontend`.
- Create `harness`, `scripts`, `archive/legacy`.
- Move `nutriscan-harness/` to `harness/`.
- Move `Backend/FoodValidate-service/` to `apps/api/`.
- Move ASP.NET project `NutriScanAI/NutriScan/` to `apps/frontend/NutriScan/`.
- Move OCR Flask code from `NutriScanAI/` into `apps/ocr-service/`.

Checks:

- `rg --files apps harness docs scripts storage archive`.

## Phase 3: Move Files While Preserving Logic

Actions:

- Preserve current ASP.NET namespaces and project file initially.
- Preserve FastAPI package as `app`.
- Preserve OCR parser/engine logic.
- Move uncertain files to `archive/legacy/NutriScanAI-root/`:
  - `README.md.bak`
  - old integration examples
  - old setup/run scripts if superseded by `scripts/`
- Do not delete nested `.git`; report it.

Checks:

- Source files are present in target apps.
- Legacy archive contains uncertain old files.

## Phase 4: Update Import Paths And Run Commands

Actions:

- Update ASP.NET `PythonOcrBackgroundService` or config to point to new OCR service path if still used.
- Update scripts to run:
  - FastAPI API from `apps/api`
  - OCR Flask service from `apps/ocr-service`
  - ASP.NET frontend from `apps/frontend/NutriScan`
- Update README commands.

Checks:

- `python -m compileall apps/api/app apps/ocr-service/app`
- Import test for `apps/api/app/main.py`.
- Import test for OCR app.

## Phase 5: Layer Route/Service/Repository/Schema

Actions:

- For FastAPI API:
  - Keep `app/api` as routes/controllers.
  - Keep business logic in `app/services`.
  - Keep schemas in `app/schemas`.
  - Keep provider/external calls in `app/integrations` or preserve `app/providers` if renaming is too risky.
  - Keep config/errors in `app/core`.
- For OCR service:
  - `app/main.py`: Flask app factory/bootstrap.
  - `app/routes.py`: HTTP route handlers.
  - `app/service.py`: business logic.
  - `app/schemas.py`: response/request helpers.
  - `app/ocr_engine.py`: OCR engine integration.
- For ASP.NET frontend/BFF:
  - Keep controllers thin.
  - Keep scan orchestration in services.
  - Keep EF access isolated in data/repository-like service.

Checks:

- `pytest` in `apps/api` if tests exist.
- `.NET build` if local SDK exists.

## Phase 6: Documentation, Gitignore, Final Verification

Actions:

- Update root `README.md`.
- Add `.env.example` files where useful.
- Update root `.gitignore`.
- Create `docs/REFACTOR_REPORT.md`.

Required final checks:

- `python -m compileall apps/api/app apps/ocr-service/app`
- `pytest` for API tests if present.
- Import main API app.
- Import OCR main app.
- `dotnet build apps/frontend/NutriScan/NutriScan.csproj` using local `.dotnet` if available.

## Known Risks

- `NutriScanAI/.git` is a nested git root and will not be deleted automatically.
- OCR runtime depends on a local Tesseract binary if PaddleOCR is not installed.
- Moving ASP.NET project may affect relative path discovery in `PythonOcrBackgroundService`; this must be updated carefully.
- Existing SQLite database may have been created in the old project path; storage location should be made explicit later.
