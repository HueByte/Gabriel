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


IDocsLookup defines a minimal, async contract for discovering and reading Gabriel's official internal documentation from pluggable backends. It is implemented by local and remote lookups (e.g., LocalDocsLookup and GitHubDocsLookup) and composed through a facade such as CompositeDocsLookup to present a single, unified API to tooling.

IDocsLookup exposes two asynchronous operations: ListAsync to enumerate available documentation entries, and ReadAsync to retrieve the content of a doc by path. This interface is designed as a facade over multiple inner sources, allowing tooling to remain agnostic to where docs come from (local disk, remote repositories, or cached stores).

## Remarks
IDocsLookup centralizes access to Gabriel's docs, enabling the tooling to swap sources or combine results without changing call sites. By decoupling consumers from concrete storage, it supports scenarios such as local development, caching layers, and remote provisioning while keeping the surface stable.

## Notes
- Both ListAsync and ReadAsync are asynchronous and accept CancellationToken to support cooperative cancellation.
- ReadAsync may return null if the specified path cannot be resolved by any configured source.
- The exact interpretation of the path is implementation-specific; callers should rely on the underlying DocsEntry semantics when constructing paths.

---

## DocsSources
> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** class

```csharp
public static class DocsSources
```


DocsSources is a static utility class that defines canonical identifiers used by the documentation lookup system. It exposes two string constants named LocalLlmNative and GitHub, with values "local-llm-native" and "github", respectively. Use these constants instead of literal strings to avoid typos and to enable centralized updates. When building a documentation query or selecting a source, reference DocsSources.LocalLlmNative or DocsSources.GitHub. Since the class is static and contains only constants, it cannot be instantiated and does not maintain state—it's a typed bag of well-known keys.

## Remarks
DocsSources centralizes the source keys to prevent scattering of magic strings and to provide a clear contract for what sources are supported by the docs subsystem. It helps decouple consumers from exact string literals and makes it easier to evolve the set of sources over time. The constants reflect the supported destinations and are intended to be consumed by the repository's tooling that resolves documentation sources.

## Example
```csharp
string key = DocsSources.LocalLlmNative;
```

## Notes
- The constants are compile-time constants; they cannot be changed at runtime.
- Add new sources here and update all call sites accordingly to maintain consistency.

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


DocsContent is a lightweight, immutable data holder that encapsulates a single documentation page retrieved by the docs lookup workflow. It carries the page's path, the raw content, an optional canonical URL used for citations, and a source indicator that reveals where the content originated. This record is typically produced by ReadAsync and consumed by renderers or indexers that need both content and provenance in one object.

## Remarks
DocsContent's role is to separate content from its provenance while offering a stable, serializable container that can be passed through the rendering pipeline or to citation/branding features. The CanonicalUrl property helps maintain a stable, linkable reference to the doc—even if its local path changes—while Source records the provenance, aiding diagnostics and caching.

## Example
```csharp
// Example: construct a DocsContent instance representing a locally stored doc
var doc = new DocsContent(
    "docs/intro.md",
    "# Introduction\nThis is an introduction.",
    "file:///C:/Projects/Gabriel.Engine/docs/intro.md",
    "local"
);
```

## Notes
- CanonicalUrl may be null; callers should gracefully handle the absence of a canonical link when rendering citations.
- DocsContent being a record means it is immutable; use the with-expression to derive a modified copy rather than mutating.

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


DocsEntry is an immutable, value-based record that holds the metadata for a single documentation entry: Path, an optional Title, and Source. It serves as a lightweight data carrier within the documentation lookup/catalog workflow, enabling components to transport, compare, and render references to documentation without embedding behavior.

## Remarks

Because DocsEntry is a record, it benefits from value equality and deconstruction, making it easy to use as a dictionary key, cache entry, or pass-through object in pipelines that assemble documentation catalogs. The Source field encodes provenance (e.g., local-llm-native or github), allowing consumers to group or display entries by origin without inspecting the doc content.

## Example

```csharp
// Example: representing a locally sourced docs entry
var entry = new DocsEntry("docs/api.md", "Docs API", "local-llm-native");

// Also supports missing titles
var entryNoTitle = new DocsEntry("docs/intro.md", null, "github");
```

## Notes

- Title is nullable; downstream rendering should gracefully handle a missing Title.
- Path should uniquely identify the document within its Source; consider normalization if comparing across different sources.

---