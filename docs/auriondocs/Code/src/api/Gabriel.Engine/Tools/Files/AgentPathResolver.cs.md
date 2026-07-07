# AgentPathResolver

> **File:** `src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs`  
> **Kind:** class

```csharp
public sealed class AgentPathResolver : IAgentPathResolver
```


AgentPathResolver centralizes the logic for turning a provided path (relative or absolute) into a fully qualified path that stays within a sandbox root. It supports Host mode, which targets a configured host root, and Project mode, which resolves within the current conversation's project directory. It validates the input, resolves the appropriate root, normalizes and verifies the final path to prevent escaping the root, and returns a ResolvedPath containing the absolute path, a display-friendly relative path, the root, and the mode. Use this class to enforce consistent, safe path handling wherever the agent operates on file-system resources, rather than duplicating validation logic in disparate call sites.

## Remarks
The abstraction isolates environment-specific root scoping (host vs project) and centralizes security checks to prevent path traversal. It is intentionally stateless, relying on injected services, which makes it easy to unit-test and reuse across the agent tooling. It codifies OS-aware path comparisons and prefix checks, ensuring predictable behavior on Windows and UNIX-like systems and providing a consistent, copy-pasteable display path (normalized to forward slashes).

## Example
```csharp
// Example usage: resolve a path within the current project sandbox
var resolved = await resolver.ResolveAsync("docs/guide.md", PathRootMode.Project, ct);
```

## Notes
- Throws DomainException when the input path is empty.
- Throws DomainException if Host mode is requested but HostRoot is not configured, or if Project mode is requested without an attached project context.
- Throws DomainException when the resolved absolute path escapes the allowed root.