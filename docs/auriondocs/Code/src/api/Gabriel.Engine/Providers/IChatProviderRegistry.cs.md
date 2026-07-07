# IChatProviderRegistry.cs

> **Source:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`

## Contents

- [IChatProviderRegistry](#ichatproviderregistry)
- [ChatProviderRegistry](#chatproviderregistry)

---

## IChatProviderRegistry
> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** interface

```csharp
public interface IChatProviderRegistry
```


Represents a registry that maps provider names to concrete IChatProvider instances and exposes the set of available providers. It enables runtime selection of a chat provider by name rather than wiring a specific implementation. Providers are registered as singleton IChatProvider instances; at request time the registry lets agents pick the provider named 'grok' (or any available provider) without changing call sites.

## Remarks
By abstracting the lookup behind Resolve and listing options via AvailableProviders, this symbol acts as an IoC-like boundary for chat backends. It reduces coupling between clients and concrete providers and makes it straightforward to add, swap, or mock providers in tests.

## Example
```csharp
// Resolve a provider by name and obtain its IChatProvider instance
IChatProvider provider = registry.Resolve("grok");
```

## Notes
- The contract does not specify error handling for unknown provider names; callers should validate against AvailableProviders before calling Resolve or be prepared to handle an exception or null depending on the implementation.

---

## ChatProviderRegistry
> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** class

```csharp
public sealed class ChatProviderRegistry : IChatProviderRegistry
```


ChatProviderRegistry is a small registry that indexes IChatProvider implementations by their Name and resolves a provider by name. It builds an in-memory, case-insensitive map from the provided IChatProvider instances, enabling consumers to select a provider by a string key rather than by a concrete type. If the requested name is not registered, it throws a descriptive InvalidOperationException listing available providers.

## Remarks
It decouples consumers from concrete chat provider implementations by enabling runtime provider selection via a simple string key. Resolution is a fast dictionary lookup, and AvailableProviders can be used to surface supported options to users or configuration. The registry relies on provider.Name values being stable and unique (case-insensitive) to avoid conflicts.

## Notes
- If two providers share the same Name (case-insensitive), the registry construction will throw due to key collisions.
- The registry is effectively read-only after construction; to reflect changes, supply a new collection to the constructor.
- AvailableProviders exposes the set of registered names for discovery and UI display.

---