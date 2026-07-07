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


This interface defines a contract for resolving a tool’s path argument into an absolute filesystem path on disk. Implementations centralize security-critical checks by validating path prefixes, applying mode-based root gating, and enforcing project authorization, guaranteeing these hardening steps occur exactly once per resolution. Callers pass a string that may be relative or absolute, a PathRootMode that selects the root scope (host root vs. project sandbox), and a CancellationToken to support cooperative cancellation. All filesystem agent tools route through this resolver to ensure consistent, auditable path resolution.

## Remarks
By funneling path resolution through IAgentPathResolver, the system ensures consistent scoping of tool-accessible files and prevents leakage between host and project sandboxes. The abstraction decouples the concerns of path normalization and security checks from individual tools, simplifying testing and evolving the underlying policy. It also makes it straightforward to swap in different resolution strategies (e.g., in-process vs. remote) without changing call sites.

## Example
```csharp
// Typical usage via dependency injection
var resolver = serviceProvider.GetRequiredService<IAgentPathResolver>();
ResolvedPath path = await resolver.ResolveAsync("logs/output.txt", PathRootMode.HostRoot, cancellationToken);
```

## Notes
- Ensure path normalization prevents directory traversal outside the allowed root; implementers must enforce the prefix check exactly once during resolution.
- Observe CancellationToken to support cancellation in I/O or authorization checks.
- Respect PathRootMode semantics; mixing modes across calls could bypass sandbox boundaries.

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


Represents the outcome of resolving a path within a rooted file system context. It bundles both the actual path opened by the tool (Absolute) and a user-friendly representation (Display) that is relative to the resolved root (RootAbsolute). The Mode indicates how the path was rooted, guiding consumers on how to interpret the Display value.

## Remarks
By centralizing path-related information in a single, immutable record, this symbol separates IO concerns from presentation concerns. Consumers can use Absolute for file access while presenting Display to users, and rely on RootAbsolute to understand the anchored root. PathRootMode further clarifies the rooting strategy, enabling callers to adapt Display formatting without changing how the underlying file system is accessed.

## Notes
- Absolute is the concrete filesystem path opened by the tool; Display is intended for user-facing presentation and may be rooted relative to RootAbsolute.
- RootAbsolute serves as the anchored root directory used to compute or interpret Display; ensure it remains consistent with how Display is produced.
- As a sealed record, ResolvedPath has value-based equality and is immutable once constructed, which supports straightforward caching and comparison across components.

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


PathRootMode represents which root context to use when resolving file system paths in the agent's path-resolution workflow. It exposes two values, Host and Project, to distinguish between paths resolved against the host machine's filesystem and those resolved within the current project workspace. Use this enum whenever a path-resolution operation must be contextualized to a specific root rather than assuming a single default.

## Remarks
This enum provides a lightweight abstraction that enables a multi-root path resolver to support different resolution strategies without scattering root-specific logic across callers. It cleanly separates concerns, letting the IAgentPathResolver switch behavior based on the selected root while other components simply pass a PathRootMode value.

## Notes
- Ensure that all path-resolution code paths handling PathRootMode remain in sync when adding new roots.
- Prefer using the enum type in switch statements and comparisons rather than raw integers to avoid invalid values.

---