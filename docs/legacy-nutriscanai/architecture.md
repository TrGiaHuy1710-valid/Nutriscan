# NutriScan Architecture

## Runtime Boundaries

NutriScan is split into three clear runtime surfaces:

- `NutriScan/`: ASP.NET Core MVC application and BFF API. Controllers stay thin and call application services.
- `nutriscan_ocr/`: Flask OCR API package. `app.py` is only the process entrypoint.
- `Backend/FoodValidate-service/`: separate food validation service consumed through `IFoodValidateClient`.

## ASP.NET Core Layout

- `Controllers/`: HTTP boundary only.
- `Services/Scans/`: scan orchestration use cases.
- `Services/Ocr/`: client contract for the OCR service.
- `Services/FoodValidation/`: client contract for food validation.
- `Configuration/`: dependency injection, database bootstrap, and middleware pipeline.
- `Data/`: persistence models and SQLite context.
- `DTOs/`: API response contracts.

`Program.cs` should remain small. Add new dependencies in `Configuration/ServiceCollectionExtensions.cs` and new pipeline concerns in `Configuration/WebApplicationExtensions.cs`.

## OCR API Layout

- `app.py`: Flask process entrypoint for local, Docker, or PaaS deployment.
- `nutriscan_ocr/application.py`: app factory for tests and WSGI hosting.
- `nutriscan_ocr/settings.py`: environment-based configuration.
- `nutriscan_ocr/routes.py`: HTTP routes, upload validation, and error mapping.
- `nutriscan_ocr/analyzer.py`: OCR use-case adapter around the existing OCR module.
- `ocr/`: OCR engine and nutrition parsing implementation.

## Configuration

OCR service environment variables:

- `NUTRISCAN_OCR_HOST`, default `0.0.0.0`
- `NUTRISCAN_OCR_PORT`, default `5000`
- `NUTRISCAN_OCR_DEBUG`, default `false`
- `NUTRISCAN_UPLOAD_FOLDER`, default `uploads`
- `NUTRISCAN_MAX_FILE_SIZE`, default `16777216`
- `NUTRISCAN_ALLOWED_EXTENSIONS`, default `png,jpg,jpeg,gif,bmp,webp`

ASP.NET service endpoints live in `NutriScan/appsettings.json` under `Services:Ocr` and `Services:FoodValidate`.

## CI/CD

`.github/workflows/ci.yml` performs:

- Python dependency install.
- Python compile check for `app.py`, `ocr`, and `nutriscan_ocr`.
- .NET restore and release build for `NutriScan/NutriScan.csproj`.

Future deployment can package each service independently because each now has a stable entrypoint and isolated configuration.
