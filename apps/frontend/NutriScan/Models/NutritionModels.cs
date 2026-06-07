using System.Text.Json.Serialization;

namespace NutriScan.Models
{
    /// <summary>
    /// Represents a scanned nutrition item
    /// </summary>
    public class NutritionItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        
        public decimal? Calories { get; set; }
        public decimal? Fat { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Protein { get; set; }
        
        public DateTime ScannedDate { get; set; } = DateTime.Now;
        public bool IsFavorite { get; set; } = false;
        public string? Notes { get; set; }
        
        public decimal CalculateCaloriesFromMacros()
        {
            decimal total = 0;
            if (Fat.HasValue) total += Fat.Value * 9;
            if (Carbs.HasValue) total += Carbs.Value * 4;
            if (Protein.HasValue) total += Protein.Value * 4;
            return total;
        }
    }

    /// <summary>
    /// Represents a comparison between two nutrition items
    /// </summary>
    public class ComparisonViewModel
    {
        public NutritionItem? Item1 { get; set; }
        public NutritionItem? Item2 { get; set; }
        
        public decimal CalorieDifference => (Item1?.Calories ?? 0) - (Item2?.Calories ?? 0);
        public decimal FatDifference => (Item1?.Fat ?? 0) - (Item2?.Fat ?? 0);
        public decimal CarbsDifference => (Item1?.Carbs ?? 0) - (Item2?.Carbs ?? 0);
        public decimal ProteinDifference => (Item1?.Protein ?? 0) - (Item2?.Protein ?? 0);
    }

    /// <summary>
    /// Statistics view model
    /// </summary>
    public class NutritionStatsViewModel
    {
        public int TotalScans { get; set; }
        public int TotalFavorites { get; set; }
        public decimal AverageCalories { get; set; }
        public decimal AverageProtein { get; set; }
        public decimal AverageFat { get; set; }
        public decimal AverageCarbs { get; set; }
        public List<NutritionItem> RecentScans { get; set; } = new();
        public Dictionary<string, int> ScansByDate { get; set; } = new();
    }
}
