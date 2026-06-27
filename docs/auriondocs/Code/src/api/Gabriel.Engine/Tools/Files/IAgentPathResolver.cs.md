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

Resolves a tool-supplied path into an absolute, validated path on disk that is scoped either to the host root or to the active conversation's project sandbox. Reach for this interface whenever an agent filesystem tool needs a canonical, authorization-checked path so all normalization, root-scoping and access gating are applied consistently.

## Remarks
This interface centralizes path hardening for filesystem tools: implementations perform the same prefix checks, mode-based gating and project authorization once, so callers don't need to duplicate those checks. That reduces the chance of inconsistent enforcement across different tools and makes it straightforward to change scoping or authorization logic in one place.

## Example
```csharp
// Resolve a relative path for project-scoped access
CancellationToken ct = CancellationToken.None;
IAgentPathResolver resolver = /* injected implementation */;
var resolved = await resolver.ResolveAsync("relative/path/to/file.txt", PathRootMode.Project, ct);
// 'resolved' is a validated, absolute representation of the path; use it with your filesystem operations
```

## Notes
- Choose the correct PathRootMode (host vs project) — using the wrong mode can result in denied access or incorrect scoping.
- Do not bypass this resolver when implementing agent filesystem tools; doing so risks inconsistent authorization and path-traversal vulnerabilities.
- Honour the provided CancellationToken to avoid long-running or blocked operations.

---

## ResolvedPath

> **File:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`  
> **Kind:** record

```csharp
// `Display` is the user-friendly form to show in tool output - relative to
// the resolved root. `Absolute` is what we actually open. `RootAbsolute` is
// the directory the path is pinned under, surfaced for "Path: X (rooted at Y)"
// output and for callers that want to do their own walks.
public sealed record ResolvedPath(
    string Absolute,
    string Display,
    string RootAbsolute,
    PathRootMode Mode)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Y` | `rooted at` | — |


Represents a path that has been resolved against a known root and provides both a user-facing form and the concrete filesystem location to operate on. Reach for this record when a resolver must return what to display to the user (Display), what to actually open or read (Absolute), and which root the path is pinned under (RootAbsolute), along with the resolution mode (Mode).

## Remarks
This type exists to separate presentation from action: Display is intended for human-friendly output (usually relative to the resolved root), while Absolute is the canonical filesystem path callers should use for I/O. RootAbsolute records the directory the path was resolved/pinned under so callers can show context like "rooted at X" or perform their own directory walks relative to that root. Mode conveys how the path was resolved and can influence caller behavior when handling the result.

## Example
```csharp
// Typical use: present a path to the user, but use Absolute for file operations
var rp = new ResolvedPath(
    Absolute: "/work/project/src/File.cs",
    Display: "src/File.cs",
    RootAbsolute: "/work/project",
    Mode: PathRootMode.Pinned
);

Console.WriteLine($"Path: {rp.Display} (rooted at {rp.RootAbsolute})");
using var stream = File.OpenRead(rp.Absolute);
```

## Notes
- Do not assume Display is an absolute path; prefer Absolute for any file system operations.
- RootAbsolute is provided for context and for callers that need to enumerate or walk the pinned root themselves.
- Mode indicates how the resolver derived the returned path and may affect how callers interpret or validate the other fields.

---

## PathRootMode

> **File:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`  
> **Kind:** enum

Specifies which base/root should be used when resolving file paths for agent operations: Host treats paths relative to the machine/host filesystem, while Project treats paths relative to the current project or workspace root. Use this enum when calling path-resolution APIs to indicate whether a path should be resolved against the project sandbox or against the host environment.

## Remarks
This enum lets path-resolution logic be explicit about scope, enabling the same resolver APIs to operate in either project-local or host-wide contexts. It helps prevent accidental access to files outside the intended workspace (use Project to stay within the workspace) or to intentionally reference host-level locations (use Host when the agent must access system paths).

## Example
```csharp
// Typical usage with a resolver that accepts a root mode
var mode = PathRootMode.Project; // resolve relative to the project's root
var resolvedPath = agentPathResolver.Resolve("logs/output.txt", mode);

mode = PathRootMode.Host; // resolve relative to the host filesystem
var hostPath = agentPathResolver.Resolve("/var/log/service.log", mode);
```

## Notes
- Relative paths are interpreted with respect to the selected root; prefer Project for workspace-scoped operations to avoid leaking access to host files.
- Choosing Host can expose files outside the project; validate inputs when paths originate from untrusted sources.

---