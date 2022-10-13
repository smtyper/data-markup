using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models.Account;
using DataMarkup.Api.Models.Markup;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataMarkup.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskManagerController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public TaskManagerController(UserManager<User> userManager, ApplicationDbContext applicationDbContext)
    {
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
    }

    [HttpPost]
    [Route("add-task-type")]
    public async Task<IActionResult> AddMarkupTask([FromBody] Models.Dto.TaskManager.TaskType taskTypeDto)
    {
        var userId = _userManager.GetUserId(HttpContext.User);
        var currentUser = await _applicationDbContext.Users
            .Include(user => user.TaskTypes)
            .Where(user => user.Id == userId)
            .SingleAsync();

        if (currentUser.TaskTypes.Any(taskType => taskType.Name == taskTypeDto.Name))
            return Conflict(new { Message = "The task type with the same name already exists." });

        var taskType = taskTypeDto.Adapt<TaskType>() with
        {
            Id = Guid.NewGuid(),
            User = currentUser,
            QuestionTypes = taskTypeDto.Questions
                .Select(dto => dto.Adapt<QuestionType>() with { Id = Guid.NewGuid() })
                .ToArray()
        };

        foreach (var questionType in taskType.QuestionTypes)
            questionType.TaskType = taskType;

        await _applicationDbContext.TaskTypes.AddAsync(taskType);
        await _applicationDbContext.SaveChangesAsync();

        var types = await _applicationDbContext.Users
            .Where(user => user.Id == userId)
            .Include(user => user.TaskTypes)
            .ToArrayAsync();

        return Ok();
    }
}
