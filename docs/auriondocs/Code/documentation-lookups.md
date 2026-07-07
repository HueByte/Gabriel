# Documentation sources with multi-source lookups

> A multi-source docs lookup system supporting local, GitHub, and composite strategies.

A short, robust multi-source docs lookup lets the running service prefer fast, local model docs while falling back to a GitHub-hosted repository and presenting a single, deduplicated catalog to callers. These files implement that pattern: a composite coordinator that queries ordered sources, a local filesystem-backed provider optimized for offline and primary use, and a GitHub-backed provider that catalogs and fetches live content while respecting rate limits and traversal safety.

## CompositeDocsLookup.cs
Combines multiple docs sources into a unified lookup.

The [CompositeDocsLookup](../Code/src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs.md) class implements an IDocsLookup that wraps an ordered list of other IDocsLookup instances and exposes a unified ListAsync and ReadAsync surface. For ListAsync it materializes the input sources into a list to preserve registration order, then unions entries by relative path using case-insensitive comparison, deduplicating duplicates so that entries from the primary (earlier) source win when paths collide. For ReadAsync it queries each inner source in order and returns the first non-null content; input-validation and argument exceptions short-circuit immediately while other exceptions are logged and cause the composite to continue to the next source. In error scenarios CompositeDocsLookup logs per-source failures and, if all sources either fail or return nothing and at least one threw, rethrows the last transient exception to help surface root causes (e.g., rate limits or upstream 5xx). This class depends on and delegates to the [GitHubDocsLookup](../Code/src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs.md) and [LocalDocsLookup](../Code/src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs.md) implementations listed below.

## GitHubDocsLookup.cs
Implements docs lookup from GitHub docs repository.

The [GitHubDocsLookup](../Code/src/api/Gabriel.Infrastructure/Tools/Docs/GitHubDocsLookup.cs.md) concrete implements IDocsLookup by treating GitHub as two transports: a Trees API listing used by ListAsync to build a catalog of Markdown entries (cached up to a configured ListCacheMinutes) and a Raw content endpoint used by ReadAsync to fetch live document text. It protects against path traversal by rejecting any request with '..' segments or absolute-prefixed paths before making HTTP calls, and it recommends configuring two named HttpClient instances (one for the trees API and one for raw files) so each can be tuned independently. The implementation uses a semaphore and caching to be thread-safe and rate-limit friendly under concurrent load, and it logs response bodies on Trees API failures and throws an HttpRequestException so callers can treat listing failures as operational errors. This provider is consumed by the composite coordinator when local docs are absent or do not contain a requested path.

## LocalDocsLookup.cs
Implements docs lookup from local docs store.

The [LocalDocsLookup](../Code/src/api/Gabriel.Infrastructure/Tools/Docs/LocalDocsLookup.cs.md) class implements an IDocsLookup that sources Markdown files from a local on-disk self-docs root and is designed to act as the primary, offline-capable source. It lazily resolves the root directory on first use by honoring an absolute LocalDocsOptions.Path if provided and existing; otherwise it walks upward from the current working directory and AppContext.BaseDirectory up to eight levels to find a matching root. Its ListAsync enumerates every Markdown file under that root and yields DocsEntry items with a relative path, a title parsed from the file's first H1 (which may be null if absent), and a source marker indicating LocalLlmNative; ReadAsync validates containment and returns the file contents with a canonical URL and the same source marker, or null when the path is not within the resolved root. When the root cannot be found or LocalDocsOptions.Path is not configured, the implementation behaves as though there are no docs (ListAsync returns empty and ReadAsync returns null), allowing the [CompositeDocsLookup](../Code/src/api/Gabriel.Infrastructure/Tools/Docs/CompositeDocsLookup.cs.md) to fall back to remote sources such as GitHub.

How the pieces fit

CompositeDocsLookup is the coordinator: it prefers the primary source and merges catalogs across providers, delegating to LocalDocsLookup for fast, offline-first behavior and to GitHubDocsLookup when live or fallback content is needed. The Composite handles error isolation and deduplication rules (primary wins) while Local performs lazy root resolution and containment checks and GitHub provides cached listings plus live reads with traversal protections and rate-limit-aware semaphores. Together they present a single, ordered lookup surface that favors local self-docs but transparently falls back to a remote repository when local content is missing or unavailable.

---
*Synthesised by Aurion on 2026-07-07 18:11:07 UTC*
