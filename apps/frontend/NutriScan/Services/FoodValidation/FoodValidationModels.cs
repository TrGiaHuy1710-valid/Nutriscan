using NutriScan.Services.Ocr;

namespace NutriScan.Services.FoodValidation
{
    public class FoodValidationRequest
    {
        public string RawText { get; set; } = "";
        public string FileName { get; set; } = "";
        public OcrNutrition OcrNutrition { get; set; } = new();
        public FoodValidationUserContext UserContext { get; set; } = new();
    }

    public class FoodValidationUserContext
    {
        public string GoalType { get; set; } = "";
        public double DailyCalorieTarget { get; set; }
        public List<string> Allergies { get; set; } = new();
        public List<string> DietTags { get; set; } = new();
    }

    public class FoodValidationResult
    {
        public bool IsFoodLabel { get; set; } = true;
        public double Confidence { get; set; }
        public string? NormalizedName { get; set; }
        public string? Brand { get; set; }
        public string? ServingSize { get; set; }
        public FoodValidationNutrition? Nutrition { get; set; }
        public List<FoodValidationFlag> Flags { get; set; } = new();
        public List<string> Alternatives { get; set; } = new();
    }

    public class FoodValidationNutrition
    {
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double? Sugar { get; set; }
        public double? Sodium { get; set; }
    }

    public class FoodValidationFlag
    {
        public string Type { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
