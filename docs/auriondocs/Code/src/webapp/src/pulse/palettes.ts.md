# palettes.ts

> **Source:** `src/webapp/src/pulse/palettes.ts`

## Contents

- [Palette](#palette)
- [RGB](#rgb)
- [brightestStop](#brighteststop)
- [gradientCssFromStops](#gradientcssfromstops)
- [paletteAccent](#paletteaccent)
- [paletteForSeed](#paletteforseed)
- [paletteGradientCss](#palettegradientcss)
- [paletteVarsFromStops](#palettevarsfromstops)
- [pickPalette](#pickpalette)
- [rgbToCss](#rgbtocss)
- [sampleGradient](#samplegradient)

---

## Palette
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** interface

```typescript
export interface Palette
```


Palette is a lightweight data contract that pairs a human-friendly name with a sequence of RGB color stops. It serves as a simple, reusable carrier for color palettes that can be consumed by theming, charts, or gradient-generation logic without scattering individual color values and labels throughout the codebase.

## Remarks
Palettes act as a boundary between color data and UI presentation, enabling components to switch palettes or render different ramps without changing call sites. They also support serialization and tooling that enumerates available palettes. Because Palette is only a data shape, it carries no behavior—consumers implement the logic that uses its stops for rendering or interpolation.

## Example
```typescript
const sunsetPalette: Palette = {
  name: 'Sunset',
  stops: [
    { r: 255, g: 94, b: 58 },
    { r: 255, g: 176, b: 56 },
    { r: 255, g: 214, b: 102 }
  ]
};
```

## Notes
- Mutating the stops array or the RGB color objects after creation will affect all references to that palette; prefer creating a new Palette or cloning to preserve immutability when sharing.
- The interface does not enforce runtime validation (e.g., non-empty stops or valid color ranges); downstream code should enforce any invariants required by the application.

---

## RGB
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** type alias

```typescript
export type RGB = readonly [number, number, number]
```


RGB is a type alias for a readonly 3-tuple of numbers representing the red, green, and blue components in that fixed order. Use it when you need a color value that cannot be mutated through the container, and you want to enforce the exact length and component ordering at compile time rather than using a mutable array.

## Remarks

Modeling RGB as a readonly tuple communicates a strict color-contract across APIs: a three-component, ordered RGB value that should not be altered in place. This improves clarity and safety when colors flow through functions and data structures. Note that the immutability here is at the TypeScript type level; at runtime the underlying value is still a normal array unless you explicitly freeze it. This type pairs well with palette-oriented code by making color data explicit in function signatures and data models.

## Example

```typescript
// Common usage
const color: RGB = [255, 0, 128];
// color[0] = 100; // Error: Cannot assign to '0' because it is a read-only property
```

```typescript
// Returning a new color while keeping RGB semantics
function brighten(color: RGB, amount: number): RGB {
  const [r, g, b] = color;
  return [Math.min(255, r + amount), Math.min(255, g + amount), Math.min(255, b + amount)];
}
```

## Notes

- Immutability is compile-time only; runtime mutation of the underlying array is still possible unless the value is frozen or wrapped. 
- There is no built-in enforcement of channel ranges (e.g., 0–255); validate values as needed in your domain logic. 
- When constructing or transforming colors, prefer returning a new RGB value (instead of mutating an existing one) to preserve the immutable contract.

---

## brightestStop
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function brightestStop(stops: readonly RGB[]): RGB
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `stops` | `readonly RGB[]` | — |

**Returns:** `RGB`


brightestStop picks the color stop with the largest sum of its RGB channels, using that sum as a lightweight proxy for luminance to identify the most vivid color in a palette. It is useful when you want a single, representative bright color from a list of stops (for example, client or server palettes) without implementing a full perceptual luminance calculation, and it remains fast for small to moderately sized palettes. The function assumes a non-empty input and begins comparisons with the first element.

## Remarks

By centralizing the 'brightest' choice, this function provides a consistent selection rule across palettes and keeps palette generation decoupled from UI presentation. It aligns with the project’s approach where simple, fast heuristics are preferred for palette selection, and it preserves existing behavior for pulse palettes, where the last stop is already brightest.

## Example

```typescript
const stops: readonly RGB[] = [[10, 20, 30], [120, 110, 100], [200, 180, 160]];
const best = brightestStop(stops);
// best === [200, 180, 160]
```

## Notes

- Assumes non-empty input; an empty array will throw when accessing stops[0].

- The sum-based luminance proxy is naive and does not account for gamma correction or perceptual luminance; for precise brightness handling, replace with a proper luminance calculation.

- Time complexity is O(n) with a single pass; the memory footprint is O(1) beyond input storage.

---

## gradientCssFromStops
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function gradientCssFromStops(stops: readonly RGB[]): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `stops` | `readonly RGB[]` | — |

**Returns:** `string`


Converts an array of RGB color stops into a CSS linear-gradient string suitable for use in UI palettes. It gracefully handles edge cases: no stops yields transparent, a single stop yields that color, and multiple stops produce a left-to-right gradient built from a compact, representative subset of the colors.

## Remarks
This abstraction gives you a predictable gradient that reflects a palette's dim-to-bright progression without being overwhelmed by every color. By sorting stops by brightness and sampling up to five evenly distributed colors, it preserves a meaningful gradient even for large palettes. It delegates color formatting to rgbToCss to keep CSS output consistent across the codebase. It is designed for palette rendering in the UI where a concise, legible gradient helps convey the overall mood of a palette.

## Example
```typescript
type RGB = [number, number, number];
const stops: RGB[] = [
  [12, 34, 56],
  [128, 200, 60],
  [255, 180, 0],
];

const css = gradientCssFromStops(stops);
console.log(css);
// linear-gradient(90deg, rgb(12,34,56) 0%, rgb(128,200,60) 50%, rgb(255,180,0) 100%)
```

## Notes
- Input stops are not mutated; a shallow copy is sorted to compute the gradient.
- If there are more than five stops, a representative subset is used, which may omit some colors from the final gradient.

---

## paletteAccent
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function paletteAccent(palette: Palette): RGB
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `palette` | `Palette` | — |

**Returns:** `RGB`


paletteAccent returns the RGB color that represents the palette's accent by selecting the brightest stop from the given Palette's stops. Use it when you want a consistent highlight color sourced from the palette itself instead of picking a fixed RGB value.

## Remarks
By delegating to brightestStop, paletteAccent encapsulates the notion of "accent color" as the visually strongest color in the palette. This helps keep UI theming cohesive: changing a palette's stops automatically influences the accent color wherever it's used, without touching the usage sites.

## Example
```typescript
// Common usage: derive an accent color from a palette
const palette: Palette = { stops: [
  { r: 255, g: 0, b: 0 },
  { r: 0, g: 128, b: 255 },
] };
const accent: RGB = paletteAccent(palette);
```

## Notes
- If palette.stops is empty, paletteAccent cannot determine a brightest stop; ensure the palette contains at least one stop.

---

## paletteForSeed
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function paletteForSeed(seed: number, name?: string): Palette
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `seed` | `number` | — |
| `name` | `string` | — |

**Returns:** `Palette`


paletteForSeed is a small convenience wrapper that deterministically returns a Palette from a numeric seed (and an optional name). It constructs a seeded RNG by calling mulberry32(seed) and passes that generator to pickPalette to obtain a Palette.

## Remarks
This abstraction centralizes seed-based palette generation and ensures repeatable palettes for the same seed, which is handy for theming, testing, or user-specific color schemes. It hides the details of RNG seeding and palette selection behind a simple API; callers only supply a seed (and optionally a name).

## Example
```ts
import { paletteForSeed } from './palettes';
const oceanPalette = paletteForSeed(1234, 'ocean');
```

## Notes
- Same seed and name will always produce the same Palette.
- The optional name is forwarded to pickPalette and may influence the final palette depending on its implementation.
- If you need non-deterministic palettes, avoid this helper and use a non-seed-based random source.

---

## paletteGradientCss
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function paletteGradientCss(palette: Palette): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `palette` | `Palette` | — |

**Returns:** `string`


This function returns a CSS gradient string by delegating to gradientCssFromStops with the provided palette’s stops. It serves as a small adapter to turn a Palette into a ready-to-use CSS gradient, so UI code can apply the palette’s gradient without reimplementing stop-to-string formatting.

## Remarks
PaletteGradientCss acts as a boundary between the Palette data model and visual styling. By centralizing gradient string generation, it guarantees consistent output wherever a Palette is rendered and simplifies testing by exposing a single entry point for gradient construction.

## Notes
- Pure function: no mutation; output depends only on input.
- Delegation: formatting logic lives in gradientCssFromStops; updates there affect all callers.
- Validation: TypeScript typings help catch missing or malformed palettes at compile time; at runtime, gradientCssFromStops will determine behavior for unexpected input.

---

## paletteVarsFromStops
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function paletteVarsFromStops(stops: readonly RGB[]): Record<string, string>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `stops` | `readonly RGB[]` | — |

**Returns:** `Record<string, string>`


Converts an array of RGB color stops into a compact CSS palette by selecting the brightest stop as the accent color and exposing three CSS variables: --palette-accent for the solid accent, --palette-accent-soft for a translucent variant, and --palette-gradient derived from the stops for gradient usage. Use this when you want a consistent, themeable set of CSS variables derived from a color-stop array without duplicating color-conversion logic in your styling code.

## Remarks
Palette creation logic is centralized here, ensuring consistent color interpretation across the UI. By exposing CSS variable names, it encourages downstream styles to reference the same palette rather than computing colors inline, which improves maintainability and theming flexibility. It also encapsulates the color math (brightest stop selection, gradient generation) behind a small, reusable surface. If you later change how stops are chosen or how the gradient is generated, you only update this function.

## Example
```typescript
type RGB = { r: number; g: number; b: number };

const stops: readonly RGB[] = [
  { r: 255, g: 45, b: 0 },
  { r: 0, g: 128, b: 255 },
  { r: 255, g: 255, b: 0 }
];

const vars = paletteVarsFromStops(stops);
Object.assign(document.documentElement.style, vars);
```

## Notes
- Ensure stops is non-empty; calling brightestStop on an empty array may throw. 
- The returned values are CSS color strings appropriate for applying as CSS variables (e.g., on style or root elements). 
- The --palette-accent-soft value uses 22% opacity, suitable for translucent accents and overlays.

---

## pickPalette
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function pickPalette(rng: Rand, name?: string): Palette
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `rng` | `Rand` | — |
| `name` | `string` | — |

**Returns:** `Palette`


pickPalette returns a Palette by either a requested name or, if no match exists, a random choice from PALETTES using the provided RNG. If a name is supplied and a palette with that name exists in PALETTES, that palette is returned; otherwise, the function selects a random palette from the array using Math.floor(rng() * PALETTES.length).

## Remarks
Centralizes palette selection logic to keep theming consistent across the UI. It supports deterministic outcomes by using the supplied RNG, enabling reproducible palettes for a given seed. It also allows direct control by name when a specific palette is desired.

## Notes
- If PALETTES is empty, the final access PALETTES[0] may yield undefined, which could lead to a runtime error.
- If multiple palettes share the same name, only the first match is returned.

---

## rgbToCss
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function rgbToCss([r, g, b]: RGB, alpha = 1): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `[r, g, b]` | `RGB` | — |
| `alpha` | — | `1` |

**Returns:** `string`


Converts a numeric RGB triple into a CSS color string. Given an RGB tuple [r, g, b], rgbToCss returns a string formatted for CSS. If the alpha argument is 1 (the default) or greater, the result is rgb(r g b). If alpha is less than 1, it uses the CSS Color Module Level 4 slash syntax rgb(r g b / a) to include transparency. The function uses spaces between color components (e.g. rgb(255 0 128)) rather than comma-separated values.

## Remarks
This helper centralizes color formatting for UI palettes that store colors as numeric RGB triples. By returning CSS-ready strings, it keeps rendering code simple and consistent across components, avoiding scattered string construction logic. The behavior around alpha makes it easy to opt into transparency only when a fractional alpha is explicitly provided.

## Example
```typescript
// Basic opaque color
rgbToCss([255, 0, 128]); // "rgb(255 0 128)"

// Color with transparency
rgbToCss([12, 34, 56], 0.5); // "rgb(12 34 56 / 0.5)"
```

## Notes
- Alpha values >= 1 are treated as opaque; transparency is only applied when alpha < 1. If you need real transparency, pass a value in [0,1].
- No validation is performed on the color channels or alpha; callers should ensure r, g, b are in 0–255 and alpha is in a meaningful range for your use case.

---

## sampleGradient
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

```typescript
export function sampleGradient(stops: readonly RGB[], t: number): RGB
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `stops` | `readonly RGB[]` | — |
| `t` | `number` | — |

**Returns:** `RGB`


Computes the color value at a point t along a piecewise-linear gradient defined by the provided stops. The input t is clamped to the range [0,1], and the function linearly interpolates between consecutive RGB stops to produce a rounded RGB triplet. If there is only a single stop, that color is returned unchanged. This utility is useful when you need a deterministic color from a gradient without implementing interpolation yourself.

## Remarks
This helper encapsulates gradient evaluation so callers can map a scalar progress value to a color without reimplementing interpolation math. It outputs integer RGB channels by rounding, ensuring compatibility with common 0–255 color representations. The gradient is assumed to be defined by an ordered sequence of stops with equal-length segments between consecutive stops.

## Notes
- Empty stops arrays are not supported; at least one stop must be provided. An empty array would lead to a division by zero when computing segment length. 
- RGB values outside the 0–255 range may be produced if the input stops contain out-of-range numbers; clamp if you need strict bounds.

## Example
```typescript
// Common usage: map a 0..1 progress value to a gradient color
type RGB = [number, number, number];
const stops: readonly RGB[] = [
  [0, 0, 0],
  [255, 0, 0],
  [255, 255, 0],
  [255, 255, 255]
];
const color = sampleGradient(stops, 0.5); // interpolates between stops[1] and stops[2]
```


---