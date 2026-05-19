namespace Gabriel.Core.Identity;

// Server-side refresh-token record. The plaintext token is only ever returned
// to the client at issuance; we persist a SHA-256 hash so a DB leak doesn't
// immediately compromise active sessions.
//
// Rotation policy: every successful /jwt/refresh issues a new token AND marks
// the old one as Replaced. If the old token is presented again after being
// replaced, that's a strong theft signal - the JwtTokenService revokes the
// entire family for that user.
public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("TokenHash is required.", nameof(tokenHash));

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime),
        };
    }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    public void Revoke()
    {
        if (RevokedAt is null) RevokedAt = DateTimeOffset.UtcNow;
    }

    public void MarkReplacedBy(Guid replacementId)
    {
        if (replacementId == Guid.Empty)
            throw new ArgumentException("Replacement id is required.", nameof(replacementId));
        ReplacedByTokenId = replacementId;
        if (RevokedAt is null) RevokedAt = DateTimeOffset.UtcNow;
    }
}
