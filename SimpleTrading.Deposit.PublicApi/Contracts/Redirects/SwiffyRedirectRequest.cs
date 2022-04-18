using Microsoft.AspNetCore.Mvc;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Redirects
{
    public class SwiffyRedirectRequest
    {
        [FromQuery(Name = "success")] public bool Success { get; set; }
        [FromQuery(Name = "merchant_reference")] public string MerchantReference { get; set; }
        [FromQuery(Name = "gateway_reference")] public string GatewayReference { get; set; }
        [FromQuery(Name = "transaction_id")] public string TransactionId { get; set; }
        [FromQuery(Name = "organisation_id")] public string OrganisationId { get; set; }
        [FromQuery(Name = "payment_key")] public string PaymentKey { get; set; }
        [FromQuery(Name = "reason")] public string Reason { get; set; }
        [FromQuery(Name = "src")] public string Source { get; set; }
        [FromQuery(Name = "activityId")] public string ActivityId { get; set; }
    }
}
