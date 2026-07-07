# PaletteTemplates

> **File:** `src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs`  
> **Kind:** class

```csharp
internal static class PaletteTemplates
```


PaletteTemplates is an internal static helper that defines a catalog of named color-gradient templates used to drive the avatar color system in Gabriel. Each Template holds a small gradient (2–4 RGB stops) which the code expands into a 16-entry Palette to keep visuals coherent with the web app and the procedural avatar. Pick(seed) deterministically selects a template from the catalog by folding the seed, ensuring the same personality’s palette remains recognizable across regenerations of its Live State. PickByName(name) enables explicit overrides by name, honoring PaletteOverride values when provided. ExpandTo(template, size = 16) expands a Template into a Palette by sampling the gradient evenly across the requested size; palette[0] is the quiescent shadow and palette[15] (the brightest) is the accent, producing a vivid, high-contrast color ramp. The stops are intentionally bold to push the avatar toward vivid color rather than muted tones and to maintain visual consistency with the app’s palette philosophy.

## Remarks
PaletteTemplates decouples color identity from frame-generation logic, centralizing palette decisions so different seeds or personalities can share perceptually related palettes while remaining distinct. It enables deterministic yet override-friendly palette selection: seeds drive identity, while explicit names can enforce a particular aesthetic when needed.

## Example
```csharp
var seed = 123456789L;
var template = PaletteTemplates.Pick(seed);
var palette = PaletteTemplates.ExpandTo(template); // 16-entry Palette
```

## Notes
- ExpandTo accepts a custom size; the default 16 entries are chosen to align with the avatar rendering surface. 
- If a template has only two stops, interpolation still produces a smooth ramp across all 16 entries. 
- Use PickByName to honor explicit palette overrides; if it returns null, fall back to Pick(seed).
