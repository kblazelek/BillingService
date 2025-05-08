using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers
{
    [ApiController]
    [Route("bills")]
    public class BillsController : ControllerBase
    {
        private readonly ILogger<BillsController> _logger;

        public BillsController(ILogger<BillsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
