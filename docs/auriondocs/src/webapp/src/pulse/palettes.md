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

Represents a named collection of RGB color stops.

Use this interface to declare or pass a palette that pairs a human-readable name with an array of RGB colors — for example when providing theming information, gradients, or color sets for visualizations.

## Notes
- The interface is a plain data shape: immutability, validation, and normalization are not enforced by the type itself.
- The order of entries in `stops` is preserved by the array; if consumers rely on a particular sequence (e.g., gradient progression), ensure the array is constructed in the required order.
- See the `RGB` type for the exact structure of each color stop.

---

## RGB

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** type

An immutable, fixed-length tuple representing a color as its red, green, and blue channel values in that order: [R, G, B]. Use this type when you need a compact, strongly-typed RGB triple (for palette entries, pixel values, conversions, etc.) and want the compiler to enforce a length of three and prevent in-place mutation.

## Remarks
This alias encodes both the ordering and arity of an RGB color (three numeric channels) while marking the tuple readonly to avoid accidental modification. Prefer this over number[] when the exact three-channel structure matters to callers and maintainers.

## Example
```typescript
const brightCyan: RGB = [0, 255, 255];

function toCssRgb(c: RGB): string {
  const [r, g, b] = c;
  return `rgb(${r}, ${g}, ${b})`;
}

const css = toCssRgb(brightCyan); // "rgb(0, 255, 255)"
```

## Notes
- The type only enforces three numeric components; it does not validate numeric ranges (e.g., 0–255 vs 0–1).
- Because the tuple is readonly, attempts to mutate elements (e.g., rgb[0] = 128) will be a type error; create a new tuple to represent a changed color.
- This type does not include an alpha channel; use a separate RGBA type if needed.

---

## brightestStop

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Selects and returns the RGB stop with the largest sum of its channels (R + G + B). Reach for this when you need a fast, simple heuristic for the "brightest" or most vivid color in a small palette; it is intentionally lightweight compared with perceptual luminance calculations.

## Remarks
The implementation uses the sum of the three channels as a cheap proxy for perceived brightness. This heuristic works well for the small curated palettes used in the project and preserves existing behavior for pulse palettes (where the brightest stop is typically the last one). The function is deterministic: among stops with equal sums the earliest one is returned.

## Example
```typescript
const palette: readonly [number, number, number][] = [
  [10, 20, 30],
  [200, 100, 50],
  [50, 50, 50]
];
const brightest = brightestStop(palette);
// brightest === [200, 100, 50]
```

## Notes
- The `stops` array must be non-empty; calling this with an empty array will throw because the function accesses `stops[0]`.
- Ties are broken by first occurrence: a later stop with an equal channel sum will not replace an earlier one.
- The function returns a reference to an element from the input array (not a copy); mutating the returned RGB will mutate the original palette.

---

## gradientCssFromStops

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Constructs a CSS linear-gradient string from an array of RGB color stops and is intended for producing a compact, visually clear left‑to‑right gradient for display in the web UI. Use this when you have a palette (an array of RGB triples) and need a single CSS background value rather than rendering many individual color blocks.

## Remarks
For palettes with many colors the function first sorts stops by simple brightness (sum of R, G and B) to ensure a clear dim→bright progression, then samples up to five representative stops (evenly taken across the sorted array) so the resulting gradient avoids looking muddy when dozens of colors are present. The produced gradient uses a 90deg direction (left→right) and places the sample colors at evenly spaced percentages across the gradient.

## Notes
- Empty input returns the literal string `transparent`; callers that expect a `linear-gradient(...)` should handle this case.
- A single stop returns that color string directly (no `linear-gradient`).
- Brightness is computed as R+G+B (not a perceptual luminance formula), and the function does not consider alpha channels; colors are reordered by brightness when there are multiple stops, which may differ from the original stop order.

---

## paletteAccent

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Returns the brightest color stop from a Palette and exposes it as an RGB value. Reach for this helper when you need a single, prominent accent color derived from a palette (for example, a UI highlight or primary accent) rather than manually inspecting the palette's stops.

## Remarks
This is a small convenience wrapper that delegates to brightestStop(palette.stops). It captures the common intent of "use the palette's most visually prominent color as an accent" without requiring callers to work directly with the stops array. It does not perform contrast adjustments or conversion to CSS strings — it simply returns the raw RGB value chosen by the brightness metric.

