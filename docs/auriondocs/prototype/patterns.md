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

Generates fractal Brownian motion (fBm) noise by summing multiple octaves of valueNoise at increasing frequencies and decreasing amplitudes. Use this when you need multi-scale, natural-looking noise (terrain heightmaps, textures, clouds, procedural variation) instead of a single-scale valueNoise sample.

## Remarks
This function repeatedly samples valueNoise with frequency doubled each octave and amplitude halved (common fBm configuration: lacunarity = 2, gain = 0.5). Each octave uses a slightly different seed (seed + i * 17) so the layers are decorrelated. The result is normalized by the total amplitude so the returned value is the weighted average of all octaves rather than their raw sum.

## Example
```javascript
// sample fBm at position (x, y) with seed 123 and 5 octaves
const x = 12.34, y = 56.78, seed = 123, octaves = 5;
const n = fbm(x, y, seed, octaves);
// n can be used as a height value or blended into color/opacity for procedural textures
```

## Notes
- octaves should be a positive integer; passing 0 will leave total == 0 and produce NaN (divide-by-zero).
- fbm depends on a valueNoise(x, y, seed) function in scope; behavior and output range depend on that function's implementation.
- Amplitude attenuation is fixed at 0.5 per octave and frequency is doubled each octave; change the function if you need different lacunarity or gain.
- Performance scales linearly with octaves because valueNoise is called once per octave.

---

## hash2

> **File:** `prototype/patterns.js`  
> **Kind:** function

Generates a deterministic, non-cryptographic pseudo-random number in the range [0, 1] from three integer inputs (x, y, seed). Reach for this when you need a fast, repeatable scalar value derived from 2D integer coordinates plus a seed — for example, in procedural textures, grid-based randomness, or simple noise lookups.

## Remarks
This function mixes the three 32-bit integer inputs using 32-bit integer multiplication (Math.imul) with large odd constants and bitwise XOR/shift mixing to produce an effectively scrambled 32-bit result. The final value is converted to an unsigned 32-bit integer and normalized by 4294967295 to yield a JavaScript Number in [0, 1]. It is intentionally lightweight and deterministic; it trades cryptographic strength for speed and simplicity.

## Example
```javascript
// Get a repeatable pseudo-random value for grid cell (i, j) with a chosen seed
const value = hash2(10, 20, 12345);
console.log(value); // deterministic number between 0 and 1

// Use as a threshold to place objects on a grid
for (let i = 0; i < 100; i++) {
  for (let j = 0; j < 100; j++) {
    if (hash2(i, j, 42) > 0.7) {
      // place an object at (i, j)
    }
  }
}
```

## Notes
- Inputs are coerced to signed 32-bit integers via |0; passing floating-point numbers will truncate them to 32-bit integers (wrapping occurs for large values).
- The output range is [0, 1] inclusive; because the result is divided by 2^32-1 (4294967295), an output of exactly 1 is possible. If you need a half-open range [0, 1), divide by 4294967296 instead.
- Not cryptographically secure — do not use for security-sensitive randomness.
- Collisions are possible (32-bit state); this is intended for procedural/randomization use cases, not unique identifiers.

---

## pick

> **File:** `prototype/patterns.js`  
> **Kind:** function

Returns a uniformly random element from an array (or array-like) using Math.random. Reach for this small helper when you need a quick, non-cryptographic random choice from a collection and don't want to write the index calculation every time.

## Remarks
This is a tiny utility wrapper around Math.random that selects an element by computing a random index in [0, length). It does not mutate the input and works with any object that exposes a numeric length and numeric indices (arrays, array-like objects, strings). It is intentionally minimal — it prioritizes brevity over safety checks.

## Example
```javascript
const colors = ['red', 'green', 'blue'];
console.log(pick(colors)); // -> 'green' (for example)

console.log(pick('abc')); // -> 'a' | 'b' | 'c'  (works with strings)

console.log(pick([])); // -> undefined
```

## Notes
- Passing an empty array returns undefined; the function does not throw for empty arrays.
- Passing null/undefined (or objects without a numeric length) will throw or behave unexpectedly — ensure the argument is an array-like value.
- Uses Math.random(), so it is not suitable for cryptographic needs or situations requiring reproducible seeded randomness.

---

## pickPattern

> **File:** `prototype/patterns.js`  
> **Kind:** function

Returns the pattern entry for the given name (an object with { name, def }) when that name exists in the global PATTERNS map. If no name is provided, or the provided name isn't present in PATTERNS, the function logs a warning for unknown names and returns a randomly selected pattern from PATTERNS instead. Use this to resolve a pattern name to its definition with a built-in fallback.

## Remarks
This helper centralizes the logic for resolving a pattern name into the corresponding definition stored in PATTERNS and provides a deterministic branch (when the name is known) plus a non-deterministic fallback (random selection) when no usable name is supplied. It intentionally emits a console.warn when an explicit but unknown name is passed so callers can be alerted while still receiving a usable pattern.

## Example
```javascript
// Known name
const p1 = pickPattern('stripe'); // => { name: 'stripe', def: PATTERNS['stripe'] }

// Unknown name (warning logged), fallback to a random pattern
const p2 = pickPattern('does-not-exist'); // => { name: '<randomly-chosen>', def: PATTERNS[... ] }

// No name provided, choose randomly
const p3 = pickPattern(); // => { name: '<randomly-chosen>', def: PATTERNS[... ] }
```

