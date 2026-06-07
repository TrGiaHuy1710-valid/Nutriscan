using NutriScan.DTOs;

namespace NutriScan.Services.Scans
{
    public interface IScanAnalysisService
    {
        Task<ScanAnalyzeResponse> AnalyzeAsync(IFormFile file, CancellationToken cancellationToken);
    }
}
