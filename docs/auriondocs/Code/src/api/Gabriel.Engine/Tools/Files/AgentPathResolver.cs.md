# AgentPathResolver

> **File:** `src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs`  
> **Kind:** class

```csharp
public sealed class AgentPathResolver : IAgentPathResolver
```


AgentPathResolver encapsulates the logic for turning a potentially relative path into an absolute, project-scoped path while enforcing a strict root boundary. It supports two rooted contexts through PathRootMode: Host (the operator's host workspace) and Project (the current conversation's project directory). When ResolveAsync is called with a path and mode, it first resolves the appropriate root, converts the input into an absolute path, ensures the path cannot escape the root, and returns a ResolvedPath containing the computed absolute path, a user-friendly relative display, the root used, and the mode.

## Remarks
Centralizes path boundary checks and normalization to prevent directory traversal outside the permitted root. It abstracts host vs. project roots behind PathRootMode, coordinating with IToolExecutionContext and IProjectFileService to locate the right root. The returned ResolvedPath provides a stable, display-friendly path that can be safely presented to users or consumed by downstream tooling without reimplementing the resolution logic.

## Example
```csharp
// Resolve a path within the current project sandbox
var resolver = new AgentPathResolver(context, projectFiles, options);
var resolvedProj = await resolver.ResolveAsync("assets/logo.png", PathRootMode.Project, ct);

// Resolve a path relative to the operator's host workspace
var resolvedHost = await resolver.ResolveAsync("config/server.json", PathRootMode.Host, ct);
```

## Notes
- Path traversal attempts that escape the configured root will throw DomainException to prevent unauthorized access.
- The root boundary check is platform-aware: Windows uses a case-insensitive comparison, while POSIX systems are case-sensitive.
- If PathRootMode.Host is requested but HostRoot is not configured, a DomainException is thrown; callers should either configure HostRoot or switch to PathRootMode.Project.