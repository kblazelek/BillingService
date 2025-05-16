using BillingService.API.Entities;

namespace BillingService.API.Repository
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetCustomerAsync(string name);
    }

    /// <summary>
    /// Mock repository that returns the <see cref="Customer"/> given a name
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        public Task<Customer?> GetCustomerAsync(string name)
        {
            if (name == "Bob's Taxis")
            {
                var customer = new Customer("Bob's Taxis", 0.207m, ["abcd123", "xyz12345"], "GBP");
                return Task.FromResult<Customer?>(customer);
            }
            return Task.FromResult<Customer?>(null);
        }
    }
}
