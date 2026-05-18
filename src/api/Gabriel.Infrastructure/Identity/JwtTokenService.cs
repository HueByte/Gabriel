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
        var existing = await _refreshStore.FindByHashAsync(hash, ct)
            ?? throw new UnauthorizedAccessException("Refresh token is invalid.");

        if (existing.IsRevoked)
        {
            // Reuse of a token that was already rotated — strong theft signal.
            // Burn the world for this user; they re-authenticate.
            if (existing.ReplacedByTokenId is not null)
            {
                _logger.LogWarning(
                    "Refresh-token reuse detected for user {UserId} (token {TokenId} was replaced by {Replacement}); revoking all sessions.",
                    existing.UserId, existing.Id, existing.ReplacedByTokenId);
                await _refreshStore.RevokeAllForUserAsync(existing.UserId, ct);
                await _uow.SaveChangesAsync(ct);
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
        if (existing is null || existing.IsRevoked) return;  // idempotent — already gone is fine

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
        // SHA-256 is sufficient — refresh tokens are high-entropy random, not user passwords.
        // The hash exists so a DB leak doesn't immediately compromise live sessions.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToHexStringLower(bytes);
    }
}