## Example
```typescript
const palette: Palette = {
  stops: [
    { r: 30, g: 120, b: 200 },
    { r: 255, g: 220, b: 0 }, // brightest in this example
  ],
};

const accent: RGB = paletteAccent(palette);
// use accent with your rendering code (convert to CSS string if needed)
```

## Notes
- If palette.stops is empty the result depends on the behavior of brightestStop; callers should ensure the palette contains at least one stop.
- The function returns an RGB value only; any alpha handling, contrast checks, or CSS formatting must be done by the caller.
- This function is pure and does not mutate the provided palette.

---

## paletteForSeed

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Returns a Palette chosen deterministically from a numeric seed. The function seeds an internal pseudo‑random generator (mulberry32) with the provided seed and forwards the generator plus the optional name to pickPalette; use this when you want reproducible palette selection derived from a numeric seed (for example, deterministic avatars or procedurally generated visuals).

## Remarks
This is a small convenience wrapper that hides the PRNG seeding and palette selection details — callers supply a simple numeric seed (and optionally a name) and receive a palette. It ensures the same seed (and same optional name) will produce the same Palette as long as the underlying mulberry32 and pickPalette implementations remain unchanged.

## Example
```typescript
// Get a palette from a numeric seed
const paletteA = paletteForSeed(12345);

// Use a name to bias or scope selection (name is forwarded to pickPalette)
const paletteB = paletteForSeed(12345, 'warm-theme');

// Reproducible: same seed and name => same palette
const paletteC = paletteForSeed(12345, 'warm-theme');
console.log(paletteB === paletteC); // true (assuming referential equality or equivalent comparison)
```

## Notes
- The reproducibility depends on the implementations of mulberry32 and pickPalette; changing either will change which palette a given seed produces.
- The seed is a JavaScript number and may be coerced by bitwise ops inside the PRNG; prefer using integer seeds for stable, repeatable results.
- The optional name is forwarded to pickPalette and may alter selection — include the same name when you need exact reproduction.

---

## paletteGradientCss

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Returns a CSS gradient string for the given Palette by delegating to gradientCssFromStops with the palette's stops. Use this when you have a Palette object and need a ready-to-use CSS background (or similar) value built from that palette's color stops.

## Remarks
This is a thin convenience wrapper that extracts the stops from a Palette and passes them to gradientCssFromStops. It centralizes the common operation of producing a CSS gradient from a Palette so callers do not need to access palette.stops themselves.

## Example
```typescript
// apply a palette gradient to an element's inline style
const el = document.getElementById('preview') as HTMLElement;
el.style.backgroundImage = paletteGradientCss(myPalette);
```

## Notes
- The function directly reads palette.stops and does not guard against a null/undefined palette; ensure the Palette instance is valid before calling.
- The returned string format and accepted structure of each stop are determined by gradientCssFromStops; pass stops in the expected shape.

---

## paletteVarsFromStops

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Generate CSS custom properties from an array of RGB color stops and return them as a plain object suitable for applying to an element or stylesheet. The returned record includes --palette-accent (the brightest stop as a CSS color), --palette-accent-soft (the same color with an alpha of 0.22), and --palette-gradient (a CSS gradient string composed from the provided stops). Use this when you need a small set of ready-to-use CSS variables derived from a palette's stops.

## Remarks
This function composes three helpers: brightestStop (to pick the accent color), rgbToCss (to convert an RGB value to a CSS color string, optionally with alpha), and gradientCssFromStops (to build the gradient CSS). It exists to centralize how palette-derived CSS variables are created so callers don't need to repeat the logic for selecting the accent, soft accent alpha, or gradient formatting.

## Example
```typescript
const stops: readonly RGB[] = [ { r: 10, g: 120, b: 200 }, { r: 230, g: 90, b: 40 } ];
const vars = paletteVarsFromStops(stops);
// Apply to an element
const el = document.querySelector('.card') as HTMLElement;
for (const [name, value] of Object.entries(vars)) {
  el.style.setProperty(name, value);
}
// In CSS you can then reference var(--palette-accent), etc.
```

## Notes
- The function expects a non-empty array of stops; behaviour with an empty array depends on brightestStop and may throw or produce undefined results.
- Returned keys are full CSS custom property names (including the leading "--"); set them directly with element.style.setProperty or include them in a style block.
- "--palette-accent-soft" uses a fixed alpha of 0.22. If you need a different translucency, compute it with rgbToCss yourself.

---

