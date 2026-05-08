import os
import sys
import traceback
from PIL import Image

# Ensure stdout/stderr use UTF-8 to avoid UnicodeEncodeError on Windows consoles
try:
    sys.stdout.reconfigure(encoding='utf-8')
    sys.stderr.reconfigure(encoding='utf-8')
except Exception:
    # Python <3.7 or environments that don't support reconfigure will skip
    pass

# Prefer PaddleOCR when available, fall back to WinRT (Windows) or pytesseract.
try:
    from paddleocr import PaddleOCR
    PADDLE_AVAILABLE = True
    # initialize once; models will be downloaded on first run
    # _paddle_ocr = PaddleOCR(use_angle_cls=True, lang='vi', enable_mkldnn=False)  # Cấu hình cho tiếng Việt và sử dụng MKL-DNN nếu có
    _paddle_ocr = PaddleOCR(
        use_angle_cls=True, 
        lang='vi', 
        # use_gpu=False,      # Đảm bảo dùng CPU nếu không có GPU ổn định
        enable_mkldnn=False, # Tránh lỗi Intel như trước
        rec_model_dir=None,  # Để nó dùng model mặc định ổn định
        det_model_dir=None
    )
except Exception as e:
    print("PaddleOCR import error:")
    traceback.print_exc()
    PADDLE_AVAILABLE = False

try:
    import winrt.windows.media.ocr as ocr
    import winrt.windows.graphics.imaging as imaging
    import winrt.windows.storage as storage
    WINRT_AVAILABLE = True
except Exception as e:
    print("WinRT import error:")
    traceback.print_exc()
    WINRT_AVAILABLE = False


async def _extract_text_winrt_async(image_path):
    abs_path = os.path.abspath(image_path)
    file = await storage.StorageFile.get_file_from_path_async(abs_path)
    stream = await file.open_async(storage.FileAccessMode.READ)
    decoder = await imaging.BitmapDecoder.create_async(stream)
    software_bitmap = await decoder.get_software_bitmap_async()
    engine = ocr.OcrEngine.try_create_from_user_profile_languages()
    result = await engine.recognize_async(software_bitmap)
    return result.text if result is not None else ""


def extract_text(image_path):
    """Extract text from image using PaddleOCR primary, then WinRT, then Tesseract.

    Keeps the same `extract_text(image_path)` signature so other components/UI remain unchanged.
    """
    # 1) PaddleOCR
    if PADDLE_AVAILABLE:
        try:
            print("--- Đang sử dụng PaddleOCR ---")
            # Bỏ cls=True đi
            results = _paddle_ocr.ocr(image_path) 
            texts = []
            
            # Kiểm tra xem có nhận diện được chữ nào không
            if results and results[0]:
                for line in results[0]:
                    # line có dạng: [[[x1, y1], ...], ('Text', probability)]
                    detected_text = line[1][0]
                    texts.append(detected_text)
                    
            if texts:
                return "\n".join(texts)
        except Exception as e:
            print(f"PaddleOCR error: {e}. Falling back...")

    # 2) WinRT / Windows.Media.Ocr
    if WINRT_AVAILABLE:
        import asyncio
        try:
            print("--- Đang sử dụng Windows OCR ---")
            return asyncio.run(_extract_text_winrt_async(image_path))
        except Exception as e:
            print(f"Windows OCR gặp lỗi: {e}. Đang chuyển sang Tesseract...")

    # 3) Fallback to pytesseract
    try:
        import pytesseract
        pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'
        img = Image.open(image_path)
        print("--- Đang sử dụng Tesseract ---")
        return pytesseract.image_to_string(img, lang='vie+eng')
    except Exception as e:
        raise RuntimeError(f"No OCR engine available: {e}")