# palettes.js

> **Source:** `prototype/palettes.js`

## Contents

- [pickPalette](#pickpalette)
- [sampleGradient](#samplegradient)

---

## pickPalette
> **File:** `prototype/palettes.js`  
> **Kind:** function

```javascript
function pickPalette()
```


pickPalette returns a randomly selected element from the PALETTES array. It encapsulates the common pattern of choosing a random palette so callers don't need to repeat the Math.random-and-index logic each time they need a palette for theming or UI styling.

## Remarks
pickPalette centralizes the source of palettes and the randomness, decoupling call sites from the PALETTES collection. This makes it easier to swap out how palettes are provided (for example, lazy loading, filtering, or deterministic testing) without changing every consumer.

## Notes
- If PALETTES.length is 0, the function yields undefined because the computed index will be 0 and PALETTES[0] is undefined. Ensure at least one palette exists before relying on a return value.
- For deterministic tests, you can mock Math.random or PALETTES to produce a known index, keeping tests fast and simple.

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


Computes a color at position t along a gradient defined by a sequence of RGB stops. The function clamps t to the range [0,1], linearly interpolates between the two surrounding stops, and returns the resulting [r,g,b] with each component rounded to the nearest integer. If only one stop is provided, it returns that color directly.

## Remarks
sampleGradient is a pure function with deterministic output for the same inputs, making it suitable for palette generation and gradient previews. It expects stops to be ordered from start to end and to be 3-element arrays representing RGB values. The interpolation uses the derived segment length based on the number of stops, so non-uniform spacing between stops is honored along the t parameter. Note that channels are rounded to integers, which can slightly bias the result for very small or very large values.

## Example
```javascript
const stops = [[0, 0, 0], [255, 0, 0], [255, 255, 0]];
const c = sampleGradient(stops, 0.25); // [128, 0, 0]
console.log(c);
```

## Notes
- Requires at least one stop; an empty stops array will lead to undefined behavior in normal usage.
- The result is an integer RGB triplet due to rounding; use non-rounded calculations if you need fractional channels.


---