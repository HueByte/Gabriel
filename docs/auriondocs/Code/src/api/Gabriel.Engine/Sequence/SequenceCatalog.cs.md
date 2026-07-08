# SequenceCatalog

> **File:** `src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs`  
> **Kind:** class

```csharp
public static class SequenceCatalog
```


SequenceCatalog is a static utility that enumerates valid pattern and palette identifiers used by a Project or standalone Conversation. It provides canonical, case-insensitive lists (Patterns and Palettes) and helpers to validate, normalize, and parse names into stable forms for persistence and interpretation. Unknown identifiers are silently ignored at generation time—the generator falls back to the seed-derived pick.

## Remarks
SequenceCatalog centralizes the vocabulary and decouples wire-identifiers from internal representations, enabling the catalog to grow without database migrations. NormalizePattern/NormalizePalette ensure inputs are persisted in canonical form, while TryParsePattern bridges string names to the PatternKind enum. Together, these members reduce duplication and error-prone casing or whitespace handling across the codebase.

## Example
```csharp
string input = " Plasma ";
var canonicalPattern = SequenceCatalog.NormalizePattern(input);
var kind = SequenceCatalog.TryParsePattern(input);

if (canonicalPattern != null && kind != null)
{
    // Persist canonicalPattern as the authoritative identifier or apply as an override
}
else
{
    // Fall back to seed-derived selection
}
```

## Notes
- NormalizePattern/NormalizePalette return null when the input is unknown; use TryParsePattern to map to PatternKind when appropriate. Unknown inputs are effectively ignored and trigger fallback behavior.
