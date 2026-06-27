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

A minimal abstraction for reading Gabriel's internal documentation from any backing source. Use this interface when your code needs to enumerate available documentation entries or read the contents of a specific document without depending on the concrete storage (local files, GitHub, or a composite of multiple sources).

## Remarks
This interface separates consumers of documentation from the mechanics of how documents are stored or retrieved. Concrete implementations (for example a local on-disk lookup, a GitHub-backed lookup, or a composite that queries multiple inner lookups in order) provide the I/O and resolution logic; callers use ListAsync to discover available entries and ReadAsync to obtain the content for a given path.

## Example
```csharp
// Example: list all entries and read the first document if present
async Task PrintFirstDocAsync(IDocsLookup docsLookup, CancellationToken ct)
{
    var entries = await docsLookup.ListAsync(ct);
    if (entries.Count == 0) return;

    var first = entries[0];
    var content = await docsLookup.ReadAsync(first.Path, ct);
    if (content != null)
    {
        Console.WriteLine(content.Text);
    }
}
```

## Notes
- ReadAsync may return null when the requested path does not exist or cannot be resolved by the implementation; callers must handle a null result.
- Both methods are asynchronous and may perform I/O; respect the provided CancellationToken to allow prompt cancellation.
- Implementations can surface I/O or network exceptions — callers should handle or propagate exceptions according to their needs.

---

## DocsSources

> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** class

Shared string identifiers for documentation sources used across the docs lookup and retrieval code. Use these constants instead of hard-coded literals when selecting a documentation source (for example, when calling a lookup or loader), to avoid typos and keep source names consistent.

## Remarks
This static holder centralizes the canonical keys for known documentation sources (for example, a local LLM-backed source and GitHub). It exists to provide a single place to reference those identifiers so callers and implementors remain consistent; the class contains only compile-time constants and has no behavior.

## Example
```csharp
// Use a named source identifier instead of a raw string literal
var source = DocsSources.GitHub;
var docs = docsLookup.GetDocumentation(source, "owner/repo", "docs/usage.md");

// or
var local = DocsSources.LocalLlmNative;
var result = docsLookup.GetDocumentation(local, "model-name", "prompt-id");
```

## Notes
- These fields are consts, which means their values are inlined at compile time; changing a constant's value requires recompiling any assemblies that reference it.
- The class only defines identifiers — it does not imply any particular capabilities or configuration for a source. Match these keys with the code that registers or resolves doc sources.

---

## DocsContent

> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** record

Represents the content returned by a docs lookup operation (for example, the result of ReadAsync). It holds the source-specific path, the file's full text content, an optional canonical URL intended for citations/links, and a short identifier of the source that produced the content.

## Remarks
Produced by an IDocsLookup implementation, this record is used by consumers that need both the raw text and metadata useful for linking or indexing documentation (the CanonicalUrl and Source). Because it is a C# record, instances are immutable value objects with structural equality, making them suitable for use as keys or for simple comparisons in tests.

## Example
```csharp
// Typical use: a docs lookup returns the content of a documentation file
var doc = new DocsContent(
    Path: "src/Features/Parsing.md",
    Content: "# Parsing\nThis document explains parsing...",
    CanonicalUrl: "https://github.com/example/repo/blob/main/src/Features/Parsing.md",
    Source: "github"
);

Console.WriteLine(doc.Path);         // "src/Features/Parsing.md"
Console.WriteLine(doc.CanonicalUrl); // a URL suitable for citations or linking
```

## Notes
- CanonicalUrl is nullable; a provider may omit it when no stable external URL exists. The value is not validated by this type (it may be a file:// URI, an HTTP(s) blob URL, or any provider-specific URI).
- Content is stored as a single string in memory. Large files will increase memory usage; consumers that need streaming should use a different API.
- Path and Source are provider-specific: Path is the identifier the source uses for the document (not necessarily an absolute filesystem path), and Source indicates which lookup produced the content (e.g., "local", "github").

---

## DocsEntry

> **File:** `src/api/Gabriel.Engine/Tools/Docs/IDocsLookup.cs`  
> **Kind:** record

Represents a single documentation entry discovered or returned by the docs lookup subsystem. It holds the file path, an optional human-friendly title (if available), and a source tag that identifies where the entry originated — use this type when returning or aggregating documentation references from an IDocsLookup implementation.

## Remarks
This is a compact, positional sealed record intended as a simple DTO: it provides value-based equality, immutability, deconstruction, and a concise ToString implementation inherited from records. Title is nullable to indicate that some documentation files may not have a discoverable title; Path and Source are non-nullable by declaration but the record performs no runtime validation, so callers should ensure paths and source tags are normalized before creating instances if that matters.

## Example
```csharp
var entry = new DocsEntry("README.md", "Project README", "local-llm-native");
Console.WriteLine(entry.Path); // "README.md"
var (path, title, source) = entry; // deconstruction
```

## Notes
- Title may be null when a human-friendly title cannot be determined.
- The record is sealed and immutable; equality is by value (all three properties).
- No input validation is performed — callers are responsible for ensuring Path and Source meet any required format or constraints.


---