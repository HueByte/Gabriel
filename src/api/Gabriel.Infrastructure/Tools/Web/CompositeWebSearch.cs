using Gabriel.Engine.Tools.Web;
using Microsoft.Extensions.Logging;

namespace Gabriel.Infrastructure.Tools.Web;

// IWebSearch that fans a single query out to several backing providers in
// parallel and merges their results. Useful when you'd rather pay N providers
// for higher coverage and a cross-source quality signal than rely on any one
// of them: a URL that surfaces in multiple providers is almost always more
// relevant than a URL only one provider returned.
//
// Ranking algorithm (deliberately simple - the providers already rank their
// own results well; we just need to combine them):
//
//   1. Group by canonicalized URL (lowercase host, strip trailing slash,
//      drop fragment + utm_* / fbclid params).
//   2. Score each unique URL by
//        score = appearance_count * 1000 - min_rank_across_providers
//      so cross-provider hits sort before single-provider ones, and within
//      the same appearance count, the one ranked highest by some provider
//      wins. Constant 1000 is a safe ceiling since no provider returns >1000
//      results per query.
//   3. Each merged WebSearchResult takes its title from the provider that
//      ranked it highest and its snippet from the longest non-empty snippet
//      across providers (more text = more signal for the model).
//
// Errors from one provider don't poison the others - we catch per provider
// and log a warning, then merge whatever did come back. If every provider
// failed we surface a zero-result list to the caller, matching the contract
// any single backend would have.
public sealed class CompositeWebSearch : IWebSearch
{
    private readonly IReadOnlyList<IWebSearch> _providers;
    private readonly ILogger<CompositeWebSearch> _logger;

    public CompositeWebSearch(IReadOnlyList<IWebSearch> providers, ILogger<CompositeWebSearch> logger)
    {
        if (providers.Count == 0)
            throw new ArgumentException("CompositeWebSearch requires at least one provider.", nameof(providers));
        _providers = providers;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        // Each provider gets `limit` so the merge has room to find cross-
        // provider overlaps. We trim back to `limit` after merging.
        var tasks = _providers.Select(p => RunOneAsync(p, query, limit, ct)).ToArray();
        var perProvider = await Task.WhenAll(tasks);

        // Per-URL accumulator. We canonicalize for grouping but keep the first
        // (highest-ranked) raw URL we saw for display - canonicalization is
        // for matching, not for rendering.
        var byCanon = new Dictionary<string, MergedHit>(StringComparer.Ordinal);
        for (var pi = 0; pi < perProvider.Length; pi++)
        {
            var results = perProvider[pi];
            for (var ri = 0; ri < results.Count; ri++)
            {
                var r = results[ri];
                if (string.IsNullOrWhiteSpace(r.Url)) continue;
                var canon = CanonicalizeUrl(r.Url);
                if (!byCanon.TryGetValue(canon, out var hit))
                {
                    hit = new MergedHit(r.Url, r.Title, r.Snippet, Appearances: 1, BestRank: ri);
                    byCanon[canon] = hit;
                    continue;
                }
                // Existing hit - merge fields.
                var bestRank = Math.Min(hit.BestRank, ri);
                var preferTitle = ri < hit.BestRank && !string.IsNullOrWhiteSpace(r.Title)
                    ? r.Title
                    : hit.Title;
                var preferSnippet = (r.Snippet?.Length ?? 0) > (hit.Snippet?.Length ?? 0)
                    ? r.Snippet
                    : hit.Snippet;
                byCanon[canon] = hit with
                {
                    Title = preferTitle,
                    Snippet = preferSnippet,
                    Appearances = hit.Appearances + 1,
                    BestRank = bestRank,
                };
            }
        }

        // Sort by the score described in the class comment.
        var ordered = byCanon.Values
            .OrderByDescending(h => h.Appearances * 1000 - h.BestRank)
            .Take(limit)
            .Select(h => new WebSearchResult(h.Title ?? "", h.Url, h.Snippet ?? ""))
            .ToList();

        _logger.LogInformation(
            "Composite web search merged {ProviderCount} providers for query '{Query}' into {UniqueUrls} unique URLs (returned top {ReturnedCount}).",
            _providers.Count, query, byCanon.Count, ordered.Count);

        return ordered;
    }

    private async Task<IReadOnlyList<WebSearchResult>> RunOneAsync(IWebSearch provider, string query, int limit, CancellationToken ct)
    {
        try
        {
            return await provider.SearchAsync(query, limit, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Web search provider {ProviderType} failed for query '{Query}'; continuing with other providers.",
                provider.GetType().Name, query);
            return Array.Empty<WebSearchResult>();
        }
    }

    // Conservative URL canonicalization for dedup purposes:
    //   - lowercase scheme + host
    //   - drop default ports
    //   - drop fragment
    //   - drop tracking params (utm_*, fbclid, gclid, ref)
    //   - strip a single trailing slash from the path (so /foo and /foo/ merge)
    // We deliberately don't touch path case (some servers care) or query order.
    private static string CanonicalizeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return url.ToLowerInvariant();

        var host = uri.Host.ToLowerInvariant();
        var scheme = uri.Scheme.ToLowerInvariant();
        var path = uri.AbsolutePath.Length > 1 && uri.AbsolutePath.EndsWith('/')
            ? uri.AbsolutePath[..^1]
            : uri.AbsolutePath;

        // Filter the query string.
        var rebuiltQuery = "";
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var kept = uri.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Where(p =>
                {
                    var eq = p.IndexOf('=');
                    var key = (eq < 0 ? p : p[..eq]).ToLowerInvariant();
                    return !key.StartsWith("utm_", StringComparison.Ordinal)
                        && key != "fbclid" && key != "gclid" && key != "ref";
                })
                .ToArray();
            if (kept.Length > 0) rebuiltQuery = "?" + string.Join('&', kept);
        }

        var port = uri.IsDefaultPort ? "" : $":{uri.Port}";
        return $"{scheme}://{host}{port}{path}{rebuiltQuery}";
    }

    private sealed record MergedHit(string Url, string? Title, string? Snippet, int Appearances, int BestRank);
}
