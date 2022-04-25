using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class DirectaCallback
    {
        [JsonProperty("deposit_id")] public string PsTransactionId { get; set; }
    }
}
