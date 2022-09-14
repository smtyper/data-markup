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
        Unauthorized();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody]MarkupTaskViewModel taskViewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("NeedsAuthorization", "Home");

        if (!ModelState.IsValid)
            return View(taskViewModel);

        var currentUser = await _userManager.GetUserAsync(User);
        var markupTask = taskViewModel.Adapt<MarkupTask>() with
        {
            Id = Guid.NewGuid(),
            User = currentUser,
            Questions = taskViewModel.Questions
                .Select(questionViewModel => questionViewModel.Adapt<MarkupQuestion>())
                .ToList()
        };

        _dataMarkupContext.Tasks.Add(markupTask);

        await _dataMarkupContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> EditTask([FromBody]MarkupTaskViewModel taskViewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("NeedsAuthorization", "Home");

        var userId = Guid.Parse(_userManager.GetUserId(User));
        var user = await _dataMarkupContext.Users
            .Include(user => user.MarkupTasks)
            .SingleAsync(user => user.Id == userId);

        var task = user.MarkupTasks.SingleOrDefault(task => task.Id == taskViewModel.Id);

        if (task is null)
            return Forbid();

        task.Name = taskViewModel.Name;
        task.MaxSolutions = taskViewModel.MaxSolutions;
        task.Payment = taskViewModel.Payment;
        task.Instruction = taskViewModel.Instruction;

        await _dataMarkupContext.SaveChangesAsync();

        return Ok();
    }

    public async Task<IActionResult> EditQuestion([FromBody]MarkupQuestionViewModel questionViewModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        if (!User.Identity!.IsAuthenticated)
            return Unauthorized();

        var userId = Guid.Parse(_userManager.GetUserId(User));

        var question = await _dataMarkupContext.Tasks
            .Include(task => task.Questions)
            .Include(task => task.User)
            .Where(task => task.User.Id == userId)
            .SelectMany(task => task.Questions)
            .SingleOrDefaultAsync(question => question.Id == questionViewModel.Id);

        if (question is null)
            return Forbid();


        question.StaticContent = questionViewModel.StaticContent;
        question.DynamicContentConstraint = questionViewModel.DynamicContentConstraint;
        question.AnswerDescription = questionViewModel.AnswerDescription;

        await _dataMarkupContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("/{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        if (!User.Identity!.IsAuthenticated)
            return Unauthorized();

        var task = await _dataMarkupContext.Tasks
            .Include(task => task.User)
            .Include(task => task.Questions)
            .SingleOrDefaultAsync(task => task.Id == id);

        if (task is null)
            return NotFound();

        var userId = Guid.Parse(_userManager.GetUserId(User));

        if (task.User.Id != userId)
            return Forbid();

        var taskViewModel = task.Adapt<MarkupTaskViewModel>() with
        {
            Questions = task.Questions
                .Select(question => question.Adapt<MarkupQuestionViewModel>())
                .ToList()
        };

        return View(taskViewModel);
    }
}
