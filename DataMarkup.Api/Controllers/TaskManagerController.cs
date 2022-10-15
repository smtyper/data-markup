using System.Text.RegularExpressions;
using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models.Database.Access;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;
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
    [Route("remove-permission")]
    public async Task<IActionResult> RemovePermission([FromBody] Models.Dto.TaskManager.Permission permissionParemeters)
    {
        var currentUser = await GetCurrentUserAsync();
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Persmissions)
            .SingleOrDefaultAsync(type => type.UserId == currentUser.Id);

        if (taskType is null)
            return Unauthorized();

        var user = await _userManager.FindByNameAsync(permissionParemeters.UserName);

        if (user is null)
            return BadRequest($"Cannot find user by name '{permissionParemeters.UserName}'.");

        var permissionToRemove = await _applicationDbContext.Permissions.SingleAsync(permission =>
            permission.UserId == Guid.Parse(user.Id) && permission.TaskTypeId == permissionParemeters.TaskTypeId);

        _applicationDbContext.Permissions.Remove(permissionToRemove);
        await _applicationDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [Route("add-permission")]
    public async Task<IActionResult> AddPermission([FromBody] Models.Dto.TaskManager.Permission permissionParemeters)
    {
        var currentUser = await GetCurrentUserAsync();
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Persmissions)
            .SingleOrDefaultAsync(type => type.UserId == currentUser.Id);

        if (taskType is null)
            return Unauthorized();

        var user = await _userManager.FindByNameAsync(permissionParemeters.UserName);

        if (user is null)
            return BadRequest($"Cannot find user by name '{permissionParemeters.UserName}'.");

        var userId = Guid.Parse(user.Id);

        if (taskType.Persmissions!.Any(permission => permission.UserId == userId))
            return Ok("User already has permission.");

        var persmission = new Permission { TaskTypeId = taskType.Id, UserId = userId };

        await _applicationDbContext.Permissions.AddAsync(persmission);
        await _applicationDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    [Route("get-task-types")]
    public async Task<IActionResult> GetTaskTypes()
    {
        var currentUser = await GetCurrentUserAsync();

        var taskTypes = (await _applicationDbContext.TaskTypes
                .Include(type => type.QuestionTypes)
                .Include(type => type.Persmissions)
                .Where(type => type.UserId == currentUser.Id)
                .Select(type => type)
                .ToArrayAsync())
            .Select(type => type.Adapt<Models.Views.TaskType>())
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
                .Select(dto => dto.Adapt<QuestionType>() with
                {
                    Id = Guid.NewGuid(),
                    TaskTypeId = taskTypeId,
                })
                .ToArray()
        };

        await _applicationDbContext.TaskTypes.AddAsync(taskType);
        await _applicationDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [Route("add-task-instances")]
    public async Task<IActionResult> AddTaskInstances(
        [FromBody] Models.Dto.TaskManager.TaskInstancesParameters parameters)
    {
        var currentUser = await GetCurrentUserAsync();
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.QuestionTypes)
            .Where(type => type.UserId == currentUser.Id)
            .SingleOrDefaultAsync(type => type.Id == parameters.TaskTypeId);

        if (taskType is null)
            return BadRequest(new
            {
                Message = $"Cannot find task type by the following id: {parameters.TaskTypeId}."
            });

        foreach (var (questionTypeId, questionInstanceDtos) in parameters.QuestionDictionary)
        {

            if (taskType.QuestionTypes!.All(type => type.Id != questionTypeId))
                return BadRequest(new { Message = $"Unknown question type: {questionTypeId}." });

            var questionType = taskType.QuestionTypes!.Single(type => type.Id == questionTypeId);
            var contraintRegex = new Regex(questionType.DynamicContentConstraint);

            if (questionInstanceDtos.All(instanceDto => contraintRegex.IsFullMatch(instanceDto.Content)))
                continue;

            var mismatchedContent = string.Join(", ", questionInstanceDtos
                .Where(instanceDto => !contraintRegex.IsFullMatch(instanceDto.Content))
                .Select(instanceDto => instanceDto.Content));

            return BadRequest(new
            {
                Message =
                    $"The following instances doesn't match the pattern: ({questionType.DynamicContentConstraint}) {mismatchedContent}"
            });
        }

        var maxQuestionInstancesCount = parameters.QuestionDictionary.Values.Max(instanceDtos => instanceDtos.Count);
        var taskTypes = Enumerable
            .Range(0, maxQuestionInstancesCount)
            .Select(index =>
            {
                var taskInstanceId = Guid.NewGuid();
                var taskInstance = new TaskInstance
                {
                    Id = taskInstanceId,
                    TaskTypeId = taskType.Id,
                    QuestionInstances = parameters.QuestionDictionary
                        .Where(pair => index < pair.Value.Count)
                        .Select(pair => pair.Value[index].Adapt<QuestionInstance>() with
                        {
                            Id = Guid.NewGuid(), TaskInstanceId = taskInstanceId, QuestionTypeId = pair.Key
                        })
                        .ToArray()
                };

                return taskInstance;
            })
            .ToArray();

        await _applicationDbContext.TaskInstances.AddRangeAsync(taskTypes);
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
