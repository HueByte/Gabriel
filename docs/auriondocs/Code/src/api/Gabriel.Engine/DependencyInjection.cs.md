Static helper that wires the Gabriel engine's dependency injection surface. AddEngineServices is an extension method on IServiceCollection that configures option objects from configuration and registers the engine's core services, per-request contexts, system prompts, and the built-in tool set used by the Gabriel engine. Use it during application startup to install the engine's DI surface so all ITool implementations, the agent service, and sequence components are resolved with the intended lifetimes.

## Remarks
Dependency injection wiring is centralized in this helper. It binds the engine's option objects, core services, per-turn context, prompt tooling, and the built-in set of tools. Tool discovery relies on ToolRegistry consuming IEnumerable<ITool>, while the concrete registrations live in this method; infrastructure providers with HTTP concerns originate from Gabriel.Infrastructure and are wired there, but their interfaces and consumers are bound here to the engine.

## Notes
- Must call AddEngineServices during startup to register the engine's services and tools.
- Gabriel.Infrastructure.AddInfrastructure must be wired to supply tools that depend on external providers (e.g., WebSearch, DocsLookup).
- Lifetimes are chosen to balance per-turn state (scoped) and shared resources (singleton); changing them can affect state sharing across requests.