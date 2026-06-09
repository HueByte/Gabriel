# Architecture — HueByte/Gabriel

> *Auto-synthesized from 527 documented symbols across 243 files on `main`.*

## Topic Guides

Deep-dives into cross-cutting concerns synthesized from the per-symbol corpus.

- [Authentication and Authorization](authentication-and-authorization.md) — How the system authenticates users, issues tokens, and protects API and UI surfaces across services.
- [Configuration and Secrets Management](configuration-and-secrets-management.md) — How the application sources and merges secrets/configuration from Infisical and other configuration layers.
- [Observability and Diagnostics](observability-and-logging.md) — Logging enrichment and error handling to surface operational visibility and reliability.
- [Dependency Injection and Composition Root](dependency-injection-and-composition.md) — Centralized wiring of core, engine, and infrastructure services into the DI container.
- [Tooling and Tooling Integration](tooling-and-tooling-integration.md) — Tooling bridge between chat providers and the tool invocation surface, plus tooling discovery.

## System Overview
This repository implements a server-side application that exposes an HTTP API for multi-user conversational agents, project and file management, memories, diagnostics, and model selection; controller classes (e.g. AuthController, ConversationsController, ProjectsController) provide the HTTP surface. Agents and assistant capabilities are exposed as composable ITool implementations (search, file ops, web fetch, docs access) that workers/controllers can invoke; persistent state (conversations, messages, projects, memories, and metric events) is stored via repository abstractions backed by a database (e.g. ConversationRepository using an EF Core AppDbContext). Global request behavior and outbound integration concerns are handled by middleware/handlers such as GlobalExceptionHandler and GrokAuthHandler.

## Key Components
**Controllers** — HTTP surface for application workflows (auth, conversation lifecycle, projects, files, memories, models, diagnostics, and sequences). Implemented by [`AuthController`](src/api/Gabriel.API/Controllers/AuthController.cs.md), [`ConversationsController`](src/api/Gabriel.API/Controllers/ConversationsController.cs.md), [`DiagnosticsController`](src/api/Gabriel.API/Controllers/DiagnosticsController.cs.md), [`MemoriesController`](src/api/Gabriel.API/Controllers/MemoriesController.cs.md), [`ModelsController`](src/api/Gabriel.API/Controllers/ModelsController.cs.md), [`ProjectFilesController`](src/api/Gabriel.API/Controllers/ProjectFilesController.cs.md), [`ProjectsController`](src/api/Gabriel.API/Controllers/ProjectsController.cs.md), [`SequenceController`](src/api/Gabriel.API/Controllers/SequenceController.cs.md).

**Agent Tools** — Encapsulated, invokable capabilities an agent can call to inspect files, read docs, search the web, manage memories, and query time; these implement the `ITool` contract. Implemented by [`ITool`](src/api/Gabriel.Engine/Tools/ITool.cs.md), [`DocsListTool`](src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs.md), [`DocsReadTool`](src/api/Gabriel.Engine/Tools/Docs/DocsReadTool.cs.md), [`FileInfoTool`](src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md), [`FindTool`](src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md), [`GetCurrentTimeTool`](src/api/Gabriel.Engine/Tools/GetCurrentTimeTool.cs.md), [`GrepTool`](src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md), [`ListDirTool`](src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md), [`ListProjectFilesTool`](src/api/Gabriel.Engine/Tools/Projects/ListProjectFilesTool.cs.md), [`MemoryListTool`](src/api/Gabriel.Engine/Tools/Memory/MemoryListTool.cs.md), [`MemoryRemoveTool`](src/api/Gabriel.Engine/Tools/Memory/MemoryRemoveTool.cs.md), [`MemorySaveTool`](src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs.md), [`ReadProjectFileTool`](src/api/Gabriel.Engine/Tools/Projects/ReadProjectFileTool.cs.md), [`WebFetchTool`](src/api/Gabriel.Engine/Tools/Web/WebFetchTool.cs.md), [`WebSearchTool`](src/api/Gabriel.Engine/Tools/Web/WebSearchTool.cs.md).

**Repositories** — Persistence boundaries and abstractions for storing and querying domain aggregates (conversations, projects, memories, metrics). Implemented by [`ConversationRepository`](src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md), [`IConversationRepository`](src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md), [`IMemoryRepository`](src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md), [`IMetricRepository`](src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md), [`IProjectRepository`](src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md).

**Middleware / Handlers** — Cross-cutting request and outbound HTTP handling such as standardized error translation and attaching credentials. Implemented by [`GlobalExceptionHandler`](src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs.md) and [`GrokAuthHandler`](src/api/Gabriel.Infrastructure/Providers/GrokAuthHandler.cs.md) (attaches Grok API key to outbound requests).

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
*Generated by Aurion on 2026-06-09 03:27:01 UTC*
