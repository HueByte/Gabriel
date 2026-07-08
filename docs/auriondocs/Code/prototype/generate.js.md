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


Produces FRAMES frames of SIZE-by-SIZE pixel grids by sampling a chosen procedural pattern over time, then applies a small per-pixel jitter and an ambient bias before clamping to [0, 1]. It selects a palette and pattern, initializes the pattern parameters, and returns both the frame sequence and a meta object that captures the generation settings for downstream reuse.

## Remarks

This abstraction cleanly separates per-pattern logic from the frame-generation loop and time progression. Pattern sampling is delegated to pattern.sample (fed with time, coordinates, and per-pattern params), while generate orchestrates frame construction, injecting controlled noise and an ambient floor to preserve gradient structure. The returned meta exposes the pattern, its parameters, and the noise/ambient configuration to aid inspection, tweaking, or reproducibility in tooling.

## Notes

- The per-pixel noise is nondeterministic (uses Math.random), so results will vary across runs unless the RNG is seeded externally.
- The function relies on global constants SIZE and FRAMES and on the availability of pickPalette and pickPattern; ensure these are defined and aligned with the surrounding runtime.


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


Generates a random floating-point number within the provided numeric range by scaling Math.random(). It returns a value in [min, max) when min <= max, using a uniform distribution. Use it whenever you need a quick, readable way to obtain a random value within a numeric range, instead of writing the scaling expression inline.

## Remarks
By encapsulating the common min-to-max formula, rand communicates intent and centralizes the randomness logic. It's a pure function with no side effects, depending solely on Math.random() and the numeric inputs.

## Example
```javascript
// Example: get a random value between 5 and 10
const r = rand(5, 10);
console.log(r);
```

## Notes
- No input validation: non-numeric inputs may produce NaN.
- If min > max, the result lies between max and min; ensure proper order to get the conventional [min, max) interval.
- Not cryptographically secure; for security-critical randomness, use a crypto-secure API (e.g., crypto.getRandomValues in browsers or Node's crypto module).

---