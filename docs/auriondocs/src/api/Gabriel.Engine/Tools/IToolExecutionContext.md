# IToolExecutionContext.cs

> **Source:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`

## Contents

- [IToolExecutionContext](#itoolexecutioncontext)
- [ToolExecutionContext](#toolexecutioncontext)

---

## IToolExecutionContext

> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** interface

Represents the per-turn execution context that an agent populates before invoking tools. Tools read these identifiers (ConversationId, UserId, ProjectId) from this scoped context instead of requiring the model to include them in each tool call's JSON arguments.

## Remarks
This interface is intended to be a short-lived, request-scoped container set once by the AgentService at the start of a turn and read by any tool implementations invoked during that same turn. Centralizing these identifiers keeps tool argument payloads simpler and ensures all tools executing within a single HTTP request see the same conversation/user/project values.

## Notes
- The three properties are nullable; callers should assume they may be null until AgentService calls Set for the current turn. After Set is invoked, ConversationId and UserId will be provided as non-null values per the method signature.
- Set overwrites the stored values for the current scope — it is intended to be called once per turn by the agent orchestration code, not by individual tools.
- This abstraction is intended to be registered with a scoped lifetime in dependency injection and must not be persisted or reused beyond the request/turn boundary. Concurrent mutation from multiple threads within the same scope is not guarded by this interface and should be avoided.

---

## ToolExecutionContext

> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** class

A small, mutable container that holds identifiers used during a tool execution: ConversationId, UserId and an optional ProjectId. Use this when a tool or operation needs to carry the current execution context (who initiated the action, which conversation it belongs to, and optionally which project) instead of passing individual identifiers around.

## Remarks
This sealed implementation backs the IToolExecutionContext abstraction with three nullable Guid properties and a single Set method to initialise or replace them. The two required identifiers (conversation and user) are accepted as non-nullable parameters to Set; properties are declared nullable to represent an uninitialised state prior to calling Set. The class is intentionally minimal — it serves as a simple DTO for execution metadata rather than performing validation or access control.

## Example
```csharp
var ctx = new ToolExecutionContext();
ctx.Set(conversationId: Guid.NewGuid(), userId: Guid.Parse("d3c9f3a2-..."), projectId: null);
// later
Guid? conversation = ctx.ConversationId;
Guid? user = ctx.UserId;
Guid? project = ctx.ProjectId;
```

## Notes
- Properties are nullable and remain null until Set is called; call Set before relying on non-null values.
- Set overwrites any previously stored values; callers should ensure this is the intended behavior.
- The class has no synchronization; if it is accessed from multiple threads concurrently, callers must provide their own synchronization.

---