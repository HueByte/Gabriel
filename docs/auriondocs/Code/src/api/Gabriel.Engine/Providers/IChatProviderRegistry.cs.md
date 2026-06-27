# IChatProviderRegistry.cs

> **Source:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`

## Contents

- [IChatProviderRegistry](#ichatproviderregistry)
- [ChatProviderRegistry](#chatproviderregistry)

---

## IChatProviderRegistry

> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** interface

Maps a provider name to the concrete IChatProvider instance and exposes the set of known provider names. Use this interface when callers need to select a chat provider at runtime by name (for example, an AgentService that must pick the provider named "grok") instead of depending on a single concrete IChatProvider.

## Remarks
The registry provides a thin indirection over dependency injection that lets the application register multiple IChatProvider implementations and resolve one by a string key. Implementations typically register each provider as a singleton IChatProvider so consumers receive a single shared instance per provider name; the registry itself is responsible for the name→instance mapping and discovery via AvailableProviders.

## Example
```csharp
// Discover and resolve a provider safely
var desired = "grok";
if (registry.AvailableProviders.Contains(desired))
{
    var provider = registry.Resolve(desired);
    // use provider...
}
else
{
    // fallback or error handling
}
```

## Notes
- Behavior when Resolve is called with an unknown name is implementation-defined (some implementations may throw, others may return null); callers should guard by checking AvailableProviders or handle exceptions.
- AvailableProviders is an `IReadOnlyCollection<string>`; callers should treat it as the public view of registered names and not attempt to modify it.
- Implementations should document their concurrency and lifecycle semantics (e.g., whether Resolve is thread-safe and whether providers are singletons) since the interface does not mandate them.

---

## ChatProviderRegistry

> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** class

A simple registry that maps chat provider names to their IChatProvider implementation and resolves providers by name. Construct this with the collection of available IChatProvider implementations (commonly provided by dependency injection) when you want a centralized, name-based lookup for chat providers instead of wiring specific implementations throughout the codebase.

## Remarks
This class builds a case-insensitive dictionary from the provided IChatProvider instances using each provider's Name. It centralizes lookup logic so callers only need a provider name to obtain the corresponding IChatProvider. The registry performs exact (case-insensitive) name matching and throws a clear InvalidOperationException when a name is not registered.

## Example
```csharp
var providers = new IChatProvider[] { openAiProvider, azureProvider };
var registry = new ChatProviderRegistry(providers);

// Resolve by name (case-insensitive)
var provider = registry.Resolve("OpenAI");

// List available provider names
foreach (var name in registry.AvailableProviders)
{
    Console.WriteLine(name);
}
```

## Notes
- Passing null for providerName to Resolve will result in ArgumentNullException from the underlying dictionary lookup.
- The constructor uses ToDictionary with StringComparer.OrdinalIgnoreCase; duplicate provider names that are equal ignoring case will cause an exception during construction.
- AvailableProviders exposes the dictionary's Keys collection (a live view), not a frozen snapshot.

---