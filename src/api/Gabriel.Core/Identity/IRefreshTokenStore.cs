namespace Gabriel.Core.Identity;

// Persistence boundary for refresh tokens. Saves go through IUnitOfWork so a
// rotation (mark-old-replaced + insert-new) commits atomically.
public interface IRefreshTokenStore
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    // Bulk-revoke every active token for a user. Used by RevokeAllForUserAsync
    // and by the theft-detection path in JwtTokenService.
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}
