using System.Net;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Engine.Tools.Docs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Tools.Docs;

// IDocsLookup over GitHub. Two transport paths:
//   - List   : api.github.com/repos/{owner}/{repo}/git/trees/{branch}?recursive=1
//   - Read   : raw.githubusercontent.com/{owner}/{repo}/{branch}/{path}
//
// Paths share one namespace with LocalDocsLookup: self-docs pages (under
// GitHubDocsOptions.SelfDocsPath) are bare names, everything else is relative to
// DocsPath. A bare name always means the self-doc - so `README.md` is the
// self-docs index, never docs/README.md (the human docs map, which is dropped
// from the listing rather than shown under a name that reads a different file).
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
    private Listing? _listCache;

    // Entries in docs_list order plus the namespace-path -> repo-path map reads
    // resolve through, so a read can only fetch a page the listing shows.
    private sealed record Listing(
        DateTimeOffset Until,
        IReadOnlyList<DocsEntry> Entries,
        IReadOnlyDictionary<string, string> RepoPaths);

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
        => (await GetListingAsync(ct)).Entries;

    private async Task<Listing> GetListingAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        if (_listCache is { } cached && cached.Until > now)
            return cached;

        await _listLock.WaitAsync(ct);
        try
        {
            // Re-check after acquiring the lock - another caller may have filled the cache.
            if (_listCache is { } recheck && recheck.Until > now)
                return recheck;

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

            var docsPrefix = _options.DocsPath.Trim('/') + "/";
            var selfPrefix = docsPrefix + _options.SelfDocsPath.Trim('/') + "/";
            var selfDocs = new List<(string Name, string RepoPath)>();
            var humanDocs = new List<(string Name, string RepoPath)>();

            if (doc.RootElement.TryGetProperty("tree", out var tree))
            {
                foreach (var node in tree.EnumerateArray())
                {
                    var type = node.GetProperty("type").GetString();
                    if (type != "blob") continue;

                    var path = node.GetProperty("path").GetString();
                    if (path is null || !path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) continue;

                    if (path.StartsWith(selfPrefix, StringComparison.Ordinal))
                        selfDocs.Add((path[selfPrefix.Length..], path));
                    else if (path.StartsWith(docsPrefix, StringComparison.Ordinal))
                        humanDocs.Add((path[docsPrefix.Length..], path));
                }
            }

            // Self-docs first; TryAdd then drops any human doc whose name a
            // self-doc already claimed.
            var repoPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var entries = new List<DocsEntry>();
            foreach (var (name, repoPath) in selfDocs.Concat(humanDocs))
            {
                if (repoPaths.TryAdd(name, repoPath))
                    entries.Add(new DocsEntry(name, null, DocsSources.GitHub));
            }

            _listCache = new Listing(now.AddMinutes(_options.ListCacheMinutes), entries, repoPaths);
            return _listCache;
        }
        finally
        {
            _listLock.Release();
        }
    }

    public async Task<DocsContent?> ReadAsync(string path, CancellationToken ct)
    {
        ValidatePath(path);

        var repoPath = await ResolveRepoPathAsync(path, ct);
        if (repoPath is null) return null;

        var http = _httpFactory.CreateClient(RawHttpClientName);
        // Absolute URL - overrides any BaseAddress on the named client.
        var url = $"https://raw.githubusercontent.com/{_options.Owner}/{_options.Repo}/{_options.Branch}/{repoPath}";

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
        var canonicalUrl = $"https://github.com/{_options.Owner}/{_options.Repo}/blob/{_options.Branch}/{repoPath}";
        return new DocsContent(path, content, canonicalUrl, DocsSources.GitHub);
    }

    // Falls back to the self-docs-first guess if the listing itself is
    // unavailable - the bare-name form the self-docs index tells the model to use.
    private async Task<string?> ResolveRepoPathAsync(string path, CancellationToken ct)
    {
        var normalized = path.Replace('\\', '/');
        try
        {
            var listing = await GetListingAsync(ct);
            return listing.RepoPaths.GetValueOrDefault(normalized);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "GitHub docs list unavailable while resolving '{Path}'; assuming a self-docs page.", path);
            return $"{_options.DocsPath.Trim('/')}/{_options.SelfDocsPath.Trim('/')}/{normalized}";
        }
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
