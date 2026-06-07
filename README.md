# NutriScan2

NutriScan2 is organized as a multi-app nutrition scanning workspace.

## Structure

```text
apps/
  api/                 FastAPI FoodValidate / AI service
  ocr-service/         Flask OCR service
  frontend/NutriScan/  ASP.NET Core MVC frontend and BFF
docs/                  Repository-level documentation
harness/               Agent/product harness, stories, validation docs
scripts/               Local run scripts
storage/               Runtime uploads, outputs, traces
archive/legacy/        Preserved legacy files
```

## Run API

```powershell
cd apps/api
.\venv\Scripts\python.exe -m uvicorn app.main:app --host 127.0.0.1 --port 8000
```

Or from the repository root:

```powershell
.\scripts\run-api.ps1
```

## Run OCR Service

```powershell
cd apps/ocr-service
.\venv\Scripts\python.exe -m app.main
```

Or from the repository root:

```powershell
.\scripts\run-ocr-service.ps1
```

## Run Frontend

```powershell
.\.dotnet\dotnet.exe run --project apps/frontend/NutriScan/NutriScan.csproj --urls http://127.0.0.1:5099
```

Or:

```powershell
.\scripts\run-frontend.ps1
```

## Verification

```powershell
python -m compileall apps/api/app apps/ocr-service/app
cd apps/api
.\venv\Scripts\python.exe -m pytest -q
cd ..\..
.\.dotnet\dotnet.exe build apps/frontend/NutriScan/NutriScan.csproj
```

## Notes

- Real `.env` files are ignored. Use `.env.example` files as templates.
- Runtime files belong under `storage/`.
- `NutriScanAI/.git` is a nested git root intentionally left untouched.
