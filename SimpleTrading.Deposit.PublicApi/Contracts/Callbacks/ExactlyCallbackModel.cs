using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class ExactlyCallbackModel
    {
        [JsonProperty("webhook")] public WebhookData WebhookData { get; set; }
        [JsonProperty("meta")] public MetaData MetaData { get; set; }
        [JsonProperty("data")] public Data Data { get; set; }
    }

    public class WebhookData
    {
        [JsonProperty("type")] public string Type { get; set; }
    }

    public class MetaData
    {
        [JsonProperty("server_time")] public long ServerTime { get; set; }
        [JsonProperty("server_timezone")] public string ServerTimezone { get; set; }
        [JsonProperty("api_version")] public string ApiVersion { get; set; }
    }

    public class Data
    {
        [JsonProperty("charge")] public ChargeData Charge { get; set; }
    }

    public class ChargeData
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("id")] public string TransactionId { get; set; }
        [JsonProperty("attributes")] public AttributesData Attributes { get; set; }
    }

    public class AttributesData
    {
        [JsonProperty("livemode")] public bool LiveMode { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("amount")] public double Amount { get; set; }
        [JsonProperty("paid_amount")] public double? PaidAmount { get; set; }
        [JsonProperty("fee")] public double? Fee { get; set; }
        [JsonProperty("rolling")] public double? Rolling { get; set; }
        [JsonProperty("total_amount")] public double? TotalAmount { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; }
        [JsonProperty("pay_method")] public string PayMethod { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("source")] public SourceData SourceData { get; set; }
        [JsonProperty("failure")] public FailureData? Failure { get; set; }
        [JsonProperty("reference_id")] public string? ReferenceId { get; set; }
        [JsonProperty("created_at")] public string? CreatedAt { get; set; }
        [JsonProperty("updated_at")] public string? UpdatedAt { get; set; }
        [JsonProperty("valid_till")] public string? ValidTill { get; set; }
    }

    public class SourceData
    {
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("ip_address")] public string IpAddress { get; set; }
        [JsonProperty("country_code")] public string CountryCode { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("card_number")] public string CardNumber { get; set; }
        [JsonProperty("brand")] public string Brand { get; set; }
        [JsonProperty("issuer_country_code")] public string IssuerCountyCode { get; set; }
        [JsonProperty("issuer_name")] public string IssuerName { get; set; }
    }

    public class FailureData
    {
        [JsonProperty("code")] public string Code { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
    }
}