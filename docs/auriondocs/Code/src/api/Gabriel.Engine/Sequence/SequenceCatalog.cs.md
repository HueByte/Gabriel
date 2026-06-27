# SequenceCatalog

> **File:** `src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs`  
> **Kind:** class

A small, authoritative catalog of valid pattern and palette identifiers that clients can pin on a Project or a standalone Conversation. Use this class to validate, normalize and (for patterns) convert client-supplied string identifiers into the canonical string form or the PatternKind enum; prefer the Normalize* results for storage so database rows carry the canonical identifier.

## Remarks
Identifiers are treated as case-insensitive, trimmed strings rather than wire-enumerated values so the catalog can grow without database migrations. Unknown identifiers are intentionally ignored at generation time and the system falls back to seed-derived selection — this class exists to centralize the set of supported names, to provide culture-invariant normalization, and to keep palette ordering/curation independent of template definitions.

## Example
```csharp
// Validate and persist the canonical palette name
string rawPalette = "  Plasma ";
if (SequenceCatalog.IsKnownPalette(rawPalette))
{
    string canonical = SequenceCatalog.NormalizePalette(rawPalette)!; // "plasma"
    SavePaletteSelection(canonical);
}

// Parse a pattern override to an enum or fall back to a seed-derived value
PatternKind pattern = SequenceCatalog.TryParsePattern(userInput) 
                      ?? ComputePatternFromSeed(seedValue);
```

## Notes
- IsKnown*/Normalize* accept null and whitespace; null/blank inputs are considered unknown.
- Normalize* returns the trimmed, lowercased canonical form when known — callers should persist that returned value rather than the raw input.
- The pattern-to-enum mapping in TryParsePattern only covers the explicit names listed; adding new patterns requires updating this mapping if enum values are desired.
- Comparison/normalization uses ToLowerInvariant(), so behavior is culture-invariant.