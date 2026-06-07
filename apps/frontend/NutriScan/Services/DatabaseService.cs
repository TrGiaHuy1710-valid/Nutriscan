using NutriScan.Data;
using Microsoft.EntityFrameworkCore;

namespace NutriScan.Services
{
    public interface IDatabaseService
    {
        Task<ScanRecord> AddScanAsync(
            string productName,
            int calories,
            double fat,
            double carbs,
            double protein,
            string imagePath,
            string rawOcr,
            string? normalizedProductName = null,
            string? brand = null,
            string? servingSize = null,
            double? sugar = null,
            double? sodium = null,
            double validationConfidence = 0,
            string validationLevel = "unverified",
            string validationWarningsJson = "[]",
            string validationSource = "OCR");
        Task<List<ScanRecord>> GetScansAsync(int limit = 50);
        Task<List<ScanRecord>> GetFavoritesAsync();
        Task<ScanRecord?> GetScanByIdAsync(int id);
        Task<bool> ToggleFavoriteAsync(int id);
        Task<bool> UpdateNotesAsync(int id, string notes);
        Task<int> GetTotalScansAsync();
        Task<int> GetTotalFavoritesAsync();
        Task<double> GetAverageCaloriesAsync();
        Task<(double Calories, double Fat, double Carbs, double Protein)> GetAverageNutritionAsync();
        Task<bool> DeleteScanAsync(int id);
        
