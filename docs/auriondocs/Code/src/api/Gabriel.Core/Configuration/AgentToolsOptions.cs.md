# AgentToolsOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentToolsOptions.cs`  
> **Kind:** class

Options for the filesystem and shell agent tools. Use this class when you need to configure how agent-facing filesystem operations behave (which root path the host-mode tools are allowed to access, how much of a file is previewed, and limits on directory listings). The settings are bound from the configuration section named by AgentToolsOptions.SectionName ("AgentTools").

## Remarks
This POCO centralizes safety and ergonomics knobs for agent file/shell operations. The most important control is HostRoot: when set, all host-mode filesystem operations are canonicalized against this absolute directory and any path that resolves outside it is rejected. Other properties act as hard or friendly limits to prevent excessive resource use (large previews, runaway directory traversal) and to provide sensible defaults for listings and file previews.

## Example
```csharp
// Bind in Startup/Program.cs
services.Configure<AgentToolsOptions>(Configuration.GetSection(AgentToolsOptions.SectionName));

// Consume via IOptions<AgentToolsOptions>
public class MyAgentComponent
{
    private readonly AgentToolsOptions _opts;
    public MyAgentComponent(Microsoft.Extensions.Options.IOptions<AgentToolsOptions> opts)
    {
        _opts = opts.Value;
    }

    public void ShowConfiguredLimits()
    {
        Console.WriteLine($"HostRoot: {_opts.HostRoot}");
        Console.WriteLine($"MaxPreviewBytes: {_opts.MaxPreviewBytes}");
    }
}
```

## Notes
- HostRoot must be an absolute directory; leaving it null or empty disables host-mode access (project-sandbox mode continues to work).
- MaxPreviewBytes is a byte count (default 10 * 1024 * 1024 = 10 MiB). Files larger than this will have size reported but their preview will be skipped.
- MaxListEntries is a hard cap applied to any single listing operation to prevent runaway recursion; DefaultListEntries is the non-paginated default and may be lower than the hard cap.
- DefaultPreviewLines controls head/tail lines shown in previews (default 6).