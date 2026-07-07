# AgentToolsOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentToolsOptions.cs`  
> **Kind:** class

```csharp
public sealed class AgentToolsOptions : IConfigSection<AgentToolsOptions>
```


AgentToolsOptions defines configuration options for the agent’s host-mode filesystem and shell tooling. It is bound to the AgentTools configuration section and centralizes safeguards and defaults that govern how host-mode operations resolve paths, preview file contents, and enumerate directories. Use this class when you need to enable, constrain, or disable host-mode behavior (by setting HostRoot) and to adjust how much data is loaded or listed during previews and directory listings.

## Remarks
This abstraction isolates host-mode safety and resource constraints behind a single config object, so all host filesystem interactions consistently respect the same bounds. By providing sensible defaults, it protects against runaway previews and listings while remaining opt-in through HostRoot. It also simplifies testing by allowing tests to override only a few knobs without touching the rest.

## Example
```csharp
var options = new AgentToolsOptions
{
    HostRoot = "/sandbox/agent",
    MaxPreviewBytes = 5 * 1024 * 1024, // 5 MiB
    MaxListEntries = 500,
    DefaultListEntries = 100,
    DefaultPreviewLines = 4
};
```

## Notes
- If HostRoot is null or empty, host-mode is effectively disabled; code that relies on host-mode behavior should handle this case gracefully.
- MaxPreviewBytes bounds the amount of file content shown in previews; large files will report their size but skip preview content beyond this limit.
- MaxListEntries caps the number of directory entries returned by listing operations to prevent runaway recursion or extremely large responses.
