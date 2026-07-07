# IConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/IConversationStateUpdater.cs`  
> **Kind:** interface

```csharp
public interface IConversationStateUpdater
```


IConversationStateUpdater defines a contract for evolving the ongoing conversation state from an optional previous state and the latest user input. Implementations take the current ConversationState (if any) and the new userMessage and return the next ConversationState, encapsulating the rules for how context, history, and metadata advance across turns. Because this is a stateless service, concrete implementations should not retain per-instance data and should be safe to register as a singleton, enabling reuse across requests.

## Remarks

By isolating state evolution behind this interface, you centralize the policy governing how conversations grow, reset, or prune context. This makes testing easier and lets different parts of the system swap in alternative strategies for state advancement without touching the message handling logic. The contract explicitly allows a null current state, enabling a fresh conversation to start when no prior state exists. The stateless design also supports thread-safe usage and singleton lifecycles, reducing coupling between components that process user messages and those that manage context.