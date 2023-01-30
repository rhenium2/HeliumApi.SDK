using System.Globalization;
using HeliumApi.SDK.Helpers;
using HeliumApi.SDK.Responses;
using HeliumApi.SDK.Responses.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace HeliumApi.SDK.Services;

public static class HotspotService
{
    public static async Task<Hotspot> GetHotspot(string hotspotId)
    {
        var cachedHotspot = CacheHelper.GetOne<Hotspot>(x => x.Address.Equals(hotspotId));
        if (cachedHotspot != null)
        {
            return cachedHotspot;
        }

        var uri = $"/v1/hotspots/{hotspotId}";
        var data = await HeliumClient.Get(uri);
        var hotspot = JsonConvert.DeserializeObject<Hotspot>(data.First());
        CacheHelper.InsertOne<Hotspot>(hotspot);

        return hotspot;
    }

    public static async Task<Hotspot> GetHotspotByName(string name)
    {
        var cachedHotspot = CacheHelper.GetOne<Hotspot>(x => x.Name.Equals(name));
        if (cachedHotspot != null)
        {
            return cachedHotspot;
        }

        var uri = $"/v1/hotspots/name/{name}";
        var data = await HeliumClient.Get(uri);
        var hotspots = HeliumHelpers.DeserializeAll<Hotspot>(data.ToArray());
        if (hotspots.Count > 1)
        {
            throw new Exception("There are more than one hotspot for this name. Please use hotspot address instead.");
        }

        var hotspot = hotspots.First();
        CacheHelper.InsertOne<Hotspot>(hotspot);
        return hotspot;
    }

    /// <summary>
    /// Retrieves the list of hotspots the given hotspot witnessed over the last 5 days.
    /// </summary>
    /// <param name="hotspotId"></param>
    /// <returns></returns>
    public static async Task<List<Hotspot>> GetWitnessed(string hotspotId)
    {
        var uri = $"/v1/hotspots/{hotspotId}/witnessed";
        var allData = await HeliumClient.Get(uri);
        return HeliumHelpers.DeserializeAll<Hotspot>(allData.ToArray());
    }

    public static async Task<List<HotspotRolesResponse>> GetRoles(string hotspotId)
    {
        var uri = $"/v1/hotspots/{hotspotId}/roles";
        var allData = await HeliumClient.Get(uri);
        return HeliumHelpers.DeserializeAll<HotspotRolesResponse>(allData.ToArray());
    }

    public static async Task<Transaction> GetTransaction(string transactionHash)
    {
        var uri = $"/v1/transactions/{transactionHash}";
        var allData = await HeliumClient.Get(uri);
        var firstData = allData.First();
        return HeliumHelpers.DeserializeTransaction(firstData);
    }

    /// <summary>
    /// Lists the challenge (receipts) that the given hotspot was a challenger, challengee or witness in
    /// </summary>
    /// <param name="hotspotId"></param>
    /// <param name="minTime"></param>
    /// <returns></returns>
    public static async Task<List<PocReceiptsV2Transaction>> GetChallenges(string hotspotId, DateTime? minTime)
    {
        var uri = $"/v1/hotspots/{hotspotId}/challenges";
        if (minTime.HasValue)
        {
            uri += "?min_time=" + minTime.Value.ToString("o", CultureInfo.InvariantCulture);
        }

        var allData = await HeliumClient.Get(uri);
        return HeliumHelpers.DeserializeAll<PocReceiptsV2Transaction>(allData.ToArray());
    }

