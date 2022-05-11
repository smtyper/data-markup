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
    public IActionResult Create() => View(new MarkupTaskViewModel { Questions = new List<MarkupQuestionViewModel>() });

    [HttpGet]
    public IActionResult Create(MarkupTaskViewModel currentModelState) => View();
}
