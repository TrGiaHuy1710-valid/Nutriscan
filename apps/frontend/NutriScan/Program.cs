using NutriScan.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNutriScanApplication(builder.Configuration, builder.Environment);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.InitializeNutriScanDatabase();
app.ConfigureNutriScanPipeline();

app.Run();
