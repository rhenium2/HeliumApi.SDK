using System.Text.Json;
using HeliumApi.SDK.Responses.Account;
using HeliumApi.SDK.Services;
using RichardSzalay.MockHttp;

namespace HeliumApi.SDK.Test;

public class AccountServiceTest
{
    private readonly string _cachedAddress;
    private readonly string _notCachedAddress;

    public AccountServiceTest()
    {
        _cachedAddress = "cached";
        _notCachedAddress = "not-cached";
    }

    [Fact]
    public async Task GetAccountRefreshTest()
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
        account = await AccountService.GetAccount(_cachedAddress, false);
        Assert.Equal(account.Address, _cachedAddress);

        // Refresh account 
        account = await AccountService.GetAccount(_cachedAddress, true);
        Assert.Equal(account.Address, _notCachedAddress);
    }
}