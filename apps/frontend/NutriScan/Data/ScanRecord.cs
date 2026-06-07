namespace NutriScan.Data
{
    public class ScanRecord
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public int Calories { get; set; }
        public double Fat { get; set; }
        public double Carbs { get; set; }
        public double Protein { get; set; }
        public DateTime ScannedDate { get; set; }
        public bool IsFavorite { get; set; }
        public string Notes { get; set; } = "";
        public string RawOcrText { get; set; } = "";
        public string? NormalizedProductName { get; set; }
        public string? Brand { get; set; }
        public string? ServingSize { get; set; }
        public double? Sugar { get; set; }
        public double? Sodium { get; set; }
        public double ValidationConfidence { get; set; }
        public string ValidationLevel { get; set; } = "unverified";
        public string ValidationWarningsJson { get; set; } = "[]";
        public string ValidationSource { get; set; } = "OCR";
        public bool CorrectedByUser { get; set; }
        public string? MealType { get; set; }
        public double ServingMultiplier { get; set; } = 1;
    }
}
