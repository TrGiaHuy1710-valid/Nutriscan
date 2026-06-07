import re


def parse_number(text):
    if not text:
        return None

    # Remove common OCR garbage characters
    text = text.replace('I', '1').replace('l', '1').replace('O', '0').replace('o', '0')
    
    # Look for first occurrence of digit pattern
    match = re.search(r'(\d+(?:[\.,]\d+)?)', text)

    if not match:
        return None

    value = match.group(1).replace(',', '.')

    return float(value) if '.' in value else int(value)


def extract_nutrition(text):
    data = {}

    # Chuẩn hóa text
    normalized = text.replace('\n', ' ').replace('\r', ' ').lower()

    patterns = {
        # Calories - multiple Vietnamese variations
        'calories': r'(?:calories|calo|năng lượng|energy|kcal|cal)\s*[:\-]?\s*(?:around|khoảng)?\s*([\d.,]+)',
        
        # Fat - multiple Vietnamese variations
        'fat': r'(?:total\s+fat|chất\s+béo|béo|lipid|fat|mỡ)\s*[:\-]?\s*(?:around|khoảng)?\s*([\d.,]+)',
        
        # Carbohydrates - multiple Vietnamese variations
        'carb': r'(?:total\s+carbohydrate|carbohydrate|carbohydrat|tinh\s+bột|đường|carb|tảo|glucose)\s*[:\-]?\s*(?:around|khoảng)?\s*([\d.,]+)',
        
        # Protein - multiple Vietnamese variations
        'protein': r'(?:protein|chất\s+đạm|đạm|protêin)\s*[:\-]?\s*(?:around|khoảng)?\s*([\d.,]+)',
    }

    for key, pattern in patterns.items():
        match = re.search(pattern, normalized)

        if match:
            value = parse_number(match.group(1))

            if value is not None:
                data[key] = value

    return data
