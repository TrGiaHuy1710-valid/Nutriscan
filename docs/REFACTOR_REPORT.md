# NutriScan2 Refactor Report

## Summary

The repository was reorganized into a clearer multi-app structure:

- `apps/api`: FastAPI FoodValidate / AI service.
- `apps/ocr-service`: Flask OCR service.
- `apps/frontend/NutriScan`: ASP.NET Core MVC frontend and BFF.
- `harness`: product stories, decisions, prompts, validation docs.
- `storage`: runtime uploads, outputs, traces.
- `archive/legacy`: preserved legacy files that should not be deleted without review.

No primary source code was intentionally deleted. Generated/cache files were removed only after verifying they were under the workspace and outside `.dotnet`/`venv`.

## Moved Files And Directories

### Harness

- `nutriscan-harness/` -> `harness/`

### API

- `Backend/FoodValidate-service/` -> `apps/api/`
- `apps/api/app/api/` -> `apps/api/app/routes/`
- `apps/api/app/providers/` -> `apps/api/app/integrations/providers/`
- `apps/api/app/graph/` -> `apps/api/app/modules/food/graph/`

### Frontend / BFF

- `NutriScanAI/NutriScan/` -> `apps/frontend/NutriScan/`
- `NutriScanAI/global.json` -> `global.json`
- `NutriScanAI/dotnet-install.ps1` -> `scripts/dotnet-install.ps1`

### OCR Service

- `NutriScanAI/requirements.txt` -> `apps/ocr-service/requirements.txt`
- `NutriScanAI/ocr/` -> `apps/ocr-service/app/ocr_core/`
- New OCR layered files:
  - `apps/ocr-service/app/main.py`
  - `apps/ocr-service/app/routes.py`
  - `apps/ocr-service/app/service.py`
  - `apps/ocr-service/app/schemas.py`
  - `apps/ocr-service/app/ocr_engine.py`

### Runtime Storage

- `traces/` -> `storage/traces/root-runtime/`
- `Backend/FoodValidate-service/uploads/` -> `storage/uploads/foodvalidate/`
- `NutriScanAI/uploads/` -> `storage/uploads/ocr-service/`
- `apps/frontend/NutriScan/Data/nutriscan.db*` -> `storage/outputs/`

### Legacy Archive

The following legacy NutriScanAI root files were moved to `archive/legacy/NutriScanAI-root/`:

- `app.py`
- `__init__.py`
- `nutriscan_ocr/`
- `README.md`
- `README.md.bak`
- `GITHUB_INTEGRATION.md`
- `INTEGRATION_EXAMPLE.py`
- `INTEGRATION_SUMMARY.md`
- `run.bat`
- `setup.py`
- `start-nutriscan.ps1`
- `start-nutriscan.sh`
- `verify_integration.py`
- `.github/`

## New Structure

```text
NutriScan2/
├── apps/
│   ├── api/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   ├── integrations/
│   │   │   ├── modules/
│   │   │   ├── routes/
│   │   │   ├── schemas/
│   │   │   ├── services/
│   │   │   └── main.py
│   │   ├── tests/
│   │   ├── requirements.txt
│   │   ├── .env.example
│   │   └── README.md
│   ├── ocr-service/
│   │   ├── app/
│   │   ├── requirements.txt
│   │   ├── .env.example
│   │   └── README.md
│   └── frontend/
│       └── NutriScan/
├── archive/
├── docs/
├── harness/
├── scripts/
├── storage/
├── .gitignore
├── global.json
└── README.md
```

## Run Commands

### API

```powershell
cd apps/api
.\venv\Scripts\python.exe -m uvicorn app.main:app --host 127.0.0.1 --port 8000
```

From repository root:

```powershell
.\scripts\run-api.ps1
```

### OCR Service

```powershell
cd apps/ocr-service
.\venv\Scripts\python.exe -m app.main
```

From repository root:

```powershell
.\scripts\run-ocr-service.ps1
```

### Frontend / BFF

```powershell
.\.dotnet\dotnet.exe run --project apps/frontend/NutriScan/NutriScan.csproj --urls http://127.0.0.1:5099
```

From repository root:

```powershell
.\scripts\run-frontend.ps1
```

## Verification Results

- `python -m compileall apps/api/app apps/ocr-service/app`: passed.
- `apps/api/.venv python -m pytest -q`: passed, 7 tests.
- API import check: passed, imported `app.main:app`.
- OCR import check: passed, imported `app.main:create_app`.
- `.NET restore`: passed, with NuGet vulnerability feed warning because network access to `https://api.nuget.org/v3/index.json` was unavailable.
- `.NET build`: passed, with NuGet vulnerability feed warning because network access to `https://api.nuget.org/v3/index.json` was unavailable.

## Remaining Issues

- `NutriScanAI/.git` is a nested git root. It was reported and left untouched per instruction.
- `git status` from repository root returned `fatal: not a git repository` despite a root `.git` directory being present. Root git metadata may need manual inspection; it was not modified.
- `apps/api/.env` exists and was not read or copied. `.gitignore` now ignores `.env`; use `apps/api/.env.example` as the safe template.
- OCR still depends on a system OCR engine. In the current Python 3.13 environment, PaddleOCR is optional and not installed; Tesseract must be available for real OCR fallback.
- NuGet vulnerability metadata cannot be fetched in the restricted network environment; restore/build still completed.

## Files To Review Before Manual Deletion

- `NutriScanAI/.git`: nested repository metadata.
- `archive/legacy/NutriScanAI-root/`: old entrypoints, scripts, and docs preserved for review.
- `apps/api/skills/SKILL_food_ai_backend.md`: historical implementation guidance, retained near API.
- `storage/uploads/foodvalidate/`: legacy uploaded images.
- `storage/traces/root-runtime/`: prior runtime logs.
