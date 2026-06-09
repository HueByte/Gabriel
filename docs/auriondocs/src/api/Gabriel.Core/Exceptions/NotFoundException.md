# NotFoundException

> **File:** `src/api/Gabriel.Core/Exceptions/NotFoundException.cs`  
> **Kind:** class

Represents a domain-level "not found" error for a specific resource and key. Throw this when an aggregate or resource lookup fails and you want to signal a 404-style absence; an upper-layer error handler or middleware typically translates this exception into an HTTP 404 response.

## Remarks
This is a lightweight exception that carries the resource name and lookup key so consumers (or error handlers) can construct a user-facing message or a structured response. It is intended to be thrown from application/service/repository code when an expected entity cannot be found, instead of returning null or an optional value in contexts where an exception-based flow is preferred.

## Example
```csharp
// In a repository or service when an entity is missing
if (user == null)
{
    throw new NotFoundException("User", userId);
}

// In an ASP.NET Core error handler you might translate this to a 404:
// (pseudo-code)
try
{
    // call into service layer
}
catch (NotFoundException ex)
{
    return Results.NotFound(new { resource = ex.Resource, key = ex.Key });
}
```

## Notes
- The exception message uses string interpolation of the key, so passing sensitive values as the key can leak them into logs or responses; avoid including secrets.
- The Key property is typed as object — prefer simple, well-serializable key types (int, string, Guid) to ease logging and response formatting.
- There is no special serialization constructor; depending on your serializer or remoting scenario you may need to add one if binary/remote serialization is required.
