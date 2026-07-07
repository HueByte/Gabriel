# NotFoundException

> **File:** `src/api/Gabriel.Core/Exceptions/NotFoundException.cs`  
> **Kind:** class

```csharp
public class NotFoundException : Exception
```


Thrown when a requested aggregate is missing; this exception signals a not-found condition in the domain and is intended to translate to an HTTP 404 in API responses. Throw it during lookups when a concrete entity cannot be located, instead of returning null or a generic error, so upper layers can consistently map the outcome to a not-found response. It also carries contextual data—Resource and Key—that identify what was missed.

## Remarks
NotFoundException serves as a domain-level signal that a particular resource was not found, decoupling not-found handling from transport concerns. By carrying Resource and Key, it provides actionable context for logs and clients while enabling centralized exception handling to translate the condition to a 404 consistently across endpoints.

## Example
```csharp
public Order GetOrder(string orderId)
{
    var order = _orderRepository.Find(orderId);
    if (order == null)
        throw new NotFoundException("Order", orderId);

    return order;
}
```

## Notes
- The exception's message is constructed from Resource and Key (e.g. "Order with key '123' was not found."); be mindful of leaking sensitive information in logs or API responses by sanitizing the Key where appropriate.
- Use NotFoundException strictly for missing-resource scenarios; for permissions or other failures, prefer a different exception type and HTTP semantics.