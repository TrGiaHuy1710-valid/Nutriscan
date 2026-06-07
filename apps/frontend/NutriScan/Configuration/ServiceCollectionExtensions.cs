using Microsoft.EntityFrameworkCore;
using NutriScan.Data;
using NutriScan.Services;
using NutriScan.Services.FoodValidation;
using NutriScan.Services.Ocr;
using NutriScan.Services.Scans;

namespace NutriScan.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNutriScanApplication(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddControllersWithViews();
            services.AddHttpClient();

            services.Configure<OcrOptions>(configuration.GetSection("Services:Ocr"));
            services.Configure<FoodValidateOptions>(configuration.GetSection("Services:FoodValidate"));

            services.AddNutriScanDatabase(environment);
            services.AddNutriScanServices();
            services.AddHostedService<PythonOcrBackgroundService>();

            return services;
        }

        private static IServiceCollection AddNutriScanDatabase(
            this IServiceCollection services,
            IWebHostEnvironment environment)
        {
            var repoRoot = Directory.GetParent(environment.ContentRootPath)?.Parent?.Parent?.FullName
                ?? environment.ContentRootPath;
            var dbPath = Path.Combine(repoRoot, "storage", "outputs", "nutriscan.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            services.AddDbContext<NutriScanDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            return services;
        }

        private static IServiceCollection AddNutriScanServices(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<INutritionCalculatorService, NutritionCalculatorService>();
            services.AddScoped<IFoodRecommendationService, FoodRecommendationService>();
            services.AddScoped<IOcrClient, PythonOcrClient>();
            services.AddScoped<IFoodValidateClient, FoodValidateClient>();
            services.AddScoped<IScanAnalysisService, ScanAnalysisService>();

            return services;
        }
    }
}
