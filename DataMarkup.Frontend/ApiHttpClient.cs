using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.SessionStorage;
using DataMarkup.Entities.Parameters.Account;
using DataMarkup.Entities.Parameters.TaskManager;
using DataMarkup.Entities.Views.Account;
using DataMarkup.Entities.Views.TaskManager;
using DataMarkup.Frontend.Extensions;
using DataMarkup.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace DataMarkup.Frontend;

public class ApiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ISessionStorageService _sessionStorage;
    private readonly ApplicationAuthenticationStateProvider _applicationAuthenticationStateProvider;

    public ApiHttpClient(HttpClient httpClient, ISessionStorageService sessionStorage,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
        _applicationAuthenticationStateProvider = (ApplicationAuthenticationStateProvider) authenticationStateProvider;
    }

    public async ValueTask<LoginResult?> LoginAsync(LoginParameters loginParameters)
    {
        const string url = "Account/login";

        using var response = await _httpClient.PostAsJsonAsync(url, loginParameters);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<LoginResult>(jsonContent);

        return result;
    }

    public async ValueTask<RegisterResult?> RegisterAsync(RegisterParameters registerParameters)
    {
        const string url = "Account/register";

        using var response = await _httpClient.PostAsJsonAsync(url, registerParameters);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<RegisterResult>(jsonContent);

        return result;
    }

    public async ValueTask<GetTaskTypesResult?> GetTaskTypesAsync()
    {
        const string url = "TaskManager/get-task-types";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<GetTaskTypesResult>(jsonContent);

        return result;
    }

    public async ValueTask<AddTaskTypeResult?> AddTaskTypeAsync(TaskTypeParameters parameters)
    {
        const string url = "TaskManager/add-task-type";

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parameters) };
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<AddTaskTypeResult>(jsonContent);

        return result;
    }

    public async ValueTask<GetTaskTypeResult?> GetTaskTypeAsync(Guid taskTypeId)
    {
        var url = $"TaskManager/get-task-type/{taskTypeId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<GetTaskTypeResult>(jsonContent);

        return result;
    }

    private async ValueTask<HttpResponseMessage> SendWithAuthorizationHeaderAsync(HttpRequestMessage requestMessage)
    {
        async ValueTask<string?> GetUpdatedToken(UserSession userSession)
        {
            if (DateTime.UtcNow.AddSeconds(-45) > userSession.RefreshTokenExpiration)
            {
                await _applicationAuthenticationStateProvider.UpdateAuthenticationStateAsync(null);

                return null;
            }

            var parameters = new RefreshTokenParameters { RefreshToken = userSession.RefreshToken };
            var refreshResult = await RefreshTokenAsync(parameters);

            if (!refreshResult.Successful)
                throw new UnauthorizedAccessException();

            var updatedUserSession = userSession with
            {
                Token = refreshResult.Token!,
                TokenExpiration = refreshResult.Expiration!.Value
            };
            await _applicationAuthenticationStateProvider.UpdateAuthenticationStateAsync(updatedUserSession);

            return refreshResult.Token;
        }

        var userSession = await _sessionStorage.GetBase64ValueAsync<UserSession>(nameof(UserSession)) ??
                          throw new UnauthorizedAccessException();

        var token = DateTime.UtcNow.AddSeconds(-45) > userSession?.TokenExpiration ?
            await GetUpdatedToken(userSession) :
            userSession!.Token;

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(requestMessage);

        return response;
    }

    private async ValueTask<RefreshTokenResult> RefreshTokenAsync(RefreshTokenParameters refreshTokenParameters)
    {
        const string url = "refresh-token";

        using var response = await _httpClient.PostAsJsonAsync(url, refreshTokenParameters);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<RefreshTokenResult>(jsonContent)!;

        return result;
    }

    private static T? TryDeserialize<T>(string jsonValue) where T : class
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(jsonValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return value;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
