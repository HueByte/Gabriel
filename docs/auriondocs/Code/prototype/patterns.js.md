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


fbm computes fractal Brownian motion by summing multiple octaves of 2D value noise with increasing frequency and decreasing amplitude, then normalizing by the total amplitude. Use it when you want natural-looking texture detail derived from a single noise source, adjustable via seed and octaves rather than composing several noises manually.

## Remarks
fbm is a standard technique for creating richer procedural patterns: layering octaves produces both large-scale structure and fine detail. The normalization by the sum of amplitudes keeps the output in a predictable range as more octaves are added. Each octave uses a distinct seed (seed + i * 17) to decorrelate scales, ensuring higher-frequency details do not simply repeat lower-frequency patterns.

## Example
```javascript
// Example usage
const value = fbm(0.1, 0.2, 1234, 6);
```

## Notes
- The function is deterministic for a given (x, y, seed, octaves) input.
- Performance scales with the number of octaves.
- Output range depends on the underlying valueNoise; normalization helps keep results comparable across different octave counts.


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


hash2 is a compact, deterministic hash helper that maps three integers (x, y, and seed) to a floating-point value in the range [0, 1]. It uses 32-bit integer arithmetic via Math.imul along with bitwise shifts to mix the inputs into a single value and then normalizes it to a [0, 1] result. The output is fully deterministic for a given triple, making this suitable as a lightweight source of seeded randomness for procedural generation or sampling tasks without pulling in a heavier PRNG.

## Remarks
Hash2 is a tiny, stateless primitive: for the same (x, y, seed) inputs, it always returns the same number. It is intended as a fast helper for deriving a seeded value from coordinates or identifiers rather than as a security primitive. The two-stage mixing—an initial combination using three multiplications, followed by a second mixing step (h ^ (h >>> 16))—helps reduce simple correlations between nearby inputs and yields a value distributed across the [0, 1] range when normalized.

## Example
```javascript
// Example usage: derive a deterministic value from coordinates and a seed
const v = hash2(12, 34, 56);
console.log(v); // v is a number in [0, 1]
```

## Notes
- Inputs x, y, and seed are coerced to 32-bit integers via |0; non-numeric values (e.g., NaN or strings) will be treated as 0.
- This function is not cryptographically secure. Use a proper cryptographic hash if security properties are required.
- The return value is a JavaScript number in [0, 1]; in rare cases the result can be exactly 1 because the final normalization divides by 4294967295.

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


pick(arr) returns a random element from arr by indexing at Math.floor(Math.random() * arr.length). If arr is empty, the result is undefined.

## Remarks
Tiny utility that centralizes the common task of picking a random item from a list. It relies on the global Math.random(), so it is suitable for non-critical randomness but not for security-sensitive decisions. It does not validate input beyond the existing array protocol, so an empty array yields undefined and non-array inputs may throw at runtime.

## Example
```javascript
const items = ["apple", "banana", "cherry"];
const choice = pick(items);
```

## Notes
- Returns undefined for empty arrays.
- Not suitable for cryptographic randomness; for secure randomness, use a cryptographic RNG.
- No input validation: passing non-array inputs may throw at runtime; ensure you pass a proper array.

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


pickPattern returns the pattern object for a given name from the PATTERNS registry and yields an object with the shape { name, def }. If the provided name exists in PATTERNS, the exact named pattern is returned; if the name is missing or unknown, a console warning is emitted and a randomly selected pattern from PATTERNS is returned instead. This function is useful when a caller needs a concrete pattern by name, with a safe fallback to a random pattern when the requested entry does not exist.

## Remarks
pickPattern acts as a lightweight adapter over the PATTERNS registry, standardizing the output as { name, def } and shielding callers from the registry’s internal structure. The warning path communicates that the requested key wasn’t found and a random alternative was chosen, which helps maintain stable downstream behavior without throwing.

