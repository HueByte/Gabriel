# DomainException

> **File:** `src/api/Gabriel.Core/Exceptions/DomainException.cs`  
> **Kind:** class

Represents a domain-level rule violation. Throw this from business/domain logic when an operation cannot proceed because a business rule was violated; the application's global exception handler treats instances of this type as client errors and converts them into a 400 Bad Request response.

## Remarks
Acts as a marker base class for expected, client-caused failures that originate from domain rules rather than infrastructure faults. Using this type (or a derived type) lets the error handling pipeline distinguish recoverable/expected domain problems from unexpected server errors.

## Example
```csharp
// Throwing directly in domain logic
if (order.Total <= 0)
{
    throw new DomainException("Order total must be greater than zero.");
}

// Creating a specific domain exception
public class OrderValidationException : DomainException
{
    public OrderValidationException(string message) : base(message) { }
}
```

## Notes
- Message text may be returned to API clients by the global handler; avoid including sensitive or internal diagnostic information.
- This exception is intended for business-rule failures (client errors). Do not use it for unexpected system or infrastructure failures, which should remain unhandled here so they surface as server errors.
- The mapping to HTTP 400 is performed by the global exception handler—changing the HTTP behavior requires updating that handler as well.