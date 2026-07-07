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


Palette defines a named color sequence as an array of RGB colors. It is an interface that groups a name with stops: RGB[]. Use it when you need to pass around a palette as a single unit for theming, charts, or UI components, rather than scattering individual colors.

## Remarks
Palette acts as a lightweight data contract between data/config and rendering code. By exposing a stable shape with a name and a stops array, it enables palettes to be defined once and reused across components. The stops are ordered RGB colors; the order matters for gradients or step-wise color schemes, and consumers can translate these stops into the rendering API that expects RGB triplets.

## Notes
- This is a pure data contract; there is no behavior or methods encoded in the interface.
- Consumers should validate the stops array and the RGB triplets at or before use, as the interface itself does not enforce color-range constraints.
- As an interface, implementations may vary in how the RGB data is stored or produced, but they must conform to the named palette shape described here.

---

## RGB
> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** type alias

```typescript
export type RGB = readonly [number, number, number]
```


RGB is a type alias for a readonly three-element tuple that encodes a red-green-blue color as numbers. It models colors as a fixed-length, immutable triplet [red, green, blue], making color values safer to pass around than a mutable array.

## Remarks
RGB exists to model colors as a distinct, strongly-typed value rather than a generic numeric array. By enforcing a 3-component shape and immutability, it provides a clear contract for functions that consume colors and reduces the risk of accidental mutation or misordered components.

## Example
```typescript
const color: RGB = [255, 0, 128] as const;
console.log(`rgb(${color[0]}, ${color[1]}, ${color[2]})`); // rgb(255, 0, 128)
```

## Notes
- Mutating a value of type RGB is not allowed: attempts like `color[0] = 128` will produce a compile-time error due to the readonly tuple.
- If constructing an RGB value from a mutable array, you may need to cast to a readonly tuple (e.g., via `as const`) to satisfy the type.


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


brightestStop selects a single RGB stop from a list by maximizing the sum of its red, green, and blue components. This cheap proxy for luminance yields sensible, vivid picks for the small curated palettes shipped with the client and for the larger palettes the server emits. The implementation is a simple linear scan: it starts with the first stop as the current best and replaces it whenever it encounters a stop with a larger channel sum. Because the comparison uses a strict greater-than, ties preserve the earlier stop in the array. The function assumes a non-empty input; callers should ensure there is at least one stop to avoid a runtime error. In pulse palettes the last stop is already brightest, so this routine preserves that established behavior.

## Remarks
brightestStop acts as a tiny palette-utility: it provides a fast, deterministic way to pick a representative color without engaging a perceptual color model. It keeps palette processing lightweight while delivering a result that aligns with intuition for vivid colors in common palettes. The approach is intentionally simple and dependency-free, which makes it suitable for use in rendering previews or quick snapshot analyses of color stops.

## Notes
- Input must be non-empty; an empty array results in a runtime error when indexing the first element.
- Tie-breaking favors the earliest stop with the maximum sum because the comparison is strict (`>`).


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


Converts an array of RGB color stops into a CSS linear-gradient string. It gracefully handles edge cases (no stops yield the string 'transparent'; a single stop yields that color via rgbToCss) and, for multiple stops, sorts the colors by the sum of their channels to establish a brightness order, then samples up to five representative colors to form a left-to-right gradient with a clear dim-to-bright progression.

When there are more than five colors, the function selects a subset using the positions 0, 0.25, 0.5, 0.75, and 1 across the brightness-sorted list, converts each sampled color to CSS via rgbToCss, and assigns evenly spaced percentages from 0% to 100%. The final string is a 90-degree linear gradient, e.g. linear-gradient(90deg, color1 0%, color2 25%, …).

## Remarks
To avoid muddy gradients when a palette contains many stops, the function ignores the original input order and instead uses a brightness-ordered sequence with a compact, representative subset. This guarantees a perceptible left-to-right progression from darker to brighter hues while keeping the produced CSS concise. rgbToCss is delegated the color formatting responsibility, while this function orchestrates the gradient geometry and stop placement.

## Notes
- Empty input results in 'transparent' instead of a gradient.
- The original order of stops is not preserved in the output gradient; the gradient reflects a brightness-ordered sampling rather than the input sequence.

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


paletteAccent selects the accent color for a given Palette by returning the brightest color stop among the palette's stops. It provides a concise, intention-revealing way to derive an RGB color for UI accents from a palette instead of picking a color manually.

## Remarks
This is a small wrapper around brightestStop(palette.stops). By exposing paletteAccent, callers express intent clearly: use the palette's most vibrant color as an accent. It also centralizes theming behavior so changes to how brightness is computed or how stops are stored stay localized to this abstraction.

## Notes
- If the palette has no stops, brightestStop behavior may fail; ensure at least one stop exists before calling paletteAccent.
- The result depends on the brightness definition used by brightestStop; a different brightness metric would yield a different accent color.

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


paletteForSeed returns a deterministic Palette by seeding a Mulberry32 PRNG with the provided seed and delegating to pickPalette, with an optional name. Use it when you need repeatable palettes tied to a numeric seed rather than generating a new palette on every call.

## Remarks
Conceptually, this function hides the details of the PRNG and palette selection behind the Palette interface. It composes two collaborators—mulberry32 for seeded randomness and pickPalette for palette construction—so callers can rely on a stable Palette instance for a given seed. Because the returned object implements the Palette API (including Count and an indexer), you can inspect its size or sample colors without knowing how the palette was produced.

