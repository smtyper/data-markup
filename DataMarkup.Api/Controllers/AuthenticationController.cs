using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DataMarkup.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationControllerSettings _settings;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthenticationController(IOptions<AuthenticationControllerSettings> options,
        UserManager<IdentityUser> userManager)
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

        var user = new IdentityUser
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


        var token = GetJwtSecurityToken();

        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token), Expiration = token.ValidTo });
    }

    private JwtSecurityToken GetJwtSecurityToken()
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

        var token = new JwtSecurityToken(
            issuer: _settings.ValidIssuer,
            audience: _settings.ValidAudience,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; init; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; init; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; init; }
    }

    public record LoginModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; init; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; init; }
    }
}

public record AuthenticationControllerSettings
{
    public string ValidAudience { get; init; } = null!;

    public string ValidIssuer { get; init; } = null!;

    public string Secret { get; init; } = null!;
}
