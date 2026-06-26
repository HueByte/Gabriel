# patterns.ts

> **Source:** `src/webapp/src/pulse/patterns.ts`

## Contents

- [FlowParams](#flowparams)
- [NoiseParams](#noiseparams)
- [Pattern](#pattern)
- [PlasmaParams](#plasmaparams)
- [PulseParams](#pulseparams)
- [ShimmerCell](#shimmercell)
- [ShimmerParams](#shimmerparams)
- [SpiralParams](#spiralparams)
- [WavesParams](#wavesparams)
- [PatternName](#patternname)
- [fbm](#fbm)
- [hash2](#hash2)
- [pickPattern](#pickpattern)
- [range](#range)
- [smooth](#smooth)
- [valueNoise](#valuenoise)

---

## FlowParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface FlowParams
```


Groups the numeric configuration used to render or animate a "flow" pulse pattern. Use this interface when creating or passing a single parameter object that describes the geometry, timing, and motion of a flowing, banded visual effect (angle, band width, number of bands, cycle period, speed, and center coordinates).

## Remarks
This is a plain data-transfer object that consolidates related numeric values into one argument for pattern-rendering or animation functions. It does not validate values or enforce units — callers and consumers must agree on the meaning (for example, whether angles are degrees or radians and what coordinate system cx/cy use).

## Example
```typescript
const params: FlowParams = {
  angle: 45,
  bandWidth: 20,
  bands: 3,
  period: 2000,
  speed: 1.0,
  cx: 150,
  cy: 100,
};

// Pass to a renderer/animator that expects these fields
renderFlowPattern(canvasContext, params);
```

## Notes
- All properties are plain numbers; there is no runtime validation in the interface itself.
- Properties are mutable (not readonly); freeze or copy the object if immutability is required.
- Confirm units and coordinate conventions with the consumer (angle units, time units for period, and coordinate origin for cx/cy) to avoid mismatches.

---

## NoiseParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents a small set of numeric configuration values used to drive a procedural noise generator (for example, when generating patterns or animated noise-based effects). Use this interface wherever a noise-producing function or module needs a compact bundle of parameters.

## Remarks
This interface is a plain data contract — it does not implement any behavior. It collects common knobs used by fractal/noise generators: overall scale, a value used to make the noise loop/seamless, a seed for deterministic randomness, and the number of octaves (layers) to combine.

## Example
```typescript
const params: NoiseParams = {
  scale: 0.5,
  loopR: 1.0,
  seed: 42,
  octaves: 3,
};

// Pass `params` into a noise function that expects these values.
```

## Notes
- All four properties are required by the interface; any semantic constraints (valid ranges, integer-only for octaves, non-negative loopR, etc.) are enforced by the consumer implementation, not by this type.
- Treat `seed` as an opaque numeric value used to initialize a pseudorandom sequence; its interpretation is implementation-dependent.

---

## Pattern

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents a reusable spatial–temporal pattern generator used by the pulse system. Implement this interface when you need a procedural pattern that can be initialized once (with a size and RNG) and then sampled repeatedly at specific coordinates and times to produce numeric amplitudes.

## Remarks
`Pattern<P>` separates expensive or random initialization (init) from the inner sampling routine (sample). init is intended to produce and return a pattern-specific parameter bag of type P (for example precomputed tables, seeds, or scale factors) and is typically called once per pattern instance. sample is called frequently (once per sample point / pixel / time step) and therefore should be implemented as a cheap, pure function that uses only its inputs and the params returned by init.

## Example
```typescript
// A trivial constant pattern that always returns 1
const constantPattern: Pattern<null> = {
  init(size, rng) { return null; },
  sample(t, x, y, params, time) { return 1; }
};

// A pattern that stores a random phase in init and uses it when sampling
const randomPhase: Pattern<{ phase: number }> = {
  init(size, rng) { return { phase: rng.next() * Math.PI * 2 }; },
  sample(t, x, y, params, time) {
    // simple oscillation using stored phase
    return Math.sin((t || time) + params.phase);
  }
};
```

## Notes
- init may be called with different size or RNG values; store only what is necessary for sampling.  
- sample is expected to be side-effect free and performant — it may be invoked many times per frame.  
- The codebase does not document units for x/y (pixel vs. normalized) or the distinction between t and time; implementers should clearly document which units and semantics their Pattern uses to avoid confusion.

---

## PlasmaParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

A compact, strongly typed container for the numeric parameters used to configure a plasma/pulse visual pattern. All ten properties are plain numbers and required — this interface is intended to be passed as a single argument to code that generates or animates a plasma-style pattern.

## Remarks
This interface groups together a small fixed set of coefficients and offsets so callers can pass a single object instead of many separate numeric arguments. The field names are minimal (a, b, c, d, sa, sb, sc, sd, cx, cy) and represent whatever coefficients, scales, or center offsets the consuming pattern renderer expects; the interface itself enforces only the presence and numeric type of each value.

## Example
```typescript
const params: PlasmaParams = {
  a: 1.2,
  b: 0.8,
  c: 2.5,
  d: 1.0,
  sa: 0.5,
  sb: 0.75,
  sc: 0.25,
  sd: 1.5,
  cx: 0.0,
  cy: 0.0,
};

// Pass `params` to the routine that generates or animates the plasma pattern
// drawPlasmaPattern(params);
```

## Notes
- All properties are required; there is no runtime validation in the type itself — callers should ensure values are finite and within any application-specific ranges.
- The field names are shorthand and carry no enforced semantic meaning here; consult the pattern generator implementation for expected units and effects.
- Mutating a shared PlasmaParams object while it is used by a renderer may produce visual glitches; prefer creating a new object when updating parameters.

---

## PulseParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents the configuration for a radial pulse/ripple pattern: the pulse center (cx, cy), the width of each wave, how many ripples to produce, and the maximum radius the effect reaches. Use this interface when passing a compact set of pulse parameters to rendering or animation helpers that generate ripple effects.

## Remarks
This is a simple data-transfer shape that groups geometry (cx, cy, maxRadius) with visual parameters (waveWidth, ripples). It keeps APIs that create or draw pulse patterns small and explicit; callers or the rendering code are responsible for interpreting units (pixels vs normalized coordinates) and validating ranges.

## Example
```typescript
const params: PulseParams = {
  cx: 120,       // center x coordinate
  cy: 80,        // center y coordinate
  waveWidth: 16, // thickness of each wave
  ripples: 3,    // number of concentric waves
  maxRadius: 200 // largest radius the pulse will reach
};

// Pass to a drawing/animation helper
drawPulse(context, params);
```

## Notes
- The interface does not perform validation; ensure waveWidth and maxRadius are positive and ripples is a non-negative integer.
- Coordinate space for cx/cy depends on the consumer (e.g., canvas pixels vs normalized coordinates) — be consistent across callers.
- PulseParams is a plain object. Mutating it after starting an animation may affect ongoing rendering if the consumer holds a reference.

---

## ShimmerCell

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents a single segment (cell) of a shimmer/pulse animation: a compact DTO that encodes spatial bounds (from → to) and temporal placement (startTime and duration). Use this when building, serializing, or interpreting the sequence of animation cells that make up a shimmer effect.

## Remarks
This minimal interface separates spatial information (from/to) from timing (startTime/duration) so rendering code can map cells onto layout coordinates or normalized ranges and schedule them on an animation timeline. It is intended as a portable, JSON-friendly shape passed between animation generators, renderers, and serializers.

## Example
```typescript
const cell: ShimmerCell = {
  from: 0,
  to: 120,      // e.g. pixels or normalized coordinate units depending on the pipeline
  startTime: 0, // e.g. milliseconds since animation start
  duration: 300 // e.g. milliseconds
};
```

## Notes
- Units for from/to and startTime/duration are not specified by this type; callers must agree on units (pixels vs. normalized units, ms vs. seconds) across the animation pipeline.
- Expected invariants: from <= to, startTime >= 0, duration >= 0. Negative or non-finite numbers are likely invalid for rendering.
- The interface contains only primitive numbers and is safe to JSON.stringify/parse for transport between components.

---

## ShimmerParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Configuration for a shimmer animation run. Provides the 2‑D grid of ShimmerCell values to animate, a numeric minimum and maximum duration used to pick per-cell timing, and a Rand instance used for any randomized decisions. Reach for this when you need to create or configure a shimmer/pattern generator so it has the pixel layout, timing bounds, and RNG it requires.

## Remarks
This interface centralizes the inputs needed by the shimmer pattern generator: the pixels matrix defines the layout/state for each cell, minDur and maxDur define the inclusive range used to compute each cell's animation duration, and rng supplies randomness so the timing can be deterministic (when using a seeded RNG) or non-deterministic. Passing an RNG separately makes tests and reproducible animations easier.

## Example
```typescript
// Example construction (adjust to your Rand / ShimmerCell implementations)
const params: ShimmerParams = {
  pixels: [
    [{ /* ShimmerCell */ }, { /* ShimmerCell */ }],
    [{ /* ShimmerCell */ }, { /* ShimmerCell */ }]
  ],
  minDur: 100,      // numeric duration lower bound (commonly milliseconds)
  maxDur: 500,      // numeric duration upper bound
  rng: myRand       // implements Rand
};

startShimmer(params);
```

## Notes
- Ensure minDur <= maxDur; otherwise the consumer that samples durations may behave unexpectedly.
- pixels should match the dimensions and cell structure expected by the consumer code.
- rng is provided so callers can supply a seeded generator for reproducible results in tests or demos.

---

## SpiralParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

A compact bag of numeric settings used to configure a spiral pattern: number of arms, how tightly the spiral winds, its animation speed and edge sharpness, and the spiral's center coordinates (cx, cy). Use this interface whenever you need to pass or store all spiral-related parameters together (for example, from UI controls to a renderer or generator).

## Remarks
This interface centralizes the parameters that describe a spiral so callers and renderers share a stable, self-describing shape configuration. It keeps shape, animation and positioning concerns in one plain object, making it convenient to serialize, bind to UI controls, or pass through pipeline stages that build or animate spiral patterns.

## Example
```typescript
const params: SpiralParams = {
  arms: 3,
  tightness: 0.6,
  speed: 1.2,
  sharpness: 0.8,
  cx: 128,
  cy: 96
};

// Example usage (consumer depends on the codebase):
// drawSpiral(canvasContext, params);
```

## Notes
- All properties are numeric; consumers expect finite numbers.
- cx and cy are coordinates in the renderer's coordinate space — confirm whether that space is pixels, normalized units, or something else before supplying values.
- Semantic interpretation of tightness, speed and sharpness is defined by the consumer; adjust values experimentally if visuals differ from expectation.

---

## WavesParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

A compact configuration object describing the visual and temporal characteristics of a "waves" pulse pattern. Use this interface when creating, configuring, or passing a set of numeric parameters that control orientation, spatial frequency, propagation speed, waveform sharpness, and the pattern's center point.

## Remarks
This interface groups related scalar values into a single parameter bag so callers and renderers can accept a stable, typed shape instead of many separate arguments. It does not enforce units, ranges, or normalization; those semantics are determined by the consumer (the renderer/animator) that interprets these fields.

## Example
```typescript
const params: WavesParams = {
  angle: 0,        // pattern orientation
  freq: 2.5,       // spatial frequency
  speed: 1.0,      // propagation speed
  sharpness: 0.8,  // edge/shaping parameter
  cx: 0.5,         // center x coordinate
  cy: 0.5          // center y coordinate
};

// Pass the parameters to a renderer/animator (hypothetical API)
renderWaves(params);
```

## Notes
- The interface only specifies numeric properties; consult the renderer's documentation for expected units (e.g. degrees vs radians for angle) and whether cx/cy are normalized coordinates or pixel values.
- No runtime validation is performed by this type alone — ensure values are within any required ranges before passing to the consumer.
- Instances are plain mutable objects; clone or Object.freeze if immutability is required.

---

## PatternName

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** type

A string literal union type representing the keys of the runtime PATTERNS object. Use PatternName whenever a value must be one of the named patterns defined in PATTERNS (for function parameters, object lookups, or discriminated handling), ensuring TypeScript-level consistency with the PATTERNS definitions.

## Remarks
This type is derived directly from the PATTERNS object so the allowed values come from a single source of truth: the runtime PATTERNS map. Using keyof typeof PATTERNS keeps compile-time checks synchronized with the actual pattern identifiers, reducing copy-and-paste errors and making it easy to add or remove supported patterns by editing PATTERNS.

## Example
```typescript
function renderPattern(name: PatternName) {
  const pattern = PATTERNS[name];
  // ...use pattern to render or configure behavior
}

// valid if 'pulse' is a key in PATTERNS
renderPattern('pulse');
```

## Notes
- PatternName is computed from the PATTERNS object at compile time; changing PATTERNS will change this type after recompilation.
- If PATTERNS is declared with a wide index signature or as any, the resulting keyof typeof PATTERNS may be less specific than intended (e.g. string | number).

---

## fbm

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Computes a 2D fractal Brownian motion (fBm) value by summing multiple octaves of valueNoise. Use this when you need richer, multi-scale noise (e.g., procedural textures, terrain heightmaps, animated turbulence) instead of a single-frequency noise sample.

## Remarks
This function builds fBm by iterating "octaves" times: each octave samples valueNoise at an increased frequency (doubles each octave) and with a halved amplitude (gain = 0.5). The contributions are accumulated and then normalized by the total amplitude so the returned value stays on the same relative scale as the base noise function. The octave-specific seed is offset by i * 17 to decorrelate successive octaves deterministically.

## Example
```typescript
// Sample fBm at (x, y) with seed 123 and 5 octaves
const n = fbm(12.34, 56.78, 123, 5);

// If valueNoise produces values in [-1, 1], map to grayscale [0,255]
const intensity = Math.round((n * 0.5 + 0.5) * 255);
const color = `rgb(${intensity}, ${intensity}, ${intensity})`;

// If valueNoise is in [0,1], simply map directly
const intensity01 = Math.round(n * 255);
```

## Notes
- octaves must be >= 1. Passing 0 yields a division by zero (total remains 0) and produces NaN.
- Higher octave counts increase CPU cost roughly linearly; keep octaves as low as needed for detail.
- The final value range depends on the range of valueNoise; normalization preserves relative amplitude but does not force a particular numeric range.
- The amplitude decay (gain = 0.5) and frequency doubling (lacunarity = 2) are fixed here; vary those if you need different spectral characteristics.

---

## hash2

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Generates a fast, deterministic 32-bit hash from three integer inputs (x, y, seed) and maps it to a floating value in the unit interval. Reach for this when you need a lightweight pseudo-random value that depends on two coordinates and a seed (for procedural patterns, deterministic noise, or reproducible randomization) rather than cryptographic strength.

## Remarks
This function coerces its numeric arguments to 32-bit integers and uses 32-bit integer multiplications and xors to produce a pseudo-random 32-bit result which is then normalized to [0, 1]. It is designed as a cheap, deterministic hash for graphics/algorithmic use (e.g., seeding particles, generating pattern values per grid cell) and is not suitable for cryptographic or security-sensitive purposes.

## Example
```typescript
// Basic usage: get a deterministic pseudo-random number for a cell (x,y) with a seed
const value = hash2(10, 20, 0); // value in [0, 1]

// Convert to an integer in range [0, n-1] safely
const n = 256;
const idx = Math.min(Math.floor(hash2(10, 20, 0) * n), n - 1);
```

## Notes
- Inputs are truncated with x | 0, y | 0, seed | 0, so non-integer values are coerced by dropping fractional parts and large numbers are reduced to 32-bit signed integers.
- The returned value is in the inclusive range [0, 1]. Because 1.0 can occur, multiplying the result by a length and flooring it can produce an out-of-range index; clamp or use Math.min(..., length-1) if needed.
- Not cryptographically secure; collisions and patterns are possible for related inputs. The function prioritizes speed and determinism over statistical perfection.
- Uses Math.imul for correct 32-bit integer multiplication behavior across JavaScript engines.

---

## pickPattern

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Selects and returns a pattern definition along with its canonical name. Pass an optional PatternName to retrieve a specific pattern; omit the name to have the function choose one (it requires a Rand instance which is used for selection when no name is supplied). The return value is always an object containing the pattern's name and its definition.

## Remarks
Centralizes pattern lookup and sampling so callers don't need to know the underlying pattern collection or selection logic. Returning both the canonical name and the Pattern makes it easy to display or persist which pattern was chosen while also providing the pattern data to use immediately.

## Example
```typescript
// Choose a random pattern (rng should be a seeded or deterministic RNG for repeatable results)
const { name, def } = pickPattern(rng);

// Explicitly request a named pattern
const chosen = pickPattern(rng, 'sine');
console.log(chosen.name, chosen.def);
```

## Notes
- The function requires an RNG even when a name is provided; callers must supply a Rand instance.
- The return type is non-nullable: callers can rely on receiving a concrete pattern name and definition.
- If deterministic behavior is required for the random selection path, provide a seeded Rand implementation.

---

## range

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Returns a floating-point value mapped linearly from the unit output of the provided random-number generator into the interval between min and max. Reach for this helper when you have a RNG that produces values on a 0–1 scale and you need a random value scaled to an arbitrary numeric range.

## Remarks
This small utility abstracts the common formula for scaling a unit random sample into an arbitrary numeric interval: min + u * (max - min). It deliberately returns an unrounded, potentially fractional number so callers can decide whether to keep it as a float or convert to an integer. The function performs no validation — it is a thin mapper that keeps the RNG injection explicit for testability.

## Example
```typescript
// Using Math.random as the RNG to get a float in [10, 20)
const value = range(Math.random, 10, 20);

// Producing an integer in [10, 20] (inclusive) from a unit RNG
// note: add 1 to max and floor the result to include the upper bound
const intInclusive = Math.floor(range(() => Math.random(), 10, 21));
```

## Notes
- Assumes the provided rng produces values on a 0..1 scale (commonly [0,1)); if rng yields values outside that range, results will be correspondingly shifted or scaled.
- No checks are made for min <= max; if min > max the returned value will lie between max and min according to the same linear formula.
- The result is a floating-point number; use Math.floor/Math.round/Math.ceil when integer behavior is required.

---

## smooth

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Maps a numeric parameter t into a smooth cubic easing curve (the classic "smoothstep": 3t^2 - 2t^3). Returns 0 at t = 0 and 1 at t = 1 with zero first derivatives at both endpoints — use this to produce smooth start/stop transitions for animations, pulse envelopes, or any interpolation where you want no abrupt velocity changes.

## Remarks
This is the common smoothstep polynomial implemented as t * t * (3 - 2 * t). It is intentionally simple and very cheap to compute, meant to be applied to normalized parameters (0..1). The function does not clamp its input; when given values outside [0, 1] it will extrapolate the polynomial rather than saturate, so callers should clamp or normalize inputs if a strict 0..1 output range is required.

## Example
```typescript
// Convert a linear progress value into a smooth eased value
const progress = 0.33; // expected in [0, 1]
const eased = smooth(progress);
// use eased to interpolate between two values
const value = start * (1 - eased) + end * eased;
```

## Notes
- The input is expected to be normalized to [0, 1]; if you need clamping, do: const tClamped = Math.min(Math.max(t, 0), 1).
- The function guarantees zero velocity at the ends (derivative 0 at 0 and 1), which removes abrupt starts/stops but does not create overshoot.
- Extremely cheap to compute; suitable for per-frame animation/easing calculations.

---

## valueNoise

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Produces a 2D value-noise sample at the given (x, y) coordinates using a per-corner pseudo-random value generator and smooth bilinear interpolation. Reach for this when you need a cheap, continuous noise field (e.g., textures, procedural patterns, simple terrain) and you don't need gradient-consistent noise like Perlin or Simplex.

## Remarks
This implementation samples a pseudo-random value at each integer lattice corner using an external hash2 function and blends those four corner values with a smooth interpolation function (smooth) applied to the fractional coordinate. The seed parameter is forwarded to hash2 so the same (x, y, seed) triple always yields the same output; changing seed gives a different noise realization. Because interpolation is applied to the corner values rather than to gradients, valueNoise is faster and simpler than gradient noise but produces different visual characteristics and non-smooth derivatives across the domain.

## Example
```typescript
// produce a small 2D heightmap using valueNoise
const width = 64, height = 64;
const seed = 12345;
const scale = 0.1; // scale down coordinates to get larger features
const heightmap: number[][] = [];
for (let y = 0; y < height; y++) {
  heightmap[y] = [];
  for (let x = 0; x < width; x++) {
    heightmap[y][x] = valueNoise(x * scale, y * scale, seed);
  }
}
```

## Notes
- The numerical range and distribution of returned values depend entirely on the implementation of hash2; do not assume a specific range unless you inspect hash2.
- This function is continuous but not derivative-continuous; visual artifacts can appear if you rely on smooth gradients (use gradient noise for that).
- To change the apparent frequency of the noise, scale the input coordinates rather than modifying the function itself; tiling requires a tile-aware hash2 or additional logic.

---