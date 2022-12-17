using Newtonsoft.Json;

namespace HeliumApi.SDK.Responses.Transactions;

public class Transaction
{
    [JsonProperty("type")] public string Type { get; set; }
}