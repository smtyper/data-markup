using DataMarkup.Data;
using DataMarkup.Models;
using DataMarkup.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Controllers;

public class MarkupTasksController : Controller
{
    private readonly DataMarkupContext _dataMarkupContext;
    private readonly UserManager<User> _userManager;

    public MarkupTasksController(DataMarkupContext dataMarkupContext, UserManager<User> userManager)
    {
        _dataMarkupContext = dataMarkupContext;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Board()
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("NeedsAuthorization", "Home");

        var userId = Guid.Parse(_userManager.GetUserId(User));
        var user = await _dataMarkupContext.Users
            .Include(user => user.MarkupTasks)
            .SingleAsync(user => user.Id == userId);

        return View(user.MarkupTasks);
    }

    [HttpGet]
    public IActionResult Create() => User.Identity!.IsAuthenticated ?
        View(new MarkupTaskViewModel
        {
            CurrentQuestion = new MarkupQuestionViewModel(),
            Questions = new List<MarkupQuestionViewModel>()
        }) :
        RedirectToAction("NeedsAuthorization", "Home");

    [HttpGet("/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await _dataMarkupContext.MarkupTasks
            .Include(task => task.User)
            .Include(task => task.MarkupQuestions)
            .SingleOrDefaultAsync(task => task.Id == id);

        if (task is null)
            return NotFound();

        var userId = Guid.Parse(_userManager.GetUserId(User));

        if (task.User.Id != userId)
            return RedirectToAction("AccessDenied", "Home");

        return View(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody]MarkupTaskViewModel taskViewModel)
    {
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("NeedsAuthorization", "Home");

        if (!ModelState.IsValid)
            return View(taskViewModel);

        var currentUser = await _userManager.GetUserAsync(User);
        var markupTask = taskViewModel.Adapt<MarkupTask>() with
        {
            Id = Guid.NewGuid(),
            User = currentUser,
            MarkupQuestions = taskViewModel.Questions
                .Select(questionViewModel => questionViewModel.Adapt<MarkupQuestion>())
                .ToList()
        };

        _dataMarkupContext.MarkupTasks.Add(markupTask);

        await _dataMarkupContext.SaveChangesAsync();

        return RedirectToAction("Board", "MarkupTasks");
    }
}
