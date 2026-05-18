using Gabriel.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Middleware;

// Single point of translation between domain exceptions and HTTP responses.
// Registered via builder.Services.AddExceptionHandler<GlobalExceptionHandler>()
// + app.UseExceptionHandler() in Program.cs.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException             => (StatusCodes.Status404NotFound, "Not Found"),
            DomainException               => (StatusCodes.Status400BadRequest, "Bad Request"),
            ArgumentException             => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedAccessException   => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _                             => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogInformation("Handled {ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path,
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
