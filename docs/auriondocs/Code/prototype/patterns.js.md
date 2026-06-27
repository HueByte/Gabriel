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

Generates a fractal Brownian motion (fBm) value by summing multiple octaves of valueNoise at increasing frequencies and decreasing amplitudes. Reach for this when you need smoothly varying, natural-looking 2D noise (terrain height, texture masks, cloud-like patterns) without manually combining noise octaves yourself; this function applies the common lacunarity=2 and gain=0.5 pattern and normalizes the result by the total amplitude.

## Remarks
This is a simple fBm implementation that repeatedly samples valueNoise(x, y, seed) across octaves. Each octave doubles the frequency (freq *= 2) and halves the amplitude (amp *= 0.5), which are common defaults (lacunarity = 2, gain = 0.5) producing finer details at higher octaves while preserving overall coherence at low frequencies. The seed is offset by i * 17 for each octave to decorrelate samples between octaves; the constant 17 is an arbitrary odd offset to vary seeds.

## Example
```javascript
// Fill a small grayscale canvas using fbm as a heightmap
const width = 256, height = 256;
const octaves = 5;
const seed = 12345;
for (let y = 0; y < height; y++) {
  for (let x = 0; x < width; x++) {
    // Map pixel coordinates to noise space (scale down for larger features)
    const nx = x / width * 4;
    const ny = y / height * 4;
    const v = fbm(nx, ny, seed, octaves); // expected normalized noise value
    const shade = Math.floor((v + 1) * 0.5 * 255); // if valueNoise returns [-1,1]
    // set pixel to shade...
  }
}
```

## Notes
- fbm depends on a valueNoise(x, y, seed) implementation; the returned numeric range and bias of fbm depend on valueNoise's output range (e.g., [0,1] vs [-1,1]).
- octaves is treated as the loop limit; pass an integer for predictable results (non-integer octaves will cause the loop to iterate floor(octaves)+1 times when octaves has a fractional part).
- Performance: each additional octave doubles sampling frequency and costs another call to valueNoise; keep octaves reasonable (commonly 3–8) for real-time use.
- The seed perturbation uses a fixed step (17). If stronger independence between octaves is required, consider varying the seed strategy or mixing coordinates instead.

---

## hash2

> **File:** `prototype/patterns.js`  
> **Kind:** function

A small, fast non-cryptographic 2D integer hash that mixes two integer inputs (x and y) plus an integer seed and returns a deterministic pseudorandom floating-point value in the range [0, 1]. Use this when you need a cheap, repeatable noise/hash value for grid coordinates, procedural content, or randomized decisions that must be reproducible across runs.

## Remarks
This function intentionally operates on 32-bit integers (inputs are coerced with |0) and uses Math.imul and bitwise shifts/xors to perform integer mixing with a few large prime constants. It is designed for speed and determinism in JavaScript environments and is not suitable for cryptographic purposes. The returned number is produced by treating the final 32-bit result as an unsigned integer and normalizing it to the unit interval.

## Example
```javascript
// Generate a repeatable pseudorandom value for a grid cell (x, y)
const x = 42;
const y = -7;
const seed = 12345;
const v = hash2(x, y, seed); // v is a Number between 0 and 1

// Use it to choose a color
const color = v > 0.5 ? 'light' : 'dark';
```

## Notes
- Inputs are coerced to signed 32-bit integers via `| 0`; fractional inputs are truncated.
- The function is non-cryptographic and can have collisions or statistical biases; do not use where cryptographic security or perfect uniformity is required.
- The result uses division by 4294967295 so the output can reach exactly 1.0 when the internal unsigned 32-bit value equals 0xFFFFFFFF; if you need values strictly in [0, 1), divide by 4294967296 instead or clamp the result below 1.
- Because it relies on bitwise and Math.imul behavior, results are deterministic within typical JavaScript engines but may vary if engines change their integer semantics.

---

## pick

> **File:** `prototype/patterns.js`  
> **Kind:** function

Returns a single, uniformly random element from the provided array using Math.random. Reach for this small helper when you need to select one item at random from a list; for deterministic or cryptographically secure selection use a different RNG.

## Remarks
This is a minimal convenience wrapper around picking an index with Math.random and Math.floor; it does not copy or modify the input. It is useful for quick random sampling in tests, examples, or simple UI behavior where cryptographic security or reproducibility is not required.

## Example
```javascript
const items = ['apple', 'banana', 'cherry'];
const one = pick(items);
console.log(one); // e.g. 'banana'

// Safe usage with an empty-array guard
if (!Array.isArray(items) || items.length === 0) {
  // handle empty case explicitly
} else {
  const chosen = pick(items);
  // use chosen
}
```

## Notes
- If the array is empty, the function returns undefined (arr[Math.floor(...)] yields undefined).
- Passing null or undefined for arr will throw a TypeError when accessing arr.length; validate inputs if needed.
- Uses Math.random, which is not suitable for cryptographic needs or repeatable seeded randomness.

---

## pickPattern

> **File:** `prototype/patterns.js`  
> **Kind:** function

Returns a pattern entry from the global PATTERNS collection by name or picks one at random when no valid name is provided. Use this when you want a convenient way to resolve a named pattern or fall back to a random choice without handling selection logic yourself.

## Remarks
The function checks the truthiness of the provided name: if a non-empty name matches a key in PATTERNS it returns that entry immediately as an object { name, def }. If a name is provided but not found, it emits a console.warn listing available patterns and then selects a pattern at random. Passing a falsy name (undefined, null, empty string, 0, false) skips the warning and selects randomly. The function depends on a global PATTERNS object being available and uses Math.random for selection.

