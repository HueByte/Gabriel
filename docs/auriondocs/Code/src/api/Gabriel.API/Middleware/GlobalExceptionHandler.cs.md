# GlobalExceptionHandler

> **File:** `src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs`  
> **Kind:** class

```csharp
public class GlobalExceptionHandler : IExceptionHandler
```


Centralizes the translation of domain exceptions into HTTP responses. It maps common domain- and runtime-exceptions to appropriate HTTP status codes and returns a RFC 7807 ProblemDetails payload, ensuring consistent error responses across the API. Use this when you want a single, testable path for exception handling rather than sprinkling try/catch blocks throughout controllers.

## Remarks

Acts as the single point of translation between domain-layer failures and client-facing errors. It decides the HTTP status and user-facing title, and it uses a generic detail for server errors to avoid leaking sensitive information while still logging the full exception for diagnostics. This abstraction cleanly separates error presentation from business logic and works with the exception handler middleware registered in Program.cs.

## Notes

- 500 responses do not include exception details; the original exception is logged at error level for troubleshooting.
- Non-500 responses map known domain exceptions to specific statuses (NotFound -> 404, Domain/Argument -> 400, Unauthorized -> 401), while all other exceptions fall back to 500.
- Ensure the handler is registered in the DI container and wired into the middleware pipeline (AddExceptionHandler and UseExceptionHandler); without this wiring, exceptions won't be translated.