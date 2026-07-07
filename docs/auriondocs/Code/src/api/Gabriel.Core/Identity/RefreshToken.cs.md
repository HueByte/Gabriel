# RefreshToken

> **File:** `src/api/Gabriel.Core/Identity/RefreshToken.cs`  
> **Kind:** class

```csharp
public class RefreshToken
```


Represents a server-side, persisted refresh token and encapsulates its lifecycle (creation time, expiration, revocation and replacement). Reach for this type when you need a single authoritative record for a user's refresh token in the database — the class stores only a token hash (the plaintext token is returned to clients only at issuance) and exposes simple methods and properties used by token rotation and revocation logic.

## Remarks
This entity is designed to support a rotation-first refresh policy: each successful refresh issues a new token and the previous token is marked as replaced. The class stores a TokenHash (the source comments indicate a SHA-256 hash is expected) rather than the plaintext token so a database leak does not immediately expose active sessions. The private parameterless constructor exists to allow persistence frameworks/ORMs to materialize instances while the static Create factory enforces required fields when creating a new token from application code.

## Example
```csharp
// Create a new refresh token record for a user
var lifetime = TimeSpan.FromDays(30);
var tokenHash = ComputeSha256Hash(plaintextToken);
var refreshToken = RefreshToken.Create(userId, tokenHash, lifetime);

// Check status
if (refreshToken.IsActive) { /* allow refresh */ }

// Revoke explicitly (idempotent)
refreshToken.Revoke();

// Mark that this token was replaced by another token
refreshToken.MarkReplacedBy(replacementTokenId);
```

## Notes
- Create throws ArgumentException if userId is Guid.Empty or tokenHash is null/whitespace.
- MarkReplacedBy throws ArgumentException if replacementId is Guid.Empty.
- Revoke and MarkReplacedBy set RevokedAt only if it was previously null, so repeated calls are safe (idempotent for the timestamping).
- IsActive and IsExpired use DateTimeOffset.UtcNow for comparisons; tests that assert time-dependent behavior should control or mock the clock.
- The class does not validate the hashing algorithm or format of TokenHash — callers are responsible for producing a secure (e.g. SHA-256) hash before calling Create.