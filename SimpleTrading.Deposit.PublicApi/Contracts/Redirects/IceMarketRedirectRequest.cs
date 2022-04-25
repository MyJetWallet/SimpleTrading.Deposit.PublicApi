using Microsoft.AspNetCore.Mvc;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Redirects
{
    public class IceMarketRedirectRequest
    {
        [FromQuery(Name = "status")] public string Status { get; set; }
        [FromQuery(Name = "transaction_id")] public string TransactionId { get; set; }
    }
}