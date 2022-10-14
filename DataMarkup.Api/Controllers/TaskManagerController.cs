using System.Text.RegularExpressions;
using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models.Account;
using DataMarkup.Api.Models.Markup;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoreLinq.Extensions;
using Newtonsoft.Json;

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

    [HttpGet]
    [Route("get-task-types")]
    public async Task<IActionResult> GetTaskTypes()
    {
        var currentUser = await GetCurrentUserAsync();

        var taskTypes = (await _applicationDbContext.TaskTypes
                .Include(type => type.QuestionTypes)
                .Where(type => type.UserId == currentUser.Id)
                .Select(type => type)
                .ToArrayAsync())
            .Select(type => type with { User = null })
            .ToArray();

        return Ok(taskTypes);
    }

    [HttpPost]
    [Route("add-task-type")]
    public async Task<IActionResult> AddTaskType([FromBody] Models.Dto.TaskManager.TaskType taskTypeDto)
    {
        var currentUser = await GetCurrentUserAsync();
        var taskTypes = await _applicationDbContext.TaskTypes.Include(type => type.QuestionTypes).ToArrayAsync();

        if (taskTypes.Any(taskType => taskType.Name == taskTypeDto.Name))
            return Conflict(new { Message = "The task type with the same name already exists." });

        foreach (var constraint in taskTypeDto.Questions
                     .SelectMany(questionDto =>
                         new[] { questionDto.DynamicContentConstraint, questionDto.AnswerConstraint }))
            if (!constraint.IsValidRegex())
                return BadRequest(new
                {
                    Message = $"Cannot use {constraint} as a pattern."
                });

        var taskTypeId = Guid.NewGuid();
        var taskType = taskTypeDto.Adapt<TaskType>() with
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            QuestionTypes = taskTypeDto.Questions
                .Select(dto => dto.Adapt<QuestionType>() with { Id = Guid.NewGuid(), TaskTypeId = taskTypeId })
                .ToArray()
        };

        await _applicationDbContext.TaskTypes.AddAsync(taskType);
        await _applicationDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [Route("add-task-instance")]
    public async Task<IActionResult> AddTaskInstance([FromBody] Models.Dto.TaskManager.TaskInstance taskInstanceDto)
    {
        var currentUser = await GetCurrentUserAsync();

        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.QuestionTypes)
            .Where(type => type.UserId == currentUser.Id)
            .SingleOrDefaultAsync(type => type.Id == taskInstanceDto.TaskTypeId);

        if (taskType is null)
            return BadRequest(new
            {
                Message = $"Cannot find task type by the following id: {taskInstanceDto.TaskTypeId}."
            });

        var questionsMap = taskInstanceDto.QuestionInstances
            .LeftJoin(taskType.QuestionTypes,
                instance => instance.QuestionTypeId,
                type => type.Id,
                instance => (instance, null)!,
                (instance, type) => (instance, type))
            .ToArray();

        if (questionsMap.Any(pair => pair.type is null))
        {
            var unknownQuestionTypeIds = string.Join(", ", questionsMap
                .Where(pair => pair.type is null)
                .Select(pair => pair.instance.QuestionTypeId));

            return BadRequest(new { Message = $"Unknown question types: {unknownQuestionTypeIds}." });
        }

        if (questionsMap.Any(pair => !Regex.IsMatch(pair.instance.Content, pair.type.DynamicContentConstraint)))
        {
            var mismatchedQuestionInstances = string.Join(", ", questionsMap
                .Where(pair => !Regex.IsMatch(pair.instance.Content, pair.type.DynamicContentConstraint))
                .Select(pair => $"({pair.instance.Content}) : ({pair.type.DynamicContentConstraint})"));

            return BadRequest(
                new { Message = $"Question instances doesn't match the pattern: {mismatchedQuestionInstances}" });
        }

        var taskInstanceId = Guid.NewGuid();
        var taskInstance = new TaskInstance
        {
            Id = taskInstanceId,
            TaskTypeId = taskType.Id,
            QuestionInstances = questionsMap
                .Select(pair => new QuestionInstance
                {
                    Id = Guid.NewGuid(),
                    Content = pair.instance.Content,
                    TaskInstanceId = taskInstanceId,
                    QuestionTypeId = pair.type.Id
                })
                .ToArray()
        };

        await _applicationDbContext.TaskInstances.AddAsync(taskInstance);
        await _applicationDbContext.SaveChangesAsync();

        return Ok();
    }

    private async ValueTask<User> GetCurrentUserAsync()
    {
        var userId = _userManager.GetUserId(HttpContext.User);
        var user = await _applicationDbContext.Users
            .Where(user => user.Id == userId)
            .SingleAsync();

        return user;
    }
}
