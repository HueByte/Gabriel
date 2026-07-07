# GlobalExceptionHandler

> **File:** `src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs`  
> **Kind:** class

```csharp
public class GlobalExceptionHandler : IExceptionHandler
```


GlobalExceptionHandler centralizes the translation of domain exceptions into HTTP responses by mapping NotFoundException to 404, DomainException and ArgumentException to 400, UnauthorizedAccessException to 401, and all other exceptions to 500, returning a ProblemDetails payload that includes status, title, and a contextual detail. It is wired into the ASP.NET Core middleware (registered via DI and invoked through UseExceptionHandler) to provide consistent error responses across the API without duplicating error handling in controllers.

## Remarks
By consolidating error translation in one class, this abstraction reduces boilerplate in controllers and ensures a uniform client-facing error surface. It also centralizes logging decisions: domain/client errors are logged at information, while unexpected errors are logged as errors, aiding operators while keeping sensitive internals hidden from clients. The ProblemDetails payload uses the request path as the instance to help clients correlate errors to specific requests.

## Notes
- The 500 internal server error path returns a generic message to avoid leaking internals; the actual exception is logged with its stack trace.
- If you introduce new domain exceptions, extend the mapping accordingly to preserve consistency.