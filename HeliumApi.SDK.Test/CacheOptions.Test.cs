using System.Text.Json;
using HeliumApi.SDK.Responses;
using HeliumApi.SDK.Responses.Account;
using HeliumApi.SDK.Services;
using RichardSzalay.MockHttp;

namespace HeliumApi.SDK.Test;

public class CacheOptionsTest
{
    private readonly string _cachedAddress;
    private readonly string _notCachedAddress;

    public CacheOptionsTest()
    {
        _cachedAddress = "cached";
        _notCachedAddress = "not-cached";
    }

    [Fact]
    public async Task DisableCacheTest()
    {
        // Arrange
        var url = $"https://api.helium.io/v1/accounts/*";
        var mockHttp = new MockHttpMessageHandler();

        var serialize = JsonSerializer.Serialize(
            new GenericResponse<Account>
            {
                Data = new Account
                {
                    Address = _cachedAddress,
                }
            }
        );
        mockHttp.When(HttpMethod.Get, url).Respond("application/json",
            serialize);

        HeliumClient.HttpClient = mockHttp.ToHttpClient();

        // Act & Assert
        /// Retrieving and insert into cache
        var account = await AccountService.GetAccount(_cachedAddress);

        // changing http response
        mockHttp.Clear();
        mockHttp.When(HttpMethod.Get, url).Respond("application/json",
            JsonSerializer.Serialize(
                new GenericResponse<Account>
                {
                    Data = new Account
                    {
                        Address = _notCachedAddress,
                    }
                }
            ));

        /// Retrieving from cache
        account = await AccountService.GetAccount(_cachedAddress);
        Assert.Equal(account.Address, _cachedAddress);

        /// Disabling the cache
        CacheOptions.DisableCacheFor<Account>();

        /// Retrieving from http call
        account = await AccountService.GetAccount(_cachedAddress);
        Assert.Equal(account.Address, _notCachedAddress);
    }
}