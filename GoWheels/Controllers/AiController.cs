using GoWheels.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoWheels.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly MongoPostsReadService _mongoService;
    private readonly AiAnalysisService _aiService;

    public AiController(
        MongoPostsReadService mongoService,
        AiAnalysisService aiService)
    {
        _mongoService = mongoService;
        _aiService = aiService;
    }

    [HttpGet("post-summary/{postId}")]
    public async Task<IActionResult> GetPostSummary(string postId)
    {
        var post = await _mongoService.GetByIdAsync(postId);
        if (post == null)
            return NotFound("Post non trouvé");

        var commentsText = post.Comments
            .Take(8) // limiter pour le prompt
            .Select(c => c.Body)
            .ToList();

        // Construire prompt
        var prompt = $@"
You are an automotive assistant that summarizes a car listing and its user comments.

Car Listing:
Constructor: {post.Constructor}
Model: {post.ModelName}
Price: {post.Price} DT
Kilometrage: {post.Kilometrage} km

Specifications:
{string.Join(", ", post.Specifications.Select(kv => $"{kv.Key}: {kv.Value}"))}

User Comments:
{string.Join("\n", commentsText)}

Task:
1. Provide a short, user-friendly summary of this car.
2. Provide an overall sentiment impression based on the comments.
3. Add any helpful tips a buyer might want to know.

Return plain text with no JSON formatting.
";

        var result = await _aiService.AnalyzePostAsync(prompt);

        return Ok(new { summary = result });
    }
}
