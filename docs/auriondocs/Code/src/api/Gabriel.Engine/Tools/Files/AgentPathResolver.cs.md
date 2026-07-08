# AgentPathResolver

> **File:** `src/api/Gabriel.Engine/Tools/Files/AgentPathResolver.cs`  
> **Kind:** class

```csharp
public sealed class AgentPathResolver : IAgentPathResolver
```


AgentPathResolver is a sealed implementation of IAgentPathResolver that computes an absolute, sandboxed path from a user-provided relative or absolute input. It derives the root from the requested mode (Host or Project) using injected services and then validates and normalizes the resulting path to ensure it cannot escape the allowed root. This makes it safe to translate user or tool inputs into concrete file-system locations without leaking or traversing outside the designated sandbox.

## Remarks
AgentPathResolver centralizes root-bound path resolution and guards against directory traversal by verifying the computed path against the configured root. It collaborates with IToolExecutionContext to determine the current project context, IProjectFileService to locate the project directory, and AgentToolsOptions to read host-root configuration. By encapsulating this logic, it keeps path-safety concerns out of caller code and provides a single point of validation for tooling components.

## Example
```csharp
// Example usage
var resolver = new AgentPathResolver(context, projectFiles, options);
CancellationToken ct = CancellationToken.None;
var resolved = await resolver.ResolveAsync("docs/readme.md", PathRootMode.Project, ct);
```

## Notes
- If PathRootMode.Host is requested but HostRoot is not configured, a DomainException is thrown to indicate the host sandbox is disabled.
- If the computed absolute path escapes the allowed root, a DomainException is thrown to prevent access outside the sandbox.
