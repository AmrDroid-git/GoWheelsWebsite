using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPostsService _postsService;

    public HomeController(ILogger<HomeController> logger, IPostsService postsService)
    {
        _logger = logger;
        _postsService = postsService;
    }

    public async Task<IActionResult> Index()
    {
        var recentPosts = await _postsService.GetRecentPostsAsync(12);
        return View(recentPosts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}