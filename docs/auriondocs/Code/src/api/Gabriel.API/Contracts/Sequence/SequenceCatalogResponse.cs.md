# SequenceCatalogResponse

> **File:** `src/api/Gabriel.API/Contracts/Sequence/SequenceCatalogResponse.cs`  
> **Kind:** record

```csharp
public record SequenceCatalogResponse(
    IReadOnlyList<string> Patterns,
    IReadOnlyList<string> Palettes)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`Patterns`](../../../Gabriel.Engine/Sequence/Patterns.cs.md) | `IReadOnlyList<string>` | — |
| `Palettes` | `IReadOnlyList<string>` | — |


SequenceCatalogResponse represents the server-provided catalogs of selectable avatar appearance identifiers: one list of skin pattern identifiers and one list of palette identifiers. It is consumed by clients to render available options and by the server to validate and persist a user's chosen combination via PATCH endpoints; using separate string arrays rather than enums keeps the catalogs extensible without forcing client updates.

## Remarks
This abstraction decouples the client UI from the actual catalog values, allowing the server to evolve the available patterns and palettes independently of clients. The two independent lists reflect orthogonal concerns—one for patterns, one for palettes—so UI scaffolding can present them separately. As a record, SequenceCatalogResponse provides immutable, value-based equality which helps with caching, comparison, and testing, while the server can rely on a stable shape for (de)serialization. Validation on the server side ensures that only identifiers from these catalogs are persisted on the entity, preserving data integrity.

## Example
```csharp
var catalog = new SequenceCatalogResponse(
    new[] { "Checker", "Striped", "Plain" },
    new[] { "Ink", "Sunset" });
```

## Notes
- The properties are `IReadOnlyList<string>`, promoting immutability after construction.
- Pass non-null lists; null values may lead to runtime exceptions. Represent empty catalogs with empty arrays/lists when appropriate.