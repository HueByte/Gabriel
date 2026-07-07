# SequenceCatalog

> **File:** `src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs`  
> **Kind:** class

```csharp
public static class SequenceCatalog
```


SequenceCatalog is a static catalog of known pattern and palette identifiers used when pinning a Sequence to a Project or Conversation. Its values are treated case-insensitively, canonicalized, and mapped to optional enum forms where possible; unknown identifiers are silently ignored at generation time and replaced by seed-derived picks.

## Remarks
SequenceCatalog centralizes the knowledge of valid sequence identifiers, isolating normalization and parsing from consuming code. It provides canonical, storage-friendly forms for persistence while still accepting user-provided strings. The separate Pattern and Palette collections enable independent curation, allowing the catalog to evolve without forcing migrations or reordering templates.

## Example
```csharp
string? userInput = GetUserInput("pattern");
if (SequenceCatalog.IsKnownPattern(userInput))
{
    string? canonical = SequenceCatalog.NormalizePattern(userInput); // e.g. "plasma"
    PatternKind? kind = SequenceCatalog.TryParsePattern(userInput); // PatternKind.Plasma
    // Use canonical and kind as needed
}
```

## Notes
- NormalizePattern/NormalizePalette return null for unknown identifiers; always handle null before persisting or using the result.
- IsKnownPattern/IsKnownPalette perform a case-insensitive check against canonical lists; inputs are treated as trimmed and lowered before comparison.
- Unknown identifiers are ignored during generation, which keeps data stable while permitting catalog growth over time.