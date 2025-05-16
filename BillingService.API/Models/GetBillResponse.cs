namespace BillingService.API.Models
{
    public class GetBillResponse
    {
        public required DateTime FromDate { get; set; }
        public required DateTime ToDate { get; set; }
        public required List<VehicleCost> CostPerVehicle { get; set; }
        public required string CurrencyCode { get; set; }
    }
}
