using DataMarkup.Data;
using DataMarkup.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataMarkup.Controllers;

public class HomeController : Controller
{
    private readonly DataMarkupContext _dataMarkupContext;
    private readonly UserManager<User> _userManager;

    public HomeController(DataMarkupContext dataMarkupContext, UserManager<User> userManager)
    {
        _dataMarkupContext = dataMarkupContext;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult NeedsAuthorization() => View();

    [HttpGet]
    public IActionResult AccessDenied() => View();

}
