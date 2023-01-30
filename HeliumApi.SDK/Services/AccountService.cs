using HeliumApi.SDK.Helpers;
using HeliumApi.SDK.Responses.Account;
using HeliumApi.SDK.Responses.Account.Transactions;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace HeliumApi.SDK.Services;

public static class AccountService
{
    public static async Task<Account> GetAccount(string accountAddress, bool ignoreCache = false)
    {
        if (!ignoreCache)
        {
            var cachedAccount = CacheHelper.GetOne<Account>(x => x.Address.Equals(accountAddress));
            if (cachedAccount != null)
            {
                return cachedAccount;
            }
        }

        var uri = $"/v1/accounts/{accountAddress}";
        var data = await HeliumClient.Get(uri);
        var account = JsonConvert.DeserializeObject<Account>(data.First());
        CacheHelper.InsertOne<Account>(account);

        return account;
    }

    public static async Task<List<RewardTransaction>> GetRewards(string accountAddress, DateTime minTime,
        DateTime? maxTime = null)
    {
        var uri = $"/v1/accounts/{accountAddress}/rewards?min_time={Uri.EscapeDataString(minTime.ToIso8601())}";
        if (maxTime.HasValue)
        {
            uri += $"&max_time={Uri.EscapeDataString(maxTime.Value.ToIso8601())}";
        }

        var allData = await HeliumClient.Get(uri);
        var transactions = HeliumHelpers.DeserializeAll<RewardTransaction>(allData.ToArray());
        return transactions;
    }

    public static async Task<List<AccountReward>> GetDailyRewards(string accountAddress, DateTime minTime,
        DateTime maxTime)
    {
        var uri =
            $"/v1/accounts/{accountAddress}/rewards/sum?bucket=day&min_time={Uri.EscapeDataString(minTime.ToIso8601())}&max_time={Uri.EscapeDataString(maxTime.ToIso8601())}";
        var data = await HeliumClient.Get(uri);
        var accountRewards = HeliumHelpers.DeserializeAll<AccountReward>(data.ToArray());

        return accountRewards;
    }
}