## Example
```javascript
// Known pattern lookup
const a = pickPattern('Plasma');
console.log(a.name); // 'Plasma'
console.log(a.def);  // PATTERNS['Plasma']

// Unknown pattern falls back to a random pattern
const b = pickPattern('UnknownPattern');
console.log(b.name); // name of a randomly selected pattern
```

## Notes
- If PATTERNS is empty, the random fallback will attempt to select from an empty set, which may yield undefined results for name/def.
- The warning side-effect (console.warn) is informational and may be silenced in some environments; do not rely on it for control flow.
- The random fallback is non-deterministic; tests requiring a specific pattern should mock Math.random or PATTERNS accordingly.

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


Returns a uniformly distributed floating-point number in the half-open range [min, max) by scaling Math.random(). It's a tiny convenience wrapper around Math.random() that lets you write rand(min, max) instead of repeating the calculation. Use it when you need a quick, readable way to sample a number within a numeric interval; note that max is exclusive, so the returned value will never equal max.

## Remarks
This function is stateless and has no side effects. It relies on the global Math.random() for randomness. In tests or scenarios requiring reproducible results, consider injecting a custom RNG or mocking Math.random, or build a seeded RNG utility for deterministic behavior.

## Example
```javascript
// Example: generate a value in [1, 10)
const value = rand(1, 10);
console.log(value); // e.g. 6.742138...
```

## Notes
- Ensure min <= max; if max is less than min, the computed range becomes inverted and the result may be outside the expected bounds.
- Math.random() is not cryptographically secure; do not use this for security-sensitive randomness.
- The function has no built-in seeding, so for tests you may want to mock RNG behavior or replace Math.random during testing.

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


This function implements the classic smoothstep easing curve for transitions. Given a normalized input t in [0,1], it returns t * t * (3 - 2 * t), producing an S-shaped progression that starts and ends with zero slope. Use it to replace linear t in animations, interpolations, or progress calculations when a gentle acceleration and deceleration are desirable.

## Remarks
This is a pure, side-effect-free utility that encodes a common easing pattern. It yields deterministic results for the same input and is monotonic on [0,1], with zero slope at both endpoints, which makes it ideal for smoothly guiding transitions without abrupt starts or stops. Centralizing this logic in a tiny helper promotes consistency across call sites that perform interpolations or progress calculations.

## Notes
- Assumes t is in [0,1]; inputs outside this range can produce values outside [0,1]. If you need clamping, apply a clamp before or after calling smooth.
- Low computational cost: only a handful of arithmetic operations, suitable for hot paths.

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


ValueNoise computes a smoothly interpolated 2D value noise value at coordinates (x, y) by sampling pseudo-random corner values at the surrounding lattice points and performing a bilinear interpolation with a smoothing function. The four corner values are produced via hash2 at (xi, yi), (xi + 1, yi), (xi, yi + 1), and (xi + 1, yi + 1), where xi = Math.floor(x) and yi = Math.floor(y); the fractional offsets xf = x - xi and yf = y - yi are passed through a smoothing function to yield interpolation weights u and v, and the final value blends the four corners accordingly. Use this when you need a deterministic, smoothly varying scalar field (e.g., textures or terrain) derived from a simple hash-based randomness, with the seed allowing multiple independent noise patterns.

## Remarks
Value noise abstracts a common procedural pattern: randomness confined to lattice corners is smoothed into continuous data over the plane, avoiding hard edges that would occur with simple random sampling. It is designed to plug into terrain generation, textures, or other procedural effects, and it separates concerns by letting hash2 handle randomness and smooth handle interpolation. The function is fully deterministic for a given (x, y, seed), enabling repeatable results across runs.

## Example
```javascript
// Example: sample noise at a point with a fixed seed
const v = valueNoise(3.25, 7.75, 42);
console.log(v);
```

## Notes
- The value changes smoothly within a single lattice cell due to the smoothing of xf and yf, and only shifts significantly when crossing integer boundaries to sample a different corner.
- For richer textures, combine multiple octaves of valueNoise with different scales and seeds.
- This function relies on the helpers hash2 and smooth; changes to those implementations will affect outputs and determinism.

---