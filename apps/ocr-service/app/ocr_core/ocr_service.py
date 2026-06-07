import os
import sys
import traceback
from PIL import Image, ImageEnhance

# Ensure stdout/stderr use UTF-8 to avoid UnicodeEncodeError on Windows consoles
try:
    sys.stdout.reconfigure(encoding='utf-8')
    sys.stderr.reconfigure(encoding='utf-8')
except Exception:
    pass

# Try to import PaddleOCR (preferred engine for Vietnamese)
PADDLE_AVAILABLE = False
_paddle_ocr = None

try:
    from paddleocr import PaddleOCR
    PADDLE_AVAILABLE = True
    print("[INFO] Initializing PaddleOCR for Vietnamese language...")
    _paddle_ocr = PaddleOCR(
        use_angle_cls=True, 
        lang='vi',
        enable_mkldnn=False,
        rec_model_dir=None,
        det_model_dir=None
    )
    print("[INFO] PaddleOCR initialized successfully")
except Exception as e:
    print(f"[WARNING] PaddleOCR import/init error: {e}")
    PADDLE_AVAILABLE = False

# Try to import Tesseract as fallback
TESSERACT_AVAILABLE = False
try:
    import pytesseract
    pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"
    TESSERACT_AVAILABLE = True
except Exception as e:
    print(f"[WARNING] Tesseract import error: {e}")
    TESSERACT_AVAILABLE = False


def preprocess_image(image_path):
    """Preprocess image to improve OCR accuracy for Vietnamese text."""
    img = Image.open(image_path)
    
    # Convert to RGB if necessary
    if img.mode != 'RGB':
        img = img.convert('RGB')
    
    # Resize if image is too small (upscale to improve recognition)
    width, height = img.size
    if width < 400 or height < 400:
        scale_factor = max(400 / width, 400 / height)
        new_size = (int(width * scale_factor), int(height * scale_factor))
        img = img.resize(new_size, Image.Resampling.LANCZOS)
    
    # Convert to grayscale first for better thresholding
    img = img.convert('L')
    
    # Apply adaptive contrast enhancement
    enhancer = ImageEnhance.Contrast(img)
    img = enhancer.enhance(2.5)  # Stronger contrast
    
    # Apply brightness adjustment
    enhancer = ImageEnhance.Brightness(img)
    img = enhancer.enhance(0.95)
    
    # Increase sharpness dramatically to distinguish similar characters
    enhancer = ImageEnhance.Sharpness(img)
    img = enhancer.enhance(3.0)  # Much stronger sharpness
    
    return img


def extract_text_paddle(image_path):
    """Extract text using PaddleOCR (optimized for Vietnamese)."""
    try:
        if not PADDLE_AVAILABLE or _paddle_ocr is None:
            return None
        
        print("[DEBUG] Using PaddleOCR engine...")
        results = _paddle_ocr.ocr(image_path)
        
        if results and results[0]:
            texts = []
            for line in results[0]:
                detected_text = line[1][0]
                texts.append(detected_text)
            
            if texts:
                return "\n".join(texts)
        return None
    except Exception as e:
        print(f"[WARNING] PaddleOCR error: {e}")
        return None


def extract_text_tesseract(image_path):
    """Extract text using Tesseract OCR (fallback)."""
    try:
        if not TESSERACT_AVAILABLE:
            return None
        
        print("[DEBUG] Using Tesseract OCR engine...")
        img = preprocess_image(image_path)
        
        # Try with Vietnamese if available, otherwise use English
        try:
            text = pytesseract.image_to_string(img, lang='vie+eng')
        except:
            text = pytesseract.image_to_string(img, lang='eng')
        
        return text if text.strip() else None
    except Exception as e:
        print(f"[WARNING] Tesseract error: {e}")
        return None


def extract_text(image_path):
    """Extract text from image using multi-engine approach.
    
    Tries in order:
    1. PaddleOCR (Vietnamese optimized)
    2. Tesseract (fallback)
    
    Supports both English and Vietnamese text with:
    - Advanced image preprocessing
    - Multiple language support
    - Automatic fallback mechanism
    """
    if not os.path.exists(image_path):
        raise FileNotFoundError(f"Image file not found: {image_path}")
    
    # Try PaddleOCR first (best for Vietnamese)
    text = extract_text_paddle(image_path)
    if text:
        return text
    
    # Fallback to Tesseract
    text = extract_text_tesseract(image_path)
    if text:
        return text
    
    # If both failed, raise error
    raise RuntimeError(
        "No OCR engine available. Please install PaddleOCR or Tesseract. "
        "PaddleOCR: pip install paddleocr\n"
        "Tesseract: https://github.com/UB-Mannheim/tesseract/wiki"
    )

