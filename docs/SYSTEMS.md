# Systems inventory

One row per externally meaningful unit — controllers, engine services, providers, tool families, infrastructure services. **Maintenance rule (CLAUDE.md): any added/removed/renamed system updates its row here, same session.** Compiled 2026-08-11.

## API controllers (`src/api/Gabriel.API/Controllers/`)

| Controller | Surface | Notes |
| --- | --- | --- |
| `AuthController` | `/api/auth/*` | Login/register/refresh; JWT in HttpOnly cookies, refresh rotation |
| `ConversationsController` | `/api/conversations/*` | CRUD, rename, SSE message stream, regenerate, variants, sequence, metrics — full reference in [conversations-api.md](conversations-api.md) |
| `ProjectsController` | `/api/projects/*` | Project CRUD, sequence, avatar reroll |
| `ProjectFilesController` | `/api/projects/{id}/files/*` | List, multipart upload, download, delete |
| `MemoriesController` | `/api/memories/*` | List/delete saved memories (user + project scope) |
| `ModelsController` | `/api/models/*` | Available-model catalog + user's preferred model |
| `SequenceController` | `/api/sequence/*` | Gabriel Sequence rendering/diagnostics endpoints |
| `DiagnosticsController` | `/api/diagnostics/*` | Diagnostics page backend |

## Engine (`src/api/Gabriel.Engine/`)

| System | Role |
| --- | --- |
| `AgentService` | The ReAct loop: streams provider events, executes tools, persists messages, triggers compact |
| `AgentContext` | Single authority assembling provider history AND the token-metrics breakdown |
| `GabrielToolBridge` (+ `ToolCallStreamSplitter`, `ToolCallBlockParser`) | Tool-call emulation decorator for text-only providers (`ToolMode: Emulated`) |
| `ModelCatalog` / `IChatProviderRegistry` | Model + provider resolution, `ToolMode` routing |
| Personality stack (`GabrielSystemPromptBuilder`, `HeuristicConversationStateUpdater`, `PromptRegistry`, `ResponsePostProcessor`) | Conversation state → system prompt → response shaping |
| Gabriel Sequence (`GabrielSequenceGenerator`, `GabrielSequenceService`, palettes/patterns) | 64-frame seed-derived avatar engine |
| `ToolRegistry` + tools | DI-scanned `ITool` implementations (table below) |
| `NaiveTokenEstimator` | chars/4 token estimate behind `ITokenEstimator` (real tokenizer is a tracked task) |
| `MetricRecorder` | Usage metric capture |

## Tools (registered `ITool`s)

| Family | Tools |
| --- | --- |
| Web | `web_search` (DDG hardened; Brave/Tavily/composite backends), `web_fetch` |
| Docs | `docs_list`, `docs_read` (local baked-in docs + GitHub fallback) |
| Files (read-only, path-hardened) | `file_info`, `list_dir`, `find`, `grep` |
| Project files | `list_project_files`, `read_project_file` |
| Memory | `memory_save`, `memory_list`, `memory_remove` |
| Utilities | `calculate`, `base_convert`, `base64`, `hash`, `color_convert`, `json_format`, `text_stats`, `text_transform`, `get_current_time` |

## Infrastructure (`src/api/Gabriel.Infrastructure/`)

| System | Role |
| --- | --- |
| `AppDbContext` + Migrations | EF Core / SQLite; migrations are the only schema path |
| Repositories (`Conversation`, `Project`, `Memory`, `Metric`, `RefreshTokenStore`) | Persistence behind Core interfaces |
| Identity (`JwtTokenService`, `ApplicationUser`, `UserPreferencesService`) | ASP.NET Identity + JWT cookie auth |
| Providers (`GrokChatProvider`, `MockChatProvider`) | `IChatProvider` implementations; Mock fakes tool calls for credit-free dev |
| `DiskProjectFileService` | Project file storage under the configured root |
| Web search backends (`DuckDuckGoWebSearch`, `BraveWebSearch`, `TavilyWebSearch`, `CompositeWebSearch`, `InstrumentedWebSearch`) | `IWebSearch` implementations |
| Docs backends (`LocalDocsLookup`, `GitHubDocsLookup`, `CompositeDocsLookup`) | `IDocsLookup` implementations |

## Webapp (`src/webapp/`)

| System | Role |
| --- | --- |
| `Chat.tsx` + `streamChat.ts` | Chat UI + hand-rolled SSE consumer (event union in [conversations-api.md](conversations-api.md)) |
| `Markdown.tsx` / `Mermaid.tsx` | Markdown pipeline (shiki, mermaid + maximize modal, copy affordances) |
| `ContextStats.tsx` | Context-window grid breakdown (per-category token buckets) |
| `Sidebar.tsx` + `ProjectPicker` | Project-scoped conversation navigation |
| Generated API client (`src/api/generated/`) | Regenerated from OpenAPI on every API build — never hand-edited |
