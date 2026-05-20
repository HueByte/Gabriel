using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gabriel.Infrastructure.Persistence.Repositories;

// EF Core impl of IMetricRepository. Writes use AddAsync + SaveChangesAsync
// inline (no IUnitOfWork involvement) because metrics are stand-alone events:
// they don't participate in any business transaction. If a metric row fails
// to commit the underlying business operation should still succeed and the
// metric loss is acceptable.
public sealed class MetricRepository : IMetricRepository
{
    private readonly AppDbContext _db;

    public MetricRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(MetricEntry entry, CancellationToken ct = default)
    {
        await _db.Set<MetricEntry>().AddAsync(entry, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MetricEntry>> RecentAsync(string system, int limit, CancellationToken ct = default)
    {
        if (limit <= 0) return Array.Empty<MetricEntry>();
        return await _db.Set<MetricEntry>()
            .AsNoTracking()
            .Where(m => m.System == system)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MetricEntry>> RecentByPrefixAsync(string systemPrefix, int limit, CancellationToken ct = default)
    {
        if (limit <= 0) return Array.Empty<MetricEntry>();
        // EF translates StartsWith to LIKE on SQLite, which uses the index
        // when the pattern is a literal prefix (no leading wildcard).
        return await _db.Set<MetricEntry>()
            .AsNoTracking()
            .Where(m => m.System.StartsWith(systemPrefix))
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<int> DeleteOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        // ExecuteDeleteAsync issues a single bulk DELETE - no entity tracking,
        // no in-memory materialization. Right tool for cleanup tasks.
        return await _db.Set<MetricEntry>()
            .Where(m => m.CreatedAt < cutoff)
            .ExecuteDeleteAsync(ct);
    }
}
