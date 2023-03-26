using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;
using DataMarkup.Entities.Parameters.TaskManager;
using DataMarkup.Entities.Views.TaskManager;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Permission = DataMarkup.Api.Models.Database.Access.Permission;
using QuestionType = DataMarkup.Api.Models.Database.Markup.QuestionType;
using TaskType = DataMarkup.Api.Models.Database.Markup.TaskType;

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
    public async Task<IActionResult> RemovePermission([FromBody] RemovePermissionParameters removePermissionParameters)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Permissions)
            .SingleOrDefaultAsync(type => type.Id == removePermissionParameters.TaskTypeId &&
                                          type.UserId == currentUser.Id);

        if (taskType is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find task type by the following id '{removePermissionParameters.TaskTypeId}'"
            });

        var user = await _userManager.FindByNameAsync(removePermissionParameters.Username);

        if (user is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find user by name '{removePermissionParameters.Username}'."
            });

        var permissionToRemove = await _applicationDbContext.Permissions
            .SingleOrDefaultAsync(permission => permission.UserId == user.Id &&
                                       permission.TaskTypeId == removePermissionParameters.TaskTypeId);

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
    public async Task<IActionResult> AddPermission([FromBody] AddPermissionParameters addPermissionParameters)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Permissions)
            .SingleOrDefaultAsync(type => type.Id == addPermissionParameters.TaskTypeId &&
                                          type.UserId == currentUser.Id);

        if (taskType is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find task type by the following id '{addPermissionParameters.TaskTypeId}'"
            });

        var user = await _userManager.FindByNameAsync(addPermissionParameters.Username);

        if (user is null)
            return BadRequest(new RemovePermissionResult
            {
                Successful = false,
                Message = $"Unable to find user by name '{addPermissionParameters.Username}'."
            });

        if (taskType.Permissions!.Any(permission => permission.UserId == user.Id))
            return BadRequest(new AddPermissionResult
            {
                Successful = false,
                Message = "User already has permission."
            });

        var persmission = new Permission { TaskTypeId = taskType.Id, UserId = user.Id, Username = user.UserName };

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

    [HttpGet("instances/{typeId:guid}&{page:int}&{pageSize:int}")]
    public async Task<IActionResult> GetTaskInstances(Guid typeId,
        [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int pageSize = 100)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var taskType = await _applicationDbContext.TaskTypes.SingleOrDefaultAsync(type => type.Id == typeId);

        if (taskType is null)
            return BadRequest(new GetTaskInstancesResult
            {
                Successful = false,
                Message = "Unable to find task type by id."
            });

        var taskInstancesQuery = _applicationDbContext.TaskInstances
            .Include(instance => instance.QuestionInstances)
            .Include(instance => instance.Solutions)!
                .ThenInclude(solution => solution.User)
            .Include(instance => instance.Solutions)!
                .ThenInclude(solution => solution.Answers)
            .Include(instance => instance.TaskType)
                .ThenInclude(type => type!.QuestionTypes)
            .Where(instance => instance.TaskType!.UserId == currentUser.Id && instance.TaskTypeId == typeId);
        var taskInstances = await taskInstancesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var outputTasks = taskInstances
            .Select(instance =>
            {
                var questions = instance.TaskType!.QuestionTypes!
                    .Join(instance.QuestionInstances!,
                        questionType => questionType.Id,
                        questionInstance => questionInstance.QuestionTypeId,
                        (questionType, questionInstance) => (questionType, questionInstance))
                    .GroupJoin(instance.Solutions!
                            .SelectMany(solution => solution.Answers!
                                .Select(answer => (solution.User!.UserName, answer))),
                        questionItem => questionItem.questionInstance.Id,
                        answerItem => answerItem.answer.QuestionInstanceId,
                        (item, answers) => (item.questionType, item.questionInstance, answers))
                    .Select(item =>
                    {
                        var answers = item.answers
                            .Select(answer => new Entities.Views.Answer
                            {
                                Username = answer.UserName,
                                Content = answer.answer.Content
                            })
                            .ToArray();
                        var answerStatistics = answers
                            .GroupBy(answer => answer.Content)
                            .Select(group => new Entities.Views.AnswerStatistic
                            {
                                Answer = group.Key,
                                Frequency = group.Count() / (decimal)answers.Length
                            })
                            .OrderByDescending(statistic => statistic.Frequency)
                            .ToArray();
                        var relevantAnswerStatistic = answerStatistics.FirstOrDefault();

                        var questionInfo = new Entities.Views.Question
                        {
                            QuestionWording = item.questionType.StaticContent,
                            Content = item.questionInstance.Content,
                            Image = item.questionInstance.ImageSource,
                            Answers = answers,
                            AnswerStatistics = answerStatistics,
                            RelevantAnswerStatistic = relevantAnswerStatistic
                        };

                        return questionInfo;
                    })
                    .ToArray();

                var taskInfo = new Entities.Views.Task
                {
                    Id = instance.Id,
                    SolutionCount = instance.Solutions!.Count,
                    MaxSolutionCount = instance.TaskType!.SolutionsCount,
                    TaskSolvingPercent = instance.TaskType.SolutionsCount is 0 ?
                        0 :
                        instance.Solutions.Count / (decimal)instance.TaskType.SolutionsCount,
                    Questions = questions
                };

                return taskInfo;
            })
            .ToArray();

        var totalCount = await taskInstancesQuery.CountAsync();
        var fullySolvedTasksCont = await _applicationDbContext.TaskInstances
            .Join(_applicationDbContext.Solutions,
                instance => instance.Id,
                solution => solution.TaskInstanceId,
                (instance, solution) => new { instance, solution })
            .Where(item => item.instance.TaskTypeId == typeId)
            .GroupBy(item => item.instance.Id)
            .CountAsync(group => group.Count() == taskType.SolutionsCount);
        var totalPageCount = (totalCount / pageSize) + (totalCount % pageSize is 0 ? 0 : 1);

        return Ok(new GetTaskInstancesResult
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPageCount = totalPageCount,
            FullySolvedTasksCont = fullySolvedTasksCont,
            Successful = true,
            Tasks = outputTasks
        });
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
        var taskInstances = Enumerable
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

        await _applicationDbContext.TaskInstances.AddRangeAsync(taskInstances);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new AddTaskInstancesResult { Successful = true });
    }

    [HttpDelete]
    [Route("remove-task-instance/{instanceId:guid}")]
    public async Task<IActionResult> RemoveTaskInstance(Guid instanceId)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);

        var instance = await _applicationDbContext.TaskInstances
            .Include(instance => instance.TaskType)
            .Include(instance => instance.QuestionInstances)
            .Include(instance => instance.Solutions)!
            .ThenInclude(solution => solution.Answers)
            .Where(instance => instance.TaskType!.UserId == currentUser.Id &&
                               instance.Id == instanceId)
            .SingleOrDefaultAsync();

        if (instance is null)
            return BadRequest(new RemoveTaskInstanceResult
            {
                Successful = false,
                Message = "Unable to find your task instance by id."
            });

        var answers = instance.Solutions!.SelectMany(solution => solution.Answers!).OfType<object>().ToArray();

        if (answers.Any())
            _applicationDbContext.RemoveRange(answers);

        _applicationDbContext.Solutions.RemoveRange(instance.Solutions!);
        _applicationDbContext.QuestionInstances.RemoveRange(instance.QuestionInstances!);
        _applicationDbContext.TaskInstances.Remove(instance);

        await _applicationDbContext.SaveChangesAsync();

        return Ok(new RemoveTaskInstanceResult { Successful = true });
    }
}
