# IChatProviderRegistry.cs

> **Source:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`

## Contents

- [IChatProviderRegistry](#ichatproviderregistry)
- [ChatProviderRegistry](#chatproviderregistry)

---

## IChatProviderRegistry

> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** interface

Maps a provider name to a concrete IChatProvider implementation and exposes the set of registered provider names. Reach for this abstraction when code must select a chat provider at runtime by name (for example, an AgentService choosing "grok") instead of depending on a single concrete provider.

## Remarks
This interface decouples runtime selection from compile-time dependencies: implementations typically act as a registry over IChatProvider instances that have been registered in the application's dependency container (the project comment notes providers are registered as singletons). That lets consumers enumerate available providers and resolve one by name without taking a direct dependency on any single provider implementation.

## Example
```csharp
// Inspect available providers and resolve one if present
var providerNames = registry.AvailableProviders;
foreach (var name in providerNames)
{
    Console.WriteLine(name);
}

if (registry.AvailableProviders.Contains("grok"))
{
    var grok = registry.Resolve("grok");
    // use the resolved IChatProvider instance
}
```

## Notes
- The behavior when a name is not found (e.g., whether Resolve throws or returns null) is implementation-defined; check the concrete registry you depend on.
- AvailableProviders is a read-only view of registered names; do not rely on ordering or on it reflecting subsequent dynamic registrations unless the implementation documents that guarantee.
- The project registers providers as singletons in practice; consumers should not assume per-call construction or disposal responsibility from the registry itself.

---

## ChatProviderRegistry

> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** class

Registry that maps chat provider names to their IChatProvider implementations and resolves a provider by name. Use this when multiple IChatProvider implementations are registered and a provider must be selected at runtime by its Name (the registry performs case-insensitive lookup and exposes the set of available names).

## Remarks
This class centralizes provider lookup logic so callers can request a provider by name rather than depending on DI for every specific implementation. It constructs an internal, case-insensitive dictionary from the provided IChatProvider instances at creation time and is sealed to indicate its behavior is fixed (no runtime mutations or extension points). The Resolve method returns the matching provider or throws a clear InvalidOperationException listing registered names.

## Example
```csharp
// Create some providers (IChatProvider.Name must be unique, case-insensitive)
IEnumerable<IChatProvider> providers = new[] { new OpenAiProvider(), new LocalProvider() };
var registry = new ChatProviderRegistry(providers);

// Resolve by name (case-insensitive)
var provider = registry.Resolve("openai");

// Inspect available provider names
foreach (var name in registry.AvailableProviders)
{
    Console.WriteLine(name);
}
```

## Notes
- The constructor requires unique provider names; duplicates will cause ToDictionary to throw (ArgumentException) during construction.
- Passing a null providerName to Resolve will result in an exception (Dictionary key lookup does not accept null). Validate callers if null input is possible.
- Matching is case-insensitive (StringComparer.OrdinalIgnoreCase). AvailableProviders reflects the registry's keys and is effectively read-only because the registry has no mutating API after construction.

---