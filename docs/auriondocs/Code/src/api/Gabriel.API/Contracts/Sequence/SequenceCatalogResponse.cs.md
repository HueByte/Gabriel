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


SequenceCatalogResponse is a simple data transfer contract that exposes the authoritative lists of selectable avatar skin options. It provides two separate read-only collections: one for available pattern identifiers and one for available palette identifiers. Clients fetch this object to render the skin-picker UI and present valid options to users, while PATCH requests carry the chosen pattern and palette back to the server for validation and persistence on the entity. By using distinct string arrays rather than enums, the catalog can grow over time without forcing client regeneration, preserving backward compatibility as new identifiers are added.

## Remarks
The separation into two string lists decouples the client from a fixed, compile-time enum, enabling dynamic catalog updates driven by the server. This design ensures that validation and storage of a user’s selection occur against a single, authoritative source of truth and minimizes client-side coupling to server-side catalog evolution.

## Notes
- Return lists should be non-null (preferably empty) to simplify client handling and avoid null checks.
- The client must treat these lists as the authoritative source for valid identifiers when showcasing options and validating PATCH payloads; changes to the catalog are reflected only when this symbol is refreshed from the API.