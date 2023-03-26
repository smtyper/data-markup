using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using DataMarkup.Entities.Parameters.Account;
using DataMarkup.Entities.Parameters.Board;
using DataMarkup.Entities.Parameters.TaskManager;
using DataMarkup.Entities.Views.Account;
using DataMarkup.Entities.Views.Board;
using DataMarkup.Entities.Views.TaskManager;
using DataMarkup.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace DataMarkup.Frontend;

public class ApiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorageService;
    private readonly ApplicationAuthenticationStateProvider _applicationAuthenticationStateProvider;

    public ApiHttpClient(HttpClient httpClient, ILocalStorageService localStorageService,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _localStorageService = localStorageService;
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

    public async ValueTask<UpdateTaskTypeResult?> UpdateTaskTypeAsync(UpdateTaskTypeParameters parameters)
    {
        const string url = "TaskManager/update-task-type";

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parameters) };
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<UpdateTaskTypeResult>(jsonContent);

        return result;
    }

    public async ValueTask<AddPermissionResult?> AddPermissionAsync(AddPermissionParameters parameters)
    {
        const string url = "TaskManager/add-permission";

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parameters) };
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<AddPermissionResult>(jsonContent);

        return result;
    }

    public async ValueTask<RemovePermissionResult?> RemovePermissionAsync(RemovePermissionParameters parameters)
    {
        const string url = "TaskManager/remove-permission";

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parameters) };
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<RemovePermissionResult>(jsonContent);

        return result;
    }

    public async ValueTask<AddTaskInstancesResult?> AddTaskInstancesAsync(TaskInstancesParameters parameters)
    {
        const string url = "TaskManager/add-task-instances";

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parameters) };
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<AddTaskInstancesResult>(jsonContent);

        return result;
    }

    public async ValueTask<GetTaskInstancesResult?> GetTaskInstancesAsync(Guid typeId, int page, int pageSize)
    {
        var url = $"TaskManager/instances/{typeId}&{page}&{pageSize}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<GetTaskInstancesResult>(jsonContent);

        return result;
    }

    public async ValueTask<GetAvailableTaskTypesResult?> GetAvailableTaskTypesAsync()
    {
        const string url = "Board/get-available-task-types";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<GetAvailableTaskTypesResult>(jsonContent);

        return result;
    }

    public async ValueTask<GetTaskResult?> GetTaskAsync(Guid taskTypeId)
    {
        var url = $"Board/get-task/?taskTypeId={taskTypeId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<GetTaskResult>(jsonContent);

        return result;
    }

    public async ValueTask<AddSolutionResult?> AddSolutionAsync(SolutionParameters parameters)
    {
        const string url = "Board/add-solution";

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(parameters) };
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<AddSolutionResult>(jsonContent);

        return result;
    }

    public async ValueTask<RemoveTaskInstanceResult?> RemoveTaskInstanceAsync(Guid instanceId)
    {
        var url = $"TaskManager/remove-task-instance/{instanceId}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        using var response = await SendWithAuthorizationHeaderAsync(request);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<RemoveTaskInstanceResult>(jsonContent);

        return result;
    }

    private async ValueTask<HttpResponseMessage> SendWithAuthorizationHeaderAsync(HttpRequestMessage requestMessage)
    {
        async ValueTask<string?> GetUpdatedToken(UserSession userSession)
        {
            if (DateTime.UtcNow.AddSeconds(45) > userSession.RefreshTokenExpiration)
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

        var userSession = await _localStorageService.GetBase64ValueAsync<UserSession>(nameof(UserSession)) ??
                          throw new UnauthorizedAccessException();

        var token = DateTime.UtcNow.AddSeconds(45) > userSession.TokenExpiration ?
            await GetUpdatedToken(userSession) :
            userSession.Token;

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(requestMessage);

        return response;
    }

    private async ValueTask<RefreshTokenResult> RefreshTokenAsync(RefreshTokenParameters refreshTokenParameters)
    {
        const string url = "Account/refresh-token";

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
