using Newtonsoft.Json;

namespace HeliumApi.SDK.Responses.OraclePrice;

public class OraclePrice
{
    [JsonProperty("block")] public int Block { get; set; }

    [JsonProperty("price")] public int Price { get; set; }
}