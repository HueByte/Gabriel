using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gabriel.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    // How long after a refresh token is rotated we still tolerate reuse of the
    // old token without firing theft detection. Absorbs multi-tab races,
    // in-flight requests, and (the killer case) a long SSE stream that
    // finishes AFTER a parallel request has already rotated the cookie —
    // the SSE response carries the now-stale cookie back to the browser.
    // 5min is long enough to swallow normal browser-scale timing, short
    // enough that a leaked token replayed an hour later still trips. Bumped
    // from 60s after observing real-world sessions hit the boundary.
    private static readonly TimeSpan RotationGracePeriod = TimeSpan.FromMinutes(5);

    private readonly JwtOptions _options;
    private readonly IRefreshTokenStore _refreshStore;
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IOptions<JwtOptions> options,
        IRefreshTokenStore refreshStore,
        IUnitOfWork uow,
        UserManager<ApplicationUser> users,
        ILogger<JwtTokenService> logger)
    {
        _options = options.Value;
        _refreshStore = refreshStore;
        _uow = uow;
        _users = users;
        _logger = logger;
    }

    public async Task<TokenPair> IssueAsync(Guid userId, string email, CancellationToken ct = default)
    {
        EnsureConfigured();

        var (access, accessExpires) = MintAccessJwt(userId, email);
        var (refreshPlaintext, refreshEntity) = await PersistNewRefreshTokenAsync(userId, ct);

        return new TokenPair(access, accessExpires, refreshPlaintext, refreshEntity.ExpiresAt);
    }

    public async Task<TokenPair> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        EnsureConfigured();

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Refresh token is required.");

        var hash = HashToken(refreshToken);
        var existing = await _refreshStore.FindByHashAsync(hash, ct);
        if (existing is null)
        {
            // "Invalid" here means the token hash isn't in the DB at all —
            // not revoked, not expired, just missing. Almost always one of:
            //   1. DB was reset during dev while the browser kept the cookie
            //   2. A previous rotation's Set-Cookie didn't take effect on the
            //      client (and the rotation's *new* row is what the cookie
            //      should hash to, not the value we just hashed)
            //   3. Truly tampered/foreign token
            // Log the hash prefix + a tag so a recurring pattern is greppable.
            _logger.LogWarning(
                "Refresh-token lookup miss — token hash {HashPrefix}... not found in store. Possible causes: DB reset, lost Set-Cookie, or stale cookie from prior server instance.",
                hash[..Math.Min(12, hash.Length)]);
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        if (existing.IsRevoked)
        {
            // Reuse of a token that was already rotated - STRONG theft signal,
            // but only when the reuse happens well AFTER the rotation. Several
            // legitimate races look identical at the server:
            //   - Multiple tabs racing on /refresh with the same cookie value
            //   - In-flight requests during refresh briefly carrying the old cookie
            //   - Browser-internal retries / redirects
            // A grace window after rotation absorbs these: if the OLD token
            // appears within RotationGracePeriod, we still reject this specific
            // reuse (return 401) but we DON'T burn the whole session. Real
            // theft - a leaked token replayed minutes later - still trips the
            // alarm.
            if (existing.ReplacedByTokenId is not null)
            {
                var revokedAt = existing.RevokedAt ?? DateTimeOffset.UtcNow;
                var sinceRevoke = DateTimeOffset.UtcNow - revokedAt;
                if (sinceRevoke > RotationGracePeriod)
                {
                    _logger.LogWarning(
                        "Refresh-token reuse detected for user {UserId} (token {TokenId} replaced by {Replacement}, {Seconds}s after rotation); revoking all sessions.",
                        existing.UserId, existing.Id, existing.ReplacedByTokenId, (int)sinceRevoke.TotalSeconds);
                    await _refreshStore.RevokeAllForUserAsync(existing.UserId, ct);
                    await _uow.SaveChangesAsync(ct);
                }
                else
                {
                    _logger.LogInformation(
                        "Refresh-token reuse within grace window ({Seconds}s) for user {UserId} - rejecting this attempt but keeping other sessions alive.",
                        (int)sinceRevoke.TotalSeconds, existing.UserId);
                }
            }
            throw new UnauthorizedAccessException("Refresh token has been revoked.");
        }

        if (existing.IsExpired)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        var user = await _users.FindByIdAsync(existing.UserId.ToString())
            ?? throw new UnauthorizedAccessException("User no longer exists.");

        // Rotate: mint new pair, mark old token as replaced, persist atomically.
        var (newPlaintext, newEntity) = await PersistNewRefreshTokenAsync(existing.UserId, ct);
        existing.MarkReplacedBy(newEntity.Id);
        await _uow.SaveChangesAsync(ct);

        var (access, accessExpires) = MintAccessJwt(existing.UserId, user.Email ?? string.Empty);
        return new TokenPair(access, accessExpires, newPlaintext, newEntity.ExpiresAt);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;

        var hash = HashToken(refreshToken);
        var existing = await _refreshStore.FindByHashAsync(hash, ct);
        if (existing is null || existing.IsRevoked) return;  // idempotent - already gone is fine

        existing.Revoke();
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        await _refreshStore.RevokeAllForUserAsync(userId, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // --- helpers ----------------------------------------------------------------

    private void EnsureConfigured()
    {
        if (!_options.IsConfigured)
            throw new InvalidOperationException("Jwt:SigningKey is not configured (need >= 32 chars).");
    }

    private (string accessToken, DateTimeOffset expiresAt) MintAccessJwt(Guid userId, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private async Task<(string plaintext, RefreshToken entity)> PersistNewRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        var plaintext = GenerateRefreshTokenPlaintext();
        var hash = HashToken(plaintext);
        var entity = RefreshToken.Create(userId, hash, TimeSpan.FromDays(_options.RefreshTokenDays));
        await _refreshStore.AddAsync(entity, ct);
        return (plaintext, entity);
    }

    private static string GenerateRefreshTokenPlaintext()
    {
        // 64 bytes = 512 bits of entropy. Base64url so it's URL- and header-safe.
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncoder.Encode(bytes);
    }

    private static string HashToken(string plaintext)
    {
        // SHA-256 is sufficient - refresh tokens are high-entropy random, not user passwords.
        // The hash exists so a DB leak doesn't immediately compromise live sessions.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToHexStringLower(bytes);
    }
}