## Notes
- The function uses truthiness for the name check: falsy values (e.g. '', 0, null, undefined) are treated the same as "no name provided."
- If PATTERNS is empty, the returned object will have name and def as undefined (no exception is thrown from the random selection code), so callers should validate the returned def before use.
- Passing a non-string truthy value will be used as the lookup key directly (PATTERNS[name]); ensure names are valid keys present in PATTERNS to avoid unexpected fallbacks.

---

## rand

> **File:** `prototype/patterns.js`  
> **Kind:** function

Returns a pseudorandom floating-point number greater than or equal to `min` and strictly less than `max`. Use this small helper when you need a uniform random float inside a half-open interval [min, max) instead of calling Math.random() and scaling manually.

## Remarks
This is a convenience wrapper around Math.random() that scales the 0..1 range into the requested interval. It does not change the distribution (still uniform) and does not provide cryptographic security — it exists purely to reduce repeated boilerplate when producing random floats in a range.

## Example
```javascript
// random float in [0, 1)
const a = rand(0, 1);

// random float in [5, 10)
const b = rand(5, 10);

// common pattern: integer in 0..9
const i = Math.floor(rand(0, 10));

// inclusive integer 0..10
const inclusive = Math.floor(rand(0, 11));

// ensure correct ordering if min might be larger than max
function randSafe(x, y) {
  if (x > y) [x, y] = [y, x];
  return rand(x, y);
}
```

## Notes
- The result is in [min, max): it can equal `min` but will never equal `max`.
- Returns a floating-point number; convert with Math.floor/Math.ceil/Math.round for integer needs.
- Not suitable for cryptographic uses — use a secure RNG when required.
- If non-numeric arguments are passed, the function will produce NaN; if `min > max` the expression still works but yields values in a decreasingly scaled range (swap arguments if that is unintended).

---

## smooth

> **File:** `prototype/patterns.js`  
> **Kind:** function

Computes a smoothstep easing curve for the parameter `t` using the cubic Hermite polynomial (3t^2 - 2t^3). Reach for this when you need a simple ease-in/out interpolation for values typically in the [0, 1] range (for example, smoothing animation progress or blending between two values).

## Remarks
Implements the common "smoothstep" function: it maps 0 -> 0 and 1 -> 1 and yields zero first derivative at both endpoints, which removes abrupt velocity changes at the start and end of a transition. The function itself does not clamp `t` to [0,1]; callers should clamp if they require strictly bounded output.

## Example
```javascript
// Ease a linear interpolation between a and b
function lerp(a, b, t) { return a + (b - a) * t; }

const t = 0.3; // progress in [0,1]
const eased = smooth(t);
const value = lerp(0, 100, eased); // eased interpolation

// If you want to ensure t is within [0,1]:
const clamped = Math.max(0, Math.min(1, t));
const safeValue = lerp(0, 100, smooth(clamped));
```

## Notes
- Input is not clamped: values of `t` outside [0,1] will produce results outside [0,1].
- Equivalent polynomial form: `3*t*t - 2*t*t*t`.
- Boundary behavior: `smooth(0) === 0` and `smooth(1) === 1`, with zero slope at both endpoints.

---

## valueNoise

> **File:** `prototype/patterns.js`  
> **Kind:** function

Computes 2D value noise at the given (x, y) coordinates using a lattice of hashed corner values and a smooth interpolation. Reach for this when you need a simple, tileable-ish procedural noise field (e.g., terrain height, texture masks, or as a building block for fractal / octave noise) without computing gradient vectors.

## Remarks
This implements classic value noise: the function samples pseudorandom values at the four integer lattice corners around (x, y) via hash2(xi, yi, seed) and blends them using a smoothing curve (smooth) on the fractional offsets. The result is continuous across the interior of a lattice cell but the derivative may be discontinuous at cell boundaries because corner values change per-cell. The output range and statistical properties depend on what hash2 returns and how smooth() maps [0,1].

## Example
```javascript
// Example: fill a canvas with grayscale valueNoise
const width = 256, height = 256;
const seed = 42;
for (let j = 0; j < height; j++) {
  for (let i = 0; i < width; i++) {
    // scale coordinates to change feature size
    const x = i / 32;
    const y = j / 32;
    const v = valueNoise(x, y, seed); // expected to be in a known range depending on hash2
    // convert to 0..255 assuming v is in [0,1]
    const color = Math.floor(Math.max(0, Math.min(1, v)) * 255);
    // draw pixel using color
  }
}
```

## Notes
- hash2 and smooth are expected collaborators: hash2 should return a numeric noise value for integer coordinates (commonly in 0..1 or -1..1), and smooth should map fractional offsets (0..1) to eased weights; the final output range depends on these.
- Negative inputs work (Math.floor handles negatives), but be aware floor differs from truncation — the lattice cell chosen for negative coordinates is consistent with Math.floor semantics.
- This is a single-octave value noise; combine multiple scaled octaves and attenuated amplitudes for richer fractal noise (e.g., fBm).
- Performance: hash2 is called four times per sample; cache or reduce calls if sampling very densely.

---