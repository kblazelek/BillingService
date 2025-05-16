using BillingService.API.Models;
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
                return BadRequest(new ErrorResponse()
                {
                    Message = $"Parameter '{nameof(customerName)}' is required.",
                    StatusCode = 400
                });
            }

            if (!fromDateTime.HasValue || !toDateTime.HasValue)
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = $"Parameters '{nameof(fromDateTime)}' and '{nameof(toDateTime)}' are required.",
                    StatusCode = 400
                });
            }

            if (fromDateTime > toDateTime)
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = $"'fromDateTime' cannot be after 'toDateTime'.",
                    StatusCode = 400
                });
            }

            try
            {
                // In PROD environment the horizontal permissions should be checked
                var customer = await _customerRepository.GetCustomerAsync(customerName);
                if (customer == null)
                {
                    return NotFound(new ErrorResponse()
                    {
                        Message = $"Customer with name {customerName} does not exist.",
                        StatusCode = 404
                    });
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
                    return StatusCode(500, new ErrorResponse()
                    {
                        Message = "Internal server error.",
                        StatusCode = 500
                    });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a bill for customer {0}. FromDateTime: {1}, ToDateTime: {2}",
                    customerName, fromDateTime, toDateTime);
                return StatusCode(503, new ErrorResponse()
                {
                    Message = "Service unavailable.",
                    StatusCode = 503
                });
            }
        }
    }
}
