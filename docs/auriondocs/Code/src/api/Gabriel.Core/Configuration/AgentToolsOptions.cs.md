# AgentToolsOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentToolsOptions.cs`  
> **Kind:** class

```csharp
public sealed class AgentToolsOptions : IConfigSection<AgentToolsOptions>
```


AgentToolsOptions defines the configuration contract for the filesystem and shell agent tools used by the host-mode environment. It binds from the AgentTools section of the application configuration and governs host-root path canonicalization, file preview limits, and directory listing constraints for safety. The HostRoot property is the single most important knob: all host-mode filesystem operations are canonicalized under this root, and any path resolving outside it is rejected.

## Remarks

AgentToolsOptions implements [`IConfigSection<AgentToolsOptions>`](IConfigSection.cs.md), enabling centralized binding and validation of settings from configuration sources. This abstraction localizes safety concerns—such as path confinement and preview quotas—into a single, testable place, separating them from the runtime agents themselves. By centralizing these knobs, components that perform host-mode operations can rely on a single, well-defined contract for what is allowed, reducing risk of accidentally accessing outside workspace.

## Example

```csharp
// Example: enable host-mode with a concrete root and sane defaults
var options = new AgentToolsOptions
{
    HostRoot = "/sandbox/host",
    MaxPreviewBytes = 5 * 1024 * 1024, // 5 MiB
    MaxListEntries = 500,
    DefaultListEntries = 100,
    DefaultPreviewLines = 8
};
```

## Notes

- Leave HostRoot null/empty to disable host mode entirely (project-sandbox mode still works).
- Relative paths resolve against HostRoot; absolute paths must canonicalize under it (paths outside are rejected).
- Setting MaxPreviewBytes too low can cause many previews to be omitted even for moderately large files; tune to balance performance and visibility.