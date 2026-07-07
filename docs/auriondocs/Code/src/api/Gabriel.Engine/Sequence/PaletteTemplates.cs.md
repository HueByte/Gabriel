# PaletteTemplates

> **File:** `src/api/Gabriel.Engine/Sequence/PaletteTemplates.cs`  
> **Kind:** class

```csharp
internal static class PaletteTemplates
```


PaletteTemplates is a small utility that defines a family of vivid color palettes used by the avatar generator. It stores named gradient templates (each with 2–4 color stops) and exposes helpers to pick and expand these templates into full palettes. A seed selects a single template to preserve a consistent visual personality across regenerations, while PickByName allows explicit overrides to honor project or conversation palette choices. ExpandTo turns a template into a 16-entry Palette by sampling the gradient evenly, producing a progression from a shadowy base to a bright accent. This abstraction keeps color decisions decoupled from rendering logic and guarantees coherent, reproducible visuals across the system. 

