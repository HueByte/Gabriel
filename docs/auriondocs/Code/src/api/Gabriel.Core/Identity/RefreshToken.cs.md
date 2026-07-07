# RefreshToken

> **File:** `src/api/Gabriel.Core/Identity/RefreshToken.cs`  
> **Kind:** class

```csharp
public class RefreshToken
```


Represents a server-side refresh token record used for issuing and validating refresh-token exchanges. Use this type when persisting refresh tokens (store the token's hash, not the plaintext) and when implementing rotation/revocation logic: tokens are created via the Create factory, can be revoked, and can be marked as replaced by a successor token.

## Remarks
This class models the token lifecycle and encodes the rotation policy: a new refresh operation should create a replacement token and call MarkReplacedBy on the old token. The plaintext token is only returned to the client at issuance; the database should persist TokenHash (the SHA-256 hash of the token) so a DB leak doesn't directly expose active tokens. The type enforces invariants (private setters, validation in Create) so callers must use the Create factory to instantiate a token; a private parameterless constructor remains for persistence/ORM materialization.

Computed properties surface the token state: IsActive is true when the token is not revoked and not expired; IsRevoked and IsExpired reflect those individual conditions. Revoke and MarkReplacedBy set RevokedAt (only once) and MarkReplacedBy also records the replacement token id. Create sets ExpiresAt relative to UtcNow and Id/CreatedAt are initialized automatically.

## Notes
- The TokenHash parameter should be the hashed form of the plaintext token (the class itself does not hash input).
- Time comparisons use DateTimeOffset.UtcNow; distributed systems should account for clock skew when evaluating expiration.
- Revoke and MarkReplacedBy set RevokedAt only if it was null (they are safe to call multiple times without changing the first-revoked timestamp).
