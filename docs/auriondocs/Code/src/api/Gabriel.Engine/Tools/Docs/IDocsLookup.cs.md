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


IDocsLookup abstracts access to Gabriel's official internal documentation from multiple sources, exposing a single façade for listing docs and reading their content. Implementations include LocalDocsLookup (disk-based, the primary source) and GitHubDocsLookup (GitHub-hosted, the fallback); a CompositeDocsLookup composes several inner sources behind the same interface.

## Remarks
This abstraction decouples callers from storage specifics and enables graceful fallbacks between local and remote sources behind a single interface. The asynchronous methods (ListAsync and ReadAsync) with CancellationToken support allow integration into broader async workflows and enable cooperative cancellation. The composition model (CompositeDocsLookup) provides a deterministic order for trying multiple inner sources, presenting a consistent surface to tooling.

## Notes
- ReadAsync may return null if the requested path isn’t found in any configured source; callers should handle null before using the content.
- Always pass a CancellationToken to support cancellation of long-running lookups and to cooperate with higher-level cancellation policies.


---

## DocsSources
> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** class

```csharp
public static class DocsSources
```


DocsSources is a static helper class that centralizes the textual identifiers used to represent documentation sources. It exposes two compile-time constants: LocalLlmNative and GitHub. LocalLlmNative corresponds to the string `local-llm-native`, while GitHub corresponds to `github`. Consumers reference these constants instead of embedding literal strings, ensuring consistency across the Docs lookup workflow.

## Remarks
This class serves as the stable source of truth for documentation source identifiers. By exposing constants instead of scattered literals, it reduces typos and drift and makes updates in one place. It collaborates with the Docs lookup subsystem to route requests to the appropriate backend (local LLM-native or GitHub) without consumers needing to know the exact string values.

## Notes
- Because the fields are const, their values are baked into referencing assemblies at compile time; changing them requires recompilation of dependents.
- The class is static and has no instance state; treat it purely as a constant-collection utility.
- Ensure external doc backends use matching identifiers; a mismatch will result in failed lookups.

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


DocsContent is an immutable record that represents the result of ReadAsync for a documentation entry, bundling both the text and its metadata. Use it when you need to render a doc and also know where it came from and how to cite it—the canonical URL for citations, the path within the source, and the original source identifier.

## Remarks
DocsContent serves as a stable carrier for a documentation entry, pairing its text with provenance data. It decouples retrieval from rendering and citation, so consumers can display the content and still preserve a trustworthy link. The CanonicalUrl is intended for citations; if it is unavailable, Path and Source provide fallback references.

## Example
```csharp
// Most common usage: obtain the text and metadata for display and citation
DocsContent doc = await docsLookup.ReadAsync("docs/intro.md");
Console.WriteLine(doc.Content);
Console.WriteLine($"Permanent link: {doc.CanonicalUrl ?? "none"}");
```

## Notes
- CanonicalUrl may be null if the source doesn't expose a stable URL.
- Content may contain Markdown or other markup; render accordingly.
- Path is relative to the documentation root for the current source; if you need an absolute path, combine with Source.

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


Represents a documentation entry with a file path, an optional title, and a source identifier. This record is typically used to encapsulate metadata about documentation files, allowing consumers to reference the location, display title, and origin of the documentation content.

## Remarks
This record serves as a simple, immutable data container that facilitates the organization and lookup of documentation resources within the system. By including an optional title, it supports cases where the documentation may or may not have a defined heading, while the source string helps track the provenance or context of the documentation entry.


---