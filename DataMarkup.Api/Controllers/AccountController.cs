using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Entities.Parameters.Account;
using DataMarkup.Entities.Views.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DataMarkup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountControllerSettings _settings;

    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public AccountController(IOptions<AccountControllerSettings> options,
        UserManager<User> userManager, ApplicationDbContext applicationDbContext)
    {
        _settings = options.Value;

        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterParameters parameters)
    {
        var userExists = await _userManager.FindByNameAsync(parameters.Username);

        if (userExists is not null)
            return Conflict(new RegisterResult { Succesful = false, Message = "User with the same name already exists." });

        var user = new User
        {
            Email = parameters.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = parameters.Username
        };
        var creationResult = await _userManager.CreateAsync(user, parameters.Password);

        if (creationResult.Succeeded)
            return Ok(new RegisterResult { Succesful = true, Message = default });

        return UnprocessableEntity(new RegisterResult
        {
            Succesful = false,
            Message = "Registration failed. Check requirements and try again."
        });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginParameters parameters)
    {
        var user = await _userManager.FindByNameAsync(parameters.Username);

        if (user is null)
            return Unauthorized(new LoginResult { Successful = false, Message = "User is not found." });

        var checkPasswordAsync = await _userManager.CheckPasswordAsync(user, parameters.Password);

        if (!checkPasswordAsync)
            return Unauthorized(new LoginResult { Successful = false, Message = "Wrong password." });

        var token = GetJwtSecurityToken(user);
        var refreshToken = await GetRefreshToken(user);

        return Ok(new LoginResult
        {
            Successful = true,
            Message = default,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            Expiration = token.ValidTo
        });
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenParameters parameters)
    {
        var user = await ParseOutdatedToken(parameters.OutdatedToken!);

        if (user is null)
            return BadRequest(new RefreshTokenResult { Successful = false, Message = "Invalid outdated token." });

        var refreshToken = await _applicationDbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.UserId == Guid.Parse(user.Id) &&
                                           token.Expiration < DateTime.UtcNow &&
                                           token.Token == parameters.RefreshToken);

        if (refreshToken is null)
            return BadRequest(new RefreshTokenResult
            {
                Successful = false,
                Message = "Refresh token is invalid or outdated."
            });

        var refreshedToken = GetJwtSecurityToken(user);

        return Ok(new RefreshTokenResult
        {
            Successful = true,
            Token = new JwtSecurityTokenHandler().WriteToken(refreshedToken)
        });
    }

    private JwtSecurityToken GetJwtSecurityToken(IdentityUser identityUser)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

        var token = new JwtSecurityToken(
            issuer: _settings.ValidIssuer,
            claims: new []
            {
                new Claim(ClaimTypes.Name, identityUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            },
            audience: _settings.ValidAudience,
            expires: DateTime.UtcNow.Add(_settings.TokenLifetime),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    private async ValueTask<RefreshToken> GetRefreshToken(User user)
    {
        var userId = Guid.Parse(user.Id);

        var existedToken = _applicationDbContext.RefreshTokens
            .SingleOrDefault(token => token.UserId == userId);

        if (existedToken is not null)
        {
            if (existedToken.Expiration < DateTime.UtcNow)
                return existedToken;

            _applicationDbContext.RefreshTokens.Remove(existedToken);
            await _applicationDbContext.SaveChangesAsync();
        }

        using var randomNumberGenerator = RandomNumberGenerator.Create();

        var bytes = new byte[64];
        randomNumberGenerator.GetBytes(bytes);

        var base64Number = Convert.ToBase64String(bytes);
        var refreshToken = new RefreshToken
        {
            Token = base64Number,
            UserId = userId,
            Expiration = DateTime.UtcNow.Add(_settings.RefreshTokenLifetime)
        };

        await _applicationDbContext.RefreshTokens.AddAsync(refreshToken);
        await _applicationDbContext.SaveChangesAsync();

        return refreshToken;
    }

    private async ValueTask<User?> ParseOutdatedToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            return null;

        var user = principal.Identity?.Name is null ?
            null :
            await _userManager.FindByNameAsync(principal.Identity.Name);

        return user;
    }
}

public record AccountControllerSettings
{
    public string ValidAudience { get; init; } = null!;

    public string ValidIssuer { get; init; } = null!;

    public string Secret { get; init; } = null!;

    public TimeSpan TokenLifetime { get; init; }

    public TimeSpan RefreshTokenLifetime { get; init; }
}
