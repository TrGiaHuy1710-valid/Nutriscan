using NutriScan.Data;
using NutriScan.Services;

namespace NutriScan.Configuration
{
    public static class WebApplicationExtensions
    {
        public static WebApplication InitializeNutriScanDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<NutriScanDbContext>();

            dbContext.Database.EnsureCreated();
            DatabaseSchema.EnsureScanRecordColumns(dbContext);
            DatabaseSeeder.Seed(dbContext);

            return app;
        }

        public static WebApplication ConfigureNutriScanPipeline(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return app;
        }
    }
}
