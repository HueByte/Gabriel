# ICurrentUser

> **File:** `src/api/Gabriel.Core/Identity/ICurrentUser.cs`  
> **Kind:** interface

```csharp
public interface ICurrentUser
```


Represents the current user for the duration of a request. This HTTP-agnostic abstraction is implemented in the API layer by reading data from HttpContext.User, allowing the Core domain to stay unaware of HTTP plumbing. It exposes the UserId (nullable Guid), IsAuthenticated (bool), and Email (nullable string) to downstream services.

## Remarks
By abstracting the user context behind ICurrentUser, business logic can remain ignorant of HTTP details and easily testable with mock implementations. Inject ICurrentUser into services that need user context rather than anchoring logic to HttpContext or thread-local state.

## Notes
- UserId is nullable; handle null for anonymous users.
- Email may be null; it's optional and relies on the identity provider.
- Per-request nature; don't cache values across requests.