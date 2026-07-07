# SequenceCatalog

> **File:** `src/api/Gabriel.Engine/Sequence/SequenceCatalog.cs`  
> **Kind:** class

```csharp
public static class SequenceCatalog
```


SequenceCatalog acts as the canonical registry of the identifiers clients may pin to a Project or standalone Conversation. It exposes two public, read-only lists: Patterns and Palettes, aligned with PatternKind values and PaletteTemplates.All, respectively, so the catalog can grow without breaking wire formats or migrations. It provides validation helpers (IsKnownPattern, IsKnownPalette), normalization helpers (NormalizePattern, NormalizePalette) that return canonical, storage-friendly forms (lowercased and trimmed) or null when unknown, and a parser (TryParsePattern) that maps a name to the corresponding PatternKind when possible. Unknown identifiers are silently ignored at generation time—the generator falls back to the seed-derived pick instead of forcing migrations. This centralizes knowledge of valid identifiers and keeps input variability isolated from storage and downstream rendering logic.