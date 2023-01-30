using Newtonsoft.Json;

namespace HeliumApi.SDK.Responses.Account;

public class AccountReward
{
    [JsonProperty("total")]
    public double Total { get; set; }

    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonProperty("sum")]
    public long Sum { get; set; }

    [JsonProperty("stddev")]
    public double Stddev { get; set; }

    [JsonProperty("min")]
    public double Min { get; set; }

    [JsonProperty("median")]
    public double Median { get; set; }

    [JsonProperty("max")]
    public double Max { get; set; }

    [JsonProperty("avg")]
    public double Avg { get; set; }
}