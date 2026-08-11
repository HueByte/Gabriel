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


Computes fractal Brownian motion (fbm) by layering multiple octaves of valueNoise across increasing frequencies and decreasing amplitudes, then normalizing by the sum of amplitudes. This yields a smooth, natural-looking scalar field that is commonly used for terrain and texture generation. The function takes spatial coordinates x and y, a seed for deterministic results, and a positive octaves count that controls detail; per-octave seeds are derived from the base seed with an offset (seed + i * 17) to decorrelate each layer.

## Remarks

fbm is a convenience function that builds richer noise by combining a base noise source (valueNoise) at multiple scales. Normalizing by the total amplitude keeps outputs in a consistent relative range as you vary octaves. The fixed offset 17 ensures octaves do not reuse identical noise samples.

## Example

```javascript
// Example: evaluate fbm at a point with moderate detail
const value = fbm(12.5, 7.25, 1337, 5);
```

## Notes

- Octaves should be >= 1; otherwise the function divides by zero and yields NaN in JavaScript.
- The output range depends on valueNoise's range; normalization by total amplitude keeps the relative scale consistent across octaves, but callers should be aware of potential NaN if octaves is invalid or if valueNoise returns unexpected values.

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


hash2 is a compact, deterministic hash-like function that maps three 32-bit inputs (x, y, and seed) to a floating-point value in the range [0, 1]. It uses 32-bit integer arithmetic via Math.imul and bitwise operations to mix the inputs and produce a repeatable result. You would use hash2 when you need a fast, non-cryptographic pseudo-random value that is entirely determined by the inputs, making it suitable for procedural generation, texture coordinates, or noise sampling where reproducibility is important.

## Remarks
By encapsulating the bit-twiddling inside hash2, callers get a stable, coordinate- and seed-based value without re-implementing the same mixing logic. It's intentionally lightweight and fast, trading cryptographic strength for deterministic distribution suitable for non-security-critical randomness. The presence of x|0, y|0, and seed|0 makes clear that inputs are treated as 32-bit integers, ensuring consistent results across platforms that honor JavaScript's 32-bit bitwise semantics.

## Example
```javascript
// Example: deterministic value for coordinates (x, y) with a seed
const v = hash2(12, 34, 7);
console.log(v); // e.g. 0.472...
```

## Notes
- Inputs are coerced to 32-bit integers using x|0, y|0, seed|0; large or fractional inputs wrap according to 32-bit semantics.
- The function is non-cryptographic; suitable for procedural generation, not for security.
- Output is a normalized value between 0 and 1 (inclusive of 1 in edge cases) based on 32-bit mixing via Math.imul and bit shifts.

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


Returns a random element from the provided array by selecting a uniformly distributed index from 0 to arr.length - 1 using Math.random. This tiny helper consolidates the common pattern of sampling a single item without mutating the array. If the input array is empty, the function yields undefined, so callers should handle that case.

## Remarks
This abstraction centralizes the single-item random selection pattern, so callers don't repeat the index math and bounds checks. It isolates the dependency on Math.random, making it easier to swap in a seeded RNG for tests or future customization. It is intended for quick, non-mutating access to a random element from a list.

## Notes
- If arr.length === 0, the result is undefined; callers should guard against this.
- No input validation; passing non-array-like values can yield unexpected results.
- Uses Math.random(), which is not cryptographically secure; avoid using it for security-sensitive randomness.


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


pickPattern is a small helper that resolves a pattern definition from the PATTERNS catalog. If you pass a name that exists in PATTERNS, you get back an object containing that name and its definition. If you pass a name that isn't present, it emits a warning listing available patterns and falls back to picking a random pattern. If no name is provided, the function also picks a random pattern. In all cases, the function returns an object with the shape { name, def } where def is the corresponding entry from PATTERNS.

## Remarks

pickPattern centralizes the policy for selecting a pattern: honor an explicit request when possible, otherwise provide a concrete pattern by falling back to a random one instead of failing. It relies on the PATTERNS map and standard JavaScript utilities (Object.keys, Math.random) to enumerate and select patterns. Because the random path is nondeterministic, tests or callers should not rely on a fixed result unless they control Math.random or mock PATTERNS.

## Notes
- Relies on PATTERNS containing at least one entry; an empty PATTERNS would yield an object with undefined name/def when the random path is taken.
- When a non-existent name is provided, a warning is emitted via console.warn and the available keys are listed in the message.
- The randomness means outputs are non-deterministic unless Math.random (or PATTERNS) is controlled/mocked in the caller.

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


Generates a pseudo-random floating-point number in the half-open interval [min, max) by scaling Math.random() to the desired range. Use this helper when you need a quick, in-place random value between two bounds without pulling in a larger RNG utility.

## Remarks
This tiny wrapper centralizes the common pattern of sampling within a bounded range, making simple usage sites more concise and readable. The distribution is uniform across [min, max) as long as max > min; if max <= min, the result lies within [max, min), which can be surprising. Because it relies on Math.random(), it is not suitable for cryptographic randomness; for security-sensitive work, use the Web Crypto API (e.g., crypto.getRandomValues).

## Notes
- Math.random() is a pseudo-random generator and is not cryptographically secure.
- If min or max are not numeric, the result is NaN.
- If max == min, the result is exactly min. If max < min, the result lies between max and min, so ensure proper ordering or add input validation.

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


This function implements the smoothstep easing curve, taking a parameter t (typically in [0,1]) and returning a smoothly eased value between 0 and 1 using t^2(3−2t). Use it for animations or interpolations when you want an ease-in/ease-out rather than a linear ramp.

## Remarks
It is the standard cubic Hermite easing with zero tangents at both ends, producing a gentle S-shaped curve. Note that the implementation does not clamp t; inputs outside [0,1] can yield results outside the [0,1] range, so clamp t if you require strict bounds. To interpolate between two endpoints a and b, compute a + (b - a) * smooth(t).

## Example
```javascript
// Example: easing a value from start to end using smooth(t)
const start = 0;
const end = 100;
const t = 0.7;
const easedProgress = smooth(t); // in [0,1]
const value = start + (end - start) * easedProgress; // interpolated value
```

## Notes
- Not clamped: inputs outside [0,1] yield outputs outside [0,1]. Clamp if you require strict bounds.
- For simple interpolations, combine smooth(t) with a linear interpolation formula to move between two endpoints.

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


Computes a deterministic 2D value by bilinearly interpolating four corner samples produced by hash2 at the cell surrounding (x, y). A seed controls the hash-to-value mapping, and the fractional offsets are smoothed before interpolation to create a smoothly varying value noise field.

## Remarks
Value noise like this provides spatially coherent randomness suitable for textures and terrain. It encapsulates randomness behind hash2 and interpolation behind smooth, enabling consistent results for the same seed and coordinates. This modular approach makes it easy to compose multi-octave noise by layering calls with different seeds or coordinate transforms.

## Notes
- The output range depends on what hash2 returns; if you need a normalized [0, 1] value, ensure hash2 yields values in that range or clamp afterwards.
- This function yields C0 continuity across cell boundaries due to bilinear interpolation; to obtain finer fractal detail, combine multiple octaves with varying scales/seeds.
- It relies on external helpers hash2 and smooth; ensure they are pure functions to preserve determinism across identical inputs.

---