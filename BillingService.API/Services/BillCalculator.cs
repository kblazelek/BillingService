using BillingService.API.Entities;
using BillingService.API.Models;
using System.Linq;

namespace BillingService.API.Services
{
    public interface IBillCalculator
    {
        bool TryCalculateBill(
            DateTime fromDate,
            DateTime toDate,
            List<VehicleData> vehicleDataBefore,
            List<VehicleData> vehicleDataAfter,
            Customer customer,
            out GetBillResponse? getBillResponse);
    }

    public class BillCalculator : IBillCalculator
    {
        private const decimal MetersPerMile = 1609.344m;
        private readonly ILogger<BillCalculator> _logger;

        public BillCalculator(ILogger<BillCalculator> logger)
        {
            _logger = logger;
        }

        public bool TryCalculateBill(
            DateTime fromDate,
            DateTime toDate,
            List<VehicleData> vehicleDataBefore,
            List<VehicleData> vehicleDataAfter,
            Customer customer,
            out GetBillResponse? getBillResponse)
        {
            if (vehicleDataBefore == null || vehicleDataAfter == null || customer == null)
            {
                getBillResponse = default;
                return false;
            }

            // Customer has no vehicles
            if (!customer.Vins.Any())
            {
                getBillResponse = new GetBillResponse()
                {
                    CostPerVehicle = [],
                    CurrencyCode = customer.CurrencyCode,
                    FromDate = fromDate,
                    ToDate = toDate,
                };
                return true;
            }

            Dictionary<string, VehicleCost> vinToVehicleCost = customer.Vins.ToDictionary(x => x, x => new VehicleCost()
            {
                Vin = x,
                MilesCovered = 0,
                Cost = 0
            });

            // Process readings after
            foreach (var vehicleData in vehicleDataAfter)
            {
                if (vinToVehicleCost.ContainsKey(vehicleData.Vin))
                {
                    vinToVehicleCost[vehicleData.Vin] = new VehicleCost()
                    {
                        Vin = vehicleData.Vin,
                        MilesCovered = vehicleData.State.OdometerInMeters / MetersPerMile,
                        Cost = 0,
                    };
                }
            }

            // Subtract readings before
            foreach (var vehicleData in vehicleDataBefore)
            {
                if (vinToVehicleCost.TryGetValue(vehicleData.Vin, out var vehicleCost))
                {
                    var milesBefore = vehicleData.State.OdometerInMeters / MetersPerMile;
                    vehicleCost.MilesCovered -= milesBefore;
                    if (vehicleCost.MilesCovered < 0)
                    {
                        _logger.LogError(
                            "Error while calculating bill for vehicle with vin {vin}: The miles covered were negative",
                            vehicleData.Vin);
                        getBillResponse = default;
                        return false;
                    }
                    vehicleCost.Cost = vehicleCost.MilesCovered * customer.CostPerMile;
                }
            }

            getBillResponse = new GetBillResponse()
            {
                CostPerVehicle = vinToVehicleCost.Values.ToList(),
                CurrencyCode = customer.CurrencyCode,
                FromDate = fromDate,
                ToDate = toDate,
            };
            return true;
        }
    }
}
