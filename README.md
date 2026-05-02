# Nutriscan - OCR Food Label Parser

Ứng dụng OCR đọc nhãn thực phẩm và trích xuất dữ liệu dinh dưỡng tự động.

## 🎯 Tính năng

- 📸 Quét ảnh nhãn thực phẩm
- 🧠 OCR để đọc text từ ảnh bằng Tesseract
- 📊 Trích xuất dữ liệu dinh dưỡng chính (Calories, Fat, Carbs, Protein)
- 🌐 API REST để tích hợp vào website
- 💻 Giao diện web đẹp để test

## 🚀 Cài đặt

### 1. Cài đặt Tesseract OCR
- **Windows**: Tải từ https://github.com/UB-Mannheim/tesseract/wiki
- **Mac**: `brew install tesseract`
- **Linux**: `sudo apt-get install tesseract-ocr`

### 2. Cài đặt Python dependencies
```bash
pip install -r requirements.txt
```

## 📖 Sử dụng

### Chạy API server
```bash
python app.py
```
Server sẽ chạy tại `http://localhost:5000`

### Truy cập giao diện web
Mở browser và truy cập: **http://localhost:5000**

### API Endpoints

#### 1. POST `/api/analyze-food`
Gửi ảnh để phân tích

**Request:**
```bash
curl -X POST -F "file=@food_label.jpg" http://localhost:5000/api/analyze-food
```

**Response:**
```json
{
  "success": true,
  "raw_text": "Nutrition Facts\nServing Size: 1 cup\nCalories 120\nTotal Fat 2g\nTotal Carbohydrate 24g\nProtein 3g",
  "nutrition": {
    "calories": 120,
    "fat": 2,
    "carb": 24,
    "protein": 3
  }
}
```

#### 2. GET `/api/health`
Health check

```bash
curl http://localhost:5000/api/health
```

## 📁 Cấu trúc project

```
OCR_Tessereact/
├── app.py                 # Flask API server
├── main.py               # Script test (deprecated)
├── requirements.txt      # Python dependencies
├── index.html            # Giao diện web
├── .gitignore           # Git ignore
└── ocr/
    ├── __init__.py
    ├── ocr_service.py    # Module OCR (Tesseract)
    └── nutrition_parser.py # Parser dữ liệu dinh dưỡng
```

## 🔧 Customization

### Thêm các trường dữ liệu mới
Edit `ocr/nutrition_parser.py`:
```python
def extract_nutrition(text):
    data = {}
    
    # Thêm pattern mới
    sodium = re.search(r'Sodium\s*(\d+)', text)
    if sodium:
        data["sodium"] = int(sodium.group(1))
    
    return data
```

### Cải thiện độ chính xác OCR
- Crop ảnh trước khi OCR
- Cân chỉnh độ sáng/tương phản
- Dùng `--oem` hoặc `--psm` flags trong Tesseract

## 🐛 Troubleshooting

**Error: Tesseract not found**
- Kiểm tra path Tesseract trong `ocr_service.py`
- Đảm bảo Tesseract đã được cài đặt

**Image processing fails**
- Kiểm tra format ảnh (phải là PNG, JPG, GIF, BMP)
- Kiểm tra kích thước file (max 16MB)

## 📝 License
MIT

## 👨‍💻 Author
Your Name
