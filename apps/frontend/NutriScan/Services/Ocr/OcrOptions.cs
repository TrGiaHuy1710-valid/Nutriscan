namespace NutriScan.Services.Ocr
{
    public class OcrOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:5000";
        public string AnalyzePath { get; set; } = "/api/analyze-food";
        public int TimeoutSeconds { get; set; } = 45;
    }
}
