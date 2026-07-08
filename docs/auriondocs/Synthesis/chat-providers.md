# Chat providers integration and registry

> Abstractions and registry for chat providers used by the agent runtime; how providers are discovered, registered, and resolved.

This guide explains the abstractions and registry used to integrate chat-capable model providers into the agent runtime. It highlights how providers present a streaming interface for incremental outputs and how a registry maps human-friendly names to concrete provider instances so callers can select and resolve them at runtime. Readers should come away understanding the two core pieces you need to implement or call when adding or using a chat provider.

## IChatProvider.cs

Abstracts chat-capable provider behavior for streaming model outputs.

[IChatProvider.cs](Code/src/api/Gabriel.Engine/Providers/IChatProvider.cs.md) defines the core abstraction a provider must implement to be used as a chat service by the agent. The interface models a streaming conversation API: implementations produce a sequence of incremental events (text deltas, tool-call completions, finish signals) rather than returning a single final result, allowing callers to react as output is produced. When you implement or call a provider, this is the contract that shapes how you subscribe to outputs, handle partial text, detect tool-invocation completions, and finalize the exchange.

## IChatProviderRegistry.cs

Defines a registry to map provider names to concrete IChatProvider implementations.

[IChatProviderRegistry.cs](Code/src/api/Gabriel.Engine/Providers/IChatProviderRegistry.cs.md) describes the lookup layer that ties provider names to their concrete IChatProvider instances and exposes the set of known provider names. Use this interface when code needs to select a chat provider dynamically by name (for example, based on configuration or an agent directive). The registry centralizes discovery and resolution so callers ask for a provider by name and receive an implementation conforming to the streaming contract defined by IChatProvider.

Together these two pieces form a simple discovery-and-use pattern: concrete chat providers implement the streaming-focused IChatProvider interface, and an IChatProviderRegistry implementation registers those providers under names and exposes the known set. At runtime, code (for example, the agent runtime) resolves a named provider from the registry and then interacts with it via the streaming API so outputs can be consumed incrementally and reactively.

---
*Synthesised by Aurion on 2026-06-08 22:35:31 UTC*
