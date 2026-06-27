# GlobalExceptionHandler

> **File:** `src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs`  
> **Kind:** class

Translates domain and common exceptions into HTTP ProblemDetails responses and appropriate status codes. Use this class as the centralized exception-to-response translator for the ASP.NET Core pipeline by registering it with dependency injection (builder.Services.`AddExceptionHandler<GlobalExceptionHandler>`()) and enabling the exception handler middleware (app.UseExceptionHandler()).

## Remarks
Centralizes mapping of exceptions to HTTP responses so controllers and other layers can throw domain-specific exceptions without caring about HTTP details. Handled exceptions are mapped to friendly ProblemDetails objects (including Status, Title, Detail and Instance) and logged at Information level; unexpected exceptions are mapped to a generic 500 response and logged as errors. Implementing this mapping in one place ensures consistent client-facing error shapes and logging behavior across the app.

## Example
```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);
// register the handler implementation
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();
// enable the exception-handler middleware so exceptions are routed to the registered handler
app.UseExceptionHandler();

app.MapGet("/", () => "Hello World!");
app.Run();
```

## Notes
- Known exception-to-status mappings: NotFoundException -> 404, DomainException/ArgumentException -> 400, UnauthorizedAccessException -> 401, otherwise 500.
- For 500 (internal) errors the response Detail is a generic message ("An unexpected error occurred.") — the actual exception message is not exposed to the client.
- The handler always writes a ProblemDetails JSON response, sets the response.StatusCode, and returns true to indicate the exception was handled.