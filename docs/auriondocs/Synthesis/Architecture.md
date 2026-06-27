# Architecture — HueByte/Gabriel

> *Auto-synthesized from 527 documented symbols across 243 files on `main`.*

## Topic Guides

Deep-dives into cross-cutting concerns synthesized from the per-symbol corpus.

- [Authentication, tokens, and user sessions](authentication-tokens.md) — How the system authenticates users, issues and rotates tokens, and manages session cookies across API layers. This topic covers the surface API, token providers, and identity persistence.
- [Configuration and secret management via Infisical](infisical-configuration.md) — How the application reads and applies secrets/configuration from Infisical at startup and exposes it to the configuration system. This enables dynamic secrets handling in startup.
- [Observability and telemetry](observability.md) — Logging enrichment for timestamps and a lightweight metrics surface to record telemetry events. The topic covers log enrichment extensions and metric recording.
- [Persistence layer and repositories](data-repositories.md) — EF Core entity mappings and repository patterns for persisting conversations, memories, and related data; includes a unit of work for coordinated saves.
- [Chat providers integration and registry](chat-providers.md) — Abstractions and registry for chat providers used by the agent runtime; how providers are discovered, registered, and resolved.

## System Overview
This repository implements Gabriel, an HTTP API-driven agent platform that exposes authenticated endpoints for user, conversation, project, file, and memory management while also providing a collection of runtime "tools" the agent can call to inspect docs, files, web pages, and memories. The system follows an MVC-style API surface (Controllers) backed by repository-backed state (e.g., an AppDbContext-backed conversation repository) and an engine of ITool implementations that the agent runtime invokes to perform I/O and retrieval tasks. Persistent state lives in repository-backed stores (conversation, project, memory, metric repositories) and project-attached files; tools and external providers (e.g., a Grok HTTP auth handler) handle external integration and runtime operations.

## Key Components
**Controllers** — HTTP API surface for managing users, conversations, projects, files, memories, models and diagnostics. Implemented by [`AuthController`](src/api/Gabriel.API/Controllers/AuthController.cs.md), [`ConversationsController`](src/api/Gabriel.API/Controllers/ConversationsController.cs.md), [`DiagnosticsController`](src/api/Gabriel.API/Controllers/DiagnosticsController.cs.md), [`MemoriesController`](src/api/Gabriel.API/Controllers/MemoriesController.cs.md), [`ModelsController`](src/api/Gabriel.API/Controllers/ModelsController.cs.md), [`ProjectFilesController`](src/api/Gabriel.API/Controllers/ProjectFilesController.cs.md), [`ProjectsController`](src/api/Gabriel.API/Controllers/ProjectsController.cs.md), [`SequenceController`](src/api/Gabriel.API/Controllers/SequenceController.cs.md).

**Agent Tools** — A runtime tool abstraction and a set of concrete, callable tools the agent uses to read docs, fetch web content, inspect and search files, and manage memories. Implemented by [`ITool`](src/api/Gabriel.Engine/Tools/ITool.cs.md), [`DocsListTool`](src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs.md), [`DocsReadTool`](src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs.md), [`FileInfoTool`](src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md), [`FindTool`](src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md), [`GetCurrentTimeTool`](src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs.md), [`GrepTool`](src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md), [`ListDirTool`](src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md), [`ListProjectFilesTool`](src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs.md), [`MemoryListTool`](src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs.md), [`MemoryRemoveTool`](src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs.md), [`MemorySaveTool`](src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs.md), [`ReadProjectFileTool`](src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs.md), [`WebFetchTool`](src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs.md), [`WebSearchTool`](src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs.md).

**Repositories** — Persistence abstractions and implementations that store Conversations, Projects, Memories, and metric/event logs; used by controllers and background logic to read and mutate state. Implemented by [`IConversationRepository`](src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md), [`ConversationRepository`](src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md), [`IMemoryRepository`](src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md), [`IMetricRepository`](src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md), [`IProjectRepository`](src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md).

**Handlers / Middleware** — Cross-cutting request handling for errors and diagnostics that translate exceptions into HTTP ProblemDetails and surface operational diagnostics. Implemented by [`GlobalExceptionHandler`](src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs.md).

**Providers / External integrations** — Infrastructure for calling external services and attaching required auth to outbound requests (e.g., Grok). Implemented by [`GrokAuthHandler`](src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs.md).

## Component Map

- **src** — 238 documented files
- **prototype** — 5 documented files

### Components by Role

**Agent Tools**
- `DocsListTool` — `src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs`
- `DocsReadTool` — `src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs`
- `FileInfoTool` — `src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs`
- `FindTool` — `src/api/Gabriel.Engine/Tools/Files/FindTool.cs`
- `GetCurrentTimeTool` — `src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs`
- `GrepTool` — `src/api/Gabriel.Engine/Tools/Files/GrepTool.cs`
- `ITool` — `src/api/Gabriel.Engine/Tools/ITool.cs`
- `ListDirTool` — `src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs`
- `ListProjectFilesTool` — `src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs`
- `MemoryListTool` — `src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs`
- `MemoryRemoveTool` — `src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs`
- `MemorySaveTool` — `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`
- `ReadProjectFileTool` — `src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs`
- `WebFetchTool` — `src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs`
- `WebSearchTool` — `src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs`

**Controllers**
- `AuthController` — `src/api/Gabriel.API/Controllers/AuthController.cs`
- `ConversationsController` — `src/api/Gabriel.API/Controllers/ConversationsController.cs`
- `DiagnosticsController` — `src/api/Gabriel.API/Controllers/DiagnosticsController.cs`
- `MemoriesController` — `src/api/Gabriel.API/Controllers/MemoriesController.cs`
- `ModelsController` — `src/api/Gabriel.API/Controllers/ModelsController.cs`
- `ProjectFilesController` — `src/api/Gabriel.API/Controllers/ProjectFilesController.cs`
- `ProjectsController` — `src/api/Gabriel.API/Controllers/ProjectsController.cs`
- `SequenceController` — `src/api/Gabriel.API/Controllers/SequenceController.cs`

**Handlers**
- `GlobalExceptionHandler` — `src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs`
- `GrokAuthHandler` — `src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs`

**Repositories**
- `ConversationRepository` — `src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs`
- `IConversationRepository` — `src/api/Gabriel.Core/Repositories/IConversationRepository.cs`
- `IMemoryRepository` — `src/api/Gabriel.Core/Repositories/IMemoryRepository.cs`
- `IMetricRepository` — `src/api/Gabriel.Core/Repositories/IMetricRepository.cs`
- `IProjectRepository` — `src/api/Gabriel.Core/Repositories/IProjectRepository.cs`

---
*Generated by Aurion on 2026-06-08 22:37:58 UTC*
