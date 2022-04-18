using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class PayopCallbackRequest
    {
        [JsonProperty("invoice")] public Invoice Invoice { get; set; }
        [JsonProperty("transaction")] public Transaction Transaction { get; set; }
    }

    public class Invoice
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("status")] public int Status { get; set; }
        [JsonProperty("txid")] public string Txid { get; set; }
        [JsonProperty("metadata")] public object Metadata { get; set; }

        public bool IsSuccess => Status == 1;
        public bool IsPending => Status == 4 || Status == 0;
        public bool IsFailed => !IsSuccess && !IsPending;
    }

    public class Transaction
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("state")] public int State { get; set; }
        [JsonProperty("order")] public Order Order { get; set; }
        [JsonProperty("error")] public Error Error { get; set; }

        public bool IsSuccess => State == 2;
        public bool IsPending => State == 4 || State == 1;
        public bool IsFailed => !IsSuccess && !IsPending;
    }

    public class Order
    {
        [JsonProperty("id")] public string Id { get; set; }
    }

    public class Error
    {
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("code")] public string Code { get; set; }
    }

}