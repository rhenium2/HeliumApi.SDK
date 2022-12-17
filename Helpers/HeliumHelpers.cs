using GeoCoordinatePortable;
using HeliumApi.SDK.Responses;
using HeliumApi.SDK.Responses.Transactions;
using Newtonsoft.Json;

namespace HeliumApi.SDK.Helpers;

public static class HeliumHelpers
{
    public static List<T> DeserializeAll<T>(params string[] jsonStrings)
    {
        var result = new List<T>();
        foreach (var jsonString in jsonStrings)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                continue;
            }

            // var settings = new JsonSerializerSettings();
            // settings.ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };
            //var collection = JsonConvert.DeserializeObject<List<T>>(jsonString, settings);
            var collection = JsonConvert.DeserializeObject<List<T>>(jsonString);
            result.AddRange(collection);
        }

        return result;
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }

    public static string GetRelativeTimeString(int UtcSeconds)
    {
        var dateTime = UnixTimeStampToDateTime(UtcSeconds);
        var timeSpan = DateTime.UtcNow - dateTime;
        var timeSpanText = GetTimeSpanString(timeSpan);

        return $"{timeSpanText} ago";
    }

    public static string GetTimeSpanString(TimeSpan timeSpan)
    {
        (string unit, int value) = new Dictionary<string, int>
        {
            { "year(s)", (int)(timeSpan.TotalDays / 365.25) }, //https://en.wikipedia.org/wiki/Year#Intercalation
            { "month(s)", (int)(timeSpan.TotalDays / 29.53) }, //https://en.wikipedia.org/wiki/Month
            { "day(s)", (int)timeSpan.TotalDays },
            { "hour(s)", (int)timeSpan.TotalHours },
            { "minute(s)", (int)timeSpan.TotalMinutes },
            { "second(s)", (int)timeSpan.TotalSeconds },
            { "millisecond(s)", (int)timeSpan.TotalMilliseconds }
        }.First(kvp => kvp.Value > 0);

        return $"{value} {unit}";
    }

    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK");
    }
    
    public static Transaction DeserializeTransaction(string transactionString)
    {
        var transaction = JsonConvert.DeserializeObject<Transaction>(transactionString);
        switch (transaction.Type)
        {
            case Constants.TransactionType.PocReceiptsV2:
                return JsonConvert.DeserializeObject<PocReceiptsV2Transaction>(transactionString);
            case Constants.TransactionType.RewardsV2:
                return JsonConvert.DeserializeObject<RewardsV2Transaction>(transactionString);
            case Constants.TransactionType.StateChannelOpen:
                return JsonConvert.DeserializeObject<StateChannelOpenTransaction>(transactionString);
            case Constants.TransactionType.StateChannelClose:
                return JsonConvert.DeserializeObject<StateChannelCloseTransaction>(transactionString);
            case Constants.TransactionType.ValidatorHearbeat:
                return JsonConvert.DeserializeObject<ValidatorHearbeatTransaction>(transactionString);
            case Constants.TransactionType.PriceOracle:
                return JsonConvert.DeserializeObject<PriceOracleTransaction>(transactionString);
            case Constants.TransactionType.AddGateway:
                return JsonConvert.DeserializeObject<AddGatewayTransaction>(transactionString);
            case Constants.TransactionType.AssertLocationV2:
                return JsonConvert.DeserializeObject<AssertLocationV2Transaction>(transactionString);
            case Constants.TransactionType.Payment:
                return JsonConvert.DeserializeObject<PaymentTransaction>(transactionString);
            case Constants.TransactionType.PaymentV2:
                return JsonConvert.DeserializeObject<PaymentV2Transaction>(transactionString);
            case Constants.TransactionType.TransferHotspotV2:
                return JsonConvert.DeserializeObject<TransferHotspotV2Transaction>(transactionString);
            case Constants.TransactionType.Routing:
                return JsonConvert.DeserializeObject<RoutingTransaction>(transactionString);
            case Constants.TransactionType.ConsensusGroup:
                return JsonConvert.DeserializeObject<ConsensusGroupTransaction>(transactionString);
            case Constants.TransactionType.ConsensusGroupFailure:
                return JsonConvert.DeserializeObject<ConsensusGroupFailureTransaction>(transactionString);
            case Constants.TransactionType.Vars:
                return JsonConvert.DeserializeObject<VarsTransaction>(transactionString);
            default:
                throw new InvalidCastException(transaction.Type);
        }
    }
    
    public static decimal GetGain(int gain)
    {
        return (decimal)gain / 10;
    }

    public static double CalculateDistance(Hotspot first, Hotspot second)
    {
        var g1 = new GeoCoordinate(Convert.ToDouble(first.Lat), Convert.ToDouble(first.Lng));
        var g2 = new GeoCoordinate(Convert.ToDouble(second.Lat), Convert.ToDouble(second.Lng));
        return g1.GetDistanceTo(g2);
    }

    public static string ToDistanceText(double distance)
    {
        if (distance >= 1000)
        {
            return $"{distance / 1000:F1}km";
        }

        return $"{distance:F1}m";
    }

    public static string GetDirectionString(Hotspot first, Hotspot second)
    {
        var bearing = HeliumHelpers.DegreeBearing(first.Lat, first.Lng, second.Lat, second.Lng);
        var bearingDirection = HeliumHelpers.ToDirection(bearing);
        var distance = HeliumHelpers.CalculateDistance(first, second);
        return $"({ToDistanceText(distance)}/{bearingDirection}/{bearing.ToString("0")}°)";
    }

    public static double DegreeBearing(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        var dLon = ToRad(lon2 - lon1);
        var dPhi = Math.Log(
            Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
        if (Math.Abs(dLon) > Math.PI)
            dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
        return ToBearing(Math.Atan2(dLon, dPhi));
    }

    private static double ToRad(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    private static double ToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }

    private static double ToBearing(double radians)
    {
        // convert radians to degrees (as bearing: 0...360)
        return (ToDegrees(radians) + 360) % 360;
    }

    public static string ToDirection(double d)
    {
        if (d == 0) return "N";
        if (d == 90) return "E";
        if (d == 180) return "S";
        if (d == 270) return "W";

        if (d > 0 && d < 90) return "NE";
        if (d > 90 && d < 180) return "SE";
        if (d > 180 && d < 270) return "SW";
        if (d > 270 && d < 360) return "NW";

        throw new ArgumentException(d.ToString());
    }

 

    public static string GetSignalGoodness(int signal, double distance)
    {
        if (signal >= -90)
        {
            return "Excellent";
        }

        if (signal < -90 && signal >= -110)
        {
            return "Mediocre";
        }

        if (signal < -110)
        {
            return "Poor";
        }

        return "N/A";
    }
}