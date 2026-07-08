# GlobalExceptionHandler

> **File:** `src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs`  
> **Kind:** class

```csharp
public class GlobalExceptionHandler : IExceptionHandler
```


GlobalExceptionHandler serves as the single translation layer between domain exceptions and HTTP responses. Implementing IExceptionHandler, it is registered in the DI container via `AddExceptionHandler<GlobalExceptionHandler>`() and wired into the HTTP pipeline with app.UseExceptionHandler() so that all unhandled exceptions flowing through the pipeline are captured in one place. The handler maps a set of domain-specific exceptions to appropriate HTTP status codes and ProblemDetails, logs the outcome, and returns a structured JSON payload to clients.

When an exception occurs, the handler categorizes it into one of several outcomes: 404 for NotFoundException, 400 for DomainException or ArgumentException, 401 for UnauthorizedAccessException, and 500 for anything else. For non-500 errors, it logs an informational message including the exception type and message; for 500 errors, it logs the exception as an error. It then constructs a ProblemDetails object containing the HTTP status, a short title, a detailed message (the exception message for non-500s, or a generic "An unexpected error occurred." for 500s), and the request path as the Instance. This ProblemDetails object is serialized to JSON and written to the response body with the corresponding status code, and the method returns true to indicate the exception has been handled.

This approach provides a consistent, API-friendly error surface across the application, ensuring clients receive structured error information while the server maintains a centralized, predictable error-handling policy.