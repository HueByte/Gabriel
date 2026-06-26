# IConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/IConversationStateUpdater.cs`  
> **Kind:** interface

Updates the conversation-level state using the latest user message. Use this abstraction when you need to encapsulate how incoming user text transforms or augments the stored ConversationState (for example: updating history, tracking turn counts, flags, or extracted entities) and when you want implementations that are safe to register as application singletons.

## Remarks
The interface is intentionally minimal: implementations receive the previous state (which may be null) and the new user message, and produce a new ConversationState. Implementations are expected to be stateless (no instance-level mutable data) so they can be registered as singletons in DI containers. This keeps state management explicit and testable — the updater never stores state internally, it only derives a new state from its inputs.

## Example
```csharp
// Simple implementation skeleton: derive a new state from the prior one and the incoming message
public class SimpleConversationStateUpdater : IConversationStateUpdater
{
    public ConversationState Update(ConversationState? current, string userMessage)
    {
        // treat 'current' as immutable: create and return a new ConversationState instance
        var next = current == null ? new ConversationState() : current.Clone();

        // apply changes based on the user message (pseudo-code)
        next.AppendMessage(userMessage);
        next.LastUpdatedUtc = DateTime.UtcNow;

        return next;
    }
}

// Registering and calling the updater (typical DI + usage)
// services.AddSingleton<IConversationStateUpdater, SimpleConversationStateUpdater>();
// var newState = updater.Update(existingState, "Hello world");
```

## Notes
- The current parameter may be null; implementations must handle that case.
- Do not mutate the provided ConversationState in-place unless callers expect mutable state; prefer returning a new/updated instance to avoid surprising shared-state bugs.
- Implementations should avoid relying on instance-level mutable data or external side effects so they remain safe to register as singletons and to be used concurrently.
- Keep Update fast and non-blocking; long-running I/O or blocking operations can hurt overall responsiveness if the updater is used on hot paths.
