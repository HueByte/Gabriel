# Dependency injection and service bootstrap

> Centralized DI wiring across core, engine, and infrastructure layers to compose the app.

# Dependency injection and service bootstrap

This topic documents the centralized dependency-injection wiring that composes the application's core domain, engine runtime, and infrastructure providers so the app can resolve services predictably at runtime. The three DependencyInjection extensions divide responsibilities: core domain implementations and small-scope services live in the Core wiring, engine-level runtime behaviors and per-request contexts live in the Engine wiring, and external provider configuration and HTTP/web plumbing live in the Infrastructure wiring. Together they let startup compose a layered DI graph with clear lifetime boundaries and a single place to change or swap implementations.

## DependencyInjection.cs
Provides DI wiring for core services.
The [DependencyInjection](../Code/src/api/Gabriel.Core/DependencyInjection.cs.md) class exposes an extension (AddCoreServices) that registers the core Gabriel.Core domain services into the application's IServiceCollection. Concretely, the extension wires concrete implementations behind domain interfaces — for example ChatService, ProjectService, and MemoryService are registered to resolve for IChatService, IProjectService, and IMemoryService respectively — and all are registered with a scoped lifetime. The method returns IServiceCollection to allow fluent composition during startup and intentionally keeps domain registrations separate from engine-level wiring so the domain layer can evolve independently and be reused in tests or different host compositions.

This core wiring is consumed by application startup and is orthogonal to the engine’s runtime wiring: it does not register engine tools or providers itself but is meant to be composed alongside engine and infrastructure registrations (see the engine and infrastructure DependencyInjection files).

## DependencyInjection.cs
Provides DI wiring for engine services.
The [DependencyInjection](../Code/src/api/Gabriel.Engine/DependencyInjection.cs.md) class configures the engine’s runtime DI graph by binding configuration sections (AgentOptions, PersonalityOptions, AgentToolsOptions) and registering engine-level collaborators so controllers and higher-level services can request them via constructor injection. It registers engine services such as IAgentService, tool-related types (ITool, IToolRegistry), per-request contexts like ToolExecutionContext, and cross-cutting singletons like MetricRecorder and PromptRegistry, with an emphasis on scoping most tools to Scoped so they align with per-turn processing. The extension also registers every built-in ITool implementation that the engine provides, but it deliberately defers concrete provider bindings (e.g., chat or search providers) to the infrastructure layer.

Because it relies on provider implementations and other infrastructure bindings, the engine wiring expects [Gabriel.Infrastructure's DependencyInjection](../Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) to be present in the host composition; it centralizes engine concerns while keeping provider wiring and HTTP client/resilience concerns out of the engine assembly.

## DependencyInjection.cs
Provides DI wiring for infrastructure services.
The [DependencyInjection](../Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) class supplies an AddInfrastructure extension that registers the application's infrastructure services and helpers. It wires disk-backed project file storage configured from Projects:Files (persisting under a {Root}/{ProjectId:N} path), configures a single HttpClient-based web page fetcher with sensible defaults and UA handling, and outlines a composable web-search pipeline that can combine multiple providers with instrumentation and fallback behavior. The file also contains helper registration methods referenced by the extension (AddChatProvider, AddDocsLookup, AddWebFetch, AddWebSearch) plus helper configurers like ConfigureDdgHttpClient and ConfigureGrokResilience.

One specific provider wiring example is [AddChatProvider](../Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) which always registers a MockChatProvider as a safe default and conditionally wires Grok support when a Providers:Grok configuration section with active models is present; it binds Grok options via Options, reads secrets via the PROVIDERS__GROK__APIKEY environment key, and only registers Grok when configured, leaving MockChatProvider as fallback to avoid crashes in development. Overall, this file centralizes external provider and HTTP/resilience concerns so those implementations are available for the engine to consume.

How the pieces fit
Startup composes these three layers in a predictable order: the infrastructure wiring (AddInfrastructure) attaches external providers, HTTP clients, and persistence details; the core wiring (AddCoreServices) registers domain implementations with scoped lifetimes; the engine wiring (AddEngineServices) binds engine runtime options, tool registrations, and per-request contexts and consumes provider bindings supplied by the infrastructure layer. Lifetimes are deliberate: most tools and domain services are Scoped to align with request/turn processing, while telemetry and registries may be singletons. This separation keeps domain, engine, and infrastructure concerns decoupled while allowing a single startup composition to produce a complete DI graph.

---
*Covers 3 of 3 source files identified for this topic.*

*Synthesised by Aurion on 2026-07-08 05:44:36 UTC*
