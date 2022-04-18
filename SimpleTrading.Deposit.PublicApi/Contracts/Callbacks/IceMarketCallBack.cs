using Microsoft.AspNetCore.Mvc;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class IceMarketCallBack
    {
        [FromForm(Name = "transaction_id")] public string TransactionId { get; set; }
        [FromForm(Name = "status")] public string Status { get; set; }
        [FromForm(Name = "response_code")] public string ResponseCode { get; set; }
        [FromForm(Name = "paid_amount")] public string PaidAmount { get; set; }
    }
}