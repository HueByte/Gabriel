# Architecture

## PURPOSE
Project layering, dependency direction, and what kind of code lives where.

## USE WHEN
- User asks where some class / concept lives.
- User asks why a type is in one project and not another.
- User asks how to add a new feature spanning layers.
- You need to decide which project a new file should go in.

## QUICK REFERENCE

| Project | Role | References |
| --- | --- | --- |
| `Gabriel.Core` | Pure domain. Entities, value objects, repository + identity *contracts*, domain exceptions. | nothing |
| `Gabriel.Engine` | Application/agent layer. ReAct loop, tools, personality, sequence, provider *contracts*. | `Core` |
| `Gabriel.Infrastructure` | Adapters. EF Core, ASP.NET Identity stores, HTTP clients, concrete `IChatProvider` / `IWebSearch` / `IDocsLookup`. | `Engine`, `Core` |
| `Gabriel.API` | HTTP boundary. Controllers, SSE, middleware, `Program.cs`, DI composition root. | `Engine`, `Infrastructure`, `Core` |

Onion rule: **dependencies point inward**. Core has zero project references.

## DETAILS

### Where a given concept lives

| Concept | Project | Notes |
| --- | --- | --- |
| `Conversation`, `Message` entities | Core | Domain language; both Engine and Infra need them. |
| `ConversationState`, `Mood` value objects | Core | Even though Engine *operates* on them, `Conversation.GetState()` exposes them, so Core must own the type. |
| `IConversationRepository`, `IUnitOfWork`, `ICurrentUser`, `IJwtTokenService` | Core | Contracts. Engine consumes; Infrastructure implements. |
| `IChatService` / `ChatService` (CRUD over conversations) | Core | Not LLM-flavored, lives with the domain it manipulates. |
| `IChatProvider`, `ITool`, `IToolRegistry`, `IAgentService`, `ITokenEstimator` | Engine | Agent orchestration. |
| `Personality/*` (state updater, prompt builder, post-processor) | Engine | The classes; the value objects they pass around (`ConversationState`, `Mood`) live in Core. |
| `Sequence/*` (avatar generator, palette templates, patterns) | Engine | Pure CPU compute. |
| `GrokChatProvider`, `MockChatProvider` | Infrastructure | HTTP + transport. |
| `AppDbContext`, EF configs, migrations | Infrastructure | EF Core specifics. |
| `JwtTokenService`, ASP.NET Identity stores | Infrastructure | ASP.NET concretes. |
| `GitHubDocsLookup`, `LocalDocsLookup`, `CompositeDocsLookup` | Infrastructure | Concrete `IDocsLookup` implementations. |
| `DuckDuckGoWebSearch`, `BraveWebSearch`, `HttpUrlFetcher` | Infrastructure | Concrete `IWebSearch` / `IUrlFetcher`. |
| Controllers, middleware, `Program.cs`, Serilog wiring | API | HTTP boundary. |

### Folder map inside `Gabriel.Engine`

```
Gabriel.Engine/
├── DependencyInjection.cs       AddEngineServices()
├── Providers/                   IChatProvider + DTOs
├── Tools/                       ITool + IToolRegistry + every tool
│   ├── Docs/                    DocsListTool, DocsReadTool, IDocsLookup
│   ├── Files/                   FileInfoTool, ListDirTool, FindTool, GrepTool
│   ├── Memory/                  MemorySave / List / Remove
│   ├── Projects/                ListProjectFiles, ReadProjectFile
│   └── Web/                     WebSearchTool, WebFetchTool
├── Services/                    IAgentService, AgentService, AgentEvent, AgentOptions, token estimator
├── Personality/                 IConversationStateUpdater, ISystemPromptBuilder, IResponsePostProcessor
└── Sequence/                    IGabrielSequenceGenerator, palette templates, patterns
```

### Request flow across layers (text form)

1. Browser → `Gabriel.API` `ConversationsController` (POST `/conversations/{id}/messages/stream`).
2. Controller → `IAgentService.RunAsync` (in Engine).
3. AgentService loads via `IConversationRepository` (Core contract, Infra impl).
4. AgentService updates `ConversationState` via `IConversationStateUpdater`.
5. AgentService streams `IChatProvider.StreamAsync(history, tools)` — concrete `GrokChatProvider` (Infra) makes the HTTPS call.
6. AgentService yields `AgentEvent` items; controller serializes them as SSE `data:` frames.

The Engine **never** speaks HTTP directly. Every external system is reached through an interface defined in Engine and implemented in Infrastructure.

## INVARIANTS

- `Engine` MUST NOT reference `Infrastructure`. If you need a new external dep from Engine, define the interface in Engine and put the implementation in Infrastructure.
- `Core` MUST NOT reference `Engine` or `Infrastructure`.
- All migrations and EF configurations live in `Infrastructure`. Domain entities know nothing about column types or indexes.
- New tools register their `ITool` in `Gabriel.Engine.DependencyInjection.AddEngineServices`. Their external-dep providers (HTTP clients, search APIs) register in `Gabriel.Infrastructure.DependencyInjection.AddInfrastructure`.

## PITFALLS

- "Where does `Mood` live?" — Core, not Engine. The state object is domain-owned; the *updater* is in Engine.
- "Where does `IDocsLookup` live?" — interface in `Gabriel.Engine.Tools.Docs`, implementations in `Gabriel.Infrastructure.Tools.Docs`.
- Don't conflate `IChatService` (CRUD over `Conversation`, in Core) with `IAgentService` (the LLM loop, in Engine). They are different services with different responsibilities.

## SEE ALSO

- `agent-loop.md` — what the Engine actually does per request.
- `tools.md` — `ITool` model and registered tools.
- Human-prose companion: `Gabriel.Engine/architecture.md` (has diagrams).
