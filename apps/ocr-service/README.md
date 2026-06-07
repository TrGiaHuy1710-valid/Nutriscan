# NutriScan OCR Service

Flask service for food label OCR and nutrition parsing.

## Layout

```text
app/
  main.py       app factory and process entrypoint
  routes.py     HTTP routes
  service.py    business service
  schemas.py    settings/result models
  ocr_engine.py OCR adapter
  ocr_core/     legacy OCR extraction and parser implementation
```

## Setup

```powershell
python -m venv venv
.\venv\Scripts\python.exe -m pip install -r requirements.txt
```

## Run

```powershell
.\venv\Scripts\python.exe -m app.main
```

The service listens on `http://localhost:5000` by default.

## Notes

PaddleOCR is optional in the current Python 3.13 environment. If PaddleOCR is not installed, the service falls back to Tesseract when available.
