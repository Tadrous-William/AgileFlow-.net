using System.Net;
using System.Text.Json;
using AgileTaskManager.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgileTaskManager.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Global exception: {Message}", context.Exception.Message);
        
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        
        context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller);
        context.ActionDescriptor.RouteValues.TryGetValue("action", out var action);

        var errorResponse = new ErrorResponse
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = isDevelopment ? context.Exception.Message : "An internal server error occurred.",
            RequestId = context.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        // Add detailed error info in development
        if (isDevelopment)
        {
            errorResponse.Details = new Dictionary<string, object>
            {
                ["ExceptionType"] = context.Exception.GetType().Name,
                ["StackTrace"] = context.Exception.StackTrace ?? "No stack trace available",
                ["Controller"] = controller ?? "Unknown",
                ["Action"] = action ?? "Unknown"
            };
        }

        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}
