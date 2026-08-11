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


pickPalette returns a random element from the PALETTES collection, encapsulating the common pattern of selecting a palette without exposing the underlying array or the random-index calculation to callers. It relies on Math and PALETTES, so callers simply call pickPalette() to obtain a color palette for UI or visualization tasks.

## Remarks
By deriving the result from PALETTES.length, the function ensures each palette has an equal probability of being selected. The approach centralizes the randomness strategy in one place, making it easy to adjust the source list of palettes in one location. This abstraction helps keep visual-theming consistent across the codebase by routing palette selection through a single helper.

## Notes
- Edge-case: If PALETTES.length is 0, the function returns undefined.
- If PALETTES is mutated at runtime (e.g., swapped out or cleared), subsequent calls will reflect the new state and probabilities.


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


Maps a normalized parameter t in [0, 1] to a color along a linear gradient defined by an array of RGB color stops; it clamps t, handles the single-stop case, and linearly interpolates between adjacent stops to produce an RGB triplet.

## Remarks
This function encapsulates the common gradient-lookup pattern used when rendering color palettes or themed UI elements. It hides the segment calculation and per-channel interpolation behind a simple, reusable API, so callers can focus on palette design rather than math. The contract assumes stops are ordered along the gradient and that each stop is an [r, g, b] triplet of numbers; input validation is left to higher-level code.

## Example
```javascript
// Example: interpolate halfway between black and white
const stops = [[0, 0, 0], [255, 255, 255]];
console.log(sampleGradient(stops, 0.5)); // [128, 128, 128]
```

## Notes
- At least one stop is required; an empty array will lead to incorrect behavior. If stops.length === 1, the single color is returned.
- t is clamped to [0, 1], so values outside this range map to the gradient endpoints.
- Each color component is rounded to the nearest integer; ensure inputs are numeric and within 0–255 for predictable results.

---