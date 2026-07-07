# IRefreshTokenStore

> **File:** `src/api/Gabriel.Core/Identity/IRefreshTokenStore.cs`  
> **Kind:** interface

```csharp
public interface IRefreshTokenStore
```


IRefreshTokenStore defines the persistence boundary for refresh tokens. It abstracts how tokens are stored and retrieved by their hash, added to the store, and revoked, with all writes routed through a unit of work so a rotation (mark-old-replaced + insert-new) commits atomically.

## Remarks
IRefreshTokenStore exists to decouple refresh-token persistence from the authentication flow and the JwtTokenService. It enables safe rotation and bulk revocation through a single, atomic write path, reducing the risk of torn-writes during renewal or theft-detection scenarios. By operating on token hashes rather than plaintext tokens and exposing dedicated methods for finding, adding, and revoking tokens, it provides a focused API that enforces the security lifecycle of refresh tokens.

## Notes
- The FindByHashAsync method relies on a consistent hash of the input token; hash the presented token in the same way before lookup.
- All methods accept a CancellationToken to support cooperative cancellation.
- Ensure the implementor routes all mutations through the unit of work to preserve atomicity of rotation and revocation operations.