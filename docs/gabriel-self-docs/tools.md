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
| `web_search` | Web | `IWebSearch` | Search the public web. |
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

Selection via `Tools:Web:Active`:
- `"ddg"` (default) → `DuckDuckGoWebSearch`. POSTs `q=` to `https://html.duckduckgo.com/html/`, regex-parses results, unwraps `/l/?uddg=…` redirects. Free, no key. Brittle if DDG changes class names — falls back to "no results" log line.
- `"brave"` → `BraveWebSearch`. Requires `Tools:Web:Brave:ApiKey`. Cleaner, 2000 queries/month free tier.

Tool description steers the model: *use for recent events, public docs of external libraries, factual lookups; DO NOT use for questions about Gabriel itself — use `docs_list` / `docs_read`.*

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

### Memory tools (Claude-style)

Two scopes:
- `user` — cross-project, per-account.
- `project` — current project only.

`memory_save` schema: `{ "key": string, "value": string, "scope": "user"|"project", "tags"?: string[] }`. `memory_list` schema: `{ "scope"?: "user"|"project"|"both", "query"?: string }`. `memory_remove` schema: `{ "id": string }`.

### Project-file tools

`list_project_files` and `read_project_file` operate on the **active project's** uploaded files (Phase 8 project storage). They depend on `IToolExecutionContext` to know which project the current turn is scoped to — if no project is active, both refuse.

### Filesystem tools (`file_info`, `list_dir`, `find`, `grep`)

Operate on the agent's host filesystem via `IAgentPathResolver`. The resolver enforces host-vs-project sandboxing — by default they're scoped to safe roots; project files go through the project tools above.

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
- DDG search can return zero results without an error if their HTML changes; check logs for `"DuckDuckGo returned no parseable results"`.
- The GitHub docs list is cached for 5 minutes — newly-added docs won't appear immediately.
- `docs_read` paths are relative to the source root, not absolute or repo-relative.

## SEE ALSO

- `agent-loop.md` — how the loop dispatches tool calls.
- `config.md` — every config section consumed by tools.
- Human-prose companion: `Gabriel.Engine/tools.md`.
