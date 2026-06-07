using System;

namespace NutriScan.Data
{
    public class WorkoutPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = ""; // "Cardio", "Strength", "Flexibility"
        public int DurationMinutes { get; set; }
        public int CaloriesBurned { get; set; }
        public string Difficulty { get; set; } = ""; // "Dễ", "Trung bình", "Khó"
        public string MuscleGroup { get; set; } = ""; // "Toàn thân", "Ngực/Vai", "Bụng", "Mông/Đùi", "Lưng/Tay"
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Instructions { get; set; } = ""; // Semicolon-separated or newline-separated instructions
    }
}
