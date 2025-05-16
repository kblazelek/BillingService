using System.Net;
using BillingService.API.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using BillingService.API.Services;
using Microsoft.Extensions.DependencyInjection;
using BillingService.API.Utils;
using System.Net.Http.Json;
using BillingService.API.Models;
using BillingService.API.Repository;
using BillingService.API.Entities;

namespace BillingService.API.Tests
{
    public class BillsControllerTests
    {
        // Test data
        private readonly VehicleData _scodaData;
        private readonly VehicleData _volkswagenData;
        private readonly VehicleData _toyotaData;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Customer _existingCustomer;

        public BillsControllerTests()
        {
            _scodaData = new VehicleData(
                "ABCDEF123456",
                "PO1 ABC",
                "Skoda",
                "Octavia",
                new VehicleState(150000.50m, 65.2f, new DateTime(2025, 5, 8, 17, 50, 0, DateTimeKind.Utc)));

            _volkswagenData = new VehicleData(
                "GHIJKL789012",
                "KR2 XYZ",
                "Volkswagen",
                "Golf",
                new VehicleState(85230.75m, 30.8f, new DateTime(2025, 5, 8, 17, 51, 30, DateTimeKind.Utc)));

            _toyotaData = new VehicleData(
                "MNOPQR345678",
                "WA3 123",
                "Toyota",
                "Yaris",
                new VehicleState(210500.00m, 45.5f, new DateTime(2025, 5, 8, 17, 52, 15, DateTimeKind.Utc)));

            _existingCustomer = new Customer("Bob's Taxis", 0.1m, [_scodaData.Vin, _volkswagenData.Vin], "GBP");
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _customerRepositoryMock.Setup(x => x.GetCustomerAsync(_existingCustomer.Name)).ReturnsAsync(_existingCustomer);
        }

        [Fact]
        public async Task RetrievesVehicleDataSuccessfully()
        {
            // Arrange
            var vehiclesDataBefore = new List<VehicleData>
            {
                _scodaData,
                _volkswagenData,
                _toyotaData
            };

            var newScodaState = _scodaData.State with { OdometerInMeters = _scodaData.State.OdometerInMeters + 1609344 };

            var vehiclesDataAfter = new List<VehicleData>
            {
                _scodaData with { State = newScodaState },
                _volkswagenData,
                _toyotaData
            };

            var expectedScodaCost = new VehicleCost()
            {
                Cost = 100,
                MilesCovered = 1000,
                Vin = _scodaData.Vin
            };

            var expectedVolkswagenCost = new VehicleCost()
            {
                Cost = 0m,
                MilesCovered = 0,
                Vin = _volkswagenData.Vin
            };

            var vehicleTelematicsServiceMock = new Mock<IVehicleTelematicsService>();

            var webApplicationFactory = new WebApplicationFactory<BillsController>();
            var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    var srv = ((ServiceCollection)services);
                    srv.AddSingleton(vehicleTelematicsServiceMock.Object);
                    srv.AddSingleton(_customerRepositoryMock.Object);
                }))
                .CreateClient();
            var fromDateTime = new DateTime(2021, 2, 1);
            var toDateTime = new DateTime(2022, 2, 1);
            var fromDateTimeUrlEncoded = DateTimeConverter.ConvertDateTimeToString(fromDateTime);
            var toDateTimeUrlEncoded = DateTimeConverter.ConvertDateTimeToString(toDateTime);
            vehicleTelematicsServiceMock.Setup(x => x.GetVehiclesHistoryAsync(fromDateTime)).Returns(Task.FromResult(vehiclesDataBefore));
            vehicleTelematicsServiceMock.Setup(x => x.GetVehiclesHistoryAsync(toDateTime)).Returns(Task.FromResult(vehiclesDataAfter));

            // Act
            using var response =
                await client.GetAsync($"/api/bills?fromDateTime={fromDateTimeUrlEncoded}&toDateTime={toDateTimeUrlEncoded}&customerName={_existingCustomer.Name}");
            var getBillResponse = await response.Content.ReadFromJsonAsync<GetBillResponse>();

            // Assert
            Assert.NotNull(getBillResponse);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal([expectedScodaCost, expectedVolkswagenCost], getBillResponse.CostPerVehicle);
        }
    }
}