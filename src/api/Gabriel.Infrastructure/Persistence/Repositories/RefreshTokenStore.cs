using Gabriel.Core.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gabriel.Infrastructure.Persistence.Repositories;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _ctx;

    public RefreshTokenStore(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default)
        => _ctx.Set<RefreshToken>().FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await _ctx.Set<RefreshToken>().AddAsync(token, ct);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        // ExecuteUpdateAsync issues a single UPDATE - no need to load the rows
        // into the change tracker for a bulk revoke.
        var now = DateTimeOffset.UtcNow;
        await _ctx.Set<RefreshToken>()
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }
}
