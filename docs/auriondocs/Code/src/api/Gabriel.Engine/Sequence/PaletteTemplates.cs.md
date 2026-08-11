# PaletteTemplates

> **File:** `src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs`  
> **Kind:** class

```csharp
internal static class PaletteTemplates
```


PaletteTemplates centralizes the palette logic used to render vivid, personality-driven color palettes for avatars. It defines a set of named 2–4-stop gradients and provides utilities to deterministically pick a template from a seed and to expand that template into a full 16-color palette for rendering. The seed-based Pick ensures a given seed yields the same appearance across regenerations, while PickByName enables explicit overrides to honor PaletteOverride values. ExpandTo samples the chosen template's gradient evenly to produce an ordered palette where index 0 is the quiescent shadow and index 15 is the brightest accent, enabling consistent visuals with the Gabriel webapp's pulse/palettes.ts shape.

## Remarks
PaletteTemplates acts as a single source of truth for avatar color decisions, decoupling palette generation from rendering logic. This separation ensures a consistent visual language across regenerations and UI components, and makes it straightforward to tweak gradient stops or interpolation without touching rendering code. The PickByName path allows explicit palette overrides (e.g., per Project or Conversation overrides) without altering the main seed-driven flow.

## Example
```csharp
// Common usage: pick a palette by seed and expand it to 16 colors
var template = PaletteTemplates.Pick(12345L);
var palette = PaletteTemplates.ExpandTo(template);
var first = palette[0];                 // quiescent shadow color
var last = palette[palette.Count - 1];   // brightest accent color

// Explicit override by name (if available)
var named = PaletteTemplates.PickByName("heat");
if (named != null)
{
    var p = PaletteTemplates.ExpandTo(named);
}
```

## Notes
- ExpandTo supports 2–4 stop templates and expands them to size 16 by sampling the gradient evenly. If a template has a single stop, expansion returns that color for all entries.
- Pick mixes the seed with a deterministic scramble (folded seed) to avoid shadowing other seed-derived state, ensuring palette selection remains stable per-seed while not interfering with frame-pattern seeds.
- PickByName returns null when no matching template exists (caller should fall back to seed-based Pick).