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

    // public IActionResult Edit(MarkupTaskViewModel taskViewModel)
    // {
    //     if (!User.Identity!.IsAuthenticated)
    //         return RedirectToAction("NeedsAuthorization", "Home");
    //
    //
    // }

    [HttpGet("/{id:guid}")]
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

        var taskViewModel = task.Adapt<MarkupTaskViewModel>() with
        {
            Questions = task.MarkupQuestions
                .Select(question => question.Adapt<MarkupQuestionViewModel>())
                .ToList()
        };

        return View(taskViewModel);
    }


}
