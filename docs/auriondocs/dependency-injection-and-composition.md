# Dependency Injection and Composition Root

> Centralized wiring of core, engine, and infrastructure services into the DI container.

Centralizes the application's composition root: these helpers register domain, engine, and infrastructure services into an IServiceCollection so Program.cs (or Startup) can wire the system consistently. By keeping domain registrations, engine tooling, and persistence/web providers in focused extension methods, startup code remains declarative and tests can swap implementations easily. This guide walks the key registration points and how they relate so you know which extension to call for each service category.

## DependencyInjection.cs
Registers core domain services into the DI container.

The Core-level registration extension (primary symbol: `DependencyInjection`) provides the domain-facing service registrations your application needs. Use [DependencyInjection.cs](Code/src/api/Gabriel.Core/DependencyInjection.cs.md) from Program.cs to compose domain services into your IServiceCollection; this is where domain interfaces, validators, domain services, and other core abstractions are bound so higher layers can depend on interfaces rather than concrete implementations.

## DependencyInjection.cs
Registers engine services and configuration into the DI container.

The Engine-level registration extension (primary symbol: `DependencyInjection`) wires Gabriel's runtime machinery: agent(s), tooling, metrics and other engine-specific services. Call [DependencyInjection.cs](Code/src/api/Gabriel.Engine/DependencyInjection.cs.md) from application startup to add the engine's services and configuration into the same IServiceCollection used by the rest of the app, so the engine components are resolved alongside core and infrastructure services.

## DependencyInjection.cs
Configures infrastructure services for DI (EF, repositories, etc.).

This Infrastructure overview groups the infrastructure registration helpers that actually provide implementations for the abstractions used by core and engine. See [DependencyInjection.cs](Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) for the aggregated registrations that configure project file storage, web-fetching, web-search providers, and other runtime implementations; call this from startup to ensure concrete infrastructure pieces are available to the rest of the system.

## DependencyInjection.cs
Adds EF DbContext, repositories and related infrastructure services.

The `AddInfrastructure` helper in [DependencyInjection.cs](Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md) registers persistence-related services such as EF DbContext and repository implementations. Use this method when you need the application's database and repository wiring present in the IServiceCollection so domain and engine layers can resolve persistent stores.

## DependencyInjection.cs
Configures the HTTP fetch stack and named HttpClient for web operations.

`AddWebFetch` (in [DependencyInjection.cs](Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md)) configures the HTTP client and related fetch pipeline used for web interactions. Call this registration to ensure a named HttpClient and any associated handlers or policies are available to services that perform web requests (for example, remote documentation fetchers or web-based providers).

## DependencyInjection.cs
Registers the documentation lookup stack for local and remote docs providers.

`AddDocsLookup` (in [DependencyInjection.cs](Code/src/api/Gabriel.Infrastructure/DependencyInjection.cs.md)) registers the documentation lookup components that the application uses to resolve local and remote documentation sources. Include this when your application needs the docs provider implementations wired into the DI container so engine and domain logic can query documentation consistently.

All of these extensions form the composition root pattern: startup code composes the system by calling the Core, Engine, and Infrastructure registration extensions into a single IServiceCollection. The dependency direction is explicit — Core provides domain abstractions, Infrastructure supplies concrete implementations (EF, HTTP, doc providers), and Engine wires runtime behavior and tooling — and the shared DI container is the single place those layers are composed for runtime and testing.

---
*Synthesised by Aurion on 2026-06-09 03:23:12 UTC*
