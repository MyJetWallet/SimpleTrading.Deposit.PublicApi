using Destructurama.Attributed;
using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class RealDepositsCallbackRequest
    {
        [LogMasked(ShowFirst = 5, ShowLast = 5, PreserveLength = true)]
        [JsonProperty("application_key")] public string ApplicationKey { get; set; }
        [JsonProperty("transaction_type")] public string TransactionType { get; set; }
        [JsonProperty("transaction_status")] public string TransactionStatus { get; set; }
        [JsonProperty("transaction_id")] public string TransactionId { get; set; }
        [JsonProperty("trace_id")] public int TraceId { get; set; }
        [JsonProperty("variable1")] public object Variable1 { get; set; }
        [JsonProperty("variable2")] public object Variable2 { get; set; }
        [JsonProperty("pin")] public string Pin { get; set; }
        [LogMasked(ShowFirst = 5, ShowLast = 5, PreserveLength = true)]
        [JsonProperty("auth_token")] public string AuthToken { get; set; }
        [JsonProperty("created_by")] public object CreatedBy { get; set; }
        [JsonProperty("payment_processor")] public string PaymentProcessor { get; set; }
        [JsonProperty("payment_method")] public string PaymentMethod { get; set; }
        [JsonProperty("edited_by")] public object EditedBy { get; set; }
        [LogMasked(ShowFirst = 6, ShowLast = 4, PreserveLength = true)]
        [JsonProperty("card_number")] public string CardNumber { get; set; }
        [JsonProperty("card_type")] public string CardType { get; set; }
        [JsonProperty("card_exp")] public string CardExp { get; set; }
        [JsonProperty("account_identifier")] public object AccountIdentifier { get; set; }
        [JsonProperty("amount")] public int Amount { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; }
        [JsonProperty("charge_amount")] public int ChargeAmount { get; set; }
        [JsonProperty("charge_currency")] public string ChargeCurrency { get; set; }
        [JsonProperty("cascade_level")] public int CascadeLevel { get; set; }
        [JsonProperty("reference_id")] public object ReferenceId { get; set; }
        [LogMasked(ShowFirst = 5, ShowLast = 5, PreserveLength = true)]
        [JsonProperty("gateway")] public string Gateway { get; set; }
        [JsonProperty("order_id")] public string OrderId { get; set; }
        [JsonProperty("error_code")] public string ErrorCode { get; set; }
        [JsonProperty("error_details")] public string ErrorDetails { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("timestamp")] public long Timestamp { get; set; }
        [JsonProperty("merchant_id")] public string MerchantId { get; set; }
        [JsonProperty("signature")] public string Signature { get; set; }
    }
}