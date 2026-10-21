# Systems inventory

One row per externally meaningful unit — controllers, engine services, providers, tool families, infrastructure services. **Maintenance rule (CLAUDE.md): any added/removed/renamed system updates its row here, same session.** Compiled 2026-08-11; semantic memory + task-loop tools added 2026-08-12.

## API controllers (`src/api/Gabriel.API/Controllers/`)

| Controller | Surface | Notes |
| --- | --- | --- |
| `AuthController` | `/api/auth/*` | Login/register/refresh; JWT in HttpOnly cookies, refresh rotation |
| `ConversationsController` | `/api/conversations/*` | CRUD, rename, SSE message stream, edit-and-resend, stop, regenerate, variants, sequence, metrics — full reference in [conversations-api.md](conversations-api.md) |
| `ProjectsController` | `/api/projects/*` | Project CRUD, sequence, avatar reroll |
| `ProjectFilesController` | `/api/projects/{id}/files/*` | List, multipart upload, download, delete |
| `MemoriesController` | `/api/memories/*` | List/delete saved memories (user + project scope) |
| `TasksController` | `/api/tasks/*` | Autonomous tasks: create (drafts contract), edit contract, approve, pause, resume, answer, cancel |
| `ModelsController` | `/api/models/*` | Available-model catalog + user's preferred model |
| `SequenceController` | `/api/sequence/*` | Gabriel Sequence rendering/diagnostics endpoints |
| `DiagnosticsController` | `/api/diagnostics/*` | Diagnostics page backend |

## Engine (`src/api/Gabriel.Engine/`)

| System | Role |
| --- | --- |
| `AgentService` | The ReAct loop: streams provider events, executes tools (parallel-safe tools concurrently, capped by `Agent:MaxParallelToolCalls`), persists messages, triggers compact, injects semantic memory recall |
| Autonomous tasks (`AgentTaskService`, `AgentTaskRunner`, `TaskStepPolicy`, `TaskContractDrafter`, `TaskStepReviewer`) | Delegated long-running work: the API side, the per-step engine (one agent turn per step), the pure decision rules, and the two tool-less judgement calls. Driven by `AgentTaskWorker` (Infrastructure) |
| `ActiveTurnRegistry` | In-process map of running turns so `POST .../messages/stop` can cancel one; the loop then persists the partial reply and ends the stream normally |
| `ConversationTitleGenerator` | Auto-titles placeholder-titled conversations from the first user message; started by `AgentService` in parallel with the turn, emitted as `titleUpdated` (`Agent:AutoTitle`) |
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
| Project files | `list_project_files`, `read_project_file`, `write_project_file`, `edit_project_file` (text only, confined to project storage) |
| Memory | `memory_save`, `memory_list`, `memory_remove`, `memory_search` (semantic, Qdrant-backed; reports unavailable when `SemanticMemory:Enabled=false`) |
| Tasks | `todo_write`, `todo_read` (Claude Code-style per-conversation plan, persisted on `Conversation.TodoListJson`), `task_checkpoint` (end-of-step report inside an autonomous task) |
| Shell | `shell_execute` (off by default — `AgentTools:Shell:Enabled`; deny-list + timeout + output cap) |
| Utilities | `calculate`, `base_convert`, `base64`, `hash`, `color_convert`, `json_format`, `text_stats`, `text_transform`, `get_current_time` |

Tools declare `ITool.IsParallelSafe`; the loop runs flagged tools from one batch concurrently while DbContext-backed tools stay serial.

## Infrastructure (`src/api/Gabriel.Infrastructure/`)

| System | Role |
| --- | --- |
| `AppDbContext` + Migrations | EF Core / SQLite; migrations are the only schema path |
| Repositories (`Conversation`, `Project`, `Memory`, `Metric`, `RefreshTokenStore`) | Persistence behind Core interfaces |
| Identity (`JwtTokenService`, `ApplicationUser`, `UserPreferencesService`) | ASP.NET Identity + JWT cookie auth |
| Providers (`GrokChatProvider`, `MockChatProvider`) | `IChatProvider` implementations; Mock fakes tool calls for credit-free dev |
| `DiskProjectFileService` | Project file storage under the configured root; also the agent write path (`WriteTextAsync`, atomic overwrite) |
| `AgentTaskWorker` | Hosted service stepping Running autonomous tasks round-robin, one step at a time, each in its own scope acting as the task owner (`CurrentUserOverride`) |
| Web search backends (`DuckDuckGoWebSearch`, `BraveWebSearch`, `TavilyWebSearch`, `CompositeWebSearch`, `InstrumentedWebSearch`) | `IWebSearch` implementations |
| Docs backends (`LocalDocsLookup`, `GitHubDocsLookup`, `CompositeDocsLookup`) | `IDocsLookup` implementations |
| Semantic memory (`QdrantMemoryIndex`, `NoopSemanticMemoryIndex`, `SemanticMemoryBackfillService`) | `ISemanticMemoryIndex` over Qdrant REST; SQLite stays authoritative, index is rebuildable (startup backfill); no-op when disabled |
| Embeddings (`MockEmbeddingProvider`, `LocalOnnxEmbeddingProvider`, `OpenAIEmbeddingProvider`, `EmbeddingPipeline`) | `IEmbeddingProvider` — hashed BoW (zero setup), open-source all-MiniLM-L6-v2 in-process via ONNX Runtime (no key/vendor; `scripts/download-embedding-model.ps1`), or OpenAI `text-embedding-3-small`. Missing prerequisites fall back to Mock; Qdrant collections are dimension-suffixed so provider switches can't mix vectors |

## Webapp (`src/webapp/`)

| System | Role |
| --- | --- |
| `Chat.tsx` + `streamChat.ts` | Chat UI + hand-rolled SSE consumer (event union in [conversations-api.md](conversations-api.md)) |
| `Markdown.tsx` / `Mermaid.tsx` | Markdown pipeline (rehype-highlight, KaTeX, mermaid + maximize modal, copy affordances) |
| `ContextStats.tsx` | Context-window grid breakdown (per-category token buckets) |
| `Sidebar.tsx` + `ProjectPicker` | Project-scoped conversation navigation |
| Generated API client (`src/api/generated/`) | Regenerated from OpenAPI on every API build — never hand-edited |
