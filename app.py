from flask import Flask, request, jsonify, render_template_string
from werkzeug.utils import secure_filename
import os
from ocr.ocr_service import extract_text
from ocr.nutrition_parser import extract_nutrition

app = Flask(__name__, static_folder=None)

# Cấu hình upload
UPLOAD_FOLDER = os.path.join(os.path.dirname(__file__), 'uploads')
ALLOWED_EXTENSIONS = {'png', 'jpg', 'jpeg', 'gif', 'bmp'}

if not os.path.exists(UPLOAD_FOLDER):
    os.makedirs(UPLOAD_FOLDER)

app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
app.config['MAX_CONTENT_LENGTH'] = 16 * 1024 * 1024  # 16MB max file size


def allowed_file(filename):
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS


@app.route('/api/analyze-food', methods=['POST'])
def analyze_food():
    """
    Endpoint để đọc ảnh thực phẩm và trích xuất dữ liệu dinh dưỡng
    
    Input: File ảnh (multipart/form-data)
    Output: JSON với key data (calories, fat, carb, protein)
    """
    try:
        # Kiểm tra file được upload
        if 'file' not in request.files:
            return jsonify({'error': 'Không tìm thấy file'}), 400
        
        file = request.files['file']
        
        if file.filename == '':
            return jsonify({'error': 'Chưa chọn file'}), 400
        
        if not allowed_file(file.filename):
            return jsonify({'error': 'File phải là ảnh (png, jpg, jpeg, gif, bmp)'}), 400
        
        # Lưu file tạm
        filename = secure_filename(file.filename)
        filepath = os.path.join(app.config['UPLOAD_FOLDER'], filename)
        file.save(filepath)
        
        # Bước 1: OCR - Đọc text từ ảnh
        raw_text = extract_text(filepath)
        
        # Bước 2: Parse - Trích xuất dữ liệu quan trọng
        nutrition_data = extract_nutrition(raw_text)
        
        # Xóa file tạm
        os.remove(filepath)
        
        # Trả về kết quả
        return jsonify({
            'success': True,
            'raw_text': raw_text,
            'nutrition': nutrition_data
        }), 200
    
    except Exception as e:
        return jsonify({'error': str(e)}), 500


@app.route('/api/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({'status': 'OK'}), 200


@app.route('/', methods=['GET'])
def index():
    """Serve main HTML page"""
    with open(os.path.join(os.path.dirname(__file__), 'index.html'), 'r', encoding='utf-8') as f:
        return f.read()


if __name__ == '__main__':
    # Debug=True để phát triển, Debug=False khi deploy
    app.run(debug=True, host='0.0.0.0', port=5000)
