using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace HeliumApi.SDK.Test;

public class GenericResponse<T>
{
    [JsonProperty("data")]
    [JsonPropertyName("data")]
    public T Data { get; set; }

    [JsonProperty("cursor")]
    [JsonPropertyName("cursor")]
    public string Cursor { get; set; }
}