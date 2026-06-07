# AI Fallback Policy

The intended flow is:

1. API receives image.
2. API stores upload and calls OCR service.
3. OCR service runs local OCR and candidate extraction.
4. If no food is detected or confidence is below `0.65`, OCR service may use Gemini when `GEMINI_API_KEY` is configured.
5. If OCR service is unavailable, API falls back to the existing legacy graph/Gemini workflow so local development can still proceed.

Secrets are read from environment variables only. API keys are not logged or written into reports.
