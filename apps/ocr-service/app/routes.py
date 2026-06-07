from pathlib import Path
from typing import Optional

from flask import Blueprint, Flask, current_app, jsonify, request
from werkzeug.datastructures import FileStorage
from werkzeug.utils import secure_filename

from .schemas import OcrServiceSettings
from .service import FoodLabelOcrService


def create_routes(
    settings: OcrServiceSettings,
    service: Optional[FoodLabelOcrService] = None,
) -> Blueprint:
    routes = Blueprint("ocr", __name__)
    service = service or FoodLabelOcrService()

    @routes.get("/")
    def index():
        return jsonify(
            {
                "service": "NutriScan OCR",
                "status": "OK",
                "analyzeEndpoint": "/api/analyze-food",
            }
        )

    @routes.get("/api/health")
    def health():
        return jsonify({"status": "OK", "service": "NutriScan OCR", "version": "1.0.0"}), 200

    @routes.post("/internal/v1/ocr/analyze-food-image")
    def analyze_food_image_internal():
        saved_path: Optional[Path] = None
        request_id = request.form.get("request_id") or ""
        debug = request.form.get("debug", "false").lower() == "true"

        try:
            file = _get_valid_file(settings, field_name="image")
            saved_path = _save_upload(settings.upload_folder, file)

            current_app.logger.info("Analyzing uploaded food image: %s", saved_path.name)
            result = service.analyze_food_image(
                image_path=saved_path,
                request_id=request_id,
                debug=debug,
            )

            return jsonify(result.to_internal_response(request_id=request_id)), 200
        except ValueError as exc:
            return jsonify({"request_id": request_id, "error_code": "INVALID_IMAGE", "message": str(exc)}), 400
        except FileNotFoundError:
            return jsonify({"request_id": request_id, "error_code": "FILE_NOT_FOUND", "message": "File not found"}), 400
        except Exception as exc:
            current_app.logger.exception("Internal OCR image processing failed")
            return jsonify(
                {
                    "request_id": request_id,
                    "error_code": "OCR_PROCESSING_ERROR",
                    "message": f"OCR processing error: {exc}",
                }
            ), 500
        finally:
            if saved_path and saved_path.exists():
                saved_path.unlink(missing_ok=True)

    @routes.post("/api/analyze-food")
    def analyze_food():
        saved_path: Optional[Path] = None
        try:
            file = _get_valid_file(settings, field_name="file")
            saved_path = _save_upload(settings.upload_folder, file)

            current_app.logger.info("Analyzing uploaded food label: %s", saved_path.name)
            result = service.analyze(saved_path)

            return jsonify(
                {
                    "success": True,
                    "raw_text": result.raw_text,
                    "nutrition": result.nutrition,
                }
            ), 200
        except ValueError as exc:
            return jsonify({"success": False, "error": str(exc)}), 400
        except FileNotFoundError:
            return jsonify({"success": False, "error": "File not found"}), 400
        except Exception as exc:
            current_app.logger.exception("OCR processing failed")
            return jsonify({"success": False, "error": f"OCR processing error: {exc}"}), 500
        finally:
            if saved_path and saved_path.exists():
                saved_path.unlink(missing_ok=True)

    return routes


def register_error_handlers(app: Flask) -> None:
    @app.errorhandler(413)
    def request_entity_too_large(error):
        return jsonify({"success": False, "error": "File is too large"}), 413

    @app.errorhandler(404)
    def not_found(error):
        return jsonify({"success": False, "error": "Endpoint not found"}), 404


def _get_valid_file(settings: OcrServiceSettings, field_name: str = "file") -> FileStorage:
    if field_name not in request.files:
        raise ValueError("No file found")

    file = request.files[field_name]
    if not file.filename:
        raise ValueError("No file selected")

    if not _allowed_file(settings, file.filename):
        allowed = ", ".join(sorted(settings.allowed_extensions))
        raise ValueError(f"File must be an image ({allowed})")

    return file


def _allowed_file(settings: OcrServiceSettings, filename: str) -> bool:
    return "." in filename and filename.rsplit(".", 1)[1].lower() in settings.allowed_extensions


def _save_upload(upload_folder: Path, file: FileStorage) -> Path:
    upload_folder.mkdir(parents=True, exist_ok=True)
    filename = secure_filename(file.filename or "upload")
    filepath = upload_folder / filename
    file.save(filepath)
    return filepath
