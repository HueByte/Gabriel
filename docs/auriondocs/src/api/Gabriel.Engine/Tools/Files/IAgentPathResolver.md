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

Resolves a tool-supplied path (either relative or absolute) to an absolute, on-disk path that is constrained to an allowed root. Implementations map the provided path into one of two scopes — the configured host root or the active conversation's project sandbox — and apply the platform's hardening rules (prefix checks, mode-based gating, project authorization) so filesystem-accessing tools can rely on a single, consistent enforcement point.

## Remarks
This interface centralizes path resolution and security checks for all filesystem agent tools so that authorization, sandboxing, and canonicalization are applied uniformly and exactly once. Callers supply the raw path string and a PathRootMode indicating which root to consider; the returned ResolvedPath represents the absolute, authorized location an agent tool may access.

## Notes
- The input may be either a relative or absolute path; the resolver decides how to interpret relative paths with respect to the chosen root.
- The PathRootMode parameter determines whether the resolution is performed against the host root or the active project sandbox — choose it according to the tool's permission scope.
- The CancellationToken cancels the asynchronous resolution operation; callers should propagate cancellation where appropriate.
- Implementations commonly enforce authorization and will reject or fail resolution for paths outside allowed prefixes or project boundaries — be prepared to handle errors from ResolveAsync.

---

## ResolvedPath

> **File:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`  
> **Kind:** record

A small immutable DTO that represents the result of resolving a user-supplied path into an actual filesystem location. Use this when an API needs to present both the concrete path used to open files (Absolute) and a user-friendly or relative representation (Display), along with the root directory the path was pinned under (RootAbsolute) and metadata about how the resolution was performed (Mode).

## Remarks
This record is returned by path-resolution logic to separate presentation concerns from the on-disk target. Callers that operate on files should use Absolute to access the filesystem; UI or logs should prefer Display (often relative to the resolved root) and may show RootAbsolute for context. Mode conveys how the resolver treated the input (for example: anchored, substituted, or left as-is) so consumers can adjust behavior or messaging accordingly.

## Example
```csharp
// Typical use after resolving a path from user input
ResolvedPath rp = resolver.Resolve("./data/config.json");
// Open the actual file
using var stream = File.OpenRead(rp.Absolute);
// Show friendly output to the user
Console.WriteLine($"Path: {rp.Display} (rooted at {rp.RootAbsolute})");
// Adjust behavior based on resolution mode
if (rp.Mode == PathRootMode.Mapped)
{
    Console.WriteLine("This path was mapped into a pinned workspace.");
}
```

## Notes
- Display is intended for human-facing output and may be relative to RootAbsolute; do not use it for filesystem operations.
- Absolute is the canonical path the system will open but may not imply the file exists; callers should still handle IO errors and existence checks.
- This record is immutable (a C# record); its string properties are non-nullable by declaration and should be treated as already-normalized by the resolver, though callers should not assume symlink resolution or platform-specific canonicalization beyond what the resolver documents.

---

## PathRootMode

> **File:** `src/api/Gabriel.Engine/Tools/Files/IAgentPathResolver.cs`  
> **Kind:** enum

Indicates which root context should be used when resolving or interpreting file paths: either the host (machine-wide) filesystem root or a project-scoped root. Use this enum when calling path-resolution APIs to make explicit whether paths are relative to the host environment or to the current project.

## Remarks
This enum exists to disambiguate resolution behavior in components that can operate against multiple root contexts (for example, code that must choose between physical host file locations and virtual/project-local paths). Choosing Host typically means the resolver should treat paths as absolute or relative to the operating system environment, while Project means the resolver should interpret paths relative to the current project's root or workspace.

## Example
```csharp
// Example: passing the desired root mode into a hypothetical resolver
var mode = PathRootMode.Project;
var resolvedPath = agentPathResolver.ResolvePath("/data/config.json", mode);

// Example: handling the mode explicitly
switch (mode)
{
    case PathRootMode.Host:
        // access host filesystem
        break;
    case PathRootMode.Project:
        // map to project workspace
        break;
}
```

## Notes
- The enum's default numeric value is 0, which corresponds to PathRootMode.Host; be explicit when serializing or persisting values to avoid accidental defaults.
- If new modes are added later, update any switch statements or serialization handlers to maintain forward/backward compatibility.

---