## Example
```typescript
const p = paletteForSeed(42, 'Demo');
console.log(p.Count);
const first = p[0];
```

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


paletteGradientCss converts a Palette into a CSS gradient string by delegating to gradientCssFromStops using the palette.stops. This thin adapter reuses the central stop-to-css logic without duplicating formatting code in callers.

## Remarks
Palette is a sequence-like type representing color stops. This function acts as a small adapter wiring a Palette into the shared CSS-generation routine gradientCssFromStops, keeping the TypeScript-facing API compact while reusing the existing gradient formatting logic.

## Notes
- Potential API drift: The Palette type surface in dependencies lists Count and an indexer; if stops is not publicly exposed, or if the public API changes, this wrapper may need to be updated to align with the actual Palette interface.
- If stops are empty or missing, gradientCssFromStops behavior applies; ensure to handle edge cases at call sites or adjust accordingly.
- The function relies on gradientCssFromStops's contract for how stops are formatted into CSS.

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


The function paletteVarsFromStops takes a sequence of color stops and derives a small set of CSS variables that represent a cohesive palette. It selects the brightest stop as the accent color, exposes that color as --palette-accent, provides a softer variant via --palette-accent-soft, and also computes a gradient string from the stops for --palette-gradient. This encapsulates palette derivation logic in a single helper, making it easy for UI components to apply consistent colors without duplicating the color selection and formatting logic.

## Remarks
This abstraction centralizes how a color palette is produced from a list of stops. By consistently choosing the brightest stop as the accent and deriving a softer variant and a gradient from the same input, it ensures visual cohesion across components that consume the palette. It also hides the formatting details (rgbToCss and gradient construction) behind a single surface, reducing duplication and potential drift in how colors are represented in CSS.

## Notes
- Assumes a non-empty stops input; there is no explicit handling for an empty array, so callers should provide a valid stops array to avoid undefined CSS values.

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


pickPalette selects a Palette from the central PALETTES catalog. If you pass a name and a matching palette exists, that palette is returned; otherwise it falls back to selecting a random palette from PALETTES using the provided RNG function. If no name is supplied, or the named palette cannot be found, a random entry from PALETTES is returned via rng().

## Remarks
By encapsulating the lookup and the random fallback behind a single function, callers don’t need to know how PALETTES is stored or enumerated. The outcome is deterministic only if you supply a deterministic RNG, which helps in tests and demonstrations. The policy is simple and consistent: prefer a named palette when available, otherwise choose randomly from the catalog.

## Example
```typescript
// Named palette lookup
const pNamed = pickPalette(Math.random, 'Sunset');

// Random palette (no name provided or name not found)
const pRandom = pickPalette(Math.random);
```

## Notes
- If a non-existent name is provided, the function falls back to a random selection rather than throwing.
- The function assumes PALETTES is non-empty; if PALETTES.length is 0, indexing PALETTES[...] could yield undefined at runtime. Ensure the palette catalog is populated before use.

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


rgbToCss converts an RGB triplet and an optional alpha channel into a CSS color string. It accepts an RGB tuple [r, g, b] and an optional alpha that defaults to 1 (opaque). If alpha is at least 1, it emits rgb(r g b); otherwise, it emits rgb(r g b / alpha) for translucent colors.

## Remarks
This tiny helper centralizes CSS color formatting in the web UI palettes. It bridges the internal RGB representation to a CSS-ready string, so callers don't manually craft color literals when rendering palettes or inline styles. It keeps the surface area minimal by accepting a plain RGB tuple.

## Example
```typescript
rgbToCss([255, 0, 0]); // "rgb(255 0 0)"
rgbToCss([0, 128, 255], 0.5); // "rgb(0 128 255 / 0.5)"
```

## Notes
- Alpha values >= 1 are treated as opaque; the alpha component is ignored in the output.
- No runtime validation is performed on the input channels; ensure r, g, b are 0–255 and alpha is in a reasonable range for your use case.
- The translucent form uses the CSS rgb(r g b / a) syntax, which is supported by modern browsers; verify compatibility if targeting older environments.

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


SampleGradient computes the color at position t along a piecewise-linear gradient defined by the provided color stops. It treats the stops as a gradient from the first to the last color and returns an RGB color by linearly interpolating between adjacent stops. The parameter t is clamped to the range [0, 1], so values outside that range yield the nearest endpoint. If there is only a single stop, that color is returned unchanged.

## Remarks
This function centralizes gradient evaluation, ensuring consistent color computation across consumers that render palettes or heatmaps. It operates on a non-empty sequence of stops and uses simple linear interpolation between neighboring stops, rounding each channel to the nearest integer to produce valid 0–255 RGB values. Be aware that the behavior is defined for any number of stops greater than zero; the exact contour of the gradient depends on the provided stop colors.

## Example
```typescript
// RGB is a [r, g, b] tuple used by the codebase
const stops: readonly RGB[] = [
  [255, 0, 0],   // red
  [0, 255, 0],   // green
  [0, 0, 255]    // blue
];
const t = 0.25;
const color = sampleGradient(stops, t);
// color is approximately [128, 128, 0] (between red and green)
```

## Notes
- Empty stops array is invalid and will lead to a runtime error; ensure at least one stop is provided.
- If t is NaN or not a finite number, the result may be NaN or otherwise invalid due to the internal calculations.


---