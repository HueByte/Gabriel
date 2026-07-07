# Adding a new agent tool

> *Workflow template auto-derived from 4 existing exemplar(s).*

This template shows how to add a new agent-invokable tool to the engine: implement a ToolBase subclass that declares its Name, Description, Parameters, and ExecuteAsync behavior, then place it with the other tool types and register it so the agent system can resolve it. Reach for this pattern whenever you need the agent runtime to expose a new discrete operation (a small, focused action that receives parameters, can update execution context state, and returns a ToolResult).

## Scaffold

```csharp
using Aurion.Engine.AgentSystem.General;

namespace Aurion.Engine.AgentSystem.Professions.Foo;

public class SubmitFooTool : ToolBase
{
    public override string Name => "submit_foo";
    public override string Description => "Submit the final Foo result.";

    public override IReadOnlyList<ToolParameter> Parameters =>
    [
        new ToolParameter("payload", "The submitted payload.", ToolParameterType.String, Required: true)
    ];

    public override Task<ToolResult> ExecuteAsync(
        IReadOnlyDictionary<string, string> arguments,
        AgentExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequiredParameters(arguments);
        if (validation != null) return Task.FromResult(validation);

        var payload = GetString(arguments, "payload");
        context.SetState("result", payload);
        return Task.FromResult(ToolResult.Ok($"Submitted ({payload.Length} chars).", payload));
    }
}
```

## Where it lives

Place the new tool source file alongside the existing tool implementations under the engine's Tools area. The exemplars live in src/api/Gabriel.Engine/Tools/Files, so follow that folder convention (or create a parallel folder for the appropriate domain/category). Name the file with the pattern <SymbolName>Tool.cs and declare the matching public class (for the scaffold above, SubmitFooTool in SubmitFooTool.cs). Keep the namespace consistent with the folder structure used by the other tools (e.g., the Files tools are under Gabriel.Engine.Tools.Files).

## DI wiring

New tools must be discoverable by the DI/composition code that constructs available tools for agents. In practice you will add a single registration alongside the other File tools: locate where the existing tool types (for example the types listed below) are referenced or registered in the project and add a one-line registration for your new type so the container can resolve it at runtime. The exact registration statement depends on the app's DI pattern (manual AddSingleton/AddScoped, or an assembly scan), but the change is always a one-line addition near the registrations for the other tools (i.e., next to where [`FileInfoTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md), [ `FindTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md), [ `GrepTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md), or [ `ListDirTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md) are registered).

## Existing examples

- [`FileInfoTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md)
- [`FindTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md)
- [`GrepTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md)
- [`ListDirTool`](../../Code/src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md)

---
*Synthesised by Aurion on 2026-07-07 18:14:33 UTC*
