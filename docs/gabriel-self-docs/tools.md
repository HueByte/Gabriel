# Tools

## PURPOSE
The `ITool` / `IToolRegistry` contract, the full registered tool list, what each tool does and when to use it, and the recipe for adding a new tool.

## USE WHEN
- User asks what tools you have.
- User asks how to add a tool, or how the tool system works.
- User asks how `docs_list` / `docs_read` finds documents.
- User asks about SSRF defense, search providers, or rate limits.
- You yourself need to decide which tool to call.

## QUICK REFERENCE — registered tools

| Tool | Category | Provider dep | One-line purpose |
| --- | --- | --- | --- |
| `get_current_time` | Time | none | UTC ISO-8601 timestamp. |
| `calculate` | Math | none | Evaluate an arithmetic expression (precedence, `^`, `%`, functions, constants). |
| `base_convert` | Numbers | none | Convert a whole number between bases 2-36 (binary/octal/decimal/hex/...). |
| `base64` | Codecs | none | Encode text to Base64 or decode Base64 to text (UTF-8; standard + URL-safe). |
| `web_search` | Web | `IWebSearch` | Search the public web. Backend selected via `Tools:Web:Active` — one of `ddg` (default), `brave`, `tavily`, or any comma-separated subset for parallel-query + merge. |
| `web_fetch` | Web | `IUrlFetcher` | Fetch + clean a URL's text. |
| `docs_list` | **Self-docs** | `IDocsLookup` | List Gabriel's official docs. **Primary source = LLM-native `gabriel-self-docs`**. |
| `docs_read` | **Self-docs** | `IDocsLookup` | Read one Gabriel doc by path. |
| `list_project_files` | Project | `IProjectFileService` | List files in the active project's storage. |
| `read_project_file` | Project | `IProjectFileService` | Read a single project file. |
| `memory_save` | Memory | `IMemoryRepository` | Save a memory at user or project scope. |
| `memory_list` | Memory | `IMemoryRepository` | List saved memories. |
| `memory_remove` | Memory | `IMemoryRepository` | Delete a memory by id. |
| `file_info` | Files | `IAgentPathResolver` | Stat a file/dir on the agent host. |
| `list_dir` | Files | `IAgentPathResolver` | List a directory. |
| `find` | Files | `IAgentPathResolver` | Glob search. |
| `grep` | Files | `IAgentPathResolver` | Text search within files. |

## DETAILS

### Tool model

```csharp
public interface ITool
{
    string Name { get; }
    string Description { get; }
    string ParametersJsonSchema { get; }       // raw JSON schema, passed verbatim to the LLM
    Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct);
}

public interface IToolRegistry
{
    IReadOnlyList<ITool> All { get; }
    ITool? Find(string name);
    IReadOnlyList<ToolDescriptor> AsDescriptors();
}
```

`ToolRegistry` consumes `IEnumerable<ITool>` from DI — every registered `ITool` is auto-discovered. No central manifest.

`AsDescriptors()` projects each tool into the wire shape sent to the provider:
```csharp
public record ToolDescriptor(string Name, string Description, string ParametersJsonSchema);
```

### How a tool call flows

1. Provider streams `ToolCallReadyEvent(id, name, argsJson)` then `FinishEvent(ToolCalls)`.
2. `AgentService` persists the assistant tool-call message; emits `AgentToolCall`.
3. For each call: `registry.Find(name).ExecuteAsync(argsJson, ct)` → observation string.
4. Errors / unknown tools become observations: `"Error executing {tool}: {msg}"` or `"Error: tool 'foo' is not registered."` — the loop never crashes from a tool failure.
5. Persist tool result; emit `AgentToolResult`.
6. Loop continues: history now includes the tool result; provider gets called again.

### `web_search`

Schema: `{ "query": string, "limit": int = 5 }`.

Selection via `Tools:Web:Active`. The value is a **comma-separated list** of provider keys; supply one or many. When more than one is listed, all of them are queried in parallel per call and the results are merged by URL (cross-provider hits rank first). Examples:

