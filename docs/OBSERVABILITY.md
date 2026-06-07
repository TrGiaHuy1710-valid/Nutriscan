# Observability

Current observability is intentionally lightweight:

- API and OCR responses include `request_id` where supported.
- OCR service logs uploaded temporary file names, not API keys or secrets.
- OCR internal responses include `source`, `confidence`, `fallback_used`, `error_code`, and optional `debug_info`.

Recommended next steps:

- Add structured JSON logging in `apps/api/app/core`.
- Add request middleware that generates a `request_id` when missing.
- Persist OCR/API trace summaries under `storage/traces/` in development only.
