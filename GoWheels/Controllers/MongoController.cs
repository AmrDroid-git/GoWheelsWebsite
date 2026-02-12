using GoWheels.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoWheels.Controllers;

[ApiController]
[Route("api/mongo")]
public class MongoController : ControllerBase
{
    private readonly MongoPostsReadService _service;

    public MongoController(MongoPostsReadService service)
    {
        _service = service;
    }

    [HttpGet("trending")]
    public async Task<IActionResult> Trending()
    {
        var result = await _service.GetTrendingPostsAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var post = await _service.GetByIdAsync(id);
        if (post == null) return NotFound();
        return Ok(post);
    }
}