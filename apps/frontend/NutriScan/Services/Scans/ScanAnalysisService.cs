using System.Text.Json;
using NutriScan.DTOs;
using NutriScan.Services.FoodValidation;
using NutriScan.Services.Ocr;

namespace NutriScan.Services.Scans
{
    public class ScanAnalysisService : IScanAnalysisService
    {
        private readonly IOcrClient _ocrClient;
        private readonly IFoodValidateClient _foodValidateClient;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<ScanAnalysisService> _logger;

        public ScanAnalysisService(
            IOcrClient ocrClient,
            IFoodValidateClient foodValidateClient,
            IDatabaseService databaseService,
            ILogger<ScanAnalysisService> logger)
        {
            _ocrClient = ocrClient;
            _foodValidateClient = foodValidateClient;
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<ScanAnalyzeResponse> AnalyzeAsync(IFormFile file, CancellationToken cancellationToken)
        {
            ValidateFile(file);

            var ocr = await _ocrClient.AnalyzeAsync(file, cancellationToken);
            if (!ocr.Success)
            {
                throw new InvalidOperationException(ocr.Error ?? "OCR service failed");
            }

            var profile = await _databaseService.GetUserProfileAsync();
            var validation = await _foodValidateClient.ValidateAsync(new FoodValidationRequest
            {
                RawText = ocr.RawText,
                FileName = file.FileName,
                OcrNutrition = ocr.Nutrition,
                UserContext = new FoodValidationUserContext
                {
                    GoalType = profile.GoalType,
                    DailyCalorieTarget = profile.DailyCalorieTarget
                }
            }, cancellationToken);

            var nutrition = BuildNutrition(ocr, validation);
            var validationDto = BuildValidation(validation);
            var product = new ProductDto
            {
                Name = validation?.NormalizedName ?? file.FileName,
                Brand = validation?.Brand,
                ServingSize = validation?.ServingSize
            };

            var scan = await _databaseService.AddScanAsync(
                productName: product.Name,
                calories: nutrition.Calories,
                fat: nutrition.Fat,
                carbs: nutrition.Carbs,
                protein: nutrition.Protein,
                imagePath: "",
                rawOcr: ocr.RawText,
                normalizedProductName: validation?.NormalizedName,
                brand: validation?.Brand,
                servingSize: validation?.ServingSize,
                sugar: nutrition.Sugar,
                sodium: nutrition.Sodium,
                validationConfidence: validationDto.Confidence,
                validationLevel: validationDto.Level,
                validationWarningsJson: JsonSerializer.Serialize(validationDto.Warnings),
                validationSource: validationDto.Source);

            _logger.LogInformation("Scan analysis completed. ScanId={ScanId}, Validation={ValidationLevel}", scan.Id, validationDto.Level);

            return new ScanAnalyzeResponse
            {
                ScanId = scan.Id,
                Status = validationDto.Level == "unverified" ? "unverified" : "validated",
                Product = product,
                Nutrition = nutrition,
                Validation = validationDto,
                DailyImpact = BuildDailyImpact(nutrition, profile),
                RawText = ocr.RawText
            };
        }

        private static void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/webp"
            };

            if (string.IsNullOrWhiteSpace(file.ContentType) || !allowedContentTypes.Contains(file.ContentType))
            {
                throw new ArgumentException("File must be an image");
            }
        }

        private static NutritionDto BuildNutrition(OcrResult ocr, FoodValidationResult? validation)
        {
            if (validation?.Nutrition != null)
            {
                return new NutritionDto
                {
                    Calories = validation.Nutrition.Calories,
                    Protein = validation.Nutrition.Protein,
                    Carbs = validation.Nutrition.Carbs,
                    Fat = validation.Nutrition.Fat,
                    Sugar = validation.Nutrition.Sugar,
                    Sodium = validation.Nutrition.Sodium
                };
            }

            return new NutritionDto
            {
                Calories = ocr.Nutrition.Calories,
                Protein = ocr.Nutrition.Protein,
                Carbs = ocr.Nutrition.Carb,
                Fat = ocr.Nutrition.Fat,
                Sugar = ocr.Nutrition.Sugar,
                Sodium = ocr.Nutrition.Sodium
            };
        }

        private static ValidationDto BuildValidation(FoodValidationResult? validation)
        {
            if (validation == null)
            {
                return new ValidationDto
                {
                    Confidence = 0,
                    Level = "unverified",
                    Source = "OCR",
                    Warnings = new List<string> { "Da nhan dien, chua kiem chung" }
                };
            }

            return new ValidationDto
            {
                Confidence = validation.Confidence,
                Level = ConfidenceToLevel(validation.Confidence),
                Source = "FoodValidate-service",
                Warnings = validation.Flags.Select(f => f.Message).Where(m => !string.IsNullOrWhiteSpace(m)).ToList(),
                Suggestions = validation.Alternatives
            };
        }

        private static string ConfidenceToLevel(double confidence)
        {
            return confidence switch
            {
                >= 0.8 => "high",
                >= 0.55 => "medium",
                > 0 => "low",
                _ => "unverified"
            };
        }

        private static DailyImpactDto BuildDailyImpact(NutritionDto nutrition, Data.UserProfile profile)
        {
            return new DailyImpactDto
            {
                CaloriePercent = Percent(nutrition.Calories, profile.DailyCalorieTarget),
                ProteinPercent = Percent(nutrition.Protein, profile.DailyProteinTarget),
                CarbPercent = Percent(nutrition.Carbs, profile.DailyCarbTarget),
                FatPercent = Percent(nutrition.Fat, profile.DailyFatTarget)
            };
        }

        private static int Percent(double value, double target)
        {
            return target <= 0 ? 0 : (int)Math.Round(value / target * 100);
        }
    }
}
