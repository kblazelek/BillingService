using System.Text.Json.Serialization;

namespace BillingService.API.Models;

public record VehicleData(
    [property: JsonPropertyName("vin")] string Vin,
    [property: JsonPropertyName("licensePlate")] string LicensePlate,
    [property: JsonPropertyName("make")] string Make,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("state")] VehicleState State);
