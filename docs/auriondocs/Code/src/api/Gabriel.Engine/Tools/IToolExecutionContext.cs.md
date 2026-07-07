# IToolExecutionContext.cs

> **Source:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`

## Contents

- [IToolExecutionContext](#itoolexecutioncontext)
- [ToolExecutionContext](#toolexecutioncontext)

---

## IToolExecutionContext
> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** interface

```csharp
public interface IToolExecutionContext
```


Represents a per-turn execution context used by tools to determine which conversation, user, and (optionally) project apply to the current operation. The agent populates this context once per turn, and tools read the values from this interface instead of threading identifiers through every JSON argument, which keeps tool calls lean and consistent across the turn. The interface exposes three nullable GUID properties—ConversationId, UserId, and ProjectId—and a Set method to initialize them for the active turn.

## Remarks
This abstraction decouples tooling from transport payloads and hidden model state by providing a centralized, per-turn source of identity context. Since it is registered as Scoped, a single HTTP request shares the same context instance across all tools invoked during that turn, and AgentService is responsible for calling Set at the start of the turn to establish the identifiers. Consumers should rely on this context to reason about which conversation, user, and project they operate on, rather than reconstructing identity from each tool invocation.

## Example
```csharp
// Example usage: initialize the per-turn context at the start of handling a request/turn
IToolExecutionContext context = /* resolved from DI container */;
context.Set(conversationId, userId, projectId);
```

## Notes
- The properties may be null before initialization; code that consumes them should guard with HasValue or similar null checks.
- ProjectId is optional; some turns may not be associated with a project, so null is a valid value.
- Do not rely on the context across turns; it is scoped to a single turn and should be initialized once per turn to avoid inconsistent state.

---

## ToolExecutionContext
> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** class

```csharp
public sealed class ToolExecutionContext : IToolExecutionContext
```


ToolExecutionContext is a sealed, concrete implementation of IToolExecutionContext that carries the identifiers necessary to describe a tool invocation: the conversation, the user initiating the interaction, and (optionally) the project under which the tool runs. ConversationId and UserId are nullable properties that get populated via the Set method, allowing the context to be created and then initialized at a later point. ProjectId is also nullable to accommodate tool executions that are not associated with a project. The Set method assigns the provided identifiers to the instance, enabling downstream components to access a consistent execution context for the duration of a tool run.

## Remarks
ToolExecutionContext serves as a portable bundle of identity information used by the tooling stack during a tool run. By centralizing ConversationId, UserId, and optional ProjectId, it avoids scattering identity data through multiple call sites and enables consistent correlation of actions with a specific conversation and user (and optionally a project) across components. The sealed design ensures the representation remains stable across the system, preventing extensions that could diverge semantics and preserving a single source of truth for execution context.

## Notes
- Initialization occurs via Set; until then, ConversationId, UserId, and ProjectId are null.
- No thread-safety guarantees; if a shared instance is used across concurrent tool invocations, external synchronization is required.
- ProjectId is optional; expect it to be null for operations not tied to a project.

---