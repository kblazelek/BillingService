
namespace BillingService.API.Models
{
    public class VehicleCost
    {
        public required string Vin { get; set; }
        public required decimal MilesCovered { get; set; }
        public required decimal Cost { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is VehicleCost cost &&
                   Vin == cost.Vin &&
                   MilesCovered == cost.MilesCovered &&
                   Cost == cost.Cost;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Vin, MilesCovered, Cost);
        }
    }
}
