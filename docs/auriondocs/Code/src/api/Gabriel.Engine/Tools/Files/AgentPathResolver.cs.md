# AgentPathResolver

> **File:** `src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs`  
> **Kind:** class

Resolves a supplied path (relative or absolute) into a canonical absolute path anchored to either the configured host root or the current conversation's project directory, validates that the resulting path does not escape that root, and returns a ResolvedPath containing the absolute path, a normalized display form, the resolved root, and the mode. Use this when a tool needs a safe, sandboxed file path for reading or writing within the agent's allowed root.

## Remarks
AgentPathResolver centralizes path normalization and sandboxing rules so callers do not need to implement ad-hoc validation. It chooses the root based on PathRootMode (Host or Project), uses Path.GetFullPath to canonicalize segments (including resolving `..`), and then enforces that the final absolute path is either equal to the root or contained within it. The display path is returned relative to the root and normalized to forward slashes for consistent, cross-platform presentation.

## Example
```csharp
// Resolve a relative path against the current conversation's project root.
// The resolver is typically injected as IAgentPathResolver.
var resolved = await resolver.ResolveAsync("src/app/config.json", PathRootMode.Project, cancellationToken);
// `resolved` contains the canonical absolute path, a display path relative to the root
// (e.g. "src/app/config.json" or "." for the root), the root used, and the selected mode.
```

## Notes
- Empty or whitespace-only input throws DomainException: "Path cannot be empty.".
- If PathRootMode.Host is chosen but the HostRoot option is not configured, a DomainException is thrown explaining host mode is disabled.
- PathRootMode.Project requires the current execution context to have a ProjectId; otherwise a DomainException is thrown.
- Absolute inputs are accepted but still validated to ensure they do not escape the resolved root.
- Root containment checks append a directory separator to avoid false matches (e.g. "C:\\foo" vs "C:\\foobar").
- Comparison is case-insensitive on Windows and ordinal on non-Windows platforms; display paths always use forward slashes for consistency.
