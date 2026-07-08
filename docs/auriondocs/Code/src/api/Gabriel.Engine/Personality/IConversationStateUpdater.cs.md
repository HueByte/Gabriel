# IConversationStateUpdater

> **File:** `src/api/Gabriel.Engine/Personality/IConversationStateUpdater.cs`  
> **Kind:** interface

```csharp
public interface IConversationStateUpdater
```


IConversationStateUpdater provides a contract to compute the next ConversationState given the current state and the latest user message. The Update method takes a possibly null current state and the user's message and returns the updated ConversationState, enabling stateless implementations that can be registered as singletons in DI.

## Remarks
By decoupling the state evolution from storage or orchestration, this interface makes the update policy swappable and testable. Implementations are expected to be stateless, which enables safe singleton usage and predictable behavior under concurrent requests. The single Update method concentrates the business rules for transitioning a conversation into one place, making it easier to experiment with different strategies while keeping the rest of the system stable.

## Notes
- Handle null current state gracefully; define an initial state as needed by your application.
- Treat ConversationState as immutable from the updater's perspective; do not mutate the provided instance—return a new state or a fresh copy.