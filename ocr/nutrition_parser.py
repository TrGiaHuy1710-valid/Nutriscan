import re

def extract_nutrition(text):
    data = {}

    calories = re.search(r'Calories\s*(\d+)', text)
    fat = re.search(r'Total Fat\s*(\d+)', text)
    carb = re.search(r'Total Carbohydrate\s*(\d+)', text)
    protein = re.search(r'Protein\s*(\d+)', text)

    if calories:
        data["calories"] = int(calories.group(1))
    if fat:
        data["fat"] = int(fat.group(1))
    if carb:
        data["carb"] = int(carb.group(1))
    if protein:
        data["protein"] = int(protein.group(1))

    return data