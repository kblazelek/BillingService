namespace BillingService.API.Entities
{
    public class Customer
    {
        public string Name { get; set; }
        public decimal CostPerMile { get; set; }
        public string CurrencyCode { get; set; }
        public List<string> Vins { get; set; }

        public Customer(string name, decimal costPerMile, List<string> vins, string currencyCode)
        {
            Name = name;
            CostPerMile = costPerMile;
            Vins = vins;
            CurrencyCode = currencyCode;
        }
    }
}
