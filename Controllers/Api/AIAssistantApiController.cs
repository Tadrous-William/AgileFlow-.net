using AgileTaskManager.Services;
using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/ai-assistant")]
[Authorize]
public class AIAssistantController : ControllerBase
{
    private readonly IAIAssistantService _aiService;

    public AIAssistantController(IAIAssistantService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("generate-description")]
    public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescriptionRequestViewModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required." });

        try
        {
            var description = await _aiService.GenerateTaskDescriptionAsync(request.Title, request.ProjectContext);
            return Ok(new { description });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to generate description.", details = ex.Message });
        }
    }

    [HttpPost("suggest-title")]
    public async Task<IActionResult> SuggestTitle([FromBody] SuggestTitleRequestViewModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Description is required." });

        try
        {
            var title = await _aiService.SuggestTaskTitleAsync(request.Description);
            return Ok(new { title });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to suggest title.", details = ex.Message });
        }
    }

    [HttpPost("suggest-tags")]
    public async Task<IActionResult> SuggestTags([FromBody] SuggestTagsRequestViewModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Title or description is required." });

        try
        {
            var tags = await _aiService.SuggestTaskTagsAsync(request.Title ?? "", request.Description ?? "");
            return Ok(new { tags });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to suggest tags.", details = ex.Message });
        }
    }
}

public class GenerateDescriptionRequestViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? ProjectContext { get; set; }
}

public class SuggestTitleRequestViewModel
{
    public string Description { get; set; } = string.Empty;
}

public class SuggestTagsRequestViewModel
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}
