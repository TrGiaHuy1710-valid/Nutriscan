namespace NutriScan.Services.FoodValidation
{
    public class FoodValidateOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:8000";
        public string ValidatePath { get; set; } = "/api/v1/food/validate";
        public int TimeoutSeconds { get; set; } = 5;
        public bool Enabled { get; set; } = true;
    }
}
