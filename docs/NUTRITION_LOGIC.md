# Nutrition Logic

`apps/api/app/modules/food/nutrition_service.py` calculates nutrition after the user confirms foods, grams or serving size, and people count.

Current behavior:

- Uses a small local per-100g nutrition table.
- Unknown foods use a conservative placeholder and are marked `needs_manual_review`.
- Per-person values divide totals by `people_count`.
- Serving size text falls back to the first numeric value, or `100g` when no numeric value is found.

This is an explicit repository boundary. A real nutrition database can replace the in-memory table without changing route code.
