# IRefreshTokenStore

> **File:** `src/api/Gabriel.Core/Identity/IRefreshTokenStore.cs`  
> **Kind:** interface

```csharp
public interface IRefreshTokenStore
```


Persistence boundary for refresh tokens; all writes go through IUnitOfWork to guarantee atomic token rotation (mark-old-replaced plus insert-new). Reach for this interface when you need to locate, add, or bulk-revoke tokens for a user as part of issuance, refresh, or theft-detection workflows.

## Remarks
This interface isolates token persistence from the business logic of token issuance and rotation, enabling swap-in persistence implementations without changing callers. It collaborates with JwtTokenService to support issuing, refreshing, and revoking tokens, and relies on IUnitOfWork to ensure that refresh-token rotation remains atomic during updates.

## Notes
- FindByHashAsync returns null if the token hash is not found; callers must handle the null case.
- Cancellation tokens are accepted; ensure you propagate ct through to the underlying data store.
- Bulk revocation affects all active tokens for a user; use in theft-detection flows and on logout.