## Example
```javascript
// Assume PATTERNS = { stripe: {...}, dots: {...}, grid: {...} }
// Known pattern
const p1 = pickPattern('stripe');
// p1 => { name: 'stripe', def: PATTERNS['stripe'] }

// Unknown name -> warns, then returns a random entry
const p2 = pickPattern('unknown');
// console.warn("Unknown pattern \"unknown\". Available: stripe, dots, grid. Picking randomly.")
// p2 => { name: <one of 'stripe'|'dots'|'grid'>, def: PATTERNS[chosen] }

// No name (random)
const p3 = pickPattern();
// p3 => random entry from PATTERNS
```

## Notes
- The function has a side effect: it calls console.warn when a name is provided but not found in PATTERNS.
- If PATTERNS is empty or undefined, the random selection will return { name: undefined, def: undefined } — ensure PATTERNS is populated before calling.
- Name checks use JavaScript truthiness; an empty string is treated as falsy and results in a random pick without a warning.

---

## rand

> **File:** `prototype/patterns.js`  
> **Kind:** function

Returns a pseudo-random floating-point number sampled uniformly from the half-open interval [min, max) — inclusive of min and exclusive of max. Reach for this helper when you need a quick random float within a numeric range; for integer results or cryptographically secure randomness use a different helper.

## Remarks
This is a thin convenience wrapper around Math.random(), scaling its [0, 1) output into the requested range. It performs no input validation or type coercion beyond JavaScript's usual numeric operations, and it inherits Math.random()'s statistical properties (not suitable for cryptographic use).

## Example
```javascript
// random float between -1 (inclusive) and 1 (exclusive)
const f = rand(-1, 1);

// random integer between 0 and 9 inclusive (use floor and shift the upper bound)
const i = Math.floor(rand(0, 10));

// if min === max the function returns that value
const same = rand(5, 5); // 5
```

## Notes
- The generated value is in [min, max) when min < max; if min > max the result lies in (max, min] (min is still the included endpoint).
- Non-numeric or missing arguments produce NaN; the function does not validate inputs.
- For integer ranges, prefer an explicit integer helper or use Math.floor/Math.trunc as shown; for secure randomness use the Web Crypto API instead of Math.random().

---

## smooth

> **File:** `prototype/patterns.js`  
> **Kind:** function

Maps a linear parameter t to a smooth ease-in/ease-out curve using the polynomial 3t^2 - 2t^3. Reach for this helper when you need a simple, cheap easing function for animations or interpolations where the value and its first derivative should start and end at 0 (i.e., no abrupt jumps at the endpoints).

## Remarks
This is the classic "smoothstep" Hermite interpolation: it produces the identity endpoints (smooth(0) == 0, smooth(1) == 1) while making the slope zero at both ends, which yields an ease-in/ease-out effect. It's a small, pure function intended to be applied to normalized progress values (typically in the 0–1 range) before using them to interpolate other quantities.

## Example
```javascript
// normalize progress into [0,1], then apply smoothing before lerp
function lerp(a, b, t) { return a + (b - a) * t; }

let t = elapsed / duration;           // assume elapsed ∈ [0, duration]
let u = smooth(t);                    // eased progress
let position = lerp(startX, endX, u);
```

## Notes
- Intended for inputs in the 0..1 range; values outside that range will extrapolate and can produce results outside 0..1.
- Pure and side-effect free — safe to call repeatedly in tight animation loops.

---

## valueNoise

> **File:** `prototype/patterns.js`  
> **Kind:** function

Generates a smoothly interpolated scalar "value noise" at a 2D point by sampling pseudo-random values at the integer lattice around (x,y) and bilinearly blending them using a smoothing curve. Reach for this when you need a cheap, tileable-looking random field for textures, procedural patterns, or as a building block for fractal noise — instead of computing gradients (as Perlin noise does), this returns interpolated grid values controlled by a seed.

## Remarks
This implements classic value noise: it obtains four pseudo-random corner values from hash2 for the cell containing (x,y), computes local fractional offsets, applies the smooth fade function to those offsets, and performs a bilinear blend. It is deliberately small and dependency-light — the visual character and range of outputs depend entirely on the implementations of hash2 (the integer-grid sampler) and smooth (the interpolation/fade curve). Use it as a base layer for multi-octave noise or simple pattern generation when gradient continuity is not required.

## Example
```javascript
// Simple usage (requires hash2 and smooth to be defined elsewhere):
const x = 12.34, y = 45.67, seed = 42;
const n = valueNoise(x, y, seed);

// Typical use combining octaves for fractal noise
function fractalValueNoise(x, y, seed, octaves) {
  let sum = 0, amp = 1, freq = 1;
  for (let i = 0; i < octaves; i++) {
    sum += valueNoise(x * freq, y * freq, seed + i) * amp;
    freq *= 2;
    amp *= 0.5;
  }
  return sum;
}
```

## Notes
- valueNoise depends on two external helpers: hash2(xi, yi, seed) for per-grid pseudo-random values and smooth(t) for the interpolation curve; the numeric range and distribution of outputs follow hash2's contract. 
- Interpolation uses a smooth fade on the cell-local coordinates, but derivative continuity at integer boundaries depends on the chosen smooth function; if you need gradient-continuous noise, prefer a gradient noise implementation (e.g., Perlin/Simplex).

---