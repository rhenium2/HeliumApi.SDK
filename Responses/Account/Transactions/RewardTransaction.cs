using Newtonsoft.Json;

namespace HeliumApi.SDK.Responses.Account.Transactions;

public class RewardTransaction
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("timestamp")] public DateTime TimeStamp { get; set; }
        
    [JsonProperty("hash")] public string Hash { get; set; }

    [JsonProperty("gateway")] public string Gateway { get; set; }
        
    [JsonProperty("block")] public int Block { get; set; }

    [JsonProperty("amount")] public int Amount { get; set; }

    [JsonProperty("account")] public string Account { get; set; }
}