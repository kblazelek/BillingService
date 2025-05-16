using BillingService.API.Repository;
using BillingService.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.API.Controllers
{
    [ApiController]
    [Route("api/bills")]
    public class BillsController : ControllerBase
    {
        private readonly ILogger<BillsController> _logger;
        private readonly IVehicleTelematicsService _vehicleTelematicsService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IBillCalculator _billCalculator;

        public BillsController(
            ILogger<BillsController> logger,
            IVehicleTelematicsService vehicleTelematicsService,
            ICustomerRepository customerRepository,
            IBillCalculator billCalculator)
        {
            _logger = logger;
            _vehicleTelematicsService = vehicleTelematicsService;
            _customerRepository = customerRepository;
            _billCalculator = billCalculator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string customerName,
            [FromQuery] DateTime? fromDateTime,
            [FromQuery] DateTime? toDateTime)
        {
            if (string.IsNullOrEmpty(customerName))
            {
                return BadRequest($"Parameter '{nameof(customerName)}' is required.");
            }

            if (!fromDateTime.HasValue || !toDateTime.HasValue)
            {
                return BadRequest($"Parameters '{nameof(fromDateTime)}' and '{nameof(toDateTime)}' are required.");
            }

            if (fromDateTime > toDateTime)
            {
                return BadRequest("'fromDateTime' cannot be after 'toDateTime'.");
            }

            try
            {
                var customer = await _customerRepository.GetCustomerAsync(customerName);
                if (customer == null)
                {
                    return NotFound();
                }
                var vehicleHistoryBefore = await _vehicleTelematicsService.GetVehiclesHistoryAsync(fromDateTime.Value);
                var vehicleHistoryAfter = await _vehicleTelematicsService.GetVehiclesHistoryAsync(toDateTime.Value);
                if (!_billCalculator.TryCalculateBill(
                    fromDateTime.Value,
                    toDateTime.Value,
                    vehicleHistoryBefore,
                    vehicleHistoryAfter,
                    customer,
                    out var response))
                {
                    return StatusCode(500, "Internal server error.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a bill for customer {0}. FromDateTime: {1}, ToDateTime: {2}",
                    customerName, fromDateTime, toDateTime);
                return StatusCode(503, "Service unavailable.");
            }
        }
    }
}
