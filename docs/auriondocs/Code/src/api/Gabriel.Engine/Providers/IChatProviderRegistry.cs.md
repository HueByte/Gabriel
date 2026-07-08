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


Maps a provider name to the concrete IChatProvider instance and is intended to be used when a chat provider must be selected at runtime by name rather than wired statically. The registry enables AgentService to resolve the appropriate IChatProvider (e.g., the one named "grok") at request time, while all providers are registered as singleton IChatProvider instances.

## Remarks

This abstraction decouples consumers from concrete provider implementations and centralizes the discovery and lifetime management of chat providers. It supports pluggability: you can add, remove, or swap providers without changing the consumers that request them, improving testability and configurability within the chat subsystem.

## Example

```csharp
// Enumerate available providers
foreach (string name in registry.AvailableProviders)
{
    Console.WriteLine(name);
}

// Resolve a provider by name at runtime
IChatProvider grok = registry.Resolve("grok");
// Use `grok` to perform chat-related operations (call-site depends on IChatProvider API)
```

## Notes

- Lookup by provider name is exact; mismatches or unknown names may result in runtime failure when resolving. Ensure names are accurate and registered up-front.
- Providers are singletons; the implementation-backed IChatProvider should be thread-safe if shared across requests.
- Adding or removing providers should be coordinated with the application's startup configuration to ensure AvailableProviders and Resolve mappings remain consistent.

---

## ChatProviderRegistry
> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** class

```csharp
public sealed class ChatProviderRegistry : IChatProviderRegistry
```


The class provides a centralized registry for IChatProvider implementations, enabling runtime resolution of a provider by name. It builds a case-insensitive map of providers from the provided collection and exposes the available provider names for discovery and diagnostics.

## Remarks
By centralizing provider lookup, ChatProviderRegistry decouples callers from concrete implementations and supports swapping providers at runtime without changing call sites. Using a case-insensitive key for provider names makes lookups forgiving of name casing, while the registry remains a simple, readonly map after construction.

## Notes
- Duplicate provider names differing only by case will cause an exception during construction due to duplicate keys.
- Resolve(string) throws InvalidOperationException when there is no registered provider with the requested name; the exception message lists available providers to aid debugging.
- AvailableProviders is a read-only collection view of the registered provider names and is effectively immutable after construction.

---