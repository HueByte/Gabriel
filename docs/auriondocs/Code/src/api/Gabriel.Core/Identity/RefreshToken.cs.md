# RefreshToken

> **File:** `src/api/Gabriel.Core/Identity/RefreshToken.cs`  
> **Kind:** class

Represents a server-side persisted refresh token for a user and encapsulates lifecycle state used by a token-rotation policy (creation time, expiry, revocation and replacement). Reach for this type when you need a durable record of a refresh token in your database — the class expects callers to store only a hash of the token (not the plaintext) and to manage rotation/revocation via the provided methods.

## Remarks
This class models the security-minded server record for refresh tokens: the plaintext token is produced only at issuance and never stored; instead a TokenHash (typically a SHA-256 hash) is persisted. It centralizes status checks (IsActive/IsRevoked/IsExpired) and small lifecycle transitions — Revoke() marks the token as revoked and MarkReplacedBy(...) records a replacement token and also revokes the original if it wasn't already. The rotation comment in the source indicates that a higher-level JwtTokenService is expected to issue replacement tokens and revoke an entire token family when reuse of a replaced token is detected.

## Example
```csharp
// Create a new record (tokenHash should be a hashed value, e.g. SHA-256 hex)
var token = RefreshToken.Create(userId: userId, tokenHash: hashedToken, lifetime: TimeSpan.FromDays(30));

// Persist `token` to the DB. Later, check status:
if (token.IsActive)
{
    // issue access token, etc.
}

// When rotating tokens: record the replacement and persist the change
token.MarkReplacedBy(replacementId);
// Or revoke explicitly if a compromise is detected
token.Revoke();
```

## Notes
- Create validates inputs and throws ArgumentException when userId is Guid.Empty or tokenHash is null/whitespace.
- MarkReplacedBy throws if given Guid.Empty and will set RevokedAt if it wasn't already set; Revoke is idempotent (sets RevokedAt only if null).
- Time-based checks use DateTimeOffset.UtcNow; tests or special hosting scenarios may need to account for this time source.
- The class does not enforce that TokenHash is SHA-256 — the comment documents the intended usage but callers are responsible for providing the hashed value.
