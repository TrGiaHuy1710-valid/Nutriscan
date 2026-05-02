import pytesseract
from PIL import Image

pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"

def extract_text(image_path):
    img = Image.open(r"C:\Users\pc\Downloads\z7761528746293_7842f1c5852c85330ad7dd74ab8d1e66.jpg")
    text = pytesseract.image_to_string(img)
    return text