# API Contract

## Food Analyze

`POST /api/v1/food/analyze`

Multipart fields:

- `image`: image file.
- `session_id`: optional.
- `request_id`: optional.
- `debug`: optional boolean.

Response includes:

- `session_id`
- `request_id`
- `status`: `need_more_info`, `completed`, or `failed`
- `detected_foods`
- `missing_fields`
- `assistant_message`
- `confidence`
- `source`
- `error_code`
- `message`

## Confirm Food

`POST /api/v1/food/confirm`

Request body:

```json
{
  "session_id": "session-id",
  "foods": [{"name": "rice", "grams": 200}],
  "people_count": 2
}
```

Response returns a `meal_id`, per-item nutrition, and summary nutrition.

## Validation

`POST /api/v1/food/validate` remains compatible with the ASP.NET scan flow. It accepts OCR text/nutrition and returns `isFoodLabel`, confidence, normalized nutrition, flags, and alternatives.

## Meals

- `GET /api/v1/meals/history`
- `GET /api/v1/meals/{meal_id}`
