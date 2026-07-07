# ICurrentUser

> **File:** `src/api/Gabriel.Core/Identity/ICurrentUser.cs`  
> **Kind:** interface

```csharp
public interface ICurrentUser
```


ICurrentUser provides a per-request view of the currently authenticated user. Implemented in the API layer by reading HttpContext.User, it exposes a small, HTTP-agnostic contract for identity that the core can consume without knowledge of HTTP plumbing; it surfaces UserId (Guid?), IsAuthenticated (bool), and Email (string?).

## Remarks
ICurrentUser acts as a boundary between HTTP-specific concerns and domain logic. By depending on this interface, core services can access identity information without HttpContext references, enabling easier testing and clearer separation. Be aware that UserId and Email can be null; always guard these values unless IsAuthenticated is true and the value is known to be present.

## Notes
- Use IsAuthenticated and HasValue on UserId before relying on the value; UserId and Email may be null when there is no authenticated user.
- This interface is intended to be scoped per-request; avoid caching user data beyond the lifetime of the request to prevent stale information.