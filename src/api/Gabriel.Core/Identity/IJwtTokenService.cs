namespace Gabriel.Core.Identity;

// Issues short-lived access JWTs plus long-lived rotatable refresh tokens.
// Access tokens are stateless (validated via signature); refresh tokens are
// server-side state so they can be revoked.
public interface IJwtTokenService
{
    // Initial issuance - caller already authenticated via session/cookie.
    Task<TokenPair> IssueAsync(Guid userId, string email, CancellationToken ct = default);

    // Trade a valid refresh token for a fresh pair. Rotates the refresh token
    // and detects reuse (theft signal → revoke entire family for the user).
    Task<TokenPair> RefreshAsync(string refreshToken, CancellationToken ct = default);

    // Revoke a single refresh token (e.g. user-initiated sign-out of one device).
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);

    // Revoke every active refresh token for the user (sign out everywhere /
    // panic button after credential compromise).
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}

public record TokenPair(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt);
