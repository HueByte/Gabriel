# PaletteTemplates

> **File:** `src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs`  
> **Kind:** class

Provides the built-in named color-template library and utilities used to pick and expand vivid gradient-based palettes for Gabriel avatars. Reach for PaletteTemplates when you need a reproducible, seed-derived palette or when honoring an explicit palette name override; use Pick(long) for deterministic per-seed selection, PickByName(string) to honor an explicit override, and ExpandTo(Template, int) to turn a 2–4 stop template into a full Palette (default 16 entries).

## Remarks
This internal helper centralizes the palette definitions so the engine and the webapp stay visually coherent. Templates are authored as 2–4 bold RGB stops (dark → saturated mid → bright) to produce vivid, high-contrast palettes when sampled. Per-seed selection chooses exactly one template so a personality’s colors remain recognizable across regenerations; PickByName allows callers to override that behavior when an explicit project/conversation palette is requested.

## Example
```csharp
// Deterministic selection from a numeric seed and expansion to a 16-color palette
long seed = 123456789;
var template = PaletteTemplates.Pick(seed);
var palette = PaletteTemplates.ExpandTo(template); // 16-entry palette by default

// Honor an explicit override, falling back to the seed when unknown
var overrideTemplate = PaletteTemplates.PickByName("sunset") ?? PaletteTemplates.Pick(seed);
var overridePalette = PaletteTemplates.ExpandTo(overrideTemplate, size: 16);
```

## Notes
- PickByName returns null for null/empty/whitespace or when no template matches; callers should fall back to Pick(seed).
- Name matching is case-insensitive and trims surrounding whitespace.
- ExpandTo samples the template gradient evenly (default size = 16); the sampling clamps t to [0,1].
- Templates are stored as readonly data and the class is internal — intended for use inside the engine to keep visual behavior consistent across components.