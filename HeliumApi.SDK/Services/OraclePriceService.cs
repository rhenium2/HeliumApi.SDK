using HeliumApi.SDK.Helpers;
using HeliumApi.SDK.Responses.OraclePrice;
using Newtonsoft.Json;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace HeliumApi.SDK.Services;

public static class OraclePriceService
{
    public static async Task<OraclePrice> GetOraclePrice(int blockHeight)
    {
        var effectiveOraclePriceBlock = (blockHeight / 10) * 10;

        var cachedOraclePrice =
            CacheHelper.GetOne<OraclePrice>(
                x => x.Block.Equals(effectiveOraclePriceBlock));
        if (cachedOraclePrice != null)
        {
            return cachedOraclePrice;
        }

        var uri = $"/v1/oracle/prices/{blockHeight}";
        var allData = await HeliumClient.Get(uri);
        var price = JsonConvert.DeserializeObject<OraclePrice>(allData.First());
        CacheHelper.InsertOne<OraclePrice>(price);

        return price;
    }
}