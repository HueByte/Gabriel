# IConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/IConversationStateUpdater.cs`  
> **Kind:** interface

Reads the latest user message together with the previous conversation state and produces an updated ConversationState. Use this abstraction when you need a pluggable component that derives the next conversation state from the incoming user text; implementations are intended to be stateless and can be registered as singletons in your DI container.

## Remarks
This interface exists to separate state-transition logic from higher-level orchestration. Implementations encapsulate the policy for how a new ConversationState is computed from the prior state and a new user message (for example: updating turn counts, appending message history, tracking intent or context). Because the service is described as stateless, implementations should avoid holding per-conversation mutable fields and must be safe for concurrent use when registered as a singleton.

## Example
```csharp
// Typical usage inside a request handler or pipeline
public class ChatHandler
{
    private readonly IConversationStateUpdater _stateUpdater;

    public ChatHandler(IConversationStateUpdater stateUpdater)
    {
        _stateUpdater = stateUpdater;
    }

    public ConversationState HandleMessage(ConversationState? current, string userMessage)
    {
        // Compute new state from the prior state and the incoming message
        var next = _stateUpdater.Update(current, userMessage);
        // persist next as needed and continue processing
        return next;
    }
}
```

## Notes
- The prior state parameter may be null; implementations must handle a null "current" as the conversation-start case.
- Because implementations are expected to be safe as singletons, avoid capturing request-scoped resources or storing mutable per-conversation data on the implementor instance.
- Prefer returning a new ConversationState rather than mutating the provided instance to avoid accidental shared-state bugs.