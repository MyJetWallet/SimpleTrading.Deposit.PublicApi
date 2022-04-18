using Destructurama.Attributed;
using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class OctaPayCallbackRequest
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("customer_order_id")]
        public string CustomerOrderId { get; set; }

        [JsonProperty("transaction_status")]
        public string TransactionStatus { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("test")]
        public bool Test { get; set; }

        [JsonProperty("transaction_date")]
        public string TransactionDate { get; set; }

    }
}
