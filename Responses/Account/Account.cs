using Newtonsoft.Json;

namespace HeliumApi.SDK.Responses.Account;

public class Account
{
    [JsonProperty("validator_count")]
    public long ValidatorCount { get; set; }

    [JsonProperty("staked_balance")]
    public long StakedBalance { get; set; }

    [JsonProperty("speculative_sec_nonce")]
    public long SpeculativeSecNonce { get; set; }

    [JsonProperty("speculative_nonce")]
    public long SpeculativeNonce { get; set; }

    [JsonProperty("sec_nonce")]
    public long SecNonce { get; set; }

    [JsonProperty("sec_balance")]
    public long SecBalance { get; set; }

    [JsonProperty("nonce")]
    public long Nonce { get; set; }

    [JsonProperty("mobile_balance")]
    public long MobileBalance { get; set; }

    [JsonProperty("iot_balance")]
    public long IotBalance { get; set; }

    [JsonProperty("hotspot_count")]
    public long HotspotCount { get; set; }

    [JsonProperty("dc_nonce")]
    public long DcNonce { get; set; }

    [JsonProperty("dc_balance")]
    public long DcBalance { get; set; }

    [JsonProperty("block")]
    public long Block { get; set; }

    [JsonProperty("balance")]
    public long Balance { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }
}