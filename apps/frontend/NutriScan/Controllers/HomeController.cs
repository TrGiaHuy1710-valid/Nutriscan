using Microsoft.AspNetCore.Mvc;
using NutriScan.Services;
using NutriScan.Data;
using System.Threading.Tasks;
using System.Text.Json;
using NutriScan.Services.Scans;

namespace NutriScan.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatabaseService _databaseService;
        private readonly INutritionCalculatorService _calculatorService;
        private readonly IFoodRecommendationService _recommendationService;
        private readonly IScanAnalysisService _scanAnalysisService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IDatabaseService databaseService, 
            INutritionCalculatorService calculatorService,
            IFoodRecommendationService recommendationService,
            IScanAnalysisService scanAnalysisService,
            ILogger<HomeController> logger)
        {
            _databaseService = databaseService;
            _calculatorService = calculatorService;
            _recommendationService = recommendationService;
            _scanAnalysisService = scanAnalysisService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Nutrition()
        {
            return View();
        }

        public IActionResult AI()
        {
            return View();
        }

        public IActionResult Workout()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }

        public IActionResult Favorites()
        {
            return View();
        }

        public IActionResult Comparison()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var profile = await _databaseService.GetUserProfileAsync();
            return View(profile);
        }

        public IActionResult QRScan()
        {
           return Redirect("http://localhost:5000");
        }

        [HttpGet]
        [Route("api/profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var profile = await _databaseService.GetUserProfileAsync();
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/profile")]
        public async Task<IActionResult> SaveProfile([FromBody] UserProfile updatedProfile)
        {
            try
            {
                if (updatedProfile == null)
                {
                    return BadRequest(new { error = "Invalid profile data" });
                }

                // Retrieve existing profile to preserve ID
                var existing = await _databaseService.GetUserProfileAsync();
                updatedProfile.Id = existing.Id;

                // Re-calculate calorie/macro targets based on input
                var targets = _calculatorService.CalculateTargets(updatedProfile);
                updatedProfile.DailyCalorieTarget = targets.CalorieTarget;
                updatedProfile.DailyFatTarget = targets.FatTarget;
                updatedProfile.DailyCarbTarget = targets.CarbTarget;
                updatedProfile.DailyProteinTarget = targets.ProteinTarget;

                var result = await _databaseService.UpdateUserProfileAsync(updatedProfile);
                if (result == null)
                {
                    return StatusCode(500, new { error = "Could not update profile in database" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/daily-intake/summary")]
        public async Task<IActionResult> GetDailyIntakeSummary()
        {
            try
            {
                var profile = await _databaseService.GetUserProfileAsync();
                var intake = await _databaseService.GetDailyIntakeSummaryAsync(DateTime.Today);

                return Ok(new
                {
                    profile = new {
                        profile.Name,
                        profile.GoalType,
                        calorieTarget = profile.DailyCalorieTarget,
                        fatTarget = profile.DailyFatTarget,
                        carbTarget = profile.DailyCarbTarget,
                        proteinTarget = profile.DailyProteinTarget
                    },
                    intake = new {
                        date = intake.IntakeDate,
                        calories = intake.TotalCalories,
                        fat = intake.TotalFat,
                        carbs = intake.TotalCarbs,
                        protein = intake.TotalProtein
                    },
                    remaining = new {
                        calories = Math.Max(0, profile.DailyCalorieTarget - intake.TotalCalories),
                        fat = Math.Max(0, profile.DailyFatTarget - intake.TotalFat),
                        carbs = Math.Max(0, profile.DailyCarbTarget - intake.TotalCarbs),
                        protein = Math.Max(0, profile.DailyProteinTarget - intake.TotalProtein)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/recommendations/food")]
        public async Task<IActionResult> GetFoodRecommendations()
        {
            try
            {
                var profile = await _databaseService.GetUserProfileAsync();
                var intake = await _databaseService.GetDailyIntakeSummaryAsync(DateTime.Today);
                var (advice, recommendations) = _recommendationService.GetRecommendations(profile, intake);

                var formattedRecs = recommendations.Select(r => new
                {
                    name = r.Name,
                    calories = r.Calories,
                    fat = r.Fat,
                    carbs = r.Carbs,
                    protein = r.Protein,
                    reason = r.Reason
                });

                return Ok(new
                {
                    advice,
                    recommendations = formattedRecs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/workouts")]
        public async Task<IActionResult> GetWorkouts(string? type = null, string? difficulty = null)
        {
            try
            {
                var workouts = await _databaseService.GetWorkoutPlansAsync(type, difficulty);
                return Ok(workouts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/recommendations/workout")]
        public async Task<IActionResult> GetWorkoutRecommendations()
        {
            try
            {
                var profile = await _databaseService.GetUserProfileAsync();
                // Filter workout plans matching the goal. 
                // Lose weight -> more Cardio, Gain weight -> more Strength, Maintain -> balance
                string? recommendedType = profile.GoalType switch
                {
                    "Lose" => "Cardio",
                    "Gain" => "Strength",
                    "Maintain" => null, // Mix
                    _ => null
                };

                var workouts = await _databaseService.GetWorkoutPlansAsync(recommendedType);
                
                // Shuffle and take 4
                var rnd = new Random();
                var selected = workouts.OrderBy(x => rnd.Next()).Take(4).ToList();

                // If less than 4, fallback to get all
                if (selected.Count < 4 && recommendedType != null)
                {
                    var extra = await _databaseService.GetWorkoutPlansAsync();
                    foreach (var exWorkout in extra)
                    {
                        if (selected.Count >= 4) break;
                        if (!selected.Any(s => s.Id == exWorkout.Id))
                        {
                            selected.Add(exWorkout);
                        }
                    }
                }

                string advice = profile.GoalType switch
                {
                    "Lose" => "Để giảm cân an toàn và hiệu quả, hãy tập trung vào các bài tập Cardio để tăng nhịp tim và đốt calo nhanh hơn.",
                    "Gain" => "Để tăng cân và xây dựng cơ bắp săn chắc, hãy tập luyện các bài tập nhóm Strength (Tập sức mạnh) để kích hoạt cơ bắp tốt nhất.",
                    "Maintain" => "Để giữ dáng và tăng độ dẻo dai cơ thể, hãy tập phối hợp cả Cardio, Strength và giãn cơ Flexibility.",
                    _ => "Duy trì rèn luyện thể dục thể thao đều đặn mỗi ngày."
                };

                return Ok(new
                {
                    advice,
                    recommendations = selected
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/analyze-food")]
        public async Task<IActionResult> AnalyzeFood(CancellationToken cancellationToken)
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null)
                {
                    return BadRequest(new { error = "No file provided" });
                }

                var result = await _scanAnalysisService.AnalyzeAsync(file, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Legacy scan analysis endpoint failed");
                return StatusCode(500, new { error = "Scan analysis failed" });
            }
        }

        [HttpGet]
        [Route("api/scans/history")]
        public async Task<IActionResult> GetHistory(int limit = 50)
        {
            try
            {
                var scans = await _databaseService.GetScansAsync(limit);
                return Ok(scans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/scans/favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            try
            {
                var favorites = await _databaseService.GetFavoritesAsync();
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/scans/{id}")]
        public async Task<IActionResult> GetScan(int id)
        {
            try
            {
                var scan = await _databaseService.GetScanByIdAsync(id);
                if (scan == null)
                    return NotFound(new { error = "Scan not found" });
                return Ok(scan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scan");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/scans/{id}/favorite")]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            try
            {
                var result = await _databaseService.ToggleFavoriteAsync(id);
                if (!result)
                    return NotFound(new { error = "Scan not found" });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("api/scans/{id}")]
        public async Task<IActionResult> DeleteScan(int id)
        {
            try
            {
                var result = await _databaseService.DeleteScanAsync(id);
                if (!result)
                    return NotFound(new { error = "Scan not found" });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting scan");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/scans/{id}/notes")]
        public async Task<IActionResult> UpdateNotes(int id, [FromBody] JsonElement body)
        {
            try
            {
                string notes = "";
                if (body.TryGetProperty("notes", out var notesProperty))
                {
                    notes = notesProperty.GetString() ?? "";
                }

                var result = await _databaseService.UpdateNotesAsync(id, notes);
                if (!result)
                    return NotFound(new { error = "Scan not found" });
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/stats/overview")]
        public async Task<IActionResult> GetStatsOverview()
        {
            try
            {
                var totalScans = await _databaseService.GetTotalScansAsync();
                var totalFavorites = await _databaseService.GetTotalFavoritesAsync();
                var (avgCal, avgFat, avgCarbs, avgProtein) = await _databaseService.GetAverageNutritionAsync();

                return Ok(new
                {
                    totalScans,
                    totalFavorites,
                    averageCalories = avgCal,
                    averageFat = avgFat,
                    averageCarbs = avgCarbs,
                    averageProtein = avgProtein
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
