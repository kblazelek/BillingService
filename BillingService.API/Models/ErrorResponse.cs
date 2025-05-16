namespace BillingService.API.Models
{
    public class ErrorResponse
    {
        public required string Message { get; set; }
        public required int StatusCode { get; set; }
    }
}