- `"ddg"` (default) — `DuckDuckGoWebSearch`. GETs `q=` from `https://html.duckduckgo.com/html/`, regex-parses results, unwraps `/l/?uddg=…` redirects. Falls back automatically to `https://lite.duckduckgo.com/lite/` when html/ returns zero results or trips DDG's anomaly/CAPTCHA page (the lite/ endpoint lives on its own subdomain, so the GET target is set absolute, not relative to the html/ client's BaseAddress). A session-warmup pass GETs the DDG homepage once per cookie-jar lifetime so the search request carries the cookies a real browser would have collected; the request set rotates across a small pool of recent Chrome/Firefox/Edge User-Agents (picked once per session, not per request — UA churn within one cookie jar is itself a bot tell), and each fetch is preceded by 200–1200 ms of randomized jitter. Cookies + decompression run on the `HttpClientHandler` (`UseCookies = true`, `CookieContainer = new`, `AutomaticDecompression = All`, `SetHandlerLifetime = 1h`). When both endpoints come back as bot-block pages, the provider clears its session state (so the next call re-warms with a different fingerprint) AND throws an `InvalidOperationException` whose message names Brave / Tavily as the reliable alternatives — `CompositeWebSearch` swallows that in multi-provider mode so other backends still answer; single-provider mode surfaces it to `WebSearchTool` which reports it as `Error: web search failed - …`. Free, no key, hobby-grade reliability; anything more reliable needs a real API.
- `"brave"` — `BraveWebSearch`. Plain GET against `api.search.brave.com/res/v1/web/search` with `X-Subscription-Token`. Requires `Tools:Web:Brave:ApiKey`. Independent index (not Google/Bing relay); 2000 queries/month free tier.
- `"tavily"` — `TavilyWebSearch`. POSTs `{ query, max_results, search_depth }` against `api.tavily.com/search` with a Bearer token. Requires `Tools:Web:Tavily:ApiKey`. Purpose-built for LLM agents: each result's `content` field is pre-trimmed for context-window economy, so the snippet you see is already model-ready. Free tier is generous for hobby use.
- `"tavily,brave,ddg"` (or any subset, in any order) — `CompositeWebSearch` wraps the selected providers and runs them in parallel. URLs are canonicalized (lowercase host, dropped trailing slash, stripped `utm_*` / `fbclid` / `gclid` / `ref`) before merging. Ranking score is `appearances * 1000 - min_rank_across_providers`, so a URL surfaced by two providers ranks ahead of any single-provider hit. One provider failing (network error, missing key, anomaly page) does not poison the others — the merge proceeds with whatever did return, with a warn-level log per failed provider.

Unknown keys in the list are silently skipped; if the entire list resolves to nothing recognized, DDG is used as a safety fallback so the tool never crashes at first call. With a single provider in the list the composite is bypassed entirely — no merge overhead on the hot path.

Tool description steers the model: *use for recent events, public docs of external libraries, factual lookups; DO NOT use for questions about Gabriel itself — use `docs_list` / `docs_read`.*

#### Provider health metrics

Every `IWebSearch` implementation is wrapped in an `InstrumentedWebSearch` decorator at DI registration, recording one row per call into the generic metric event log (`MetricEntries` table; see `agent-loop.md` → "Metric event log" if it ever lands there). The row's `System` column is `"web_search.<provider>"` (e.g. `"web_search.tavily"`); its `Metric` column is a JSON document with `outcome` (`success` / `empty` / `error`), `query`, `result_count`, `latency_ms`, and `error_message`. The composite path picks these up automatically — and single-provider mode is tracked too, since "is the one provider you configured still working?" is exactly the question the metric exists to answer.

`GET /api/diagnostics/web-search` (authenticated, not admin-gated) reads the last N rows (default 200, override with `?windowSize=`) under the `web_search.` prefix and aggregates them on the server: one entry per provider with total / successful / error / empty counts, average latency over successful + empty calls, the most recent success and failure timestamps, and the most recent failure detail (query + error message). The response also carries a `HasUnhealthyProvider` flag the UI can use to badge the search tool. A provider counts as unhealthy when it has at least one event in the window and either has zero successful calls or its most recent event was a failure. Providers with no events in the window are absent from the response — there's nothing to report.

