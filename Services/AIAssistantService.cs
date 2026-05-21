using System.Text;
using System.Text.Json;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;

namespace AgileTaskManager.Services;

public class AIAssistantService : IAIAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AIAssistantService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GenerateTaskDescriptionAsync(string title, string? projectContext = null)
    {
        var apiKey = _configuration["AISettings:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
            return await CallOpenAiAsync($"Generate a detailed task description for: {title}. Project context: {projectContext ?? "N/A"}");

        return GenerateDescriptionFromTitle(title, projectContext);
    }

    public async Task<string> SuggestTaskTitleAsync(string description)
    {
        var apiKey = _configuration["AISettings:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
            return await CallOpenAiAsync($"Suggest a concise task title for this description: {description}");

        return GenerateTitleFromDescription(description);
    }

    public async Task<List<string>> SuggestTaskTagsAsync(string title, string description)
    {
        var apiKey = _configuration["AISettings:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            var result = await CallOpenAiAsync($"Suggest 3-5 tags for a task titled '{title}' with description: {description}. Return as comma-separated list.");
            return result.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        return GenerateTagsFromContent(title, description);
    }

    private async Task<string> CallOpenAiAsync(string prompt)
    {
        var endpoint = _configuration["AISettings:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        var apiKey = _configuration["AISettings:ApiKey"] ?? "";
        var model = _configuration["AISettings:Model"] ?? "gpt-3.5-turbo";
        var maxTokens = int.TryParse(_configuration["AISettings:MaxTokens"], out var mt) ? mt : 500;
        var temperature = double.TryParse(_configuration["AISettings:Temperature"], out var t) ? t : 0.7;

        var payload = new
        {
            model,
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = maxTokens,
            temperature
        };

        var json = JsonSerializer.Serialize(payload);
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        return text?.Trim() ?? "";
    }

    private string GenerateDescriptionFromTitle(string title, string? projectContext)
    {
        var lowerTitle = title.ToLowerInvariant();

        if (lowerTitle.Contains("login") || lowerTitle.Contains("auth") || lowerTitle.Contains("sign"))
            return "Implement user authentication functionality including login, logout, and session management. Ensure proper security measures are in place such as password hashing and secure token handling.";

        if (lowerTitle.Contains("dashboard") || lowerTitle.Contains("home"))
            return "Create a comprehensive dashboard view that displays key metrics, charts, and navigation elements. The dashboard should be responsive and provide an intuitive user experience.";

        if (lowerTitle.Contains("api") || lowerTitle.Contains("endpoint"))
            return "Develop RESTful API endpoints with proper HTTP methods, status codes, and error handling. Include input validation, authentication, and comprehensive documentation.";

        if (lowerTitle.Contains("database") || lowerTitle.Contains("db"))
            return "Design and implement database schema with proper relationships, indexes, and constraints. Ensure data integrity and optimize query performance.";

        if (lowerTitle.Contains("test") || lowerTitle.Contains("testing"))
            return "Write comprehensive unit tests and integration tests to ensure code quality and functionality. Include edge cases and error scenarios.";

        if (lowerTitle.Contains("ui") || lowerTitle.Contains("interface") || lowerTitle.Contains("frontend"))
            return "Design and implement user interface components with modern design principles. Ensure responsive layout, accessibility, and smooth user interactions.";

        if (lowerTitle.Contains("bug") || lowerTitle.Contains("fix") || lowerTitle.Contains("issue"))
            return "Investigate and resolve the reported issue. Identify the root cause, implement a fix, and ensure the solution doesn't introduce new problems.";

        if (lowerTitle.Contains("feature") || lowerTitle.Contains("implement") || lowerTitle.Contains("add"))
            return "Implement the requested feature following best practices and coding standards. Ensure the implementation is scalable, maintainable, and well-documented.";

        if (lowerTitle.Contains("refactor") || lowerTitle.Contains("optimize") || lowerTitle.Contains("improve"))
            return "Refactor the existing code to improve performance, readability, and maintainability. Follow SOLID principles and eliminate code smells.";

        var context = string.IsNullOrEmpty(projectContext) ? "" : $" in the context of {projectContext}";
        return $"Complete the task: {title}{context}. Ensure all requirements are met and the solution follows established patterns and best practices.";
    }

    private string GenerateTitleFromDescription(string description)
    {
        var lowerDesc = description.ToLowerInvariant();

        if (lowerDesc.Contains("login") || lowerDesc.Contains("authentication"))
            return "Implement User Authentication";
        if (lowerDesc.Contains("dashboard") || lowerDesc.Contains("main page"))
            return "Create Dashboard Interface";
        if (lowerDesc.Contains("database") || lowerDesc.Contains("schema"))
            return "Design Database Schema";
        if (lowerDesc.Contains("api") || lowerDesc.Contains("endpoint"))
            return "Develop API Endpoints";
        if (lowerDesc.Contains("test") || lowerDesc.Contains("testing"))
            return "Write Unit Tests";
        if (lowerDesc.Contains("bug") || lowerDesc.Contains("fix"))
            return "Fix Reported Bug";
        if (lowerDesc.Contains("feature") || lowerDesc.Contains("implement"))
            return "Implement New Feature";
        return "Complete Task Implementation";
    }

    private List<string> GenerateTagsFromContent(string title, string description)
    {
        var tags = new HashSet<string>();
        var content = (title + " " + description).ToLowerInvariant();

        if (content.Contains("api")) tags.Add("API");
        if (content.Contains("database") || content.Contains("sql")) tags.Add("Database");
        if (content.Contains("frontend") || content.Contains("ui")) tags.Add("Frontend");
        if (content.Contains("backend") || content.Contains("server")) tags.Add("Backend");
        if (content.Contains("test") || content.Contains("testing")) tags.Add("Testing");
        if (content.Contains("security") || content.Contains("auth")) tags.Add("Security");
        if (content.Contains("performance") || content.Contains("optimize")) tags.Add("Performance");
        if (content.Contains("bug") || content.Contains("fix")) tags.Add("Bug-Fix");
        if (content.Contains("feature") || content.Contains("implement")) tags.Add("Feature");
        if (content.Contains("refactor") || content.Contains("improve")) tags.Add("Refactoring");
        if (content.Contains("documentation") || content.Contains("docs")) tags.Add("Documentation");
        if (content.Contains("urgent") || content.Contains("critical")) tags.Add("High-Priority");
        if (content.Contains("minor") || content.Contains("low")) tags.Add("Low-Priority");

        return tags.ToList();
    }
}
