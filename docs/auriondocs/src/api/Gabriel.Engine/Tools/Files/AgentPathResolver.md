# AgentPathResolver

> **File:** `src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs`  
> **Kind:** class

Resolves a file path into an absolute, sandboxed path tied to either the host root or the current project directory. Use this when agent tools need a canonical absolute path but must be restricted to a configured host root or to the project directory associated with the current execution context (to prevent access outside the allowed sandbox).

## Remarks
AgentPathResolver centralizes sandbox enforcement for tool file access by combining per-turn context (project id) and an operator-configured host root. It canonicalizes inputs (handling relative segments like ".."), accepts absolute paths (then validates them), and produces a normalized display path (forward slashes) for UI/commands. Errors are surfaced as DomainException when the requested mode is not available or the resulting path would escape the allowed root.

## Example
```csharp
// Resolve a path inside the current conversation's project sandbox
var resolved = await resolver.ResolveAsync("src/config.json", PathRootMode.Project, cancellationToken);
Console.WriteLine(resolved.Display); // e.g. "src/config.json" or "." if root

// Resolve an absolute path in host mode (requires AgentTools:HostRoot configured)
var hostResolved = await resolver.ResolveAsync("C:/tools/bin/tool.exe", PathRootMode.Host, cancellationToken);
Console.WriteLine(hostResolved.Absolute);
```

## Notes
- Empty or whitespace input throws DomainException("Path cannot be empty.").
- PathRootMode.Host requires AgentTools:HostRoot to be set; PathRootMode.Project requires the execution context to have a ProjectId — otherwise DomainException is thrown.
- The root containment check is string-based and uses case-insensitive comparison on Windows; it does not resolve or validate filesystem symlink targets, so symbolic links inside the root that point outside the root may bypass the intended filesystem boundary if not otherwise restricted.