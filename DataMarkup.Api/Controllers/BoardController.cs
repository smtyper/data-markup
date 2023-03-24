using System.Text.RegularExpressions;
using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;
using DataMarkup.Entities;
using DataMarkup.Entities.Parameters.Board;
using DataMarkup.Entities.Views.Board;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace DataMarkup.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public BoardController(UserManager<User> userManager, ApplicationDbContext applicationDbContext)
    {
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
    }

    [HttpGet]
    [Route("get-available-task-types")]
    public async Task<IActionResult> GetAvailableTaskTypes()
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);

        var availableTaskTypes = await _applicationDbContext.TaskTypes
            .Include(type => type.Permissions)
            .Include(type => type.QuestionTypes)
            .Where(type => type.AccessType == AccessType.Free ||
                           type.Permissions!.Any(permission => permission.UserId == Guid.Parse(currentUser.Id)) ||
                           type.UserId == currentUser.Id)
            .Select(type => type.Adapt<Entities.Views.TaskType>())
            .ToArrayAsync();

        return Ok(new GetAvailableTaskTypesResult
        {
            Successful = true,
            TaskTypes = availableTaskTypes,
            Count = availableTaskTypes.Length
        });
    }

    [HttpGet]
    [Route("get-task")]
    public async Task<IActionResult> GetTask([FromQuery]Guid taskTypeId)
    {
        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var currentUserId = Guid.Parse(currentUser.Id);
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Permissions)
            .SingleOrDefaultAsync(type => type.Id == taskTypeId);

        if (taskType is null)
            return BadRequest(new GetTaskResult
            {
                Successful = false,
                Message = $"There is no task by folliwing id: {taskTypeId}"
            });

        if (taskType.AccessType is AccessType.WhiteList &&
            taskType.Permissions!.All(persmission => persmission.UserId == currentUserId))
            return BadRequest(new GetTaskResult
            {
                Successful = false,
                Message = "You have no permission to take this task."
            });

        var taskInstances = _applicationDbContext.TaskInstances
            .Where(instance => instance.TaskTypeId == taskTypeId)
            .Include(instance => instance.Solutions)
            .Include(instance => instance.QuestionInstances)!
            .ThenInclude(questionInstance => questionInstance.QuestionType)
            .AsEnumerable()
            .Where(instance => instance.Solutions!.Count < taskType.SolutionsCount &&
                               instance.Solutions!.All(solution => solution.UserId != currentUserId))
            .ToArray();

        if (!taskInstances.Any())
            return Ok(new GetTaskResult
            {
                Successful = true,
                Message = "There are no more tasks of this type for you."
            });

        var taskInstance = taskInstances.First();

        var resultTask = new Entities.Views.Task
        {
            InstanceId = taskInstance.Id,
            Instruction = taskType.Instruction,
            Questions = taskInstance.QuestionInstances!
                .Select(questionInstance => new Entities.Views.Question
                {
                    InstanceId = questionInstance.Id,
                    Description = questionInstance.QuestionType!.StaticContent,
                    Content = questionInstance.Content,
                    AnswerDescription = questionInstance.QuestionType!.AnswerDescription,
                    AnswerPattern = questionInstance.QuestionType!.AnswerConstraint
                })
                .ToArray()
        };

        return Ok(new GetTaskResult
        {
            Successful = true,
            Task = resultTask
        });
    }

    [HttpPost]
    [Route("add-solution")]
    public async Task<IActionResult> AddSolution([FromBody] SolutionParameters solutionParameters)
    {
        var taskInstanceId = solutionParameters.TaskInstanceId!.Value;

        var currentUser = await _userManager.GetUserAsync(HttpContext);
        var currentUserId = Guid.Parse(currentUser.Id);

        var taskInstance = await _applicationDbContext.TaskInstances
            .Include(instance => instance.Solutions)
            .Include(instance => instance.QuestionInstances)
            .Include(instance => instance.TaskType)
            .ThenInclude(type => type!.QuestionTypes)
            .SingleOrDefaultAsync(instance => instance.Id == taskInstanceId);

        if (taskInstance is null)
            return BadRequest(new AddSolutionResult
            {
                Successful = false,
                Message = $"There is no task instance by the following id: {taskInstanceId}"
            });

        var taskType = await _applicationDbContext.TaskTypes
            .Include(taskType => taskType.Permissions)
            .Include(taskType => taskType.QuestionTypes)
            .SingleOrDefaultAsync(taskType => taskType.Id == taskInstance.TaskTypeId);

        if (taskType is null)
            return BadRequest(new AddSolutionResult
            {
                Successful = false,
                Message = "Unknown task type."
            });

        if (taskType.AccessType is AccessType.WhiteList &&
            taskType.Permissions!.All(persmission => persmission.UserId != currentUserId) &&
            taskType.UserId != currentUserId.ToString())
            return BadRequest(new AddSolutionResult
            {
                Successful = false,
                Message = "You have no permission to solve this task."
            });

        if (taskInstance.Solutions!.Any(solution => solution.UserId == currentUserId))
            return Conflict(new AddSolutionResult
            {
                Successful = false,
                Message = "You have already solved this task."
            });

        if (taskInstance.Solutions!.Count >= taskType.SolutionsCount)
            return Conflict(new AddSolutionResult
            {
                Successful = false,
                Message = "The maximum number of solutions has been reached."
            });

        var questionsMap = taskInstance.QuestionInstances!
            .Join(taskType.QuestionTypes!,
                instance => instance.QuestionTypeId,
                type => type.Id,
                (instance, type) => (type, instance))
            .LeftJoin(solutionParameters.Answers,
                pair => pair.instance.Id,
                answer => answer.QuestionInstanceId,
                pair => (pair, null)!,
                (pair, answer) => (pair, answer))
            .ToArray();

        if (questionsMap.Any(item => item.answer is null))
            return Conflict(new AddSolutionResult
            {
                Successful = false,
                Message = "The one of the questions is missed."
            });

        if (questionsMap.Any(item => !new Regex(item.pair.type.AnswerConstraint).IsFullMatch(item.answer.Content)))
        {
            var mismatchedAnswers = string.Join(", ", questionsMap
                .Where(item => !new Regex(item.pair.type.AnswerConstraint).IsFullMatch(item.answer.Content))
                .Select(item => $"{item.answer.Content} : {item.pair.type.AnswerConstraint}"));

            return BadRequest(new AddSolutionResult
            {
                Successful = false,
                Message = $"The following answers doesn't match the patterns: {mismatchedAnswers}."
            });
        }

        var solutionId = Guid.NewGuid();
        var solution = new Solution
        {
            Id = solutionId,
            TaskInstanceId = taskInstanceId,
            UserId = currentUserId,
            Answers = questionsMap
                .Select(map => new Answer
                {
                    Id = Guid.NewGuid(),
                    Content = map.answer.Content,
                    SolutionId = solutionId,
                    QuestionInstanceId = map.pair.instance.Id
                })
                .ToArray()
        };

        await _applicationDbContext.Solutions.AddAsync(solution);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new AddSolutionResult { Successful = true });
    }
}
