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


pickPalette returns a random element from PALETTES by selecting an index with Math.random. It should be used whenever you need to choose a palette at runtime instead of hard-coding or wiring the choice at multiple call sites.

## Remarks
This function centralizes the randomness and the dependency on PALETTES, turning a scattered, ad-hoc palette selection into a single reusable utility. It relies on PALETTES being a non-empty array at runtime; by importing this function you commit to keeping the palette list in one place.

## Notes
- If PALETTES is empty, the function yields undefined. Ensure PALETTES is non-empty or guard with a fallback in calling code.

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


Interpolates a color along a defined sequence of RGB stops for a given parameter t in the range [0, 1]. It clamps t, handles the single-stop case by returning that color, and linearly blends the RGB channels between adjacent stops with per-channel rounding. Use this function when you need a smooth gradient color corresponding to a normalized position rather than choosing a fixed color or precomputing a gradient map.

## Remarks
This is a pure, deterministic function with no side effects. It operates on RGB triplets where each channel is in the 0–255 range and returns a new triplet by per-channel linear interpolation with rounding. The function handles the edge cases of a single stop and of t at the boundaries, making it suitable for sampling intermediate colors in UI palettes or theme generators.

## Example
```javascript
sampleGradient([[255,0,0],[0,0,255]], 0.5)
// => [128, 0, 128]
```

## Notes
- Requires at least one stop; zero stops will cause a failure.
- Output channels are rounded to integers (0–255).
- t is clamped to [0, 1]; values outside this range are treated as endpoints.

---