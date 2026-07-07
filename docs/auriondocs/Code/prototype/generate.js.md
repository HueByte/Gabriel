# generate.js

> **Source:** `prototype/generate.js`

## Contents

- [generate](#generate)
- [rand](#rand)

---

## generate
> **File:** `prototype/generate.js`  
> **Kind:** function

```javascript
function generate()
```


Generates an animation by selecting a palette and a pattern, initializing the pattern with SIZE, and then sampling a value for every pixel in a SIZE-by-SIZE grid across FRAMES frames. It adds a small random noise term and applies an ambient floor to keep quiescent pixels above the gradient’s low color, then clamps every value to the 0–1 range. The function returns an object containing the frames and metadata describing the pattern, its parameters, and the noise/ambient settings.

## Remarks
Acts as the high-level generator that bridges pattern logic and the rendering pipeline. Patterns expose a sample function (t, x, y, params, time) to compute per-pixel intensity, while generate handles frame sequencing, time stepping, and normalization. This separation lets you swap patterns or palettes without touching rendering code.

## Example
```javascript
// Example: generate frames for the selected pattern and inspect metadata
const result = generate();
console.log(`Generated ${result.frames.length} frames (size ${result.meta.size}) using pattern ${result.meta.pattern}`);
```

## Notes
- The per-pixel value is augmented with random noise via Math.random, so outputs are non-deterministic between runs unless the RNG is seeded.
- The ambient floor and noise amplitude are chosen at invocation time; changing SIZE, FRAMES, or the pattern can lead to different frame characteristics.
- This function relies on external helpers (pickPalette, pickPattern, rand) and constants (SIZE, FRAMES) defined elsewhere; ensure those are available in the environment where generate runs.

---

## rand
> **File:** `prototype/generate.js`  
> **Kind:** function

```javascript
function rand(min, max)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `min` | — | — |
| `max` | — | — |


Generates a uniform random floating-point number in the inclusive-exclusive range [min, max) by scaling Math.random(). Call rand(min, max) when you need a quick, in-range value without pulling in a heavier RNG library or extra dependencies. If min equals max, the function returns that exact value.

## Remarks
This tiny helper centralizes the common pattern of obtaining a random value within bounds. It relies on Math.random(), so its randomness is suitable for basic simulations and UI behavior but not for cryptographic purposes. The function does not validate inputs; callers should ensure min <= max (or be prepared for results outside that expectation). The lower bound is inclusive while the upper bound is exclusive.

## Example
```javascript
const x = rand(5, 10); // e.g., 7.6423
```

## Notes
- If min or max are not numbers, the result may be NaN.
- If max < min, the range is effectively reversed; pass in min <= max for predictable results.
- Not suitable for cryptographic randomness; use Web Crypto API for security-sensitive needs.

---