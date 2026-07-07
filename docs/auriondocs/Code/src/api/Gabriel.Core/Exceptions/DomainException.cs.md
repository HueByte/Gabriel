# DomainException

> **File:** `src/api/Gabriel.Core/Exceptions/DomainException.cs`  
> **Kind:** class

```csharp
public class DomainException : Exception
```


DomainException is the base exception type for violations of domain rules. Throwing this type (or a derived type) signals a business-rule failure that the global exception handler translates into a 400 Bad Request, allowing client code to receive a consistent, user-friendly error without exposing internal infrastructure details.

## Remarks
DomainException provides a centralized way to represent domain-level errors across the application. By deriving specific exceptions from it, you can categorize different rule violations while preserving uniform HTTP error handling and messages, keeping domain concerns decoupled from transport logic.

## Notes
- Use DomainException (or a derived type) for expected domain-rule violations, not for programming errors or I/O problems.
- Keep failure messages user-friendly and avoid leaking internal state or sensitive details since they will be surfaced to clients.
- If you catch DomainException to attach context or translation, prefer throwing a more specific derived type or rethrow the original to preserve the domain intent.