# OCR Module - Nutriscan

## Overview

The OCR module is a professional-grade optical character recognition system designed to extract nutritional information from food label images. It supports multiple languages (English and Vietnamese) with intelligent fallback mechanisms and advanced image preprocessing.

## Features

- 🌍 **Multi-Language Support**: Vietnamese and English text recognition
- 🎯 **Dual Engine Architecture**: PaddleOCR (primary) with Tesseract (fallback)
- 🖼️ **Advanced Image Processing**: Automatic preprocessing for improved accuracy
- 📊 **Nutrition Data Extraction**: Extracts calories, fat, carbohydrates, and protein
- ⚡ **Optimized Performance**: Lightweight and efficient for real-time applications
- 🔄 **Automatic Fallback**: Seamless switching between OCR engines if one fails

## Architecture

### OCR Engines

#### 1. **PaddleOCR** (Primary)
- Optimized for Vietnamese text recognition
- Superior accuracy for multilingual documents
- Faster inference compared to Tesseract
- Recommended for production use

#### 2. **Tesseract** (Fallback)
- Mature, widely-used OCR engine
- Used as backup if PaddleOCR is unavailable
- Requires manual system-level installation

### Image Preprocessing Pipeline

All images undergo the following preprocessing steps to improve OCR accuracy:

1. **Grayscale Conversion**: Reduces color complexity
2. **Upscaling**: Increases resolution for small images (<400px)
3. **Contrast Enhancement**: 2.5x factor for better text visibility
4. **Brightness Adjustment**: Normalized to 0.95 factor
5. **Sharpness Enhancement**: 3.0x factor to distinguish similar characters

### Nutrition Data Extraction

The `nutrition_parser` module extracts nutritional information using regex pattern matching with support for:
- Multiple Vietnamese terminology variations
- Common OCR error correction (I→1, O→0)
- Flexible numeric formats with comma/period separators

## Installation

### 1. System Dependencies

#### Option A: PaddleOCR (Recommended)
```bash
pip install paddleocr
```

#### Option B: Tesseract OCR

**Windows:**
- Download from: https://github.com/UB-Mannheim/tesseract/wiki
- Run installer and note the installation path (typically `C:\Program Files\Tesseract-OCR`)
- Installer will set up environment automatically

**macOS:**
```bash
brew install tesseract
```

**Linux:**
```bash
sudo apt-get install tesseract-ocr
```

### 2. Python Dependencies

```bash
pip install -r requirements.txt
```

## Module Structure

```
ocr/
├── ocr_service.py          # OCR engine management and image preprocessing
├── nutrition_parser.py      # Nutrition data extraction and parsing
├── __init__.py             # Module initialization
└── README.md               # This file
```

## API Reference

### `ocr_service.py`

#### `extract_text(image_path: str) -> str`
Extracts text from an image using the multi-engine approach.

**Parameters:**
- `image_path` (str): Path to the image file

**Returns:**
- `str`: Extracted text from the image

**Raises:**
- `FileNotFoundError`: If image file does not exist
- `RuntimeError`: If no OCR engine is available

**Example:**
```python
from ocr.ocr_service import extract_text

text = extract_text("path/to/nutrition_label.jpg")
print(text)
```

#### `preprocess_image(image_path: str) -> PIL.Image`
Preprocesses an image for improved OCR accuracy.

**Parameters:**
- `image_path` (str): Path to the image file

**Returns:**
- `PIL.Image`: Preprocessed image object

### `nutrition_parser.py`

#### `extract_nutrition(text: str) -> dict`
Extracts structured nutrition data from OCR text.

**Parameters:**
- `text` (str): Raw text from OCR engine

**Returns:**
- `dict`: Dictionary containing:
  - `calories`: Energy value in kcal
  - `fat`: Total fat in grams
  - `carb`: Total carbohydrates in grams
  - `protein`: Protein in grams

**Example:**
```python
from ocr.nutrition_parser import extract_nutrition

text = extract_text("path/to/label.jpg")
nutrition_data = extract_nutrition(text)
print(nutrition_data)
# Output: {'calories': 250, 'fat': 8.5, 'carb': 35, 'protein': 12}
```

#### `parse_number(text: str) -> Union[int, float, None]`
Parses the first numeric value from text, handling common OCR errors.

**Parameters:**
- `text` (str): Text containing numeric value

**Returns:**
- `Union[int, float, None]`: Parsed numeric value or None if not found

## Usage Example

```python
from ocr.ocr_service import extract_text
from ocr.nutrition_parser import extract_nutrition

# Extract text from nutrition label image
image_path = "food_label.jpg"
extracted_text = extract_text(image_path)

# Parse nutrition information
nutrition_info = extract_nutrition(extracted_text)

print(f"Calories: {nutrition_info.get('calories')} kcal")
print(f"Fat: {nutrition_info.get('fat')} g")
print(f"Carbohydrates: {nutrition_info.get('carb')} g")
print(f"Protein: {nutrition_info.get('protein')} g")
```

## Language Support

### Vietnamese
The module includes comprehensive support for Vietnamese nutrition terminology:
- `năng lượng`, `calo`, `kcal` → calories
- `chất béo`, `lipid`, `mỡ` → fat
- `tinh bột`, `carbohydrate`, `đường` → carbohydrates
- `chất đạm`, `protein`, `đạm` → protein

### English
Full support for standard English nutrition labels.

## Performance Optimization

### Tips for Best Results

1. **Image Quality**: Use clear, well-lit images for optimal recognition
2. **Resolution**: Images with at least 300 DPI are recommended
3. **Orientation**: Ensure text is horizontally oriented
4. **Contrast**: High contrast between text and background improves accuracy
5. **Language**: Specify correct language if known (affects engine selection)

### Supported Image Formats
- PNG
- JPG / JPEG
- GIF
- BMP

## Troubleshooting

### PaddleOCR Not Available
```
[WARNING] PaddleOCR import/init error
```
**Solution:** Install PaddleOCR: `pip install paddleocr`

### Tesseract Not Found
```
[WARNING] Tesseract import error
```
**Solution:** Install Tesseract following system installation instructions above.

### No OCR Engine Available
```
RuntimeError: No OCR engine available
```
**Solution:** Install at least one OCR engine (PaddleOCR or Tesseract)

### Poor OCR Accuracy
- Improve image quality and contrast
- Ensure image resolution is sufficient
- Check language settings match the text

## Dependencies

- `Pillow>=9.0.0` - Image processing
- `paddleocr>=2.7.0` - Primary OCR engine (optional)
- `pytesseract>=0.3.10` - Tesseract wrapper (optional)

At least one OCR engine must be installed for the module to function.

## Future Enhancements

- [ ] Support for additional languages (French, Spanish, etc.)
- [ ] Machine learning-based nutrition field localization
- [ ] Barcode/QR code integration
- [ ] Improved Vietnamese character recognition
- [ ] Real-time video processing
- [ ] Performance optimization with batch processing

## License

Part of the Nutriscan project.

## Author

Nutriscan Development Team

## Support

For issues or questions, please refer to the main Nutriscan repository.
