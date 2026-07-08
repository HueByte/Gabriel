# AgentToolsOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentToolsOptions.cs`  
> **Kind:** class

```csharp
public sealed class AgentToolsOptions : IConfigSection<AgentToolsOptions>
```


AgentToolsOptions is a sealed configuration container for the agent's host-mode filesystem tooling. It implements [`IConfigSection<AgentToolsOptions>`](IConfigSection.cs.md) and is surfaced under the AgentTools configuration section to group the knobs that govern host-root confinement, file previews, and directory listings.

## Remarks

This abstraction centralizes governance over host-mode operations, providing a single place to opt in to host-root confinement and to tune interactive behaviors (previews and listings). By tying host-mode to an anchored root (HostRoot), it ensures filesystem operations stay within a trusted boundary and prevents accidental access to the host system outside that boundary. The defaults are chosen to balance responsiveness with safety while remaining easy to override when a workspace requires it.

## Notes

- HostRoot is the opt-in switch for host-mode: leaving it null or empty disables host-mode; project-sandbox-style operations remain available. The operator must deliberately opt in by configuring a non-empty HostRoot.
- Absolute paths must canonicalize under HostRoot; relative paths resolve against HostRoot. Any attempt to operate outside the anchored root should be rejected by design.
- MaxPreviewBytes, DefaultPreviewLines, MaxListEntries, and DefaultListEntries establish safe, predictable defaults for previews and listings. Adjust these with care to match workspace size and performance needs; aggressive settings can impact responsiveness or memory usage.