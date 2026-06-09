# Adding a new agent tool

> *Workflow template auto-derived from 4 existing exemplar(s).*

Adding a new agent tool

Use this pattern when you need to add a new actionable tool that agents can call during execution (for example, a tool that accepts input, performs an operation, and returns a result). Tools in this codebase follow a consistent shape (Name, Description, Parameters, ExecuteAsync) and are colocated with other tool implementations so they are easy to discover and register for use by the agent runtime.

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

Put the new tool class alongside the existing file-oriented tools under src/api/Gabriel.Engine/Tools/Files and follow the naming convention used by the exemplars: a PascalCase action name suffixed with "Tool" (for example FileInfoTool, FindTool, GrepTool, ListDirTool). The namespace and folder structure should mirror the existing tools so the codebase remains discoverable and consistent with other agent tools.

## DI wiring

There are two common wiring approaches in this codebase: automatic discovery (assembly scanning) or explicit registration. First look for whether the project already discovers tool types automatically (search for code that scans the assembly or registers all ToolBase-derived types). If the codebase uses discovery, you typically do not need to add anything. If registration is manual, add a single registration line in the same composition/registration location where the other file tools are registered — locate the file that contains registrations for FileInfoTool, FindTool, GrepTool, or ListDirTool and add your SubmitFooTool next to them. The change is typically a one-line addition that registers your concrete tool type with the tool registry or DI container so the runtime can resolve it.

## Existing examples

- [FileInfoTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md)
- [FindTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md)
- [GrepTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md)
- [ListDirTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md)

---
*Synthesised by Aurion on 2026-06-09 03:26:35 UTC*
