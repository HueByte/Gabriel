# RefreshToken

> **File:** `src/api/Gabriel.Core/Identity/RefreshToken.cs`  
> **Kind:** class

```csharp
public class RefreshToken
```


Represents a server-side refresh token record that stores only the SHA‑256 hash of the token (the plaintext is returned to the client only at issuance). Use this type to persist token metadata, evaluate token state (active / revoked / expired), and perform revocation or rotation operations via Revoke and MarkReplacedBy. Create(...) is the intended factory to ensure required fields are provided and the expiry is set.

## Remarks
This class encapsulates the minimal state and behaviors needed for refresh-token rotation and revocation. It is designed so that tokens are rotated on each refresh: the previous token can be marked as replaced (ReplacedByTokenId) and revoked; presenting a replaced token is treated as a theft signal by higher-level services. Storing TokenHash (rather than plaintext) reduces risk from a database leak; the object tracks creation, expiry and revocation timestamps to make revocation decisions deterministic.

## Example
```csharp
var userId = Guid.NewGuid();
var tokenHash = "sha256-hash-of-the-token";
// Create a token that lives 30 days
var refreshToken = RefreshToken.Create(userId, tokenHash, TimeSpan.FromDays(30));

// Check state
var active = refreshToken.IsActive; // true immediately after creation (assuming clock)

// Revoke explicitly (idempotent)
refreshToken.Revoke();

// Mark as replaced by a newly issued token (also sets RevokedAt if not already set)
var replacementId = Guid.NewGuid();
refreshToken.MarkReplacedBy(replacementId);
```

## Notes
- Create throws ArgumentException if userId is Guid.Empty or tokenHash is null/whitespace; callers must validate or handle the exception.
- Revoke is idempotent: calling it multiple times only sets RevokedAt once. MarkReplacedBy also sets RevokedAt when marking a replacement.
- Time checks (IsActive, IsExpired) use DateTimeOffset.UtcNow; tests or distributed callers should account for UTC-based timing and potential clock skew.