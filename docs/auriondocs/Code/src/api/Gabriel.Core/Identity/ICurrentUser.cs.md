# ICurrentUser

> **File:** `src/api/Gabriel.Core/Identity/ICurrentUser.cs`  
> **Kind:** interface

```csharp
public interface ICurrentUser
```


ICurrentUser provides a per-request view of the active user that core components can rely on without touching HTTP plumbing. It exposes three pieces of information: an optional UserId, a boolean IsAuthenticated, and an optional Email, enabling services to make authorization and personalization decisions in a testable, framework-agnostic way.

## Remarks
Abstracting current user data behind ICurrentUser decouples business logic from HttpContext and claims plumbing. The API layer is responsible for populating this interface from HttpContext.User, while the core remains agnostic of web concerns, making it easier to test and reuse in non-HTTP contexts. It sits alongside other identity concerns and is commonly consumed by services that enforce access checks, auditing, or personalization.

## Notes
- Always verify IsAuthenticated and UserId.HasValue before using UserId.
- Treat Email as optional; don't rely on it for authentication decisions.
- Ensure the ICurrentUser implementation is registered with a per-request lifetime so it reflects the current HTTP context.