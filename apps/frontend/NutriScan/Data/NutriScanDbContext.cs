using Microsoft.EntityFrameworkCore;
using NutriScan.Data;

namespace NutriScan.Data
{
    public class NutriScanDbContext : DbContext
    {
        public NutriScanDbContext(DbContextOptions<NutriScanDbContext> options) : base(options)
        {
        }

        public DbSet<ScanRecord> ScanRecords { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<DailyIntake> DailyIntakes { get; set; }
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ScanRecord
            modelBuilder.Entity<ScanRecord>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<ScanRecord>()
                .Property(s => s.ProductName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<ScanRecord>()
                .Property(s => s.ValidationLevel)
                .HasDefaultValue("unverified");

            modelBuilder.Entity<ScanRecord>()
                .Property(s => s.ValidationWarningsJson)
                .HasDefaultValue("[]");

            modelBuilder.Entity<ScanRecord>()
                .Property(s => s.ValidationSource)
                .HasDefaultValue("OCR");

            modelBuilder.Entity<ScanRecord>()
                .Property(s => s.ServingMultiplier)
                .HasDefaultValue(1);

            modelBuilder.Entity<ScanRecord>()
                .Property(s => s.ScannedDate)
                .HasDefaultValueSql("datetime('now')");

            // Create index on ScannedDate for faster queries
            modelBuilder.Entity<ScanRecord>()
                .HasIndex(s => s.ScannedDate)
                .IsDescending();

            // Create index on IsFavorite for filtering
            modelBuilder.Entity<ScanRecord>()
                .HasIndex(s => s.IsFavorite);

            modelBuilder.Entity<ScanRecord>()
                .HasIndex(s => s.ValidationLevel);

            // Configure UserProfile
            modelBuilder.Entity<UserProfile>()
                .HasKey(u => u.Id);

            // Configure DailyIntake
            modelBuilder.Entity<DailyIntake>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<DailyIntake>()
                .HasIndex(d => d.IntakeDate);

            // Configure WorkoutPlan
            modelBuilder.Entity<WorkoutPlan>()
                .HasKey(w => w.Id);
        }
    }
}
