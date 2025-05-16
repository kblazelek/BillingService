using System.Text.Json.Serialization;

namespace BillingService.API.Models;

public record VehicleState(
    [property: JsonPropertyName("odometerInMeters")] decimal OdometerInMeters,
    [property: JsonPropertyName("speedInMph")] float SpeedInMph,
    [property: JsonPropertyName("asAt")] DateTime AsAt);
