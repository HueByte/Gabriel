# SequenceCatalog

> **File:** `src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs`  
> **Kind:** class

Provides a small, curated catalog of canonical pattern and palette identifiers that clients may pin to a Project or a standalone Conversation. Use this when validating, normalizing, or parsing user-supplied pattern/palette strings so stored values are trimmed, lowercased, and limited to the known set rather than relying on raw input.

## Remarks
The catalog stores identifiers as case-insensitive strings (not enum values on the wire) so new identifiers can be added without database migrations. Patterns and Palettes are kept as readonly lists; Palette entries are curated independently of any template definition order to allow stable, human-friendly ordering. Callers are expected to persist the normalized (trimmed + lowercased) form returned by NormalizePattern/NormalizePalette so database rows always carry the canonical identifier.

## Example
```csharp
// Normalize input, check membership, and parse to the PatternKind enum if possible
string? raw = " Plasma ";
if (SequenceCatalog.IsKnownPattern(raw))
{
    string canonical = SequenceCatalog.NormalizePattern(raw)!; // "plasma"
    PatternKind? kind = SequenceCatalog.TryParsePattern(raw);
    if (kind != null)
    {
        // use the enum (generator will accept this override)
    }
    else
    {
        // unknown to enum: generator should fall back to seed-derived choice
    }
}

// Palette check
string? palette = "Sunset";
string? normalizedPalette = SequenceCatalog.NormalizePalette(palette); // "sunset" or null
```

## Notes
- All public helpers accept null/whitespace inputs and return false/null rather than throwing; check return values before using them.
- Normalization uses Trim() + ToLowerInvariant(); the canonical form is culture-invariant and should be stored by callers.
- Patterns/Palettes are stored as small arrays and membership checks use linear Contains scans; this is fine for the current small catalog but could be revisited if the lists grow large.
- Unknown identifiers are intentionally ignored at generation time; when an override doesn't parse to a PatternKind the generator falls back to a seed-derived pick.