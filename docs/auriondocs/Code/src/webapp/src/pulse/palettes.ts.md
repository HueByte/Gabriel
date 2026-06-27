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

Represents a named collection of RGB color stops. Use this interface when you need to pass or persist a reusable, ordered set of colors (for themes, gradients, charts, etc.) rather than working with loose color values.

## Remarks
A lightweight data contract that couples a human-readable name with an array of RGB entries. The interface intentionally leaves interpretation of the stops (their ordering, interpolation rules, or required length) to consumers so it can be used in multiple contexts without enforcing rendering semantics.

## Example
```typescript
const primary: RGB = /* existing RGB value */;
const secondary: RGB = /* existing RGB value */;

const palette: Palette = {
  name: 'Sunset',
  stops: [primary, secondary]
};
```

## Notes
- The interface provides no runtime validation: callers should validate stop count and component ranges if required.
- Objects typed as Palette are mutable by default; freeze or copy them if immutability is needed.
- The meaning of the stops array (e.g., whether it is interpolated or sampled) is defined by the consumer, not by this type.

---

## RGB

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** type

Represents an RGB color as an immutable 3-tuple [R, G, B]. Use this type when a compact, ordered color value is needed (for palettes, pixel manipulation, or passing colors between functions) and you want the compiler to enforce a fixed length and immutability rather than using an object or class. The tuple order is red, green, then blue.

## Remarks
Using a readonly tuple enforces at the type level that every RGB value contains exactly three numeric channels and prevents accidental in-place mutation. This keeps color values lightweight and easy to destructure or compare element-wise without the overhead of a dedicated Color class.

## Example
```typescript
const orange: RGB = [255, 128, 0];
const [r, g, b] = orange;
const css = `rgb(${r}, ${g}, ${b})`; // "rgb(255, 128, 0)"

// Convert to normalized floats in range 0..1
const normalized: RGB = [r / 255, g / 255, b / 255];
```

## Notes
- This type does not enforce numeric ranges; callers should agree on and validate whether values are 0–255, 0–1, or some other range.
- Values may be integers or floats depending on context; the type only requires number.
- The tuple is readonly: attempting to mutate elements (e.g. orange[0] = 128) will be a type error without an explicit cast.
- Array identity (===) does not perform element-wise equality; compare channels when testing color equality.

---

## brightestStop

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Returns the RGB stop (one of the entries in the input array) whose red+green+blue channel sum is largest. Reach for this when you need a cheap, fast heuristic for the "most vivid" or brightest color in a small palette — it is intentionally simple and tuned for the curated and server-generated palettes used by the app.

## Remarks
The function uses a sum-of-channels heuristic (R+G+B) as a light-weight proxy for brightness. This is cheaper than a perceptual luminance calculation and produces sensible results for the palettes this code targets; it also preserves existing behavior for the pulse palettes where the last stop is already expected to be the brightest.

The function returns a reference to the RGB tuple from the original array (not a copy) and chooses the first stop encountered when two stops have equal channel sums.

## Example
```typescript
const palette: readonly RGB[] = [ [10, 20, 30], [200, 50, 10], [180, 180, 0] ];
const brightest = brightestStop(palette);
// brightest is one of the entries from `palette` with the largest R+G+B sum
console.log(brightest); // e.g. [180, 180, 0]
```

## Notes
- The function assumes `stops` is non-empty; calling it with an empty array will throw (attempts to read `stops[0]`).
- The R+G+B sum is not a perceptually accurate luminance measure (it does not weight green more than red/blue). Use a proper luminance formula if perceptual accuracy is required.
- Because the function returns the original array element, mutating the returned RGB will mutate the palette.
- Ties are resolved by keeping the first stop with the maximum sum (later equal-sum stops do not replace it).
- Time complexity is O(n) and memory overhead is constant.

---

## gradientCssFromStops

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Generate a CSS linear-gradient string from an array of RGB color stops and pick a small, visually distinct subset when many stops are provided. Use this when you want a compact, perceptually ordered gradient (dim→bright) instead of handing every raw stop to CSS.

## Remarks
This helper simplifies long palettes into a readable gradient by sorting colors by brightness (simple sum of RGB channels) and sampling up to five representative stops. The sampling reduces visual "mud" that can occur when many equally weighted stops are used directly, while the sort enforces a clear dim-to-bright flow. It therefore intentionally discards original ordering and reduces the number of colors for visual clarity rather than preserving exact input positions.

## Example
```typescript
// No stops -> transparent
gradientCssFromStops([]); // 'transparent'

// Single stop -> single color (no gradient)
gradientCssFromStops([[255, 0, 0]]); // 'rgb(255,0,0)'

// Many stops -> 5 sampled stops, ordered by brightness
const palette: RGB[] = [
  [10,10,10], [50,20,30], [200,180,160], [120,60,40], [240,240,240], /* ...more... */
];
const css = gradientCssFromStops(palette);
// Example result: 'linear-gradient(90deg, rgb(... ) 0%, rgb(... ) 25%, rgb(... ) 50%, rgb(... ) 75%, rgb(... ) 100%)'
```

