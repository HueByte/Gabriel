# SequenceCatalogResponse

> **File:** `src/api/Gabriel.API/Contracts/Sequence/SequenceCatalogResponse.cs`  
> **Kind:** record

Represents the server's catalog of selectable avatar sequence patterns and color palettes returned to clients for populating an avatar skin picker. Clients display these identifier lists to users and send a selected identifier back to the server (via project/conversation PATCH endpoints); the server validates the chosen identifier against this catalog before storing it.

## Remarks
This record intentionally exposes identifiers as string collections (`IReadOnlyList<string>`) rather than enums so the catalog can be extended on the server without requiring clients to regenerate code. The values are identifiers (not user-facing labels); clients are expected to map them to localized display names or UI assets. The server is responsible for validating incoming identifiers against this list.

## Example
```csharp
// Constructing a response and returning it from an ASP.NET controller
var catalog = new SequenceCatalogResponse(
    new[] { "dots", "stripes", "grid" },
    new[] { "sunset", "ocean", "monochrome" }
);
return Ok(catalog);
```

## Notes
- Treat the strings as opaque identifiers; do not attempt to parse or infer meaning from them—use exact string equality when comparing.
- The catalog can change between releases; clients should not assume identifiers are permanent or rely on a specific ordering.
- Although the properties are typed as IReadOnlyList, the underlying collection instance could be mutable; copy the lists if you require deep immutability on the client or server side.
