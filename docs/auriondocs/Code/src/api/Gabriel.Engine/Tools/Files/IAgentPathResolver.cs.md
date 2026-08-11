# IAgentPathResolver.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`

## Contents

- [IAgentPathResolver](#iagentpathresolver)
- [ResolvedPath](#resolvedpath)
- [PathRootMode](#pathrootmode)

---

## IAgentPathResolver
> **File:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`  
> **Kind:** interface

```csharp
public interface IAgentPathResolver
```


IAgentPathResolver abstracts the policy and mechanics for turning a path argument into an absolute file-system path that a tool is allowed to access. It scopes resolution to either the configured host root or the active conversation's project sandbox, and ensures the same hardening — prefix checks, mode gating, and project authorization — is enforced exactly once during resolution. Call ResolveAsync with the path (relative or absolute), the desired root mode, and a CancellationToken; it returns a ResolvedPath wrapped in a Task.

## Remarks
Centralizes security-sensitive path handling to prevent directory traversal and accidental access to forbidden areas. By providing a single place where host-root versus project-sandbox boundaries are evaluated, this abstraction reduces duplication and improves testability across filesystem-related tools. It also makes future changes to path policy easier to propagate.

## Notes
- Always propagate the CancellationToken; do not block on the returned Task to avoid deadlocks.
- Do not bypass the resolver by manually composing paths in consuming tools; rely on ResolveAsync so the host/root sandbox boundaries and authorization checks are consistently applied.

---

## ResolvedPath
> **File:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`  
> **Kind:** record

```csharp
public sealed record ResolvedPath(
    string Absolute,
    string Display,
    string RootAbsolute,
    PathRootMode Mode)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Absolute` | `string` | — |
| `Display` | `string` | — |
| `RootAbsolute` | `string` | — |
| `Mode` | `PathRootMode` | — |


ResolvedPath represents the result of resolving a path within Gabriel.Engine tooling. It carries both the actual absolute path to operate on (Absolute) and a user-facing display path (Display), together with the root directory anchoring the path (RootAbsolute) and the resolution mode that describes how the root was determined (Mode).

## Remarks
This abstraction cleanly separates IO concerns from presentation: callers can use Absolute for file operations while presenting Display to users or logs. RootAbsolute provides a stable anchor for re-rooting or walking the path, and Mode communicates the rooting semantics so callers can adapt if paths are rooted under different roots. Together, it coordinates path information across resolvers and consumers without leaking path-walking logic into business code.

## Notes
- Absolute and Display may differ (e.g., in cases where a user-facing display path is shortened or reformatted); prefer Absolute for any actual filesystem IO. 
- RootAbsolute must be consistent with Absolute to avoid confusing representations; use Display to show a more friendly version when appropriate.
- As a record, ResolvedPath is immutable; construct a new instance to reflect any change in path resolution rather than mutating an existing one.

---

## PathRootMode
> **File:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`  
> **Kind:** enum

```csharp
public enum PathRootMode
{
    Host,
    Project,
}
```


PathRootMode is an enumeration used to indicate the root context for path resolution. It allows callers to choose whether relative paths are anchored to the hosting environment root (Host) or to the project root (Project). This separation lets the path resolver adapt to different scenarios (tooling vs. project assets) without changing its internal logic.

## Remarks
PathRootMode provides a clean abstraction boundary between host-level resources and project-scoped assets. It enables consumers and tests to express intent about where a path should be resolved, reducing coupling to a particular directory structure. By centralizing this choice, the resolver can support multiple environments while keeping the calling code simple.

## Notes
- Be mindful that Host and Project roots may differ across environments (e.g., IDE-driven sessions vs. CI vs. deployed runners); ensure you select the mode that matches the actual resolution context.
- If the project root is unavailable or not configured, using Project may cause resolution to fail; consider fallbacks or configuration checks.
- Do not mix resolution modes within a single operation; pick one mode per path operation to avoid inconsistent results.

---