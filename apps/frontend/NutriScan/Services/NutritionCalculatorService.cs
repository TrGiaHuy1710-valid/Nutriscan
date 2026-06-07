using NutriScan.Data;
using System;

namespace NutriScan.Services
{
    public interface INutritionCalculatorService
    {
        (double Bmr, double Tdee, double CalorieTarget, double FatTarget, double CarbTarget, double ProteinTarget) CalculateTargets(UserProfile profile);
    }

    public class NutritionCalculatorService : INutritionCalculatorService
    {
        public (double Bmr, double Tdee, double CalorieTarget, double FatTarget, double CarbTarget, double ProteinTarget) CalculateTargets(UserProfile profile)
        {
            // 1. Calculate BMR (Mifflin-St Jeor)
            double bmr = 0;
            if (profile.Gender.Equals("Nam", StringComparison.OrdinalIgnoreCase) || profile.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
            {
                bmr = (10 * profile.CurrentWeight) + (6.25 * profile.Height) - (5 * profile.Age) + 5;
            }
            else
            {
                bmr = (10 * profile.CurrentWeight) + (6.25 * profile.Height) - (5 * profile.Age) - 161;
            }

            // 2. Calculate TDEE based on activity level
            double multiplier = profile.ActivityLevel switch
            {
                "Sedentary" => 1.2,
                "Light" => 1.375,
                "Moderate" => 1.55,
                "Active" => 1.725,
                "VeryActive" => 1.9,
                _ => 1.55 // default moderate
            };

            double tdee = bmr * multiplier;

            // 3. Calculate Daily Calorie Target based on goal
            double calorieTarget = profile.GoalType switch
            {
                "Lose" => tdee - 500,
                "Gain" => tdee + 350,
                "Maintain" => tdee,
                _ => tdee
            };

            // Set floor limits for safety
            double floorLimit = profile.Gender.Equals("Nam", StringComparison.OrdinalIgnoreCase) ? 1500 : 1200;
            if (calorieTarget < floorLimit)
            {
                calorieTarget = floorLimit;
            }

            // 4. Calculate Daily Macro Targets (grams)
            // Lose: 30% Protein, 35% Carbs, 35% Fat
            // Maintain: 25% Protein, 50% Carbs, 25% Fat
            // Gain: 25% Protein, 50% Carbs, 25% Fat
            double proteinPct = 0.25;
            double carbPct = 0.50;
            double fatPct = 0.25;

            if (profile.GoalType == "Lose")
            {
                proteinPct = 0.30;
                carbPct = 0.40;
                fatPct = 0.30;
            }
            else if (profile.GoalType == "Gain")
            {
                proteinPct = 0.30;
                carbPct = 0.45;
                fatPct = 0.25;
            }

            // 1g Protein = 4 calories
            // 1g Carb = 4 calories
            // 1g Fat = 9 calories
            double proteinTarget = (calorieTarget * proteinPct) / 4;
            double carbTarget = (calorieTarget * carbPct) / 4;
            double fatTarget = (calorieTarget * fatPct) / 9;

            return (
                Math.Round(bmr, 0),
                Math.Round(tdee, 0),
                Math.Round(calorieTarget, 0),
                Math.Round(fatTarget, 0),
                Math.Round(carbTarget, 0),
                Math.Round(proteinTarget, 0)
            );
        }
    }
}
