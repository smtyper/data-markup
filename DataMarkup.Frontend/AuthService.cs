using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using DataMarkup.Frontend.Models.Api.Account;
using Microsoft.AspNetCore.Components.Authorization;

namespace DataMarkup.Frontend;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
    }

    public async Task<HttpStatusCode> Register(RegisterModel registerModel)
    {
        const string uri = "Account/Register";

        using var response = await _httpClient.PostAsJsonAsync(uri, registerModel);

        return response.StatusCode;
    }

    public async Task<LoginResult> Login(LoginModel loginModel)
    {
        const string uri = "Account/Login";

        using var response = await _httpClient.PostAsJsonAsync(uri, loginModel);

        if (!response.IsSuccessStatusCode)
        {
            return loginResult;
        }

        var loginResult = JsonSerializer.Deserialize<LoginResponseModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        await _localStorage.SetItemAsync("authToken", loginResult.Token);
        ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginModel.Email);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

        return loginResult;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
