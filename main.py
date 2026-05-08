from ocr.ocr_service import extract_text
from ocr.nutrition_parser import extract_nutrition

image_path = r"C:\Users\pc\OneDrive\Documents\PTTKPM\File_Imange\food_label.jpg"
# Bước 1: OCR
text = extract_text(image_path)
print("OCR TEXT:\n", text)

# Bước 2: Parse
nutrition = extract_nutrition(text)
print("\nNUTRITION DATA:\n", nutrition)