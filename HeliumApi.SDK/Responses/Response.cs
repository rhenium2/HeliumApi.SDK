using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace HeliumApi.SDK.Responses;

public class Response
{
    [JsonProperty("data")]
    [JsonPropertyName("data")]
    public string Data { get; set; }

    [JsonProperty("cursor")]
    [JsonPropertyName("cursor")]
    public string Cursor { get; set; }
}