`GET /api/diagnostics/metrics?system=<exact>&limit=<N>` (or `?systemPrefix=<prefix>&limit=<N>`) is the generic raw browse, useful when you want to see the actual JSON payloads instead of the per-provider rollup.

Cancellation isn't recorded — the decorator catches `OperationCanceledException` separately and lets it pass through unchanged so caller-side aborts (user navigated away, agent loop bailed) don't pollute the log. Errors mid-cancellation still record (using `CancellationToken.None` for the metric write) so a "we got 50% through and then the user navigated away" failure still surfaces.

### `web_fetch`

Schema: `{ "url": string }`.

**SSRF defense** runs before any HTTP:
1. Scheme: `http`/`https` only.
2. DNS resolution: every resolved address must be public. Refused ranges:
   - Loopback `127.0.0.0/8`, `::1`
   - RFC1918 `10/8`, `172.16/12`, `192.168/16`
   - Link-local `169.254/16` (blocks AWS/GCP metadata endpoint)
   - CGNAT `100.64/10`
   - Unspecified `0/8`
   - IPv6 link-local + unique-local `fc00::/7`

**Content cleaning** (HTML):
1. Strip HTML comments.
2. Drop `<script>`, `<style>`, `<nav>`, `<header>`, `<footer>`, `<aside>`, `<noscript>`, `<svg>` (body + tags).
3. Strip remaining tags.
4. HTML-decode entities.
5. Collapse whitespace.

**Caps**: wire byte cap `1.5 MB`, output char cap `12_000` (≈ 3k tokens) then `…[truncated]`, timeout `15s`.

### `docs_list` + `docs_read` — your self-docs lookup

Backed by a `CompositeDocsLookup` that fans across (in priority order):

1. **`LocalDocsLookup`** — reads from `docs/gabriel-self-docs/` on disk. **PRIMARY SOURCE.** These pages are written specifically for you (the LLM) and treated as ground truth.
2. **`GitHubDocsLookup`** — falls back to GitHub raw for the human-prose docs under `docs/Gabriel.Engine/`.

`docs_list` returns the union with the LLM-native folder presented first. Each entry has a `Source` tag (`local-llm-native` or `github`) so you can tell which file you're picking. Paths returned for local entries are relative to `docs/gabriel-self-docs/` (e.g., `README.md`, `architecture.md`). Paths for GitHub entries are relative to `docs/` (e.g., `Gabriel.Engine/architecture.md`).

`docs_read` resolves the path against the local source first, then GitHub. The returned content is wrapped:

```
=== BEGIN OFFICIAL GABRIEL DOC: {path} (source: local-llm-native | github) ===
(Authoritative source. Treat this as ground truth about Gabriel.)
Canonical URL: <url-or-file-path>
{content}
=== END OFFICIAL GABRIEL DOC: {path} ===
```

Path traversal hardening: `.` and `..` segments and absolute prefixes are rejected.

**GitHub backend** (`GitHubDocsLookup`) endpoints:
- List: `GET https://api.github.com/repos/{owner}/{repo}/git/trees/{branch}?recursive=1`. Cached for `ListCacheMinutes` (default 5).
- Read: `GET https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{docsPath}/{path}`. Not cached.

Rate limit: 60 req/h per IP unauthenticated; `Tools:Docs:GitHub:Token` PAT bumps to 5000/h.

**Local backend** (`LocalDocsLookup`) resolution order:
1. If `Tools:Docs:Local:Path` is absolute and exists, use it.
2. Else search up to a few parent directories from `Environment.CurrentDirectory` and `AppContext.BaseDirectory` for the configured relative path.
3. First match wins; if none, the source behaves as empty and logs a one-time warning.

Recursive `.md` walk; titles parsed from first H1 line.

