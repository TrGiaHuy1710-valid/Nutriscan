namespace NutriScan.DTOs
{
    public class ScanAnalyzeResponse
    {
        public int ScanId { get; set; }
        public string Status { get; set; } = "unverified";
        public ProductDto Product { get; set; } = new();
        public NutritionDto Nutrition { get; set; } = new();
        public ValidationDto Validation { get; set; } = new();
        public DailyImpactDto DailyImpact { get; set; } = new();
        public string RawText { get; set; } = "";
    }

    public class ProductDto
    {
        public string Name { get; set; } = "";
        public string? Brand { get; set; }
        public string? ServingSize { get; set; }
    }

    public class NutritionDto
    {
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double? Sugar { get; set; }
        public double? Sodium { get; set; }
    }

    public class ValidationDto
    {
        public double Confidence { get; set; }
        public string Level { get; set; } = "unverified";
        public string Source { get; set; } = "OCR";
        public List<string> Warnings { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }

    public class DailyImpactDto
    {
        public int CaloriePercent { get; set; }
        public int ProteinPercent { get; set; }
        public int CarbPercent { get; set; }
        public int FatPercent { get; set; }
    }
}
