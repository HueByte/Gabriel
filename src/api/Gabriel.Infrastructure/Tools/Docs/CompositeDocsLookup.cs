using Gabriel.Engine.Tools.Docs;
using Microsoft.Extensions.Logging;

namespace Gabriel.Infrastructure.Tools.Docs;

// IDocsLookup that fans across an ordered list of inner sources. The first
// source is the PRIMARY (its entries are listed first, its reads win), every
// subsequent source is a fallback consulted only if earlier sources don't
// answer.
//
// Today the wiring is:
//   primary  = LocalDocsLookup    (LLM-native self-docs on disk)
//   fallback = GitHubDocsLookup   (human-prose docs on GitHub)
//
// Ordering matters - keep the primary first.
public sealed class CompositeDocsLookup : IDocsLookup
{
    private readonly IReadOnlyList<IDocsLookup> _sources;
    private readonly ILogger<CompositeDocsLookup> _logger;

    public CompositeDocsLookup(IEnumerable<IDocsLookup> sources, ILogger<CompositeDocsLookup> logger)
    {
        // Materialize to a list so order from registration is preserved and
        // we don't re-enumerate the IEnumerable on every call.
        _sources = sources.ToList();
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocsEntry>> ListAsync(CancellationToken ct)
    {
        // Union by Path - if two sources expose the same relative path, the
        // higher-priority (earlier) source wins. Within each source the
        // original order is preserved; across sources, primary entries come
        // first.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var merged = new List<DocsEntry>();

        // Track the last transient failure so we can surface it if EVERY
        // source returned empty. Without this, a failing GitHub API call
        // (rate limit / 5xx / DNS) gets swallowed and the agent just sees
        // "No Gabriel docs are currently available" with no clue why -
        // identical to a genuine zero-docs scenario.
        Exception? lastTransient = null;

        foreach (var source in _sources)
        {
            IReadOnlyList<DocsEntry> entries;
            try
            {
                entries = await source.ListAsync(ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // A failing source must not poison the whole list - the user
                // still benefits from whatever other sources can answer.
                _logger.LogWarning(ex, "Docs source {Source} failed during ListAsync; skipping it.", source.GetType().Name);
                lastTransient = ex;
                continue;
            }

            foreach (var e in entries)
            {
                if (seen.Add(e.Path))
                    merged.Add(e);
            }
        }

        // If at least one source returned entries we hand those back even when
        // another source failed - partial coverage beats no coverage. But if
        // every source either returned empty OR threw, and at least one threw,
        // rethrow so the user-facing tool message includes the actual cause
        // (rate limit, 5xx, DNS) instead of the generic "no docs" string.
        if (merged.Count == 0 && lastTransient is not null) throw lastTransient;

        return merged;
    }

    public async Task<DocsContent?> ReadAsync(string path, CancellationToken ct)
    {
        // Try sources in order. First non-null wins. Argument-validation
        // failures from one source (e.g. "Path may not contain '.' segments")
        // are surfaced immediately - they're a problem with the input, not
        // the source.
        Exception? lastTransient = null;
        foreach (var source in _sources)
        {
            try
            {
                var hit = await source.ReadAsync(path, ct);
                if (hit is not null) return hit;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Docs source {Source} failed reading '{Path}'; trying next source.", source.GetType().Name, path);
                lastTransient = ex;
            }
        }

        // Every source returned null OR threw transient errors. If at least one
        // threw, surface the last failure so the tool reports something useful
        // instead of silently saying "not found".
        if (lastTransient is not null) throw lastTransient;
        return null;
    }
}