    public static async Task<List<PocReceiptsV2Transaction>> GetNetworkChallenges(DateTime? minTime)
    {
        var minTimestamp = ((DateTimeOffset)minTime).ToUnixTimeSeconds();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var cachedChallenges = CacheHelper.GetMany<PocReceiptsV2Transaction>(x => x.Time >= minTimestamp)
            .OrderBy(x => x.Time).ToList();
        if (cachedChallenges.Any())
        {
            var firstItemTime = cachedChallenges.Min(x => x.Time);
            if (firstItemTime > minTimestamp)
            {
                var topMissing = await RetrieveNetworkChallenges(minTimestamp, firstItemTime);
                var uniqueTopMissing = topMissing.ExceptBy(cachedChallenges.Select(x => x.Hash), x => x.Hash).ToList();
                CacheHelper.InsertMany(uniqueTopMissing);
                cachedChallenges.AddRange(uniqueTopMissing);
            }

            var lastItemTime = cachedChallenges.Max(x => x.Time);
            if (lastItemTime < now)
            {
                var bottomMissing = await RetrieveNetworkChallenges(lastItemTime, now);
                var uniqueBottomMissing =
                    bottomMissing.ExceptBy(cachedChallenges.Select(x => x.Hash), x => x.Hash).ToList();
                CacheHelper.InsertMany(uniqueBottomMissing);
                cachedChallenges.AddRange(uniqueBottomMissing);
            }

            return cachedChallenges;
        }

        var challenges = await RetrieveNetworkChallenges(minTimestamp);
        CacheHelper.InsertMany(challenges);
        return challenges;
    }

    private static async Task<List<PocReceiptsV2Transaction>> RetrieveNetworkChallenges(long minTime,
        long? maxTime = null)
    {
        var minTimeText = HeliumHelpers.UnixTimeStampToDateTime(minTime);
        var uri = $"/v1/challenges?min_time={minTimeText.ToString("o", CultureInfo.InvariantCulture)}";
        if (maxTime.HasValue)
        {
            var maxTimeText = HeliumHelpers.UnixTimeStampToDateTime(maxTime.Value);
            uri += "&max_time=" + maxTimeText.ToString("o", CultureInfo.InvariantCulture);
        }

        var allDataStrings = await HeliumClient.Get(uri);
        var transactions = HeliumHelpers.DeserializeAll<PocReceiptsV2Transaction>(allDataStrings.ToArray());
        return transactions;
    }

    public static async Task<List<PocReceiptsV2Transaction>> GetBeaconTransactions(string hotspotId, DateTime? minTime)
    {
        var uri = $"/v1/hotspots/{hotspotId}/challenges";
        if (minTime.HasValue)
        {
            uri += "?min_time=" + minTime.Value.ToString("o", CultureInfo.InvariantCulture);
        }

        var allData = await HeliumClient.Get(uri);
        var tt = HeliumHelpers.DeserializeAll<PocReceiptsV2Transaction>(allData.ToArray());
        return tt.Where(t => t.Path.First().Challengee.Equals(hotspotId)).ToList();
    }

    public static async Task<List<Transaction>> GetBlockTransactions(int blockHeight)
    {
        var uri = $"/v1/blocks/{blockHeight}/transactions";

        var allData = await HeliumClient.Get(uri);
        var tt = HeliumHelpers.DeserializeAll<JObject>(allData.ToArray());
        return tt.Select(t => HeliumHelpers.DeserializeTransaction(t.ToString())).ToList();
    }

    public static async Task<List<PocReceiptsV2Transaction>> GetWitnessedTransactions(string hotspotId,
        DateTime? minTime)
    {
        var uri = $"/v1/hotspots/{hotspotId}/challenges";
        if (minTime.HasValue)
        {
            uri += "?min_time=" + minTime.Value.ToString("o", CultureInfo.InvariantCulture);
        }

        var allData = await HeliumClient.Get(uri);
        var tt = HeliumHelpers.DeserializeAll<PocReceiptsV2Transaction>(allData.ToArray());
        return tt.Where(c => c.Path.Any()
                             && c.Path.First().Witnesses
                                 .Any(x => x.IsValid && x.Gateway.Equals(hotspotId)))
            .OrderByDescending(x => x.Time)
            .ToList();
    }

    public static async Task<List<Hotspot>> GetHotspotsByRadius(double lat, double lng, double radiusKm)
    {
        var uri = $"/v1/hotspots/location/distance?lat={lat}&lon={lng}&distance={radiusKm * 1000}";
        var allData = await HeliumClient.Get(uri);
        return HeliumHelpers.DeserializeAll<Hotspot>(allData.ToArray());
    }

    public static async Task<List<Hotspot>> GetHotspotsByBox(double swLat, double swLon, double neLat, double neLon)
    {
        var uri = $"/v1/hotspots/location/box?swlat={swLat}&swlon={swLon}&nelat={neLat}&nelon={neLon}";
        var allData = await HeliumClient.Get(uri);
        return HeliumHelpers.DeserializeAll<Hotspot>(allData.ToArray());
    }
}