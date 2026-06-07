using System;

namespace NutriScan.Data
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Người dùng";
        public int Age { get; set; } = 25;
        public string Gender { get; set; } = "Nam"; // "Nam" hoặc "Nữ"
        public double Height { get; set; } = 170; // cm
        public double CurrentWeight { get; set; } = 70; // kg
        public double TargetWeight { get; set; } = 70; // kg
        public string ActivityLevel { get; set; } = "Moderate"; // "Sedentary", "Light", "Moderate", "Active", "VeryActive"
        public string GoalType { get; set; } = "Maintain"; // "Lose", "Maintain", "Gain"
        
        // Calculated daily targets
        public double DailyCalorieTarget { get; set; } = 2000;
        public double DailyFatTarget { get; set; } = 65; // grams
        public double DailyCarbTarget { get; set; } = 250; // grams
        public double DailyProteinTarget { get; set; } = 100; // grams

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
