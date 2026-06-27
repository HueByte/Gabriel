# Adding a new agent tool

> *Workflow template auto-derived from 4 existing exemplar(s).*

Adding a new agent tool

When you need the agent to perform a new callable operation, add an agent "tool" — a small class implementing ToolBase that exposes a name, parameters, and an ExecuteAsync implementation. This pattern is the standard way to extend the agent's capabilities in C#: create a new Tool class, drop it into the engine project, and the agent runtime will treat it as another tool the agent can invoke.

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

Tools of this kind live in the Gabriel.Engine tooling area. The existing examples are under src/api/Gabriel.Engine/Tools/Files and follow a clear naming convention: the class name ends with "Tool" and the file name matches the primary symbol (for example, FileInfoTool in FileInfoTool.cs). Add your new tool source file to the appropriate Tools subfolder in src/api/Gabriel.Engine (or a new subfolder if the tool logically belongs to a different category).

## DI wiring

In this codebase tools are typically placed into the engine assembly so the agent runtime can discover them. In practice you usually only need to add the new class file; the engine's tool discovery will pick up ToolBase implementations. If your local configuration requires manual DI registration, add a single registration in the application's composition root (where other services are registered), for example: services.AddSingleton<ToolBase, SubmitFooTool>(); — but most teams working with the existing exemplars simply add the file under the Tools folder and no further wiring is required.

## Existing examples

- [FileInfoTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md) — primary symbol FileInfoTool. FileInfoTool is a representative tool in src/api/Gabriel.Engine/Tools/Files.
- [FindTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/FindTool.cs.md) — primary symbol FindTool. FindTool is a representative tool in src/api/Gabriel.Engine/Tools/Files.
- [GrepTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/GrepTool.cs.md) — primary symbol GrepTool. GrepTool is a representative tool in src/api/Gabriel.Engine/Tools/Files.
- [ListDirTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs.md) — primary symbol ListDirTool. ListDirTool is a representative tool in src/api/Gabriel.Engine/Tools/Files.

---
*Synthesised by Aurion on 2026-06-08 22:37:31 UTC*
