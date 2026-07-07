# patterns.js

> **Source:** `prototype/patterns.js`

## Contents

- [fbm](#fbm)
- [hash2](#hash2)
- [pick](#pick)
- [pickPattern](#pickpattern)
- [rand](#rand)
- [smooth](#smooth)
- [valueNoise](#valuenoise)

---

## fbm
> **File:** `prototype/patterns.js`  
> **Kind:** function

```javascript
function fbm(x, y, seed, octaves)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `x` | — | — |
| `y` | — | — |
| `seed` | — | — |
| `octaves` | — | — |


Computes fractal Brownian motion by layering multiple octaves of value-noise at increasing frequency and decreasing amplitude, producing smoother, more natural-looking 2D noise. Given coordinates x and y, a seed, and a number of octaves, fbm combines each octave with doubling frequency and halving amplitude, then normalizes the result by the total amplitude to keep outputs consistent across octave counts. Use fbm when you need richer terrain or texture variation than a single valueNoise sample, while preserving deterministic results via the seed.

## Remarks
FBM is a standard technique in procedural generation for creating natural-looking variation. This implementation decorrelates octaves by offsetting the per-octave seed (seed + i * 17) and uses a lacunarity of 2 with a persistence of 0.5 (amplitude halved each step). Normalizing by the sum of amplitudes ensures the final value remains within a predictable range regardless of how many octaves are combined, making it a robust building block for textures and terrain maps.

## Example
```javascript
// Example usage demonstrating typical invocation
const n = fbm(10.2, 5.7, 42, 6);
console.log(n);
```

## Notes
- Octaves must be >= 1; passing 0 or a negative value will cause division by zero and yield NaN.
- This function relies on valueNoise being defined in scope; ensure valueNoise is available and returns bounded results for stable normalization.
- The per-octave seed offset (i * 17) helps decorrelate octaves; keep valueNoise implementation consistent across platforms to preserve determinism.

---

## hash2
> **File:** `prototype/patterns.js`  
> **Kind:** function

```javascript
function hash2(x, y, seed)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `x` | — | — |
| `y` | — | — |
| `seed` | — | — |


Computes a deterministic, per-coordinate pseudo-random value in [0, 1) from three 32-bit inputs x, y, and seed. It folds the inputs into a single accumulator using 32-bit integer arithmetic (via Math.imul and |0 coercions) and two rounds of mixing to produce a stable float in the [0, 1] range. Use this function when you need repeatable variation tied to a grid location (x, y) and a seed, without relying on a global RNG like Math.random.

## Remarks
hash2 is a compact, stateless primitive intended for procedural pattern generation where deterministic, coordinate‑dependent randomness is required. By avoiding any shared RNG state, it ensures that the same inputs always yield the same output across calls and across runs. It relies on Math.imul for 32-bit multiplies and a sequence of bitwise operations to mix the bits; this keeps it fast while producing good dispersion for typical grid-based usage.

## Example
```javascript
// Deterministic value for grid cell (x, y) with a chosen seed
const value = hash2(x, y, seed);
```

## Notes
- Inputs x, y, seed are coerced to 32-bit integers via x | 0, y | 0, seed | 0; non-integer inputs are truncated.
- The function is non-cryptographic and should not be used for security or cryptographic purposes.
- The result is a deterministic floating-point value in the range [0, 1] for the given inputs.


---

## pick
> **File:** `prototype/patterns.js`  
> **Kind:** function

```javascript
function pick(arr)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `arr` | — | — |


Returns a random element from the provided array by selecting a uniform index with Math.random. Use this helper when you simply need one item from a list at random, rather than implementing a loop or shuffle yourself.

## Remarks
This tiny function centralizes the common pattern of sampling a single item from an array, delegating the randomness to Math.random. It improves readability at call sites and ensures a single, obvious place to change how random selection is performed if needed. Keep in mind that Math.random() is not cryptographically secure, so replace it if you need unpredictable randomness for security-sensitive cases.

## Example
```javascript
const colors = ['red', 'green', 'blue'];
const color = pick(colors); // e.g. 'green'
```

## Notes
- If arr.length === 0, the function returns undefined because there is no valid index.
- Passing a non-array-like value (e.g., null or undefined) will throw a TypeError when attempting to read .length.
- This function uses Math.random(); for cryptographic randomness, use a proper RNG.

---

## pickPattern
> **File:** `prototype/patterns.js`  
> **Kind:** function

```javascript
function pickPattern(name)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `name` | — | — |


pickPattern resolves a pattern definition from the PATTERNS registry and returns it in a consistent object shape. When a valid name is supplied and PATTERNS contains that key, it returns { name, def } for that pattern. If the name is provided but not found, it logs a warning listing available patterns and falls back to a randomly chosen pattern. If no name is supplied, the function also selects a random pattern. Use this helper when you want a safe, uniform way to obtain a pattern definition by name, with a sane fallback rather than risking a missing key error.

## Remarks
pickPattern encapsulates the lookup and fallback logic for PATTERNS, so callers do not need to duplicate code to handle unknown keys or to fall back to a random choice. It enforces a uniform return shape ({ name, def }) and makes it straightforward to either select a specific pattern or retrieve a valid random one when input is unavailable or invalid.

## Example
```javascript
// Known pattern
const pKnown = pickPattern('uniform');
console.log(pKnown.name); // 'uniform'
console.log(pKnown.def);  // PATTERNS['uniform']

// Unknown pattern triggers warning and random fallback
const pUnknown = pickPattern('doesNotExist');
console.log(pUnknown.name); // some random pattern name
```

## Notes
- If PATTERNS is empty, the random path yields { name: undefined, def: undefined }.
- The function uses Math.random for the fallback, so results are not deterministic across invocations.

---

## rand
> **File:** `prototype/patterns.js`  
> **Kind:** function

```javascript
function rand(min, max)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `min` | — | — |
| `max` | — | — |


Returns a random floating-point number uniformly distributed in the half-open interval [min, max) by computing min + Math.random() * (max - min). Use this tiny helper when you need a quick, in-range random value without pulling in a larger randomness utility.

## Remarks

Rand abstracts the arithmetic for mapping a unit random value to a numeric range, making call sites clearer and intent explicit. It relies on Math.random() and does not validate input, so it assumes numeric bounds and that max is greater than min for predictable results.

## Notes

- No input validation: max <= min yields an ill-defined range; callers should ensure min < max.
- Upper bound is exclusive: max is never reached; to include max, adjust the formula accordingly.


---

## smooth
> **File:** `prototype/patterns.js`  
> **Kind:** function

```javascript
function smooth(t)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `t` | — | — |


smooth(t) implements the cubic smoothstep easing curve. It maps t to a value in [0,1] according to t^2(3 - 2t), yielding a gentle ease-in and ease-out when interpolating between two endpoints.

## Remarks
Use this function when you want a smooth transition between values (for animations, UI motion, or shading) without linear progress. It produces a monotonic increase on [0,1], with zero slope at both ends, making transitions feel natural and continuous when blending from start to end. Centralizing this nonlinearity in a single helper helps maintain consistency across modules.

## Example
```javascript
// Common usage: interpolate between start and end with easing
const interpolate = (start, end, t) => {
  const s = smooth(Math.max(0, Math.min(1, t)));
  return start + (end - start) * s;
};

// Example: from 0 to 100 with t = 0.4
console.log(interpolate(0, 100, 0.4)); // ~35.2
```

## Notes
- Does not clamp t to [0,1]; values outside this range will extrapolate beyond 0–1.
- The derivative at t=0 and t=1 is zero, enabling a smooth start and end but not a constant rate.
- If you need a different curve, consider other easing strategies (e.g., linear, ease-in, ease-out) or library-provided functions.

---

## valueNoise
> **File:** `prototype/patterns.js`  
> **Kind:** function

```javascript
function valueNoise(x, y, seed)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `x` | — | — |
| `y` | — | — |
| `seed` | — | — |


Calculates a smooth 2D value-noise value at coordinates (x, y) using a deterministic seed. It samples the values at the four corners of the cell containing (x, y) with hash2, then blends them after applying a smoothing function to the fractional offsets, producing a smoothly varying scalar field. This is a simple, fast alternative to gradient noise for generating procedural textures or height maps, where repeatable, seed-driven patterns are desirable.

## Remarks
Value noise stores a pseudo-random value at each lattice point and interpolates between them; because the corner values come from hash2 with a fixed seed, the pattern is deterministic for given x, y, and seed. It serves as a foundational primitive for procedural texture and terrain generation and can be combined at multiple frequencies to approximate fractal noise.

## Example
```javascript
// Common usage: sample noise at a point with a given seed
const n = valueNoise(12.34, 56.78, 42);
```

## Notes
- The output range depends on the range of hash2; if you need a specific range, clamp/normalize accordingly.
- If you need tiling, align coordinates to tile boundaries or implement a tileable hash function.
- Ensure that hash2 and smooth are pure, deterministic functions to preserve reproducible results for a given seed.

---