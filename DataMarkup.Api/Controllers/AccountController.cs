using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Entities.Parameters.Account;
using DataMarkup.Entities.Views.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DataMarkup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountControllerSettings _settings;
    private readonly UserManager<User> _userManager;

    public AccountController(IOptions<AccountControllerSettings> options,
        UserManager<User> userManager)
    {
        _userManager = userManager;
        _settings = options.Value;
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

        return Ok(new LoginResult
        {
            Successful = true,
            Message = default,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
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

    private static string GetRefreshToken()
    {
        using var randomNumberGenerator = RandomNumberGenerator.Create();

        var bytes = new byte[64];
        randomNumberGenerator.GetBytes(bytes);

        var base64Number = Convert.ToBase64String(bytes);

        return base64Number;
    }
}

public record AccountControllerSettings
{
    public string ValidAudience { get; init; } = null!;

    public string ValidIssuer { get; init; } = null!;

    public string Secret { get; init; } = null!;

    public TimeSpan TokenLifetime { get; init; }
}
