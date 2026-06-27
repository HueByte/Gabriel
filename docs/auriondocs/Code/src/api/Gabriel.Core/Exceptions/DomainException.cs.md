# DomainException

> **File:** `src/api/Gabriel.Core/Exceptions/DomainException.cs`  
> **Kind:** class

Represents a domain-level rule violation. Throw this exception from application or domain logic when a business/validation rule is broken and you want the global exception handler to translate the error into an HTTP 400 Bad Request. It is a thin subclass of System.Exception exposing only a message constructor.

## Remarks
This type exists to clearly distinguish expected domain validation errors from unexpected or infrastructure failures. The global exception handler inspects this exception type and converts it to a 400 response so domain code does not need to know about HTTP concerns; controllers and services can simply throw DomainException to signal client errors.

## Example
```csharp
// Inside a domain service or aggregate
if (age < 18)
    throw new DomainException("Customer must be at least 18 years old.");
```

## Notes
- DomainException carries only a message (no error code or additional metadata). If you need machine-readable problem details, extend the type or use a different error shape.
- The project maps this exception type to a 400 Bad Request in the global exception handler; changing that behavior requires updating the handler implementation.