## Notes
- Brightness is computed as r+g+b (unweighted); this is a fast approximation, not a perceptual luminance calculation.
- The function reorders input stops by brightness and samples at fixed fractional positions; it does not preserve the original stop sequence or exact positions.
- Sampling uses Math.round to pick indices, so adjacent sampled indices can be the same for small palettes and duplicates are possible.
- Returns the literal string 'transparent' when given an empty array and delegates single-color formatting to rgbToCss for one-element arrays.


---

## paletteAccent

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Returns the palette's accent color as an RGB value by selecting the brightest color stop from the provided Palette. Reach for this helper when you need a single prominent color from a Palette (for highlights, borders, or accent elements) instead of manually choosing a specific stop.

## Remarks
This function centralizes the rule for deriving an accent color from a Palette by delegating to brightestStop(palette.stops). It captures the convention "accent = brightest stop" so callers don't need to duplicate that heuristic. The actual brightness calculation and stop selection are performed by the brightestStop helper; this function is a thin adapter that extracts the stops from the Palette.

## Example
```typescript
// Given a Palette object (myPalette), get an RGB accent and apply to an element
const accent: RGB = paletteAccent(myPalette);
const css = `rgb(${accent.r}, ${accent.g}, ${accent.b})`;
element.style.borderColor = css;
```

## Notes
- If palette.stops is empty or malformed, brightestStop's behavior determines the result (it may throw or return an unexpected value); validate the palette beforehand if necessary.
- The function returns an RGB object; convert to hex or CSS string if needed before use.
- "Brightest" is a specific heuristic — it may not match design intent in all cases (e.g., very saturated but darker colors). Consider a different selection strategy if a different accent characteristic is required.

---

## paletteForSeed

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Returns a Palette chosen deterministically from a numeric seed. Use this when you need a reproducible color palette for a given entity, session, or test vector; the optional name parameter is forwarded to the underlying picker to allow additional disambiguation.

## Remarks
This function is a thin wrapper that creates a deterministic pseudo-random generator (mulberry32) seeded with the provided number and then calls pickPalette to select a Palette. It exists to centralize the seed→palette relationship so callers get stable, repeatable palettes without dealing with the PRNG or selection logic directly.

## Example
```typescript
// Get a palette that will be the same every time for seed=42 and the same name
const palette = paletteForSeed(42, 'project-xyz');
// Use `palette` to style charts or avatars associated with that seed/name
```

## Notes
- The output is deterministic: the same seed and name will always produce the same Palette as long as mulberry32 and pickPalette implementations remain unchanged.
- The function simply forwards the seed and name to the underlying utilities; changes to mulberry32 or pickPalette will change results globally.

---

## paletteGradientCss

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Converts a Palette object into a CSS gradient string by delegating to gradientCssFromStops using the palette's stops. Reach for this helper when you have a Palette and need a ready-to-use CSS value (for example, to assign to an element's backgroundImage) rather than handling stops yourself.

## Remarks
This is a thin adapter around gradientCssFromStops: it extracts the stops array from the Palette and passes it through. Any formatting, ordering, or gradient type decisions are handled by gradientCssFromStops; this function exists to keep callers working with a Palette type rather than raw stop arrays.

## Example
```typescript
const palette: Palette = {
  stops: [
    { color: '#ff0000', position: 0 },
    { color: '#00ff00', position: 50 },
    { color: '#0000ff', position: 100 }
  ]
};

const swatch = document.getElementById('swatch')!;
swatch.style.backgroundImage = paletteGradientCss(palette);
```

## Notes
- The function does not validate the palette or its stops; passing null/undefined will likely throw. Ensure the Palette has a valid stops array.
- The visual output and exact CSS syntax (linear vs radial, percentage vs absolute positions) are determined by gradientCssFromStops; changes to gradient behavior should be made there.

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

**Returns:** ``Record<string, string>``


Converts an array of RGB color stops into a small set of CSS custom properties that represent the palette's primary accent, a softened accent, and a gradient built from the stops. Reach for this when you want to apply a palette to an element via inline styles or to expose palette values as CSS variables for components.

## Remarks
This is a thin adapter that composes helper utilities (brightestStop, rgbToCss and gradientCssFromStops). It selects the brightest stop as the canonical accent color, formats that color for CSS, creates a softened variant with an alpha of 0.22, and generates a gradient string from the provided stops. The function centralizes the mapping from palette stops to the three named CSS variables used by the UI.