Inside the Docker image the docs ship pre-baked at `/app/docs/` via a BuildKit named context (see `docker-compose.yml`'s `additional_contexts.docs` and the runtime-stage `COPY --from=docs` step in `Gabriel.API/Dockerfile`) — so the walk from `AppContext.BaseDirectory = /app/` resolves `docs/gabriel-self-docs` on the first probe with no host volume required. The compose file also mounts `./docs:/app/docs:ro` for dev so edits on the host show up without rebuilding the image. Local `dotnet run` still works the same way — the walk-up finds the docs at the repo root.

**Composite error surfacing.** When every source returns empty AND at least one threw, `CompositeDocsLookup.ListAsync` rethrows the last transient error instead of swallowing it. So if GitHub-fallback fails (rate limit, 5xx, DNS), `docs_list` reports `Error: could not list official Gabriel docs - {real reason}` instead of the generic "No Gabriel docs are currently available" — the agent (and the user reading the agent's tool output) knows whether to retry, swap configs, or pivot to `web_search`.

### Memory tools

Two scopes:

- `user` — cross-project, per-account. Saved memories show up in the `[Saved memories]` block of every conversation.
- `project` — current project only. Saved memories show up only in conversations attached to that project.

When the current conversation is in a non-default project, the system prompt's `[Project context]` block tells you to default to `scope='project'`; use `scope='user'` only when the user clearly means the memory to follow them across every project.

`memory_save` schema:

```json
{
  "scope":       "user" | "project",
  "type":        "user" | "feedback" | "project" | "reference",
  "name":        "kebab-case-slug",
  "description": "one-line summary used at retrieval time",
  "body":        "the actual content. For feedback/project entries, lead with the rule/fact then **Why:** and **How to apply:** lines."
}
```

Idempotent — saving twice with the same `(scope, name)` updates the existing entry in place.

`memory_list` takes **no arguments**. Returns the union of user-scope memories plus the current project's project-scope memories (if any), one line per entry: `[type, scope] name — description`. Use this to scan before deciding whether to read the body of a specific entry — the system-prompt memory block lists names + descriptions but not bodies.

`memory_remove` schema:

```json
{
  "scope": "user" | "project",
  "name":  "kebab-case-slug"
}
```

Lookup is by `(scope, name)`. `scope='project'` only operates on memories saved for THIS project. Returns a confirmation string indicating whether anything matched.

### Project-file tools

`list_project_files` and `read_project_file` operate on the **active project's** uploaded files (Phase 8 project storage). They depend on `IToolExecutionContext` to know which project the current turn is scoped to — if no project is active, both refuse.

### Filesystem tools (`file_info`, `list_dir`, `find`, `grep`)

Operate on the agent's host filesystem via `IAgentPathResolver`. The resolver enforces host-vs-project sandboxing — by default they're scoped to safe roots; project files go through the project tools above.

### `calculate`

Schema: `{ "expression": string }`. No provider dependency, no I/O — a pure function of the argument string. Use it for any arithmetic instead of computing in-head, which is error-prone for multi-digit numbers.

A recursive-descent evaluator over doubles. Precedence, lowest-binding first: `+ -` → `* / %` → unary `+ -` → `^` (right-associative, so `2^3^2 = 512`). Unary binds below the exponent, matching convention: `-3^2 = -9`, and the exponent's right side accepts a unary so `2^-3 = 0.125`. Supports parentheses, the functions `sqrt abs round floor ceil sign min max pow sin cos tan asin acos atan log ln exp` (trig in radians; `log(x)` is base-10, `log(x, b)` is base-b, `round(x, digits)` takes an optional precision), and the constants `pi e tau`.

Operators are required between terms — `2*pi`, not `2pi`. Results format friendlily: integral values print without a decimal point, everything else trims to 12 fractional digits so binary-float dust folds away (`0.1 + 0.2 = 0.3`). Bad input never throws into the loop — unknown names, malformed syntax, division/modulo by zero, `sqrt` of a negative, and results that overflow to ±infinity or NaN all come back as `Error: …` observations. Input is capped at 1000 chars and nesting at depth 64.

### `base_convert`

Schema: `{ "value": string, "from_base"?: int = 10, "to_base": int }`. No provider dependency, no I/O. Use it for any base conversion (read a hex value as decimal, turn a binary literal into a number) instead of converting in-head, which is error-prone past a couple of digits.

Backed by `BigInteger`, so the magnitude is unbounded — a 40-digit hex value converts without overflow. Digit alphabet is `0-9` then `A-Z` for values 10-35; both bases must be 2-36. Input is case-insensitive, accepts a leading `-`, and ignores `_` as a grouping separator (so `1_0000_0000` is fine). Whole numbers only — no fractional part. Output echoes the result as `value (base F) = result (base T)`; result letters are uppercase. Bad input comes back as an `Error: …` observation: an out-of-range base, a digit that isn't valid for `from_base` (e.g. `'2'` in base 2, `'G'` in base 16), an empty value, or a bare sign. Input capped at 1000 chars.

### `base64`

Schema: `{ "text": string, "op": "encode" | "decode", "url_safe"?: bool = false }`. No provider dependency, no I/O. Use it to read an encoded token/payload or to encode text for transport, rather than attempting Base64 by hand. Text is UTF-8 throughout.

`encode` is `Convert.ToBase64String` over the UTF-8 bytes; `url_safe=true` swaps `+/` for `-_` and drops `=` padding. `decode` is tolerant — it folds URL-safe characters back, strips whitespace, and restores padding before `Convert.FromBase64String`, so it accepts either alphabet regardless of the flag (the flag only shapes `encode` output). Returns `Encoded: …` or `Decoded: …`. Errors come back as observations: a non-string/empty `text`, an `op` that isn't `encode`/`decode`, or input that isn't valid Base64 on decode. Input capped at 100,000 chars.

### `get_current_time`

No args. Returns `DateTimeOffset.UtcNow.ToString("o")`. Trivial starter.

## Adding a new tool

1. Declare any external-dep interface in `Gabriel.Engine/Tools/<area>/` (e.g., `IStripeClient`).
2. Implement `ITool` in `Gabriel.Engine/Tools/<area>/`. Tight JSON schema. Description should:
   - **Lead with WHEN to use** it (not what it returns).
   - **Spell out what NOT to use it for** when adjacent tools overlap.
   - **Use AUTHORITATIVE / CANONICAL framing** for trusted sources.
   - **Mention the other tool by name** when relevant.
   - **Keep schemas tight** (a 12-field schema buries the point).
3. Register:
   - `services.AddScoped<ITool, MyTool>()` in `Gabriel.Engine.DependencyInjection.AddEngineServices`.
   - Concrete external-dep impls in `Gabriel.Infrastructure.DependencyInjection.AddInfrastructure`.
   - Options POCO (implementing `IConfigSection<TSelf>`) in `Gabriel.Core.Configuration` if config-driven.

Registry picks the new tool up automatically.

## INVARIANTS

- Tool execution errors NEVER crash the agent — they become observation strings.
- Tool calls run serially inside an iteration.
- Unknown tool names return an error observation, not an exception.
- All providers (`IWebSearch`, `IUrlFetcher`, `IDocsLookup`) are singletons; HTTP clients use `IHttpClientFactory` named clients.
- `web_fetch` URLs that resolve to ANY private address are rejected.

## PITFALLS

- "Why didn't `web_search` find a Gabriel doc?" — because Gabriel's docs aren't on the public web; `docs_list` / `docs_read` is the right tool.
- DDG can return *genuinely* zero results when its HTML drifts or both endpoints' parsers come up empty on a real query — distinct from the bot-block path, which now throws with a specific error message. If the tool returns `Error: web search failed - DuckDuckGo blocked this request as bot traffic…`, the deployment's IP has been flagged; a Brave or Tavily key is the actual fix. If it returns the bare `No results for: …` string, DDG answered cleanly with nothing; try rephrasing or check logs for `"DuckDuckGo html/ returned 0 parseable results"` / `"DuckDuckGo lite/ returned 0 parseable results"` (each logs the first 200 chars of the response so you can see what the parser couldn't match).
- The GitHub docs list is cached for 5 minutes — newly-added docs won't appear immediately.
- `docs_read` paths are relative to the source root, not absolute or repo-relative.

## SEE ALSO

- `agent-loop.md` — how the loop dispatches tool calls.
- `config.md` — every config section consumed by tools.
- Human-prose companion: `Gabriel.Engine/tools.md`.
