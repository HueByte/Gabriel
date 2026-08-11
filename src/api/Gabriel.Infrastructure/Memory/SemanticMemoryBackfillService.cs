using Gabriel.Core.Memory;
using Gabriel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gabriel.Infrastructure.Memory;

// Startup convergence job: re-embeds every SQLite memory row into Qdrant so
// the index catches up on anything written while Qdrant was down, config was
// disabled, or the embedding model changed. Runs once per process start;
// counts are small (memories are curated facts, not documents) so a full
// re-upsert is cheaper than diffing. Reads AppDbContext directly instead of
// widening IMemoryRepository with an unscoped list - the cross-user read
// stays private to this class.
public sealed class SemanticMemoryBackfillService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISemanticMemoryIndex _index;
    private readonly ILogger<SemanticMemoryBackfillService> _logger;

    public SemanticMemoryBackfillService(
        IServiceScopeFactory scopeFactory,
        ISemanticMemoryIndex index,
        ILogger<SemanticMemoryBackfillService> logger)
    {
        _scopeFactory = scopeFactory;
        _index = index;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Same guard the migration block uses - the swagger-codegen host
        // instantiation must not touch the database (or the network).
        if (Environment.GetEnvironmentVariable("SKIP_DB_INIT") == "true") return;
        if (!_index.IsEnabled) return;

        // Let migrations and the web host settle before the first reindex.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entries = await db.MemoryEntries.AsNoTracking().ToListAsync(stoppingToken);
            if (entries.Count == 0) return;

            var written = await _index.ReindexAsync(entries, stoppingToken);
            _logger.LogInformation(
                "Semantic memory backfill: {Written}/{Total} entries indexed", written, entries.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Non-fatal by design: the index self-heals on the next save or
            // process restart, and searches degrade to empty rather than fail.
            _logger.LogWarning(ex, "Semantic memory backfill failed");
        }
    }
}
