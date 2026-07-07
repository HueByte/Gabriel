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


Maps a provider name to the concrete IChatProvider instance. Providers are registered as singletons, and this registry is the mechanism the system uses to select the appropriate chat provider at request time by name (for example, the provider named 'grok') instead of wiring a fixed dependency into the consumer.

## Remarks
Decoupling the consumer from concrete providers makes the system extensible: new providers can be added and registered without changing AgentService code. AvailableProviders exposes the set of known provider names, enabling runtime discovery and configuration UIs. The IChatProviderRegistry acts as the boundary between composition and per-request behavior, supporting flexible routing of chat requests to the chosen provider.

## Example
```csharp
// Example: resolve a provider by name at request time
IChatProvider provider = registry.Resolve("grok");
// Use the provider to handle a chat operation...
```


---

## ChatProviderRegistry
> **File:** `src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs`  
> **Kind:** class

```csharp
public sealed class ChatProviderRegistry : IChatProviderRegistry
```


ChatProviderRegistry serves as a centralized, case-insensitive lookup for IChatProvider instances. Built from a collection of providers, it creates an internal dictionary keyed by each provider’s Name, enabling Resolve to fetch a provider by its name string (regardless of letter case). If no match exists, Resolve throws InvalidOperationException with a message that lists the available provider names to guide corrective configuration. The registry also exposes AvailableProviders so callers can inspect which providers are currently registered.

## Remarks

By centralizing provider resolution, this type decouples consumer code from concrete implementations and from the DI wiring used to compose them. It enforces a single source of truth for provider names and surfaces a clear, actionable error when a requested provider is missing. The internal mapping is built once at construction and remains immutable, ensuring predictable behavior at runtime.

## Example

```csharp
// Example usage of the registry
var providers = new IChatProvider[] { new OpenAiChatProvider(), new EchoChatProvider() };
var registry = new ChatProviderRegistry(providers);

IChatProvider provider = registry.Resolve("OpenAI"); // resolution is case-insensitive
```

## Notes

- Duplicate provider names (case-insensitive) will throw during construction due to ToDictionary enforcing unique keys.
- The set of AvailableProviders is fixed after construction; there is no runtime API to add or remove providers.

---