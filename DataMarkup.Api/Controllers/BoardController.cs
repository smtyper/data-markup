﻿using System.Text.RegularExpressions;
using DataMarkup.Api.DbContexts;
using DataMarkup.Api.Models;
using DataMarkup.Api.Models.Database.Account;
using DataMarkup.Api.Models.Database.Markup;
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
        var currentUser = await GetCurrentUserAsync();

        var availableTaskTypes = await _applicationDbContext.TaskTypes
            .Include(type => type.Persmissions)
            .Include(type => type.QuestionTypes)
            .Where(type => type.AccessType == AccessType.Free ||
                           type.Persmissions!.Any(permission => permission.UserId == Guid.Parse(currentUser.Id)))
            .Select(type => type.Adapt<Models.Views.TaskType>())
            .ToArrayAsync();

        return Ok(availableTaskTypes);
    }

    [HttpGet]
    [Route("get-task")]
    public async Task<IActionResult> GetTask([FromQuery]Guid taskTypeId)
    {
        var currentUser = await GetCurrentUserAsync();
        var currentUserId = Guid.Parse(currentUser.Id);
        var taskType = await _applicationDbContext.TaskTypes
            .Include(type => type.Persmissions)
            .SingleOrDefaultAsync(type => type.Id == taskTypeId);

        if (taskType is null)
            return BadRequest($"There is no task by folliwing id: {taskTypeId}");

        if (taskType.AccessType is AccessType.WhiteList &&
            taskType.Persmissions!.All(persmission => persmission.UserId == currentUserId))
            return BadRequest("You have no permission to take this task.");

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
            return Ok("There is no tasks by this id to you.");

        var taskInstance = taskInstances.First();

        var resultTask = new Models.Views.Task
        {
            InstanceId = taskInstance.Id,
            Instruction = taskType.Instruction,
            Questions = taskInstance.QuestionInstances!
                .Select(questionInstance => new Models.Views.Question
                {
                    InstanceId = questionInstance.Id,
                    Description = questionInstance.QuestionType!.StaticContent,
                    Content = questionInstance.Content,
                    AnswerDescription = questionInstance.QuestionType!.AnswerDescription,
                    AnswerPattern = questionInstance.QuestionType!.AnswerConstraint
                })
                .ToArray()
        };

        return Ok(resultTask);
    }

    [HttpPost]
    [Route("add-solution")]
    public async Task<IActionResult> AddSolution([FromBody] Models.Dto.Board.Solution solutionParameters)
    {
        var taskInstanceId = solutionParameters.TaskInstanceId!.Value;

        var currentUser = await GetCurrentUserAsync();
        var currentUserId = Guid.Parse(currentUser.Id);

        var taskInstance = await _applicationDbContext.TaskInstances
            .Include(instance => instance.Solutions)
            .Include(instance => instance.QuestionInstances)
            .Include(instance => instance.TaskType)
            .ThenInclude(type => type!.QuestionTypes)
            .SingleOrDefaultAsync(instance => instance.Id == taskInstanceId);

        if (taskInstance is null)
            return BadRequest($"There is no task instance by the following id: {taskInstanceId}");

        if (taskInstance.TaskType!.AccessType is AccessType.WhiteList &&
            taskInstance.TaskType.Persmissions!.All(persmission => persmission.UserId != currentUserId))
            return BadRequest("You have no permission to solve this task.");

        if (taskInstance.Solutions!.Any(solution => solution.UserId == currentUserId))
            return BadRequest("You have already solved this task.");

        if (taskInstance.Solutions!.Count >= taskInstance.TaskType.SolutionsCount)
            return BadRequest("The maximum number of solutions has been reached.");

        var questionsMap = taskInstance.QuestionInstances!
            .Join(taskInstance.TaskType.QuestionTypes!,
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
            return BadRequest("The one of the questions is missed.");

        if (questionsMap.Any(item => !new Regex(item.pair.type.AnswerConstraint).IsFullMatch(item.answer.Content)))
        {
            var mismatchedAnswers = string.Join(", ", questionsMap
                .Where(item => !new Regex(item.pair.type.AnswerConstraint).IsFullMatch(item.answer.Content))
                .Select(item => $"{item.answer.Content} : {item.pair.type.AnswerConstraint}"));

            return BadRequest($"The following answers doesn't match the patterns: {mismatchedAnswers}.");
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