# NotFoundException

> **File:** `src/api/Gabriel.Core/Exceptions/NotFoundException.cs`  
> **Kind:** class

```csharp
public class NotFoundException : Exception
```


NotFoundException signals that a requested aggregate could not be located. It carries the name of the missing resource and the key used to locate it, enabling precise diagnostics and consistent not-found handling across the domain and API layers (for example, mapping to HTTP 404 responses).

## Remarks
NotFoundException encapsulates a not-found scenario as a concrete domain exception rather than a loose error. The Resource and Key properties provide actionable context to error handlers and user-facing responses without exposing implementation details. This pattern supports centralized not-found handling and consistent user messages across services.

## Example
```csharp
Guid id = Guid.NewGuid();
throw new NotFoundException("ResourceName", id);
```

## Notes
- Be mindful that Key is of type object; its ToString() is used in the message, so ensure it yields meaningful identifiers.
- Catch NotFoundException at boundaries where a missing resource should translate to a 404, rather than catching a generic Exception.
- The exception is intended for domain-level not-found scenarios; avoid misusing for unrelated missing data.