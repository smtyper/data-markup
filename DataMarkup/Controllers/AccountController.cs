using DataMarkup.Models;
using DataMarkup.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataMarkup.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Registration() => View();

    [HttpPost]
    public async Task<IActionResult> Registration(RegistrationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User { Email = model.Email, UserName = model.UserName, BirthDate = model.BirthDate.Date };

        var creationResult = await _userManager.CreateAsync(user, model.Password);

        if (creationResult.Succeeded)
        {
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }

        foreach (var error in creationResult.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var signInResult = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

        if (signInResult.Succeeded)
        {
            var redirectToActionResult = string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) ?
                Redirect(model.ReturnUrl) :
                (IActionResult)RedirectToAction("Index", "Home");

            return redirectToActionResult;
        }

        ModelState.AddModelError("", "Invalid username and/or password");

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }
}
