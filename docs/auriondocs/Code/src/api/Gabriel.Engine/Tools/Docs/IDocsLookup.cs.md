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

```csharp
public interface IDocsLookup
```


IDocsLookup abstracts Gabriel's official internal documentation sources, offering two asynchronous operations: ListAsync(CancellationToken ct), to enumerate available documentation entries, and ReadAsync(string path, CancellationToken ct), to fetch the content of a document by its path. Implementations such as LocalDocsLookup (reading from a local folder on disk) and GitHubDocsLookup (the human-prose docs on GitHub) can be composed via CompositeDocsLookup to present a single, ordered facade to doc-consuming tooling. This interface enables code to discover and load docs without caring about the underlying storage.

## Remarks
This abstraction decouples the documentation source from its consumers, enabling a pluggable, ordered mix of backends (local vs. GitHub) and a straightforward fallback strategy. By offering a single facade, callers can rely on consistent behavior while the actual content retrieval is delegated to the configured inner sources; this also simplifies testing by swapping in mock sources.

---

## DocsSources
> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** class

```csharp
public static class DocsSources
```


DocsSources is a static container of string constants that identifies the origins from which documentation data can be sourced in the docs lookup flow. It exposes two identifiers: LocalLlmNative, with the value "local-llm-native", and GitHub, with the value "github". Consumers reference DocsSources.LocalLlmNative or DocsSources.GitHub when configuring or querying documentation sources, rather than using raw string literals.

## Remarks
This abstraction centralizes the set of valid doc sources, making it straightforward to add new sources in the future without updating dozens of call sites. It also enforces consistency across the codebase, guarding against typos in the identifiers. It sits at the boundary between the docs subsystem and the rest of the tooling, ensuring collaborators share a single vocabulary for source selection.

## Notes
- Use these constants instead of hard-coded strings to avoid drift.
- If you add a new source, extend this class and align its value with the external system's expected identifier to keep interoperability.

---

## DocsContent
> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** record

```csharp
public sealed record DocsContent(string Path, string Content, string? CanonicalUrl, string Source)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Path` | `string` | — |
| `Content` | `string` | — |
| `CanonicalUrl` | `string?` | — |
| `Source` | `string` | — |


DocsContent is a small, immutable record that carries the data produced by the documentation lookup. It encapsulates the Path of the document being retrieved, the Content to render, an optional CanonicalUrl that should be used for citations or linking, and the original Source used to generate the Content. This type is the return shape of ReadAsync and may be null when a document cannot be found or loaded; callers typically deconstruct it or read its properties to feed rendering pipelines or caches.

## Remarks
DocsContent serves as the contract between the lookup service and the rendering layer by bundling the payload with metadata. This separation enables a clean pull-based workflow where retrieval, transformation, and presentation are decoupled. Because CanonicalUrl is nullable, consumers should gracefully handle its absence and rely on Path or Content for fallback behavior.

## Notes
- ReadAsync may return null if the document cannot be found; always null-check the result before accessing properties.
- CanonicalUrl is nullable; when present it should point to a stable, citation-friendly URL (e.g., file:// for local sources or a blob URL for hosted sources).
- The Content field contains the produced documentation text; the Content is derived from the Source and may be large, so consider caching or streaming in downstream processes if needed.

---

## DocsEntry
> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** record

```csharp
public sealed record DocsEntry(string Path, string? Title, string Source)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Path` | `string` | — |
| `Title` | `string?` | — |
| `Source` | `string` | — |


DocsEntry is an immutable value object that represents a single documentation entry. It groups together the entry's Path, an optional Title, and a Source indicator describing where the entry originated. Developers reach for this type when they need to transport or compare documentation entry metadata as a single unit, rather than passing each field separately. The sealed, positional record pattern guarantees value-based equality and convenient deconstruction, which is ideal for lightweight data carriers in tooling code like documentation lookups.

## Remarks
Because Path encodes the location of the documentation, and Source records the origin (for example, local-llm-native or github), DocsEntry helps separate concerns between storage, display, and provenance. The Title field is optional to accommodate doc entries that omit a human-readable heading; consumers should handle null gracefully when presenting UI. As a value type, DocsEntry participates in equality comparisons by value rather than reference identity, which simplifies deduplication and lookup tasks in documentation catalogs.

## Notes
- The Title property may be null; callers should handle null when rendering a title.
- Path uniquely identifies the entry and is typically treated as the identity within collections.
- Source is a free-form tag; use a consistent set of values to enable reliable filtering and grouping.

---