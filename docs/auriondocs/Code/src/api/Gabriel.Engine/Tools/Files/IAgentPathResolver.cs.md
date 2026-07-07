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


Resolves a tool's path argument to an absolute path on disk, scoped to either the configured host root or the active conversation's project sandbox. Implementations provide a single, centralized gate for path resolution, ensuring the same hardening (prefix check, mode gating, and project authorization) is applied consistently across all filesystem agent tools. Callers should rely on this abstraction whenever user-supplied or relative paths must be translated into safe, canonical paths before performing any file I/O.

## Remarks

IAgentPathResolver serves as the single policy boundary for path translation. By funneling path resolution through this interface, the engine guarantees that every path is resolved under the same security constraints (root scope, prefix validation, and authorization) before any filesystem access occurs. It decouples path policy from individual tools, enabling consistent behavior and easier testing or future policy changes.

## Notes

- Observe the CancellationToken; resolution may involve IO or authorization and should respect cancellation.
- Do not apply additional security checks at the call site; the contract expects the resolver to enforce prefix, mode, and authorization exactly once.
- Ensure the result is within the allowed roots; unexpected paths indicate a failure in the resolver's enforcement or in the provided inputs.

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


ResolvedPath is an immutable data carrier that encodes the result of resolving a path against a predefined root. It provides both the actual absolute path to operate on (Absolute) and a user-friendly representation (Display), along with the rooted directory (RootAbsolute) and a PathRootMode that indicates how the root was determined or applied.

## Remarks
This abstraction separates concerns between the filesystem interaction (Absolute) and how the path should appear to users or logs (Display).
It's commonly produced by a path resolver, such as IAgentPathResolver, and is passed through the system wherever path decisions need to be reported or validated.
Because ResolvedPath is a record, equality is value-based, which makes it suitable for caching and deduplication of resolution results.

## Notes
- Display should be treated as informational only; never rely on it for critical IO decisions.
- Keep Absolute and RootAbsolute in sync with Display to avoid confusing mismatches in UI.

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


PathRootMode is an enum that selects the root context for resolving file-system paths in the agent path resolver. It exposes two options—Host and Project—allowing callers to choose whether paths are rooted in the host environment or in the current project workspace.

## Remarks

By abstracting the root policy into this enum, the resolver can remain agnostic about where paths originate, while callers can express intent clearly at the call site. This design makes it straightforward to swap path-root semantics for different environments (for example, testing against a synthetic project workspace versus the real host file system) without changing resolver code.

## Example

```csharp
PathRootMode mode = PathRootMode.Host;
switch (mode)
{
    case PathRootMode.Host:
        // Resolve against the host file system
        break;
    case PathRootMode.Project:
        // Resolve against the project workspace
        break;
}
```

## Notes

- Be aware that PathRootMode currently has only Host and Project; adding new roots in the future requires updating call sites and any exhaustive matches.


---