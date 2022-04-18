using Newtonsoft.Json;
using SimpleTrading.Deposit.PublicApi.Extentions;

namespace SimpleTrading.Deposit.PublicApi.Contracts.ABSplits
{
    public class ABSplit
    {
        [JsonProperty("ABSplitName")] public string Name;
        [JsonProperty("ABSplitGroupType")] public string Type;
    };
}