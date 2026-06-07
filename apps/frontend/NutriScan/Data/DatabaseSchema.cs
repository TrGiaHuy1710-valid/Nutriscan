using Microsoft.EntityFrameworkCore;

namespace NutriScan.Data
{
    public static class DatabaseSchema
    {
        public static void EnsureScanRecordColumns(NutriScanDbContext dbContext)
        {
            var existingColumns = GetExistingColumns(dbContext);
            var columns = new Dictionary<string, string>
            {
                ["NormalizedProductName"] = "TEXT NULL",
                ["Brand"] = "TEXT NULL",
                ["ServingSize"] = "TEXT NULL",
                ["Sugar"] = "REAL NULL",
                ["Sodium"] = "REAL NULL",
                ["ValidationConfidence"] = "REAL NOT NULL DEFAULT 0",
                ["ValidationLevel"] = "TEXT NOT NULL DEFAULT 'unverified'",
                ["ValidationWarningsJson"] = "TEXT NOT NULL DEFAULT '[]'",
                ["ValidationSource"] = "TEXT NOT NULL DEFAULT 'OCR'",
                ["CorrectedByUser"] = "INTEGER NOT NULL DEFAULT 0",
                ["MealType"] = "TEXT NULL",
                ["ServingMultiplier"] = "REAL NOT NULL DEFAULT 1"
            };

            foreach (var (name, definition) in columns)
            {
                if (existingColumns.Contains(name))
                {
                    continue;
                }

                try
                {
                    var sql = string.Concat("ALTER TABLE ScanRecords ADD COLUMN ", name, " ", definition);
                    dbContext.Database.ExecuteSqlRaw(sql);
                }
                catch
                {
                    // Column already exists or was created by EnsureCreated for a fresh database.
                }
            }
        }

        private static HashSet<string> GetExistingColumns(NutriScanDbContext dbContext)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var connection = dbContext.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info('ScanRecords')";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                columns.Add(reader.GetString(1));
            }

            return columns;
        }
    }
}
