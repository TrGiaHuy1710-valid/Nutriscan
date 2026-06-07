using Microsoft.AspNetCore.Http;

namespace NutriScan.Services.Ocr
{
    public interface IOcrClient
    {
        Task<OcrResult> AnalyzeAsync(IFormFile file, CancellationToken cancellationToken);
    }
}
