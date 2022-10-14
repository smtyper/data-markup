using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Dto.Account;
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
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username);

        if (userExists is not null)
            return Conflict(new { Message = "User already exists." });

        var user = new User
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };
        var creationResult = await _userManager.CreateAsync(user, model.Password);

        if (creationResult.Succeeded)
            return Ok();

        return UnprocessableEntity(new { Message = "Registration failed. Check requirements and try again." });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);

        if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized();


        var token = GetJwtSecurityToken(user);

        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token), Expiration = token.ValidTo });
    }

    private JwtSecurityToken GetJwtSecurityToken(IdentityUser identityUser)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

        var token = new JwtSecurityToken(
            issuer: _settings.ValidIssuer,
            claims: new []
            {
                new Claim("sub", identityUser.Id),
                new Claim(ClaimsIdentity.DefaultNameClaimType, identityUser.Id)
            },
            audience: _settings.ValidAudience,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}

public record AccountControllerSettings
{
    public string ValidAudience { get; init; } = null!;

    public string ValidIssuer { get; init; } = null!;

    public string Secret { get; init; } = null!;
}
