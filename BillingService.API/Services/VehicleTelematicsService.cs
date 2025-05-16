using BillingService.API.Models;
using BillingService.API.Utils;

namespace BillingService.API.Services
{
    public interface IVehicleTelematicsService
    {
        Task<List<VehicleData>> GetVehiclesHistoryAsync(DateTime asAt);
    }

    public class VehicleTelematicsService : IVehicleTelematicsService
    {
        private readonly HttpClient _httpClient;

        public VehicleTelematicsService(HttpClient httpClient, IConfiguration configuration)
        {
            var baseUrl = configuration["VehicleTelematicsApiBaseUrl"] ??
                throw new ArgumentException("No base URL for the Vehicle Telematics API provided");
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<List<VehicleData>> GetVehiclesHistoryAsync(DateTime asAt)
        {
            var asAtDateTimeString = DateTimeConverter.ConvertDateTimeToString(asAt);
            string requestUri = $"api/vehicles/history/{asAtDateTimeString}";
            using var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode(); // Throw exception for bad status codes
            var responseBody = await response.Content.ReadFromJsonAsync<List<VehicleData>>();
            return responseBody ?? [];
        }
    }
}
