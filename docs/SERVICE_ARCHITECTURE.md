# Service Architecture

## Boundaries

- Frontend/BFF: `apps/frontend/NutriScan` owns MVC screens, scan orchestration, and persistence of scan records.
- API: `apps/api` owns food analysis sessions, food validation, nutrition confirmation, feedback, and meal history.
- OCR service: `apps/ocr-service` owns image OCR, local food candidate extraction, and optional Gemini fallback.
- Storage: runtime uploads remain under `storage/` or app-local ignored upload folders.

## API Layers

- `routes`: parse HTTP requests and return response models.
- `modules/food/service.py`: food business use cases.
- `modules/food/repository.py`: in-memory meal and feedback persistence boundary.
- `modules/food/session_service.py`: in-memory session persistence boundary.
- `modules/food/nutrition_service.py`: nutrition calculation.
- `integrations/ocr_client.py`: HTTP adapter to OCR service.
- `integrations/providers`: Gemini/LLM provider adapters used by the legacy graph fallback.

Frontend must call the API/BFF only. It should not call Gemini directly.
