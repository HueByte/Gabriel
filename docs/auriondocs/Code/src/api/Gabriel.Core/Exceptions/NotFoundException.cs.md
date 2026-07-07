# NotFoundException

> **File:** `src/api/Gabriel.Core/Exceptions/NotFoundException.cs`  
> **Kind:** class

```csharp
public class NotFoundException : Exception
```


NotFoundException signals that a requested aggregate or resource could not be located. It carries contextual details—the Resource name and the Key used to locate it—so callers can distinguish what was missing and how to respond (for example by translating to HTTP 404 semantics in API layers). When a lookup fails, this exception is thrown to propagate a precise, domain-meaningful not-found signal; the generated message follows the pattern '<resource> with key '<key>' was not found.'

## Remarks

This abstraction centralizes the notion of a missing entity, separating the detection of absence from how that absence is surfaced to callers. By exposing Resource and Key, it enables targeted logging and consistent error handling across layers that participate in retrieval.

## Example

```csharp
var customerId = 42;
object key = customerId;
throw new NotFoundException("Customer", key);
```

## Notes

- The Key is stored as object; pass simple values or composite keys as appropriate.
- The exception formats its message using Resource and Key, so be mindful of logging sensitive data contained in the key.
- Catch NotFoundException when you want to translate to a 404 response or to handle missing data distinctly from other errors.