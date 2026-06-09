# IDocsLookup.cs

> **Source:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`

## Contents

- [IDocsLookup](#idocslookup)
- [DocsSources](#docssources)
- [DocsContent](#docscontent)
- [DocsEntry](#docsentry)

---

## IDocsLookup

> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** interface

Abstraction for reading and enumerating Gabriel's official internal documentation. Use this interface when code needs to discover available docs entries and read content without depending on a specific backend (file system, GitHub, etc.). Implementations provide the actual lookup strategy and may be composed or swapped depending on environment (local primary source, GitHub fallback).

## Remarks
This interface presents a small, backend-agnostic facade: ListAsync returns a collection of metadata entries (DocsEntry) for available documents, and ReadAsync retrieves the text/content for a document identified by its path. It exists so callers can work with documentation uniformly while different implementations (for example: a local on-disk lookup, a GitHub-based lookup, or a composite that queries multiple sources in order) handle the I/O and source-specific behavior.

## Example
```csharp
// Typical usage: list documents and read the first one.
async Task PrintFirstDocAsync(IDocsLookup docsLookup, CancellationToken ct)
{
    var entries = await docsLookup.ListAsync(ct);
    var first = entries.FirstOrDefault();
    if (first == null) return;

    // assume DocsEntry exposes a path/identifier that ReadAsync accepts
    var content = await docsLookup.ReadAsync(first.Path, ct);
    if (content != null)
    {
        Console.WriteLine(content.Text);
    }
}
```

## Notes
- ReadAsync returns null when the requested path does not exist or the implementation cannot provide content for that path.
- Both methods typically perform I/O (disk, network) and should be called with a CancellationToken to allow cooperative cancellation and to avoid blocking threads for long operations.


---

## DocsSources

> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** class

Holds canonical string identifiers for known documentation sources used by the docs lookup system. Use these constants wherever the code needs to reference a documentation source by its machine identifier instead of embedding literal strings.

## Remarks
This static class centralizes the source keys used by documentation lookup routines so callers use a single, well-known identifier instead of repeating string literals. Treat these values as stable keys (not user-facing labels) that other components use to select or route documentation retrieval.

## Example
```csharp
// Use the constant to avoid hard-coded strings:
var doc = docsLookup.GetDocumentation(DocsSources.GitHub, "owner/repo/path");

// Or compare a source identifier returned from some API:
if (sourceId == DocsSources.LocalLlmNative)
{
    // handle local native LLM docs
}
```

## Notes
- These are declared as public const strings; their values are inlined by the C# compiler into consuming assemblies. If you change a value here, dependent assemblies must be recompiled to observe the change.
- Values are intended as internal identifiers (routing keys), not as display names for UI.

---

## DocsContent

> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** record

Represents the content read from a documentation source including where it came from and a canonical URL for citations. Use this record as the return value from IDocsLookup.ReadAsync (or when normalizing documents from multiple sources) so callers have the document text, its lookup path, the originating source identifier, and an optional permanent URL.

## Remarks
This record is a small, immutable DTO that unifies different documentation sources (local files, GitHub blobs, etc.) into a single shape consumable by downstream processors. Path is the identifier or key used by the lookup implementation, Content holds the raw document text, CanonicalUrl (nullable) is the source's idea of a stable link suitable for citations, and Source identifies the origin (e.g., a repo name or "local" provider).

## Example
```csharp
var doc = new DocsContent(
    Path: "guides/getting-started.md",
    Content: "# Getting started\nThis guide shows...",
    CanonicalUrl: "https://github.com/org/repo/blob/main/guides/getting-started.md",
    Source: "github"
);

// For a local file the canonical URL might be a file:// URI
var local = new DocsContent(
    Path: "docs/overview.md",
    Content: "...",
    CanonicalUrl: "file:///home/user/project/docs/overview.md",
    Source: "local"
);
```

## Notes
- CanonicalUrl may be null if the source cannot provide a stable external link.
- Do not assume Path is a URL; it is the lookup key/identifier and may be a relative path or other provider-specific string.
- DocsContent is immutable and uses value equality like other C# records.

---

## DocsEntry

> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** record

A small immutable DTO that represents a discovered documentation item: the file Path (relative to the source's root), an optional Title extracted from the document (null when no title is available), and a free-form Source tag identifying where the entry came from (for example "local-llm-native" or "github"). Use this type when returning or enumerating documentation results from a docs lookup or indexing component.

## Remarks
DocsEntry is a positional record intended as a lightweight value container for documentation metadata. It is immutable, supports value equality, deconstruction, and with-expressions, and is suitable for use in collections or as return elements from IDocsLookup implementations. The Path is expected to be relative to the documented source's root; Title is nullable to reflect documents that lack a discoverable title; Source is an arbitrary tag used to tell callers which source or provider produced the entry.

## Example
```csharp
// Create a new entry for a README found in a local source
var entry = new DocsEntry("README.md", "Overview", "local-llm-native");

// Deconstructing
var (path, title, source) = entry;

// Create a modified copy with a different source
var mirrored = entry with { Source = "github" };
```

## Notes
- Title may be null; callers should handle missing titles when rendering lists or building indexes.
- Path is relative to the source's own root — consumers should not treat it as an absolute filesystem path.
- Source is a free-form tag used for grouping/origin; its format is implementation-defined.

---