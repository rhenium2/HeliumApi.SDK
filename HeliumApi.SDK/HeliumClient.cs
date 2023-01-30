using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using HeliumApi.SDK.Responses;
using Polly;

namespace HeliumApi.SDK;

public static class HeliumClient
{
    public static HttpClient HttpClient { get; set; }
    private const string BaseUri = "https://api.helium.io";

    static HeliumClient()
    {
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "HeliumApi-SDK");
    }

    private static async Task<HttpResponseMessage> PollyGet(Func<Task<HttpResponseMessage>> func)
    {
        return await Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(TransientHttpFailures)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(3 * retryAttempt))
            .ExecuteAsync(func);
    }

    private static bool TransientHttpFailures(HttpResponseMessage responseMessage)
    {
        return responseMessage.StatusCode == HttpStatusCode.TooManyRequests ||
               responseMessage.StatusCode == HttpStatusCode.RequestTimeout ||
               responseMessage.StatusCode == HttpStatusCode.BadGateway ||
               responseMessage.StatusCode == HttpStatusCode.GatewayTimeout ||
               responseMessage.StatusCode == HttpStatusCode.ServiceUnavailable;
    }

    public static async Task<List<string>> Get(string relativePath)
    {
        var allData = new List<string>();
        var response = new Response();
        do
        {
            response = await CallAndGetResponse(relativePath, response.Cursor);
            if (!string.IsNullOrEmpty(response.Data))
            {
                allData.Add(response.Data);
            }
        } while (!string.IsNullOrEmpty(response.Cursor));

        return allData;
    }

    private static async Task<Response> CallAndGetResponse(string relativePath, string cursor)
    {
        var uri = BaseUri + relativePath;
        if (!string.IsNullOrEmpty(cursor))
        {
            uri += uri.Contains('?') ? $"&cursor={cursor}" : $"?cursor={cursor}";
        }

        var responseMessage = await PollyGet(() =>
        {
            Debug.WriteLine($"{DateTime.Now:T} GET {uri}");
            return HttpClient.GetAsync(uri);
        });
        Debug.WriteLine($"Response status: {responseMessage.StatusCode}");
        responseMessage.EnsureSuccessStatusCode();
        var content = await responseMessage.Content.ReadAsStringAsync();

        JsonNode? result;
        try
        {
            result = JsonNode.Parse(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var data = result["data"].ToJsonString();
        var cursorString = result["cursor"]?.ToJsonString();
        var response = new Response();
        if (!string.IsNullOrEmpty(data) && data != "[]")
        {
            response.Data = data;
        }

        if (!string.IsNullOrEmpty(cursorString))
        {
            response.Cursor = cursorString.Substring(1, cursorString.Length - 2);
        }

        return response;
    }
}