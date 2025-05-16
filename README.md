# BillingService
Billing service for the fleet of vehicles

## Usage
Example API call:
https://localhost:7017/api/bills?fromDateTime=2021-02-01T00:00:00Z&toDateTime=2021-02-28T23:59:00Z&customerName=Bob's Taxis

Expected result:
{
    "fromDate": "2021-02-01T00:00:00Z",
    "toDate": "2021-02-28T23:59:00Z",
    "costPerVehicle": [
        {
            "vin": "abcd123",
            "milesCovered": 99.99975145152310506641215303,
            "cost": 20.699948550465282748747315677
        },
        {
            "vin": "xyz12345",
            "milesCovered": 2000,
            "cost": 414.000
        }
    ],
    "currencyCode": "GBP"
}