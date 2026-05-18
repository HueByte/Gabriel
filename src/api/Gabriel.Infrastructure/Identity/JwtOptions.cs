namespace Gabriel.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    // What goes in the iss / aud claims and is validated on inbound JWTs.
    public string Issuer { get; set; } = "gabriel";
    public string Audience { get; set; } = "gabriel";

    // HS256 symmetric key. MUST be at least 32 chars (256 bits) for HS256.
    // Supply via Infisical (JWT__SIGNINGKEY) or user-secrets (Jwt:SigningKey).
    public string SigningKey { get; set; } = string.Empty;

    // Short-lived so a leaked access token has a small blast radius. Clients
    // refresh via /api/auth/jwt/refresh which rotates the refresh token too.
    public int AccessTokenMinutes { get; set; } = 15;

    // Refresh tokens are long-lived but revokable (server-side hash store, see
    // RefreshTokenStore). Rotation on every refresh; reuse of a rotated token
    // revokes the entire family as theft signal.
    public int RefreshTokenDays { get; set; } = 30;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SigningKey) && SigningKey.Length >= 32;
}
