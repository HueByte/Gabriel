# palettes.js

> **Source:** `prototype/palettes.js`

## Contents

- [pickPalette](#pickpalette)
- [sampleGradient](#samplegradient)

---

## pickPalette

> **File:** `prototype/palettes.js`  
> **Kind:** function

Returns a randomly selected palette from the global PALETTES array using Math.random. Reach for this helper when you need a quick, uniformly random palette choice for UI theming or prototyping; if you need determinism or testability, inject or stub the source instead of relying on this global helper.

## Remarks
This small utility centralizes the common operation of choosing a palette at random so callers don't duplicate the Math.random + index calculation. It intentionally reads from a global PALETTES array rather than accepting an argument, favoring convenience in small prototypes; for larger or testable code paths prefer a function that accepts the palette list as a parameter.

## Example
```javascript
// Choose a palette and apply its colors to a UI component
const palette = pickPalette();
if (palette) {
  document.body.style.background = palette.background;
  // ...apply other colors
}
```

## Notes
- If PALETTES is undefined or not an array, this will throw or return undefined; ensure PALETTES is a defined, non-empty array before calling.
- If PALETTES is an empty array the function returns undefined (no index to select).
- Selection uses Math.random (not cryptographically secure) and is non-deterministic, which can make unit testing harder unless you inject or stub the source of randomness or the palettes array.

---

## sampleGradient

> **File:** `prototype/palettes.js`  
> **Kind:** function

```javascript
function sampleGradient(stops, t)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `stops` | — | — |
| `t` | — | — |


Interpolates a color from a list of RGB stops using a normalized parameter t (0..1). Use this when you need a simple, evenly‑spaced linear interpolation across a palette of RGB stops — for example sampling colors along a gradient made from two or more anchors.

## Remarks
This utility treats the provided stops as evenly spaced anchors across the 0..1 range. It clamps t to [0,1], finds the two neighbouring stops for the segment containing t, and linearly interpolates each RGB channel independently. If only one stop is provided it is returned unchanged. The function returns a new array of integer RGB components (values are rounded).

## Example
```javascript
const stops = [ [255, 0, 0], [0, 255, 0], [0, 0, 255] ]; // red -> green -> blue
console.log(sampleGradient(stops, 0));    // [255, 0, 0]
console.log(sampleGradient(stops, 0.5));  // near [0, 255, 0] (middle stop)
console.log(sampleGradient(stops, 0.25)); // interpolated between red and green
console.log(sampleGradient(stops, 1));    // [0, 0, 255]
```

## Notes
- The function expects each stop to be an array-like RGB triple (numeric [r, g, b]); there is no runtime validation of stop element lengths or types.
- t is clamped to [0,1]; passing values outside that range will yield an endpoint color.
- Returned components are rounded to integers; alpha channel is not supported.
- If stops.length === 1 the single stop is returned directly (no copy guarantees beyond the returned array).

---