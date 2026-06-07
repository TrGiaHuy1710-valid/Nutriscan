namespace NutriScan.Services.Ocr
{
    public class OcrResult
    {
        public bool Success { get; set; }
        public string RawText { get; set; } = "";
        public OcrNutrition Nutrition { get; set; } = new();
        public string? Error { get; set; }
    }

    public class OcrNutrition
    {
        public int Calories { get; set; }
        public double Fat { get; set; }
        public double Carb { get; set; }
        public double Protein { get; set; }
        public double? Sugar { get; set; }
        public double? Sodium { get; set; }
    }
}
