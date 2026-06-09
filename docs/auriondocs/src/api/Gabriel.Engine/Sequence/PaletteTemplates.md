# PaletteTemplates

> **File:** `src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs`  
> **Kind:** class

Provides a curated set of vivid color palette templates and helpers to select and expand them into a usable Palette. Use this when you need reproducible, personality-specific color palettes for the Gabriel sequence (or any avatar/visual element that should match the webapp's pulse/palettes.ts style). Prefer PaletteTemplates over ad-hoc color choices so the same seed or explicit template name yields consistent results across regenerations.

## Remarks
PaletteTemplates centralizes a small library of 2–4 stop gradient templates (stored in Template records) that are intentionally bold: dark near-black stops, saturated mids, and near-white highlights. Templates are chosen either deterministically from a numeric seed (Pick) so a personality's palette remains recognizable across regenerations, or by explicit name (PickByName) to honor project/conversation overrides. ExpandTo samples a template's gradient evenly to produce a Palette (default 16 entries) where index 0 is the darkest shadow and the last index is the brightest accent.

## Example
```csharp
// Pick a palette deterministically from a seed and expand to 16 colors
var template = PaletteTemplates.Pick(seed: 12345L);
var palette = PaletteTemplates.ExpandTo(template); // default size = 16

// Honor an explicit override name if present, otherwise fall back to seed
var overrideName = project.PaletteOverride; // string? possibly null/whitespace
var chosen = PaletteTemplates.PickByName(overrideName) ?? PaletteTemplates.Pick(projectSeed);
var expanded = PaletteTemplates.ExpandTo(chosen, size: 16);
```

## Notes
- PickByName returns null for null/empty/whitespace input or if no template matches; callers should fall back to a seed-derived Pick.
- Name matching in PickByName is case-insensitive and trims surrounding whitespace.
- ExpandTo samples the gradient evenly; the default size is 16 and ordering is from shadow (index 0) to brightest accent (index size-1).
- Pick folds the provided 64-bit seed into a 32-bit value (using XOR with its high bits and a constant) before indexing the templates; different seeds may map to the same template due to this reduction.
- The All array is static and readonly; templates are immutable records, so reading is safe without synchronization.