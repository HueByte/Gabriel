# GlobalExceptionHandler

> **File:** `src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs`  
> **Kind:** class

Translates exceptions thrown during request processing into standardized HTTP ProblemDetails responses and logs them. Use this class as the single centralized exception-to-response translator for the API (registered via the application's service collection and wired into the exception-handling middleware) so callers always receive a consistent JSON error payload and appropriate status code.

## Remarks
This implementation maps specific exception types to HTTP status codes (NotFoundException -> 404, DomainException/ArgumentException -> 400, UnauthorizedAccessException -> 401, all others -> 500) and uses ProblemDetails to produce an RFC-compliant error body. It logs unhandled (500) exceptions as errors and other mapped exceptions as informational events. The handler writes a generic detail for 500 responses while returning the exception message for non-500 statuses, and it sets ProblemDetails.Instance to the request path.

## Example
```csharp
// In Program.cs (registration + middleware wiring)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();

// When an action throws a DomainException, the client receives a 400 with a ProblemDetails JSON body.
```

## Notes
- The handler always returns true (exception considered handled), so no further exception handlers will run after this one.
- For non-500 responses the exception.Message is included in the ProblemDetails.Detail; consider sanitizing messages if they may leak sensitive information.
- CancellationToken passed into the method is forwarded to the JSON write operation; the response write may be canceled by the request's cancellation.
