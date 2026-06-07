namespace NutriScan.Services.FoodValidation
{
    public interface IFoodValidateClient
    {
        Task<FoodValidationResult?> ValidateAsync(FoodValidationRequest request, CancellationToken cancellationToken);
    }
}
