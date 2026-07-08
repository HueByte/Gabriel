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


Represents a per-turn execution context used by tools to discover the current conversation, user, and optional project identifiers. The AgentService populates this scope at the start of each turn and registers it as Scoped so every tool invoked during that turn shares the same values, avoiding the need to thread IDs through every method call. Call Set to initialize ConversationId, UserId, and ProjectId; the properties themselves are nullable, reflecting that a turn may omit some identifiers.

## Remarks
This abstraction centralizes identity propagation for multi-step tool interactions. It decouples tool implementations from authentication details by providing a single, testable contract for the current context, which improves consistency and auditability across the tool suite. The contract ensures that all collaborators read the same IDs during a turn.

## Notes
- Nullable properties mean callers must handle nulls gracefully.
- Set should be invoked once per turn before any tool usage to ensure consistent context.
- Avoid caching the interface beyond its scoped lifetime; a new turn creates a fresh context with potentially different IDs.

---

## ToolExecutionContext
> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** class

```csharp
public sealed class ToolExecutionContext : IToolExecutionContext
```


ToolExecutionContext is a simple, concrete implementation of IToolExecutionContext that stores the identifiers describing the current tool execution: ConversationId, UserId, and an optional ProjectId. It exposes read-only access to these values and provides a single Set method to initialize them in a controlled fashion. Use it when you need a mutable, but well-scoped, container for execution context data that can be populated at the start of a tool run and consumed by downstream components.

## Remarks
This class acts as a lightweight context carrier that encapsulates cross-cutting identifiers for a tool's lifecycle. By making the properties read-only from outside and funneling updates through Set, it promotes clearer ownership and easier testing. It fits between the orchestration layer and tooling components, ensuring consistent access to ConversationId, UserId, and ProjectId wherever the tool execution context is required.

## Example
```csharp
// Initialize a new execution context and populate its IDs
var context = new ToolExecutionContext();
context.Set(Guid.NewGuid(), Guid.NewGuid(), null);
```


---