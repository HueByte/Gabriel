# DomainException

> **File:** `src/api/Gabriel.Core/Exceptions/DomainException.cs`  
> **Kind:** class

```csharp
public class DomainException : Exception
```


DomainException is the base class for domain-rule violations. Use it to represent business-rule violations that result from a client's request so the API can respond with a 400 Bad Request instead of a generic server error; when thrown, it is caught by the global exception handler and translated into the 400 response, providing a consistent, client-friendly error surface for domain errors.

## Remarks
DomainException creates a clean boundary between domain failures and transport concerns. By deriving from Exception and offering a simple message-based constructor, it can be thrown from domain logic and caught at the API boundary to produce consistent feedback to clients. It helps distinguish domain violations from other failures and keeps domain rules centralized in one exception type.

## Notes
- This base class currently provides only a message constructor; if you need inner exceptions or error codes, add additional constructors or properties.
- Ensure your global exception handler maps DomainException to HTTP 400; without the mapping, clients may receive a 500 Internal Server Error.