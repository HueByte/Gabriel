# SequenceCatalogResponse

> **File:** `src/api/Gabriel.API/Contracts/Sequence/SequenceCatalogResponse.cs`  
> **Kind:** record

Returns the available identifiers for avatar skin patterns and palettes that the API exposes to clients. Use this response when presenting a picker UI or validating a client's selection; clients receive these string identifiers and send back the chosen identifier in project or conversation PATCH requests.

## Remarks
This record intentionally exposes plain string identifier lists (Patterns and Palettes) rather than enums so the catalog can evolve without requiring client regeneration. The server treats these lists as the authoritative catalog and validates incoming selections against them before persisting.

## Example
```csharp
var response = new SequenceCatalogResponse(
    Patterns: new[] { "striped", "dotted", "solid" },
    Palettes: new[] { "summer", "monochrome", "neon" }
);

return Ok(response); // returned from a controller action to populate a client picker
```

## Notes
- The strings are identifiers/keys, not localized display names; the client should map them to user-facing labels or images.
- The record holds `IReadOnlyList<T>`, but the underlying collection can still be mutable if a mutable list is passed; prefer immutable/read-only collections when constructing this record to prevent accidental mutation.
- Clients must not assume semantic meaning or ordering in the lists — treat them as an authoritative set for validation only.