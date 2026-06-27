# IToolExecutionContext.cs

> **Source:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`

## Contents

- [IToolExecutionContext](#itoolexecutioncontext)
- [ToolExecutionContext](#toolexecutioncontext)

---

## IToolExecutionContext

> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** interface

A lightweight, per-turn context object that stores which conversation, user, and (optionally) project the agent is acting on. Tools that need to know the current conversation/user/project should read these values from this interface instead of requiring the model to supply them as JSON arguments; the AgentService sets the values once per turn and the context is registered with scoped lifetime so all tools invoked during the same HTTP request see the same values.

## Remarks
This interface centralizes transient routing metadata for a single agent turn. By keeping conversation, user, and project identifiers out of tool JSON payloads it avoids trusting the model to provide correct routing information and keeps tool signatures simpler. The Scoped registration ensures the same values are visible across all tool invocations within the same request; callers must set the values at the start of the turn.

## Example
```csharp
// AgentService (or equivalent) sets the context once per turn:
public void BeginTurn(Guid conversationId, Guid userId, Guid? projectId)
{
    _toolExecutionContext.Set(conversationId, userId, projectId);
    // then invoke tools that will read from _toolExecutionContext
}

// A tool (or service used by a tool) reads the context via DI:
public class MyTool
{
    private readonly IToolExecutionContext _ctx;

    public MyTool(IToolExecutionContext ctx) => _ctx = ctx;

    public void Execute()
    {
        var conversation = _ctx.ConversationId;
        var user = _ctx.UserId;
        var project = _ctx.ProjectId; // may be null
        // act using these identifiers (or handle nulls as appropriate)
    }
}
```

## Notes
- ConversationId and UserId are nullable; callers should handle the case where Set was not called or values are absent.
- The context is scoped to the request/turn lifetime — do not cache references and expect values to remain valid across turns.
- The AgentService (or equivalent orchestrator) is responsible for calling Set before tools are invoked; calling Set multiple times will overwrite the previous values.
- There is no built-in additional thread-safety guarantees beyond the DI scope; avoid sharing across unrelated threads.

---

## ToolExecutionContext

> **File:** `src/api/Gabriel.Engine/Tools/IToolExecutionContext.cs`  
> **Kind:** class

A small, mutable container that carries execution-scoped identifiers used by tools: ConversationId, UserId and an optional ProjectId. Instantiate the class and call Set(...) to populate these values before handing the context to components that consume IToolExecutionContext.

## Remarks
This class implements IToolExecutionContext and centralizes the common identifiers required during a tool's execution into a single object. It is sealed to keep the implementation simple and predictable; the Set method enforces that ConversationId and UserId are provided together while ProjectId remains optional.

## Example
```csharp
var ctx = new ToolExecutionContext();
ctx.Set(conversationId: Guid.NewGuid(), userId: Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"), projectId: null);

// Consumers can read the values (they are nullable until Set is called)
Guid? conversation = ctx.ConversationId;
Guid? user = ctx.UserId;
Guid? project = ctx.ProjectId;
```

## Notes
- ConversationId, UserId and ProjectId are nullable properties; they remain null until Set(...) is called. Check for null before use.
- Calling Set(...) replaces any previously stored values.
- The class does not perform synchronization; concurrent calls to Set or reads from multiple threads may require external synchronization.

---