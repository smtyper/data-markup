using System.Security.Claims;
using Blazored.SessionStorage;
using DataMarkup.Frontend.Extensions;
using DataMarkup.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace DataMarkup.Frontend;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorageService _sessionStorage;
    private readonly ClaimsPrincipal _anonymousUserClaimsPrincipal = new(new ClaimsIdentity());

    public ApiAuthenticationStateProvider(ISessionStorageService sessionStorage) => _sessionStorage = sessionStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSession = await _sessionStorage.GetBase64ValueAsync<UserSession>(nameof(UserSession));

            var claimPrincipal = userSession is null ?
                _anonymousUserClaimsPrincipal :
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userSession.Username) },
                    "JwtAuth"));

            var state = new AuthenticationState(claimPrincipal);

            return state;
        }
        catch (Exception)
        {
            return new AuthenticationState(_anonymousUserClaimsPrincipal);
        }
    }

    public async Task UpdateAuthenticationStateAsync(UserSession? userSession)
    {
        var claimsPrincipal = userSession is null ?
            _anonymousUserClaimsPrincipal :
            new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new(ClaimTypes.Name, userSession.Username) }));

        if (userSession != null)
            await _sessionStorage.RemoveItemAsync("UserSession");
        else
            await _sessionStorage.AddAsBase64Async(nameof(UserSession), userSession);

        var state = new AuthenticationState(claimsPrincipal);

        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }
}
