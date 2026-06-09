# palettes.js

> **Source:** `prototype/palettes.js`

## Contents

- [pickPalette](#pickpalette)
- [sampleGradient](#samplegradient)

---

## pickPalette

> **File:** `prototype/palettes.js`  
> **Kind:** function

Returns a single, randomly chosen entry from the global PALETTES array. Use this helper when you need a quick, non-deterministic palette selection for prototyping, UI demos, or randomized visuals.

## Remarks
This small convenience function centralizes the common pattern of selecting a random element from PALETTES so call sites do not need to repeat the Math.random/Math.floor indexing logic. Because it reads from the global PALETTES value, it is a lightweight wrapper rather than a pure function — useful for prototypes but easy to replace or stub in tests.

## Example
```javascript
// choose a palette and apply its colors to a drawing routine
const palette = pickPalette();
if (palette) {
  applyPaletteToCanvas(palette);
}

// in tests you can replace PALETTES for deterministic behavior
const original = PALETTES;
PALETTES = [ { name: 'test', colors: ['#000', '#fff'] } ];
console.assert(pickPalette().name === 'test');
PALETTES = original;
```

## Notes
- PALETTES must be defined and be an array; if PALETTES is empty the function returns undefined.
- Uses Math.random(), so selections are not seeded or reproducible across runs.
- The returned value is the original array element (not a defensive copy); mutating it will mutate the entry in PALETTES.

---

## sampleGradient

> **File:** `prototype/palettes.js`  
> **Kind:** function

Returns an interpolated RGB color from an array of evenly spaced color stops for a normalized position t (0..1). Use this when you have a small palette of RGB stops and need a sampled color at a fractional position along the gradient; the function linearly interpolates between the two surrounding stops and returns integer RGB components.

## Remarks
This function assumes stops are evenly spaced along the 0..1 range — it divides the range into (stops.length - 1) equal segments and interpolates within the segment that contains t. If the stops array contains a single element that element is returned as-is; for multiple stops a new array with rounded integer channel values is returned.

## Example
```javascript
// Red -> Blue gradient; sample at 25% along the gradient
const stops = [[255, 0, 0], [0, 0, 255]];
console.log(sampleGradient(stops, 0.25)); // => [191, 0, 64] (approx)

// Single-stop case returns that exact array reference
const single = [[128, 128, 128]];
console.log(sampleGradient(single, 0.5) === single[0]); // => true
```

## Notes
- stops must contain at least one stop; an empty array is not supported.
- Each stop is expected to be an array of numeric channel values (RGB). The function does not validate channel length or types.
- t is clamped to [0, 1]. Passing a non-numeric t (NaN) will lead to undefined behavior.
- Returned channels are rounded to integers but not explicitly clamped to 0–255; if input channels are within that range interpolation will remain within it.

---