        // New Profile, DailyIntake, and Workout methods
        Task<UserProfile> GetUserProfileAsync();
        Task<UserProfile?> UpdateUserProfileAsync(UserProfile profile);
        Task<List<ScanRecord>> GetScansForDateAsync(DateTime date);
        Task<List<WorkoutPlan>> GetWorkoutPlansAsync(string? type = null, string? difficulty = null);
        Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(int id);
        Task<DailyIntake> GetDailyIntakeSummaryAsync(DateTime date);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly NutriScanDbContext _dbContext;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(NutriScanDbContext dbContext, ILogger<DatabaseService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ScanRecord> AddScanAsync(
            string productName,
            int calories,
            double fat,
            double carbs,
            double protein,
            string imagePath,
            string rawOcr,
            string? normalizedProductName = null,
            string? brand = null,
            string? servingSize = null,
            double? sugar = null,
            double? sodium = null,
            double validationConfidence = 0,
            string validationLevel = "unverified",
            string validationWarningsJson = "[]",
            string validationSource = "OCR")
        {
            try
            {
                var scan = new ScanRecord
                {
                    ProductName = productName ?? "Unknown Product",
                    Calories = calories,
                    Fat = fat,
                    Carbs = carbs,
                    Protein = protein,
                    ImagePath = imagePath,
                    RawOcrText = rawOcr,
                    NormalizedProductName = normalizedProductName,
                    Brand = brand,
                    ServingSize = servingSize,
                    Sugar = sugar,
                    Sodium = sodium,
                    ValidationConfidence = validationConfidence,
                    ValidationLevel = validationLevel,
                    ValidationWarningsJson = validationWarningsJson,
                    ValidationSource = validationSource,
                    CorrectedByUser = false,
                    ServingMultiplier = 1,
                    ScannedDate = DateTime.UtcNow,
                    IsFavorite = false,
                    Notes = ""
                };

                _dbContext.ScanRecords.Add(scan);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Scan record added: ID={scan.Id}, Product={productName}");
                return scan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding scan record");
                throw;
            }
        }

        public async Task<List<ScanRecord>> GetScansAsync(int limit = 50)
        {
            try
            {
                return await _dbContext.ScanRecords
                    .OrderByDescending(s => s.ScannedDate)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scans");
                return new List<ScanRecord>();
            }
        }

        public async Task<List<ScanRecord>> GetFavoritesAsync()
        {
            try
            {
                return await _dbContext.ScanRecords
                    .Where(s => s.IsFavorite)
                    .OrderByDescending(s => s.ScannedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving favorites");
                return new List<ScanRecord>();
            }
        }

        public async Task<ScanRecord?> GetScanByIdAsync(int id)
        {
            try
            {
                return await _dbContext.ScanRecords.FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scan");
                return null;
            }
        }

        public async Task<bool> ToggleFavoriteAsync(int id)
        {
            try
            {
                var scan = await _dbContext.ScanRecords.FirstOrDefaultAsync(s => s.Id == id);
                if (scan == null) return false;

                scan.IsFavorite = !scan.IsFavorite;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Scan {id} favorite toggled to {scan.IsFavorite}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite");
                return false;
            }
        }

        public async Task<bool> UpdateNotesAsync(int id, string notes)
        {
            try
            {
                var scan = await _dbContext.ScanRecords.FirstOrDefaultAsync(s => s.Id == id);
                if (scan == null) return false;

                scan.Notes = notes;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes");
                return false;
            }
        }

        public async Task<int> GetTotalScansAsync()
        {
            try
            {
                return await _dbContext.ScanRecords.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting scans");
                return 0;
            }
        }

        public async Task<int> GetTotalFavoritesAsync()
        {
            try
            {
                return await _dbContext.ScanRecords.Where(s => s.IsFavorite).CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting favorites");
                return 0;
            }
        }

        public async Task<double> GetAverageCaloriesAsync()
        {
            try
            {
                var avg = await _dbContext.ScanRecords.AverageAsync(s => (double)s.Calories);
                return Math.Round(avg, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average calories");
                return 0;
            }
        }

        public async Task<(double Calories, double Fat, double Carbs, double Protein)> GetAverageNutritionAsync()
        {
            try
            {
                var count = await _dbContext.ScanRecords.CountAsync();
                if (count == 0) return (0, 0, 0, 0);

                var avgCalories = await _dbContext.ScanRecords.AverageAsync(s => (double)s.Calories);
                var avgFat = await _dbContext.ScanRecords.AverageAsync(s => s.Fat);
                var avgCarbs = await _dbContext.ScanRecords.AverageAsync(s => s.Carbs);
                var avgProtein = await _dbContext.ScanRecords.AverageAsync(s => s.Protein);

                return (
                    Math.Round(avgCalories, 2),
                    Math.Round(avgFat, 2),
                    Math.Round(avgCarbs, 2),
                    Math.Round(avgProtein, 2)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average nutrition");
                return (0, 0, 0, 0);
            }
        }

        public async Task<bool> DeleteScanAsync(int id)
        {
            try
            {
                var scan = await _dbContext.ScanRecords.FirstOrDefaultAsync(s => s.Id == id);
                if (scan == null) return false;

                _dbContext.ScanRecords.Remove(scan);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Scan {id} deleted");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting scan");
                return false;
            }
        }

        public async Task<UserProfile> GetUserProfileAsync()
        {
            try
            {
                var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync();
                if (profile == null)
                {
                    profile = new UserProfile
                    {
                        Name = "Người dùng",
                        Age = 25,
                        Gender = "Nam",
                        Height = 170,
                        CurrentWeight = 70,
                        TargetWeight = 70,
                        ActivityLevel = "Moderate",
                        GoalType = "Maintain",
                        DailyCalorieTarget = 2000,
                        DailyFatTarget = 65,
                        DailyCarbTarget = 250,
                        DailyProteinTarget = 100,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    _dbContext.UserProfiles.Add(profile);
                    await _dbContext.SaveChangesAsync();
                }
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving/creating user profile");
                return new UserProfile();
            }
        }

        public async Task<UserProfile?> UpdateUserProfileAsync(UserProfile profile)
        {
            try
            {
                var existing = await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.Id == profile.Id);
                if (existing != null)
                {
                    existing.Name = profile.Name;
                    existing.Age = profile.Age;
                    existing.Gender = profile.Gender;
                    existing.Height = profile.Height;
                    existing.CurrentWeight = profile.CurrentWeight;
                    existing.TargetWeight = profile.TargetWeight;
                    existing.ActivityLevel = profile.ActivityLevel;
                    existing.GoalType = profile.GoalType;
                    existing.DailyCalorieTarget = profile.DailyCalorieTarget;
                    existing.DailyFatTarget = profile.DailyFatTarget;
                    existing.DailyCarbTarget = profile.DailyCarbTarget;
                    existing.DailyProteinTarget = profile.DailyProteinTarget;
                    existing.UpdatedDate = DateTime.UtcNow;

                    await _dbContext.SaveChangesAsync();
                    return existing;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return null;
            }
        }

        public async Task<List<ScanRecord>> GetScansForDateAsync(DateTime date)
        {
            try
            {
                var localDateStart = date.Date;
                var localDateEnd = localDateStart.AddDays(1);
                
                // Query scans created within today's range
                return await _dbContext.ScanRecords
                    .Where(s => s.ScannedDate >= localDateStart && s.ScannedDate < localDateEnd)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scans for date");
                return new List<ScanRecord>();
            }
        }

        public async Task<List<WorkoutPlan>> GetWorkoutPlansAsync(string? type = null, string? difficulty = null)
        {
            try
            {
                var query = _dbContext.WorkoutPlans.AsQueryable();
                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(w => w.Type.ToLower() == type.ToLower());
                }
                if (!string.IsNullOrEmpty(difficulty))
                {
                    query = query.Where(w => w.Difficulty.ToLower() == difficulty.ToLower());
                }
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plans");
                return new List<WorkoutPlan>();
            }
        }

        public async Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(int id)
        {
            try
            {
                return await _dbContext.WorkoutPlans.FirstOrDefaultAsync(w => w.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workout plan");
                return null;
            }
        }

        public async Task<DailyIntake> GetDailyIntakeSummaryAsync(DateTime date)
        {
            try
            {
                var scans = await GetScansForDateAsync(date);
                var summary = await _dbContext.DailyIntakes.FirstOrDefaultAsync(d => d.IntakeDate.Date == date.Date);
                
                int totalCal = scans.Sum(s => s.Calories);
                double totalFat = Math.Round(scans.Sum(s => s.Fat), 1);
                double totalCarbs = Math.Round(scans.Sum(s => s.Carbs), 1);
                double totalProtein = Math.Round(scans.Sum(s => s.Protein), 1);

                if (summary == null)
                {
                    summary = new DailyIntake
                    {
                        IntakeDate = date.Date,
                        TotalCalories = totalCal,
                        TotalFat = totalFat,
                        TotalCarbs = totalCarbs,
                        TotalProtein = totalProtein
                    };
                    _dbContext.DailyIntakes.Add(summary);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    summary.TotalCalories = totalCal;
                    summary.TotalFat = totalFat;
                    summary.TotalCarbs = totalCarbs;
                    summary.TotalProtein = totalProtein;
                    await _dbContext.SaveChangesAsync();
                }
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving/updating daily intake summary");
                return new DailyIntake();
            }
        }
    }
}
