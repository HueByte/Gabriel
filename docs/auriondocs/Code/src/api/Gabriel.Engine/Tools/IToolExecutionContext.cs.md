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


IToolExecutionContext serves as a per-turn ambient container that tools use to understand the current execution scope. It exposes three context identifiers: ConversationId, UserId, and an optional ProjectId, which are populated at the start of a turn by the AgentService and read by tools throughout that turn. The Set method initializes the context for the current turn, ensuring all tool invocations share the same conversation, user, and (where applicable) project context without requiring these IDs to be threaded through every API call.

---

## ToolExecutionContext
> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** class

```csharp
public sealed class ToolExecutionContext : IToolExecutionContext
```


ToolExecutionContext is a small, concrete implementation of IToolExecutionContext that stores the per-execution identifiers used by tool workflows: ConversationId, UserId, and an optional ProjectId. It exposes these values via simple properties and provides a Set method to initialize them in a single call, making it easy to pass a shared context through the tool execution pipeline.

## Remarks
ToolExecutionContext centralizes execution context for tool-related operations. By encapsulating the three identifiers in a single, sealed type, it promotes consistent usage across the engine and prevents divergence of context data. The Set method serves as the explicit initialization point, while the private setters enforce that context is only updated via that method and after construction.

## Notes
- The properties are nullable until Set is called; reading them before initialization yields null values.
- This class is not designed for concurrent updates; use a single instance per logical execution or synchronize access when sharing across threads.

---