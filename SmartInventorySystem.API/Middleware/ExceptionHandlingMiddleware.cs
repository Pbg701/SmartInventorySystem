using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SmartInventorySystem.API.Middleware
{

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ValidationException validationException => new
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationException.Errors.Select(e => e.ErrorMessage).ToList()
                },
                DbUpdateConcurrencyException => new
                {
                    Success = false,
                    Message = "Concurrency conflict occurred",
                    Errors = new List<string> { "The data you're trying to update has been modified by another user" }
                },
                UnauthorizedAccessException => new
                {
                    Success = false,
                    Message = "Unauthorized access",
                    Errors = new List<string> { "You don't have permission to perform this action" }
                },
                _ => new
                {
                    Success = false,
                    Message = "An error occurred while processing your request",
                    Errors = new List<string> { exception.Message }
                }
            };

            context.Response.StatusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
