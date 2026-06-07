using NutriScan.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutriScan.Services
{
    public interface IFoodRecommendationService
    {
        (string Advice, List<(string Name, int Calories, double Fat, double Carbs, double Protein, string Reason)> Recommendations) GetRecommendations(UserProfile profile, DailyIntake currentIntake);
    }

    public class FoodRecommendationService : IFoodRecommendationService
    {
        public (string Advice, List<(string Name, int Calories, double Fat, double Carbs, double Protein, string Reason)> Recommendations) GetRecommendations(UserProfile profile, DailyIntake currentIntake)
        {
            double remainingCal = profile.DailyCalorieTarget - currentIntake.TotalCalories;
            double remainingProtein = profile.DailyProteinTarget - currentIntake.TotalProtein;
            double remainingCarbs = profile.DailyCarbTarget - currentIntake.TotalCarbs;
            double remainingFat = profile.DailyFatTarget - currentIntake.TotalFat;

            string advice = "";
            var recs = new List<(string Name, int Calories, double Fat, double Carbs, double Protein, string Reason)>();

            // 1. Generate core advice text
            if (remainingCal <= 0)
            {
                advice = "Bạn đã đạt hoặc vượt chỉ tiêu Calo cho ngày hôm nay. Hãy uống thêm nước lọc, trà xanh không đường và hạn chế nạp thêm thức ăn nặng.";
            }
            else if (remainingProtein > 15 && remainingCal > 200)
            {
                advice = $"Hôm nay bạn cần bổ sung thêm khoảng {Math.Round(remainingProtein, 0)}g Protein nữa để hỗ trợ phát triển cơ bắp và phục hồi cơ thể. Hãy tập trung các món giàu đạm, ít chất béo xấu.";
            }
            else if (remainingCarbs > 30 && remainingCal > 200)
            {
                advice = $"Cơ thể bạn đang cần nạp thêm Carb ({Math.Round(remainingCarbs, 0)}g) để duy trì năng lượng hoạt động tốt. Hãy chọn các nguồn tinh bột phức hợp như khoai lang, ngô hoặc cơm.";
            }
            else if (remainingCal > 100)
            {
                advice = $"Bạn còn thiếu khoảng {Math.Round(remainingCal, 0)} Calo. Hãy chọn một bữa ăn nhẹ lành mạnh hoặc trái cây để hoàn thành mục tiêu ngày hôm nay.";
            }
            else
            {
                advice = "Tuyệt vời! Lượng dinh dưỡng hôm nay của bạn đang rất gần mục tiêu. Hãy duy trì trạng thái cân bằng này.";
            }

            // 2. Filter predefined foods to recommend
            // Get all dishes from the static list in DatabaseSeeder
            var dishes = DatabaseSeeder.PredefinedFoods;

            // Scenarios for recommendation:
            if (remainingCal <= 0)
            {
                // Calorie limit reached -> recommend super light snacks/beverages
                var lightOptions = dishes.Where(d => d.Calories <= 100).Take(4).ToList();
                foreach (var d in lightOptions)
                {
                    recs.Add((d.Name, d.Calories, d.Fat, d.Carbs, d.Protein, "Món nhẹ ít calo, không lo vượt mục tiêu"));
                }
            }
            else if (remainingProtein > 15)
            {
                // High Protein needed -> recommend dishes with high protein-to-calorie ratio
                // Let's filter dishes where protein is high (e.g. > 15g or high ratio) and cal <= remaining calories + 100
                var proteinOptions = dishes
                    .Where(d => d.Protein >= 10 && d.Calories <= remainingCal + 100)
                    .OrderByDescending(d => d.Protein)
                    .Take(4)
                    .ToList();
                
                foreach (var d in proteinOptions)
                {
                    recs.Add((d.Name, d.Calories, d.Fat, d.Carbs, d.Protein, $"Giàu đạm ({d.Protein}g protein) giúp bạn bù đắp lượng thiếu hụt"));
                }
            }
            else if (remainingCarbs > 30)
            {
                // High Carbs needed -> recommend healthy carbs
                var carbOptions = dishes
                    .Where(d => d.Carbs >= 20 && d.Calories <= remainingCal + 100)
                    .OrderByDescending(d => d.Carbs)
                    .Take(4)
                    .ToList();

                foreach (var d in carbOptions)
                {
                    recs.Add((d.Name, d.Calories, d.Fat, d.Carbs, d.Protein, $"Nguồn tinh bột tốt ({d.Carbs}g carbs) để nạp năng lượng"));
                }
            }
            else
            {
                // General balanced dishes matching remaining calories
                var balancedOptions = dishes
                    .Where(d => d.Calories > 50 && d.Calories <= remainingCal + 50)
                    .OrderBy(d => Math.Abs(d.Calories - remainingCal))
                    .Take(4)
                    .ToList();

                foreach (var d in balancedOptions)
                {
                    recs.Add((d.Name, d.Calories, d.Fat, d.Carbs, d.Protein, "Bữa ăn cân bằng, phù hợp với năng lượng còn thiếu"));
                }
            }

            // Fallback if list is empty
            if (recs.Count == 0)
            {
                // Fallback to top fruits/healthy items
                var fallbacks = dishes.Take(4).ToList();
                foreach (var d in fallbacks)
                {
                    recs.Add((d.Name, d.Calories, d.Fat, d.Carbs, d.Protein, "Thực phẩm lành mạnh được đề xuất"));
                }
            }

            return (advice, recs);
        }
    }
}
