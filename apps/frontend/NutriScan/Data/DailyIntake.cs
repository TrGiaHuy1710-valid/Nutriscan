using System;

namespace NutriScan.Data
{
    public class DailyIntake
    {
        public int Id { get; set; }
        public DateTime IntakeDate { get; set; } = DateTime.Today;
        public int TotalCalories { get; set; }
        public double TotalFat { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalProtein { get; set; }
    }
}