## pickPalette

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Returns a Palette from the global PALETTES list. If a name is provided and a palette with that exact name exists, it is returned; otherwise a palette is chosen at random using the supplied rng function. Use this when you want either an explicit named palette or a reproducible random choice driven by a caller-provided RNG.

## Remarks
This helper centralizes palette selection so callers can request a specific palette by name or rely on a deterministic random choice (by providing a seeded RNG). The function does not validate names beyond a strict equality match and delegates randomness entirely to the provided rng, making selection reproducible when rng is deterministic.

## Example
```typescript
// Simple deterministic RNG (LCG) for demonstration
let seed = 12345;
const rng = () => (seed = (seed * 1664525 + 1013904223) % 4294967296) / 4294967296;

// Random (but reproducible) selection:
const randomPalette = pickPalette(rng);

// Named selection (returns matching palette if present):
const namedPalette = pickPalette(rng, 'solarized');
```

## Notes
- Name lookup is case-sensitive and returns the first matching palette in PALETTES.
- If a name is provided but no matching palette is found, the function falls back to a random selection.
- The function assumes PALETTES is non-empty and that rng returns a number in the [0, 1) range; if PALETTES is empty or rng returns values outside that range, the result may be undefined or unexpected.
- For reproducible behavior, pass a deterministic RNG implementation rather than Math.random.

---

## rgbToCss

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Convert an RGB triplet and optional alpha into a CSS rgb() color string. For opaque colors (alpha >= 1) the returned string uses the plain rgb(...) form; for translucent colors (alpha < 1) it uses the modern CSS syntax that includes alpha after a slash: rgb(r g b / a). Use this when you need a small, consistent helper to turn an [r, g, b] array into a CSS-ready color token.

## Remarks
This function emits the space-separated rgb() syntax from CSS Color Module Level 4 and appends "/ alpha" when translucent. It performs no validation or clamping of channel or alpha values — the inputs are interpolated into the output string exactly as provided, and any normalization (e.g. ensuring channels are 0–255 or alpha is within 0–1) must be done by the caller.

## Example
```typescript
rgbToCss([255, 0, 0]);       // "rgb(255 0 0)"        — opaque red
rgbToCss([255, 0, 0], 0.5);  // "rgb(255 0 0 / 0.5)" — semi-transparent red
```

## Notes
- The function treats any alpha >= 1 as fully opaque (no alpha component in output).
- Outputs use modern CSS syntax (space-separated channels and optional slash-delimited alpha); if you need legacy comma-separated syntax or broader browser compatibility, produce a fallback separately.
- Floating-point channel or alpha values are included verbatim; call sites should round or clamp if required.

---

## sampleGradient

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Interpolates a color from an ordered array of RGB stop colors using a normalized parameter t in [0, 1]. Use this when you need a smooth, linear blend between two or more discrete RGB stops (for example to build a palette or animate a color along a gradient). The function clamps t to the [0,1] range, divides the stops into equal segments, finds the two surrounding stops for the clamped t, linearly interpolates each channel, and returns the resulting RGB as three rounded integer components.

## Remarks
This implements a simple piecewise-linear gradient over an array of RGB stops. Stops are treated as evenly spaced along the 0..1 domain — there is no per-stop position metadata. Rounding is applied to each channel, so the returned color components are integers (suitable for typical 0–255 RGB usage). The function is intentionally minimal and deterministic: it does not apply gamma correction or color-space transforms — interpolation is done directly on channel values.

## Example
```typescript
// Assuming RGB is defined as [number, number, number]
const stops: readonly [number, number, number][] = [
  [255, 0, 0],    // red at t=0
  [255, 255, 0],  // yellow at t=0.5 for 2-stop gradient
  [0, 255, 0],    // green at t=1
];

console.log(sampleGradient(stops, 0));   // -> [255, 0, 0]
console.log(sampleGradient(stops, 0.5)); // -> color halfway between red and green via yellow stop
console.log(sampleGradient(stops, 1));   // -> [0, 255, 0]
```

## Notes
- The function expects at least one stop; passing an empty stops array will throw (indexing error). Ensure stops.length >= 1 before calling.
- t is clamped to [0, 1]; values outside that range return the endpoint colors.
- Stops are assumed equally spaced; to represent non-uniform positions, precompute an array or use a different API.
- Interpolation is linear per channel and may produce perceptual artifacts compared with interpolation in a perceptually uniform color space.
- Each channel is rounded with Math.round; ensure input channels are in the expected numeric range (commonly 0–255) to avoid unexpected results.


---