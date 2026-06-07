using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NutriScan.Services.Ocr
{
    public class PythonOcrClient : IOcrClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OcrOptions _options;
        private readonly ILogger<PythonOcrClient> _logger;

        public PythonOcrClient(
            IHttpClientFactory httpClientFactory,
            IOptions<OcrOptions> options,
            ILogger<PythonOcrClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<OcrResult> AnalyzeAsync(IFormFile file, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            using var content = new MultipartFormDataContent();
            await using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var client = _httpClientFactory.CreateClient();
            var endpoint = new Uri(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), _options.AnalyzePath.TrimStart('/'));

            try
            {
                var response = await client.PostAsync(endpoint, content, timeout.Token);
                var responseBody = await response.Content.ReadAsStringAsync(timeout.Token);

                if (!response.IsSuccessStatusCode)
                {
                    return new OcrResult
                    {
                        Success = false,
                        Error = $"OCR service returned {(int)response.StatusCode}"
                    };
                }

                return ParseOcrResponse(responseBody);
            }
            catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException or JsonException)
            {
                _logger.LogWarning(ex, "OCR analysis failed");
                return new OcrResult { Success = false, Error = ex.Message };
            }
        }

        private static OcrResult ParseOcrResponse(string responseBody)
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            var success = !root.TryGetProperty("success", out var successElement) || successElement.GetBoolean();

            var result = new OcrResult
            {
                Success = success,
                RawText = GetString(root, "raw_text") ?? GetString(root, "rawText") ?? "",
                Error = GetString(root, "error")
            };

            if (root.TryGetProperty("nutrition", out var nutrition))
            {
                result.Nutrition = new OcrNutrition
                {
                    Calories = (int)Math.Round(GetDouble(nutrition, "calories") ?? 0),
                    Fat = GetDouble(nutrition, "fat") ?? 0,
                    Carb = GetDouble(nutrition, "carb") ?? GetDouble(nutrition, "carbs") ?? 0,
                    Protein = GetDouble(nutrition, "protein") ?? 0,
                    Sugar = GetDouble(nutrition, "sugar"),
                    Sodium = GetDouble(nutrition, "sodium")
                };
            }

            return result;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
                ? value.GetString()
                : null;
        }

        private static double? GetDouble(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.Number when value.TryGetDouble(out var number) => number,
                JsonValueKind.String when double.TryParse(value.GetString(), out var number) => number,
                _ => null
            };
        }
    }
}
