using System.Net;
using System.Text.Json;
using Gabriel.Engine.Tools.Docs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Tools.Docs;

// IDocsLookup over GitHub. Two transport paths:
//   - List   : api.github.com/repos/{owner}/{repo}/git/trees/{branch}?recursive=1
//   - Read   : raw.githubusercontent.com/{owner}/{repo}/{branch}/{path}
//
// The list response is cached for GitHubDocsOptions.ListCacheMinutes (default 5)
// to keep request counts inside the unauthenticated rate-limit budget. Reads
// pass through because individual docs may be edited live during development.
//
// Path traversal is hardened: any '..' segment or absolute-prefixed path is
// rejected before the request is sent.
public sealed class GitHubDocsLookup : IDocsLookup
{
    public const string ApiHttpClientName = "GitHubDocsApi";
    public const string RawHttpClientName = "GitHubDocsRaw";

    private readonly IHttpClientFactory _httpFactory;
    private readonly GitHubDocsOptions _options;
    private readonly ILogger<GitHubDocsLookup> _logger;

    private readonly SemaphoreSlim _listLock = new(1, 1);
    private (DateTimeOffset Until, IReadOnlyList<DocsEntry> Entries)? _listCache;

    public GitHubDocsLookup(
        IHttpClientFactory httpFactory,
        IOptions<GitHubDocsOptions> options,
        ILogger<GitHubDocsLookup> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocsEntry>> ListAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        if (_listCache is { } cached && cached.Until > now)
            return cached.Entries;

        await _listLock.WaitAsync(ct);
        try
        {
            // Re-check after acquiring the lock - another caller may have filled the cache.
            if (_listCache is { } recheck && recheck.Until > now)
                return recheck.Entries;

            var http = _httpFactory.CreateClient(ApiHttpClientName);
            var url = $"repos/{_options.Owner}/{_options.Repo}/git/trees/{_options.Branch}?recursive=1";

            using var response = await http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("GitHub docs list failed: {Status} {Body}", (int)response.StatusCode, body);
                throw new HttpRequestException($"GitHub trees API returned {(int)response.StatusCode}.");
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var prefix = _options.DocsPath.TrimEnd('/') + "/";
            var entries = new List<DocsEntry>();

            if (doc.RootElement.TryGetProperty("tree", out var tree))
            {
                foreach (var node in tree.EnumerateArray())
                {
                    var type = node.GetProperty("type").GetString();
                    if (type != "blob") continue;

                    var path = node.GetProperty("path").GetString();
                    if (path is null || !path.StartsWith(prefix, StringComparison.Ordinal)) continue;
                    if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) continue;

                    var rel = path[prefix.Length..];
                    entries.Add(new DocsEntry(rel, null));
                }
            }

            _listCache = (now.AddMinutes(_options.ListCacheMinutes), entries);
            return entries;
        }
        finally
        {
            _listLock.Release();
        }
    }

    public async Task<DocsContent?> ReadAsync(string path, CancellationToken ct)
    {
        ValidatePath(path);

        var http = _httpFactory.CreateClient(RawHttpClientName);
        // Absolute URL - overrides any BaseAddress on the named client.
        var url = $"https://raw.githubusercontent.com/{_options.Owner}/{_options.Repo}/{_options.Branch}/{_options.DocsPath}/{path}";

        using var response = await http.GetAsync(url, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("GitHub docs read failed for {Path}: {Status} {Body}", path, (int)response.StatusCode, body);
            throw new HttpRequestException($"GitHub raw fetch returned {(int)response.StatusCode}.");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        var canonicalUrl = $"https://github.com/{_options.Owner}/{_options.Repo}/blob/{_options.Branch}/{_options.DocsPath}/{path}";
        return new DocsContent(path, content, canonicalUrl);
    }

    private static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));

        // No traversal, no absolute prefixes - keep the fetch scoped to the docs subtree.
        if (path.StartsWith('/') || path.StartsWith('\\'))
            throw new ArgumentException("Path must be relative.", nameof(path));
        if (path.Split('/', '\\').Any(seg => seg == ".." || seg == "."))
            throw new ArgumentException("Path may not contain '.' or '..' segments.", nameof(path));
    }
}
