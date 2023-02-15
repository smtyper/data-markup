using System.Security.Claims;
using Blazored.LocalStorage;
using DataMarkup.Frontend.Extensions;
using DataMarkup.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace DataMarkup.Frontend;

public class ApplicationAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly ClaimsPrincipal _anonymousUserClaimsPrincipal = new(new ClaimsIdentity());

    public ApplicationAuthenticationStateProvider(ILocalStorageService localStorageService) =>
        _localStorageService = localStorageService;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSession = await _localStorageService.GetBase64ValueAsync<UserSession>(nameof(UserSession));

            var claimPrincipal = userSession is null ?
                _anonymousUserClaimsPrincipal :
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userSession.Username) },
                    "JwtAuth"));

            var state = new AuthenticationState(claimPrincipal);

            return await Task.FromResult(state);
        }
        catch (Exception)
        {
            return await Task.FromResult(new AuthenticationState(_anonymousUserClaimsPrincipal));
        }
    }

    public async Task UpdateAuthenticationStateAsync(UserSession? userSession)
    {
        var claimsPrincipal = userSession is null ?
            _anonymousUserClaimsPrincipal :
            new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new(ClaimTypes.Name, userSession.Username) },
                "JwtAuth"));

        if (userSession is not null)
            await _localStorageService.AddAsBase64Async(nameof(UserSession), userSession);
        else
            await _localStorageService.RemoveItemAsync(nameof(UserSession));

        var state = new AuthenticationState(claimsPrincipal);

        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }
}
