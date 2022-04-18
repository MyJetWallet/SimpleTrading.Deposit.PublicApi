using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class SwiffyCallback
    {
        [FromForm(Name ="success")] public int Success { get; set; }
        [FromForm(Name = "Reason")] public string Reason { get; set; }

        [FromForm(Name = "organisation_id")] public string OrganisationId { get; set; }
        [FromForm(Name = "amount")] public string Amount { get; set; }
        [FromForm(Name = "refunded_amount")] public string RefundedAmount { get; set; }
        [FromForm(Name = "callpay_transaction_id")] public string CallpayTransactionId { get; set; }
        [FromForm(Name = "user")] public string User { get; set; }
        [FromForm(Name = "merchant_reference")] public string MerchantReference { get; set; }
        [FromForm(Name = "gateway_reference")] public string GatewayReference { get; set; }
        [FromForm(Name = "gateway_response")] public string GatewayResponse { get; set; }
        [FromForm(Name = "currency")] public string Currency { get; set; }
        [FromForm(Name = "payment_key")] public string PaymentKey { get; set; }

        [JsonIgnore]
        public bool IsSuccess => Success == 1;

        [JsonIgnore]
        public string Status => IsSuccess ? "Success" : "Failed";
    }
}