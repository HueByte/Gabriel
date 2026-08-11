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


SequenceCatalogResponse encapsulates the catalog of selectable avatar skin identifiers, split into patterns and palettes. It acts as the server-side reference for valid IDs that clients may submit via PATCH endpoints, with validation performed against these lists before persisting the selection on the entity. The use of plain strings (instead of enums) enables catalog growth without requiring client regeneration.

## Remarks

- It provides a stable contract for clients to retrieve the available options and a clear validation target on the server.
- The dual-list structure lets the server evolve patterns and palettes independently and safely.
- As a record, it offers value-based equality and immutability for the response payload, which helps with caching and predictable comparisons.

## Example

```csharp
var catalog = new SequenceCatalogResponse(
    Patterns: new[] { "PatternA", "PatternB" },
    Palettes: new[] { "PaletteRed", "PaletteBlue" }
);
```

## Notes

- Validation ensures only identifiers in the respective lists are accepted.
- If you have no options, pass empty lists rather than null to avoid null-reference issues.