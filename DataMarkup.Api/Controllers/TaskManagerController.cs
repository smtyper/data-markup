using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataMarkup.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskManagerController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public TaskManagerController(UserManager<IdentityUser> userManager) => _userManager = userManager;

    [HttpPost]
    [Route("add-markup-task")]
    public async Task<IActionResult> AddMarkupTask()
}
