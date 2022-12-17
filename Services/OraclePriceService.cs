using HeliumApi.SDK.Responses.OraclePrice;
using LocalObjectCache;
using Newtonsoft.Json;

namespace HeliumApi.SDK.Services;

public static class OraclePriceService
{
    public static async Task<OraclePrice> GetOraclePrice(int blockHeight)
    {
        var effectiveOraclePriceBlock = (blockHeight / 10) * 10;
        var cachedOraclePrice =
            Cache.Default.GetOne<OraclePrice>(
                x => x.Block.Equals(effectiveOraclePriceBlock));
        if (cachedOraclePrice != null)
        {
            return cachedOraclePrice;
        }

        var uri = $"/v1/oracle/prices/{blockHeight}";
        var allData = await HeliumClient.Get(uri);
        var price = JsonConvert.DeserializeObject<OraclePrice>(allData.First());
        Cache.Default.InsertOne<OraclePrice>(price);

        return price;
    }
}