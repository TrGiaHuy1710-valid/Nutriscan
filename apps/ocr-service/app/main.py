from flask import Flask

from .routes import create_routes, register_error_handlers
from .schemas import OcrServiceSettings


def create_app(settings: OcrServiceSettings | None = None) -> Flask:
    settings = settings or OcrServiceSettings.from_env()

    app = Flask(__name__, static_folder=None)
    app.config["UPLOAD_FOLDER"] = str(settings.upload_folder)
    app.config["MAX_CONTENT_LENGTH"] = settings.max_file_size

    app.register_blueprint(create_routes(settings))
    register_error_handlers(app)

    return app


if __name__ == "__main__":
    service_settings = OcrServiceSettings.from_env()
    create_app(service_settings).run(
        debug=service_settings.debug,
        host=service_settings.host,
        port=service_settings.port,
    )
