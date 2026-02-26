using ContractManagementSystem.Business.Exceptions;
using System.Text.Json;

namespace ContractManagementSystem.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
         
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(
                context,
                500,
                "An unexpected error occurred. Please try again later."
            );
        }
    }

    private static Task HandleExceptionAsync(
           HttpContext context,
           int statusCode,
           string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

}