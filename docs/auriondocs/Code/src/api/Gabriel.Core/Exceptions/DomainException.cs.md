# DomainException

> **File:** `src/api/Gabriel.Core/Exceptions/DomainException.cs`  
> **Kind:** class

```csharp
public class DomainException : Exception
```


DomainException is a base class for domain rule violations. It should be thrown when a business rule or invariant within the domain is violated. A global exception handler catches DomainException instances and translates them into a 400 Bad Request response, providing a consistent signal to clients for domain-related input errors. The class itself is intentionally minimal, exposing only a constructor that accepts a message and forwards it to the base Exception class, without adding behavior.