# IConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/IConversationStateUpdater.cs`  
> **Kind:** interface

```csharp
public interface IConversationStateUpdater
```


IConversationStateUpdater defines a contract for evolving the state of a conversation from the current state and the latest user message. Implementations should be stateless so they can be registered safely as singletons; they receive the existing ConversationState (or null for a new conversation) and the user message, and return the updated ConversationState.

Use this abstraction when you want to separate how state is updated from the rest of the conversation pipeline, or when you need pluggable strategies for managing context (for example, appending messages, pruning history, or injecting system messages). It enables swapping strategies without touching downstream components.

## Remarks

It serves as a boundary between input handling and state management, enabling you to swap in different state-update policies without changing downstream code. This improves testability by allowing mocks or fakes and promotes a clean separation of concerns. Because implementations are designed to be stateless, they can be safely registered as singletons and reused across requests.

## Notes

- Do not mutate the provided current state; treat it as input and return a new ConversationState.
- Do not rely on per-instance state or mutable static data; the interface is designed for stateless, singleton-friendly implementations.
- If the ConversationState is large or expensive to copy, consider design choices that minimize allocations and favor incremental updates when supported by ConversationState.