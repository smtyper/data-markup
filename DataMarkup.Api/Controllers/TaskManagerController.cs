using System.Text.RegularExpressions;
using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models.Database.Access;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;
using DataMarkup.Entities.Parameters.TaskManager;
using DataMarkup.Entities.Views.TaskManager;
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
    public async Task<IActionResult> RemovePermission([FromBody] PermissionParameters permissionParameters)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Permissions)
            .SingleOrDefaultAsync(type => type.Id == permissionParameters.TaskTypeId &&
                                          type.UserId == currentUser.Id);

        if (taskType is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find task type by the following id '{permissionParameters.TaskTypeId}'"
            });

        var user = await _userManager.FindByNameAsync(permissionParameters.UserName);

        if (user is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find user by name '{permissionParameters.UserName}'."
            });

        var permissionToRemove = await _applicationDbContext.Permissions
            .SingleOrDefaultAsync(permission => permission.UserId == Guid.Parse(user.Id) &&
                                       permission.TaskTypeId == permissionParameters.TaskTypeId);

        if (permissionToRemove is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = "Unable to remove non-existent permission."
            });

        _applicationDbContext.Permissions.Remove(permissionToRemove);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new RemovePermissionResult { Successful = true });
    }

    [HttpPost]
    [Route("add-permission")]
    public async Task<IActionResult> AddPermission([FromBody] PermissionParameters permissionParameters)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Permissions)
            .SingleOrDefaultAsync(type => type.Id == permissionParameters.TaskTypeId &&
                                          type.UserId == currentUser.Id);

        if (taskType is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find task type by the following id '{permissionParameters.TaskTypeId}'"
            });

        var user = await _userManager.FindByNameAsync(permissionParameters.UserName);

        if (user is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find user by name '{permissionParameters.UserName}'."
            });

        var userId = Guid.Parse(user.Id);

        if (taskType.Permissions!.Any(permission => permission.UserId == userId))
            return BadRequest(new AddPermissionResult
            {
                Successful = false,
                Message = "User already has permission."
            });

        var persmission = new Permission { TaskTypeId = taskType.Id, UserId = userId };

        await _applicationDbContext.Permissions.AddAsync(persmission);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new AddPermissionResult { Successful = true });
    }

    [HttpGet]
    [Route("get-task-types")]
    public async Task<IActionResult> GetTaskTypes()
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);

        var taskTypes = (await _applicationDbContext.TaskTypes
                .Include(type => type.QuestionTypes)
                .Include(type => type.Permissions)
                .Where(type => type.UserId == currentUser.Id)
                .Select(type => type)
                .ToArrayAsync())
            .Select(type => type.Adapt<Entities.Views.TaskType>())
            .ToArray();

        return Ok(new GetTaskTypesResult { Successful = true, TaskTypes = taskTypes, Count = taskTypes.Length });
    }

    [HttpGet("get-task-type/{id:guid}")]
    public async Task<IActionResult> GetTaskType(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskTypeRaw = await _applicationDbContext.TaskTypes
            .Include(type => type.QuestionTypes)
            .Include(type => type.Permissions)
            .Include(type => type.TaskInstances)
            .Where(type => type.UserId == currentUser.Id && type.Id == id)
            .Select(type => type)
            .SingleOrDefaultAsync();

        if (taskTypeRaw is null)
            return BadRequest(new GetTaskTypeResult
            {
                Successful = false,
                Message = "Unable to find task type by id."
            });

        var taskType = taskTypeRaw.Adapt<Entities.Views.TaskType>();

        return Ok(new GetTaskTypeResult { Successful = true, TaskType = taskType });
    }

    [HttpPost]
    [Route("add-task-type")]
    public async Task<IActionResult> AddTaskType([FromBody] TaskTypeParameters taskTypeParameters)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskTypes = await _applicationDbContext.TaskTypes.Include(type => type.QuestionTypes).ToArrayAsync();

        if (taskTypes.Any(taskType => taskType.Name == taskTypeParameters.Name))
            return Conflict(new AddTaskTypeResult
            {
                Successful = false,
                Message = $"The task type with the same name '{taskTypeParameters.Name}' already exists."
            });

        foreach (var constraint in taskTypeParameters.Questions
                     .SelectMany(questionDto =>
                         new[] { questionDto.DynamicContentConstraint, questionDto.AnswerConstraint }))
            if (!constraint.IsValidRegex())
                return BadRequest(new AddTaskTypeResult
                {
                    Successful = false,
                    Message = $"Cannot use '{constraint}' as a pattern."
                });

        var taskTypeId = Guid.NewGuid();
        var taskType = taskTypeParameters.Adapt<TaskType>() with
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            QuestionTypes = taskTypeParameters.Questions
                .Select(dto => dto.Adapt<QuestionType>() with
                {
                    Id = Guid.NewGuid(),
                    TaskTypeId = taskTypeId,
                })
                .ToArray()
        };

        await _applicationDbContext.TaskTypes.AddAsync(taskType);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new AddTaskTypeResult { Successful = true });
    }

    [HttpPost]
    [Route("update-task-type")]
    public async Task<IActionResult> UpdateTaskTypeAsync([FromBody] UpdateTaskTypeParameters parameters)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskType = await _applicationDbContext.TaskTypes
            .SingleOrDefaultAsync(taskType => taskType.UserId == currentUser.Id &&
                                              taskType.Id == parameters.TaskTypeId);

        if (taskType is null)
            return BadRequest(new UpdateTaskTypeResult
            {
                Successful = false,
                Message = "Unable to find task type by id."
            });

        _applicationDbContext.Entry(taskType).State = EntityState.Detached;

        var updatedTaskType = taskType with
        {
            Name = parameters.Name,
            SolutionsCount = parameters.SolutionsCount,
            Payment = parameters.Payment,
            Instruction = parameters.Instruction,
            AccessType = parameters.AccessType
        };
        _applicationDbContext.Update(updatedTaskType);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new UpdateTaskTypeResult { Successful = true });
    }

    [HttpPost]
    [Route("add-task-instances")]
    public async Task<IActionResult> AddTaskInstances([FromBody] TaskInstancesParameters taskInstancesParameters)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.QuestionTypes)
            .Where(type => type.UserId == currentUser.Id)
            .SingleOrDefaultAsync(type => type.Id == taskInstancesParameters.TaskTypeId);

        if (taskType is null)
            return BadRequest(new AddTaskInstancesResult
            {
                Successful = false,
                Message = $"Cannot find task type by the following id: {taskInstancesParameters.TaskTypeId}."
            });

        foreach (var (questionTypeId, questionInstanceDtos) in taskInstancesParameters.QuestionDictionary)
        {

            if (taskType.QuestionTypes!.All(type => type.Id != questionTypeId))
                return BadRequest(new AddTaskInstancesResult
                {
                    Successful = false,
                    Message = $"Unknown question type: {questionTypeId}."
                });

            var questionType = taskType.QuestionTypes!.Single(type => type.Id == questionTypeId);
            var contraintRegex = new Regex(questionType.DynamicContentConstraint);

            if (questionInstanceDtos.All(instanceDto => contraintRegex.IsFullMatch(instanceDto.Content)))
                continue;

            var mismatchedContent = string.Join(", ", questionInstanceDtos
                .Where(instanceDto => !contraintRegex.IsFullMatch(instanceDto.Content))
                .Select(instanceDto => instanceDto.Content));

            return BadRequest(new AddTaskInstancesResult
            {
                Successful = false,
                Message =
                    $"The following instances doesn't match the pattern: ({questionType.DynamicContentConstraint}) {mismatchedContent}"
            });
        }

        var maxQuestionInstancesCount = taskInstancesParameters.QuestionDictionary.Values
            .Max(instanceDtos => instanceDtos.Count);
        var taskTypes = Enumerable
            .Range(0, maxQuestionInstancesCount)
            .Select(index =>
            {
                var taskInstanceId = Guid.NewGuid();
                var taskInstance = new TaskInstance
                {
                    Id = taskInstanceId,
                    TaskTypeId = taskType.Id,
                    QuestionInstances = taskInstancesParameters.QuestionDictionary
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

        return Ok(new AddTaskInstancesResult { Successful = true });
    }
}
