using Microsoft.AspNetCore.Mvc;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Redirects
{
    public class CertusFinancRedirectRequest
    {
        [FromQuery(Name = "resultCode")] public string ResultCode { get; set; }
        [FromQuery(Name = "message")] public string Message { get; set; }
        [FromQuery(Name = "txId")] public string TransactionId { get; set; }
        [FromQuery(Name = "orderId")] public string OrderId { get; set; }
        [FromQuery(Name = "activityId")] public string ActivityId { get; set; }
        [FromQuery(Name = "sourceAmount")] public string SourceAmount { get; set; }
        [FromQuery(Name = "sourceCurrencyCode")] public string SourceCurrencyCode { get; set; }
        [FromQuery(Name = "amount")] public string Amount { get; set; }
        [FromQuery(Name = "currencyCode")] public string CurrencyCode { get; set; }
        [FromQuery(Name = "signature")] public string Signature { get; set; }

    }
}
