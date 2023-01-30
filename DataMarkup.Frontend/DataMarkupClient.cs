using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DataMarkup.Frontend.Models.Api.Account;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace DataMarkup.Frontend;

public class DataMarkupClient
{
    private readonly HttpClient _httpClient;

    public DataMarkupClient(HttpClient httpClient) => _httpClient = httpClient;

    public async ValueTask<HttpStatusCode> RegisterAsync(RegisterModel registerModel)
    {
        const string uri = "Account/Register";
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = JsonContent.Create(registerModel);

        using var response = await _httpClient.SendAsync(request);

        return response.StatusCode;
    }
}
