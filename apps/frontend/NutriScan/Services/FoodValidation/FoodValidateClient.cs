using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NutriScan.Services.FoodValidation
{
    public class FoodValidateClient : IFoodValidateClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FoodValidateOptions _options;
        private readonly ILogger<FoodValidateClient> _logger;

        public FoodValidateClient(
            IHttpClientFactory httpClientFactory,
            IOptions<FoodValidateOptions> options,
            ILogger<FoodValidateClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<FoodValidationResult?> ValidateAsync(
            FoodValidationRequest request,
            CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                return null;
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var endpoint = new Uri(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), _options.ValidatePath.TrimStart('/'));
            var json = JsonSerializer.Serialize(request, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(endpoint, content, timeout.Token);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("FoodValidate returned {StatusCode}", (int)response.StatusCode);
                    return null;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
                return await JsonSerializer.DeserializeAsync<FoodValidationResult>(stream, JsonOptions, timeout.Token);
            }
            catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException or JsonException)
            {
                _logger.LogWarning(ex, "FoodValidate failed; scan will continue as unverified");
                return null;
            }
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
}
