# NotFoundException

> **File:** `src/api/Gabriel.Core/Exceptions/NotFoundException.cs`  
> **Kind:** class

Thrown when a requested aggregate or resource cannot be found. Use this exception from application or domain logic to indicate a missing entity; API layers or middleware can map it to an HTTP 404 response.

## Remarks
Holds the resource name and the lookup key so callers, loggers, and error handlers can produce consistent messages and structured diagnostics. The exception message is pre-formatted from the Resource and Key properties, and the properties are read-only so handlers can rely on them without further inspection.

## Example
```csharp
// In a service method
if (user == null)
    throw new NotFoundException("User", userId);

// In ASP.NET Core middleware or controller advice
try
{
    // call into service
}
catch (NotFoundException ex)
{
    // translate to 404 with a helpful message or structured body
    return Results.NotFound(new { resource = ex.Resource, key = ex.Key, message = ex.Message });
}
```

## Notes
- The exception formats the message using the Key object's string representation; if Key is null the interpolated value becomes an empty string.
- There is no constructor overload for inner exceptions or a deserialization constructor; add or wrap if you need to preserve an inner exception or support binary serialization.