## Example
```typescript
// `stops` should be an array of RGB objects matching the project's RGB type
const stops = [ /* ... RGB stops ... */ ];
const vars = paletteVarsFromStops(stops);

// Example: apply as inline style in React
return <div style={vars}>Content using palette CSS variables</div>;

// Or set them on an element directly
Object.entries(vars).forEach(([name, value]) => {
  document.documentElement.style.setProperty(name, value);
});
```

## Notes
- The function assumes the helpers can handle the provided stops; passing an empty array may produce invalid or unexpected values depending on brightestStop/gradientCssFromStops.
- The softened accent uses a fixed opacity of 0.22.
- Returned values are plain CSS strings suitable for use as CSS custom property values (e.g. for inline styles or setProperty).

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
| `rng` | [`Rand`](../../../../prototype/generate.js.md) | — |
| `name` | `string` | — |

**Returns:** [`Palette`](../../../api/Gabriel.Engine/Sequence/Palette.cs.md)


Returns a Palette by name when a matching palette exists; otherwise selects and returns a random Palette from the PALETTES array using the provided random number generator (rng). Use this when you want to pick a specific palette by its exact name or fall back to a reproducible/random choice driven by a supplied RNG.

## Remarks
This function centralizes palette selection logic so callers can request a named palette (useful for explicit user choices or fixtures) or rely on a provided RNG for deterministic randomness in tests and reproducible runs. The rng parameter is expected to be a function that returns a number in the [0, 1) range (for example Math.random or a seeded RNG).

## Example
```typescript
// Choose by name (exact, case-sensitive match)
const chosen = pickPalette(Math.random, 'ocean');

// Choose randomly using a seeded RNG implementation
const seedRng = () => seededNext(); // seededNext() -> number in [0,1)
const randomPalette = pickPalette(seedRng);

// If name is provided but not found, falls back to random selection
const fallback = pickPalette(Math.random, 'nonexistent-name');
```

## Notes
- Name matching is strict and case-sensitive; only an exact match returns the named palette.
- If the provided name is absent from PALETTES the function returns a random palette instead.
- The function assumes PALETTES is non-empty; if PALETTES.length is 0 the index calculation will be invalid.
- The rng must produce values in [0, 1). If it can return 1, the computed index may be out of bounds.

---

## rgbToCss

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Converts an RGB tuple and optional alpha value into a CSS color string using the modern functional notation. Use this helper when you need a CSS-ready color string (for inline styles or style props) — it emits an opaque rgb(...) form when alpha is 1 or greater, and the alpha-aware `rgb(r g b / a)` form when alpha is less than 1.

## Remarks
This small utility centralizes creation of CSS color strings in the Level 4 functional syntax (space-separated components, optional slash-separated alpha). Keeping the formatting logic here ensures consistency across the codebase when rendering colors in the DOM.

## Example
```typescript
// opaque
rgbToCss([255, 0, 128]); // -> "rgb(255 0 128)"

// semi-transparent
rgbToCss([255, 0, 128], 0.5); // -> "rgb(255 0 128 / 0.5)"
```

## Notes
- The function does not validate or clamp component or alpha ranges; callers should ensure r/g/b and alpha are in the intended ranges (commonly 0–255 for RGB and 0–1 for alpha).
- Alpha values >= 1 are treated as fully opaque and omit the alpha channel.
- The output uses the modern CSS functional notation (space-separated values and a slash for alpha). If you need legacy browser support that requires comma-separated rgba(), convert accordingly.

---

## sampleGradient

> **File:** `src/webapp/src/pulse/palettes.ts`  
> **Kind:** function

Return an RGB color sampled from a gradient defined by an array of RGB stops at a normalized position t (0..1). The function linearly interpolates between adjacent, equally spaced stops and returns the resulting color with each channel rounded to the nearest integer. Use this when you need a simple, evenly spaced palette lookup from a normalized parameter.

## Remarks
This utility treats the provided stops as evenly distributed across the 0..1 range and performs per-channel linear interpolation in RGB space. It is purposely simple and fast — suitable for UI palettes and procedural color ramps, but it does not perform gamma correction or interpolate in perceptually uniform color spaces.

## Example
```typescript
const stops: readonly RGB[] = [
  [255, 0, 0],    // red at t=0
  [255, 255, 0],  // yellow at t≈0.5
  [0, 0, 255],    // blue at t=1
];

console.log(sampleGradient(stops, 0));    // [255, 0, 0]
console.log(sampleGradient(stops, 0.25)); // interpolated between red and yellow
console.log(sampleGradient(stops, 0.5));  // near [255, 255, 0]
console.log(sampleGradient(stops, 1));    // [0, 0, 255]
```

## Notes
- The function does not handle an empty stops array; ensure stops.length >= 1 before calling.
- The t parameter is clamped to [0, 1] inside the function.
- Returned channel values are rounded but not clamped; ensure input stops contain valid 0–255 channel values if you expect 8-bit RGB output.

---