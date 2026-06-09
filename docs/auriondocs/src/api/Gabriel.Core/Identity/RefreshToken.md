# RefreshToken

> **File:** `src/api/Gabriel.Core/Identity/RefreshToken.cs`  
> **Kind:** class

Represents a server-side refresh token record used for issuing, validating and rotating refresh tokens. Store instances in the database; only a SHA-256 hash of the plaintext token should be persisted so that a database leak does not immediately expose active sessions.

## Remarks
This class models the lifecycle of a refresh token: creation with a lifetime, marking as replaced when a rotation occurs, and revocation. The design supports a rotation policy where every successful refresh issues a new token and marks the old one as replaced; presenting a replaced token again is treated as a theft signal and can trigger revocation of the entire token family. Time comparisons use UTC, and the token's hash (TokenHash) is expected to be computed before calling Create.

## Example
```csharp
// Issue a new refresh token for a user (tokenHash should be a SHA-256 of the plaintext token)
var refresh = RefreshToken.Create(userId, tokenHash, TimeSpan.FromDays(30));

// Check active state
if (refresh.IsActive) { /* allow refresh */ }

// After issuing a rotated replacement:
var replacement = RefreshToken.Create(userId, newTokenHash, TimeSpan.FromDays(30));
refresh.MarkReplacedBy(replacement.Id);

// Revoke explicitly (idempotent)
refresh.Revoke();
```

## Notes
- TokenHash must already be the hashed representation (the plaintext token is only returned to the client at issuance).
- Create throws ArgumentException if userId is Guid.Empty or tokenHash is null/whitespace.
- Time-based properties use DateTimeOffset.UtcNow; tests or environments that alter system time can affect IsActive/IsExpired behavior.
- MarkReplacedBy requires a non-empty Guid and will set RevokedAt if the token wasn't already revoked.