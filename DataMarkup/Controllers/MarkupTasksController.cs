using System.Text.RegularExpressions;
using DataMarkup.Data;
using DataMarkup.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DataMarkup.Controllers;

public class MarkupTasksController : Controller
{
    private readonly DataMarkupContext _dataMarkupContext;

    public MarkupTasksController(DataMarkupContext dataMarkupContext) => _dataMarkupContext = dataMarkupContext;

    [HttpGet]
    public IActionResult Board() => View();

    [HttpGet]
    public IActionResult Create() => View(new MarkupTaskViewModel
    {
        CurrentQuestion = new MarkupQuestionViewModel(),
        Questions = new List<MarkupQuestionViewModel>()
    });

    [HttpPost]
    public IActionResult Create([FromBody]MarkupTaskViewModel currentTask)
    {
        var currentQuestion = currentTask.CurrentQuestion;

        if (currentQuestion.DynamicContentConstraint is null ||
            currentQuestion.AnswerDescription is null ||
            currentQuestion.AnswerConstraint is null)
        {
            ModelState.AddModelError("", "Fill in all values");

            return View(currentTask);
        }

        if (currentTask.Questions is null)
            currentTask.Questions = new List<MarkupQuestionViewModel> { currentQuestion };
        else
            currentTask.Questions.Add(currentQuestion);

        currentTask.CurrentQuestion = new MarkupQuestionViewModel();

        return View(currentTask);
    }
}
