namespace Gabriel.Engine.Tools.Web;

// Fetches the contents of a public URL and returns cleaned text suitable for
// the agent to read. Implementations are expected to:
//   - reject non-HTTP(S) schemes
//   - reject internal / private / loopback addresses (SSRF guard)
//   - cap response size so a huge page can't blow the model's context
//   - convert HTML to plain text (strip script/style/nav, then strip tags)
//
// WebFetchTool depends on this - swap implementations without touching the
// agent layer.
public interface IUrlFetcher
{
    Task<UrlFetchResult> FetchAsync(string url, CancellationToken ct);
}

public sealed record UrlFetchResult(
    string FinalUrl,         // after redirects
    string ContentType,
    string Content,          // cleaned text
    bool Truncated,          // true if the original page was bigger than our cap
    int ContentLength);      // length of `Content` after cleaning
