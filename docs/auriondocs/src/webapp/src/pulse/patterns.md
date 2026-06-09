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

Groups numeric settings used to configure a flow-style visual or animation pattern: orientation (angle), the width and count of bands, timing (period), motion speed, and the pattern center (cx, cy). Use this interface when you need to pass a compact, strongly typed set of parameters to rendering or animation code that generates banded/flowing effects.

## Remarks
This is a simple data-transfer shape intended to keep related numeric parameters together rather than scattering many primitives across function signatures. It lets rendering/animation logic accept a single object that fully describes a flow pattern's geometry and timing.

## Example
```typescript
const params: FlowParams = {
  angle: Math.PI / 4,   // orientation (radians or degrees per renderer conventions)
  bandWidth: 12,        // thickness of each band
  bands: 6,             // number of bands
  period: 2000,         // cycle period in milliseconds (renderer-defined units)
  speed: 0.5,           // relative speed multiplier
  cx: 256,              // x-coordinate of pattern center
  cy: 128               // y-coordinate of pattern center
};

// Pass `params` to the renderer/animator that consumes FlowParams
// e.g. renderer.drawFlow(params);
```

## Notes
- Units (angle in radians vs degrees, period in ms, coordinates in pixels or normalized space) are not defined by this interface; consult the consumer/renderer for conventions.
- bandWidth should typically be a positive number and bands an integer >= 1; invalid values may produce no visible pattern or runtime errors in consumer code.
- This interface contains only numeric fields and is mutable; treat instances as plain data objects.

---

## NoiseParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents a compact set of numeric options used to configure a procedural noise generator. Use this interface when passing grouped noise configuration (scale, looping radius, random seed and layer count) into functions that synthesize or sample noise for patterns or animations.

## Remarks
This interface is a simple value object that groups related numeric parameters for noise generation. It exists to keep noise-related settings together and make function signatures cleaner than passing multiple separate numeric arguments.

## Example
```typescript
const params: NoiseParams = {
  scale: 0.5,
  loopR: 10,
  seed: 42,
  octaves: 3,
};
// Pass `params` to a noise-producing function or store it with a pattern definition.
```

## Notes
- All properties are typed as number; TypeScript does not enforce integer vs. fractional values at compile time.
- There is no built-in validation in the interface—callers should ensure values are in expected ranges (e.g., positive scale, non-negative octaves) before using them.
- `octaves` is typically used as a small integer count; prefer integral values for that field.

---

## Pattern

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents a reusable spatial–temporal pattern generator. Implementations perform any one-time setup in init (using the provided size and RNG) and then produce scalar sample values for points in a 2D grid via sample. Reach for this interface when you need to separate expensive or randomized initialization from many cheap per-coordinate evaluations (for example when rendering frames, generating procedural textures, or computing time-varying fields).

## Remarks
This interface intentionally separates initialization (init) from evaluation (sample) so implementations can precompute and store whatever data they need in the generic params type P. init is where randomness and size-dependent setup should occur; sample is expected to be called frequently and should therefore be fast and deterministic given its inputs and the params returned by init. Rand is supplied to init only — sample should not depend on external RNG state.

## Example
```typescript
// Typical usage pattern
const pattern: Pattern = /* some implementation */;
const size = 128;
const rng = new Rand(/* seed */);
const params = pattern.init(size, rng);

// Evaluate the pattern at a grid position (x, y) for sample index t and time
const value = pattern.sample(0, 10, 20, params, performance.now() / 1000);
```

## Notes
- init may be called once per pattern instance and can perform expensive or randomized setup; prefer doing heavy work here rather than inside sample.  
- sample is expected to be fast and side-effect free; avoid mutating params from inside sample.  
- Rand is provided only to init; do not assume sample will have access to an RNG.  
- The signature includes both t and time; their distinct meanings are not documented here and can be ambiguous for implementers — confirm whether t is a discrete frame/index and time is continuous seconds (or some other convention) with callers/consumers before implementing.

---

## PlasmaParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

A compact data shape that groups numeric configuration values used by a plasma pattern or similar procedural effect. Use this interface when you need to pass a set of related numeric parameters (instead of many separate arguments) to renderers, generators, or serializers.

## Remarks
This interface consolidates ten numeric properties (a, b, c, d, sa, sb, sc, sd, cx, cy) into a single DTO-like object. The naming convention groups four primary parameters (a..d), four secondary/scale parameters (sa..sd) and a 2D center/offset (cx, cy). Keeping these values in one object simplifies function signatures, serialization, and applying defaulting or validation logic in one place.

## Example
```typescript
const params: PlasmaParams = {
  a: 1.0,
  b: 2.0,
  c: 3.0,
  d: 4.0,
  sa: 0.1,
  sb: 0.2,
  sc: 0.3,
  sd: 0.4,
  cx: 0.5,
  cy: 0.5,
};

// Pass the parameter bag to a renderer or generator
// renderPlasma(canvasContext, params);
```

## Notes
- All properties are required by the interface; callers must provide each numeric field.
- The interface does not enforce ranges or units — validate or clamp values where appropriate before use.
- Objects conforming to this interface are plain mutable objects; use a readonly wrapper or copy if immutability is needed.

---

## PulseParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Defines the numeric configuration for drawing a pulse/ripple pattern: the center coordinates (cx, cy), the thickness of each wave (waveWidth), how many concentric ripples to render (ripples), and the maximum radius to draw (maxRadius). Use this when supplying parameters to a pulse renderer or pattern generator to control size, position and repetition of the waves.

## Remarks
PulseParams is a lightweight DTO used to pass rendering parameters into pulse or ripple pattern algorithms. It separates geometry (cx, cy, maxRadius) from styling/structure (waveWidth and ripples) so callers can compute layout independently and then hand a single object to the drawing routine.

## Example
```typescript
const params: PulseParams = {
  cx: 150,          // center x (pixels)
  cy: 75,           // center y (pixels)
  waveWidth: 8,     // thickness of each ripple (pixels)
  ripples: 4,       // number of concentric ripples
  maxRadius: 120    // maximum radius to render (pixels)
};
// pass `params` to the pulse drawing function
```

## Notes
- cx/cy and all size values are expressed in the renderer's coordinate units (typically pixels).
- ripples is expected to be a non-negative integer; fractional or negative values may produce unexpected results.
- waveWidth and maxRadius should be non-negative; if maxRadius is smaller than waveWidth, some ripples may be clipped or not visible.

---

## ShimmerCell

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents a single segment of a shimmer/pulse pattern: a numeric range (from → to) together with timing information (startTime and duration). Use this interface when describing or exchanging the data needed to schedule and interpolate one element of a shimmer animation or pulse timeline.

## Remarks
This is a plain data shape that groups value-range and timeline information so renderers or pattern generators can treat each shimmer step as an atomic unit. Keeping range and timing together makes it easier to schedule animations, compute interpolations, and reason about ordering in a sequence of cells.

## Example
```typescript
// Define a sequence of shimmer cells for a simple timeline
const pattern: ShimmerCell[] = [
  { from: 0, to: 1, startTime: 0, duration: 200 },   // fades from 0→1 over first 200ms
  { from: 1, to: 0.5, startTime: 200, duration: 150 } // then 1→0.5 starting at 200ms
];

function valueAt(cell: ShimmerCell, t: number): number {
  const local = (t - cell.startTime) / cell.duration;
  const clamped = Math.max(0, Math.min(1, local));
  return cell.from + (cell.to - cell.from) * clamped; // linear interpolation
}
```

## Notes
- The units for from/to and startTime/duration are not enforced by this interface; confirm whether consumers expect pixels, normalized values (0..1), or milliseconds.
- The interface does not validate ordering (from may be greater than to) or non-negative duration; callers should enforce any invariants they rely on.
- This is a simple mutable data shape (TypeScript interface). If immutability is required, freeze or copy objects before sharing across components.


---

## ShimmerParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Parameters for configuring a shimmer (pulse) pattern over a 2‑D grid of ShimmerCell values. Use this when starting or generating a shimmer animation so the pattern code has (1) the pixel grid to operate on, (2) the numeric range from which to choose per‑cell durations, and (3) the random number source used to vary timings.

## Remarks
This interface encapsulates all inputs the shimmer pattern generator needs to compute timing and initial values for each cell. Injecting a Rand instance makes the pattern generation deterministic or testable when a seeded RNG is provided, and keeps randomness decoupled from the algorithm that consumes these parameters.

## Example
```typescript
// typical usage
const params: ShimmerParams = {
  pixels: cellGrid,    // ShimmerCell[][] prepared elsewhere (rows x columns)
  minDur: 80,          // lower bound for per-cell duration
  maxDur: 240,         // upper bound for per-cell duration
  rng                  // a Rand instance (possibly seeded for reproducibility)
};
startShimmer(params); // pass into the function that drives the shimmer animation
```

## Notes
- Ensure minDur <= maxDur; many implementations will sample a duration between these bounds for each cell.
- The numeric durations are raw numbers interpreted by the consumer (commonly milliseconds) — keep units consistent with the animation runner.
- Provide a Rand instance to control randomness; omitting or replacing it with a non-deterministic source will affect reproducibility.
- pixels is a 2D array (rows/columns); the consumer may mutate or read it directly depending on the implementation.

---

## SpiralParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

Represents the numeric configuration for a spiral pulse pattern — use this shape when creating or configuring a spiral generator or UI control that produces spiral-based motion/brightness patterns. Each property controls one aspect of the spiral geometry or animation: number of arms, how tightly the arms wind, animation speed, edge/contrast sharpness, and the spiral center (cx, cy).

## Remarks
This interface is a plain data contract that decouples parameter definition from the algorithm that renders or animates the spiral. It lets callers (UI code, presets, serialization) pass a single object to pattern builders or animation systems without imposing validation or units, keeping the generator implementation free to interpret these numeric values.

## Example
```typescript
const params: SpiralParams = {
  arms: 3,          // three arms in the spiral
  tightness: 0.8,   // how closely wound the arms are
  speed: 0.6,       // rotation / phase speed (unit depends on consumer)
  sharpness: 1.2,   // edge falloff / contrast
  cx: 250,          // center x coordinate (pixels or normalized)
  cy: 250           // center y coordinate
};

createSpiralPattern(params);
```

## Notes
- The interface does not enforce ranges or types beyond number — callers should validate (e.g. arms should typically be a positive integer). 
- Units (pixels vs. normalized coordinates, radians vs. degrees for speed) are not specified here; consult the spiral generator consumer for expected units.
- Negative or zero values for parameters like tightness or sharpness may be meaningless depending on the renderer; handle or validate them upstream.

---

## WavesParams

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

A plain data interface that groups numeric settings used to configure a "waves" pattern (angle, frequency, speed, sharpness and a center point cx/cy). Reach for this when you need to pass or store the numeric parameters that control a waves-style visual or animation effect.

## Remarks
WavesParams is intentionally minimal — it describes only the shape of the data, not any behavior, defaults, or validation. As an interface it can be implemented or extended by callers and is typically used as the argument type for pattern generation or rendering functions within the patterns module.

## Example
```typescript
const params: WavesParams = {
  angle: 0.5,       // orientation in radians (consumer-defined units)
  freq: 2.0,        // frequency multiplier
  speed: 1.2,       // animation speed factor
  sharpness: 0.8,   // wave edge sharpness
  cx: 0.5,          // horizontal center (unit depends on consumer)
  cy: 0.5           // vertical center
};

// Pass `params` to a renderer or generator that accepts WavesParams
// e.g. renderWaves(params);
```

## Notes
- The interface does not enforce ranges or units; callers and consumers are responsible for validating values and documenting expected units (degrees vs radians, normalized vs absolute coordinates, etc.).
- Being an interface, instances are mutable by default (plain objects). If immutability is required, freeze or copy the object before sharing.
- No defaults are provided here; supply all properties or ensure the consumer handles missing values.

---

## PatternName

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** type

A string literal union type representing the property names (keys) of the PATTERNS object. Use this type to annotate variables, parameters, or return values that should be one of the pattern identifiers defined in PATTERNS — it keeps TypeScript code in sync with the set of available patterns and provides compile-time checking instead of using loose string literals.

## Remarks
This alias delegates authority for valid pattern identifiers to the PATTERNS object: any change to PATTERNS' keys is reflected in PatternName automatically, avoiding duplicated string unions. It exists to provide a single source of truth for pattern names and reduce runtime errors caused by misspelled or outdated literal strings.

## Example
```typescript
// read a pattern from the PATTERNS map using a typed name
function getPattern(name: PatternName) {
  return PATTERNS[name];
}

// annotate a variable with a known pattern name
const currentPattern: PatternName = 'blink';

// use in a component/handler signature
function applyPattern(name: PatternName) {
  const pattern = PATTERNS[name];
  // ...apply pattern...
}
```

## Notes
- If PATTERNS is declared with a broad index signature or as Record<string, ...>, PatternName will collapse to string (losing the precise literal union). To preserve exact keys, PATTERNS should be declared with literal keys (and ideally `as const` where appropriate).
- PatternName is a compile-time (TypeScript) construct — it does not enforce validity at runtime. Validate inputs if they originate from external data (user input, network, etc.).
- Renaming or removing keys from PATTERNS will cause TypeScript errors in sites that rely on the old names, which is intentional: it forces updating all call sites when the available patterns change.

---

## fbm

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Generates fractal Brownian motion (fBm) by summing multiple octaves of valueNoise at increasing frequency and decreasing amplitude, then normalizing by the total amplitude. Use this when you want a multi-scale, smoother noise field (e.g., terrain height, organic textures, animated procedural patterns) instead of a single-frequency noise sample.

## Remarks
This implementation starts with amplitude = 1 and frequency = 1, doubles the frequency and halves the amplitude each octave (lacunarity = 2, gain = 0.5), and normalizes the final sum by the accumulated amplitudes so the output stays on the same scale as the underlying valueNoise. The seed is offset by i * 17 for each octave to decorrelate samples between octaves; 17 is an arbitrary small odd step to avoid repeating patterns. The function is deterministic for the same inputs; behavior depends on the range of valueNoise (see notes).

## Example
```typescript
// Sample a 2D fBm value at (x, y) with seed=42 and 4 octaves
const x = 12.34;
const y = 56.78;
const seed = 42;
const octaves = 4;
const height = fbm(x, y, seed, octaves);

// Map to a color or height map, depending on valueNoise's range
console.log('fBm value:', height);
```

## Notes
- Passing octaves <= 0 will cause a division by zero; octaves should be a positive integer.  
- The output range follows the range of valueNoise after normalization: if valueNoise returns in [-1, 1], fBm will remain within that interval; if valueNoise returns [0, 1], fBm will be in that range.  
- Computational cost grows linearly with octaves; increase octaves only as needed for detail.

---

## hash2

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Returns a deterministic, stateless pseudo-random floating-point value in the range [0, 1] derived from the integer inputs x, y and a seed. Use this when you need a fast, repeatable hash/noise value for 2D coordinates (e.g., procedural patterns, tiling, deterministic dithering) without maintaining RNG state.

## Remarks
This function mixes the three 32-bit inputs with integer multiplications and bitwise xors/shifts (using Math.imul and >>>) to produce a 32-bit hash which is then normalized to a floating value by dividing by 4294967295. Inputs are coerced to 32-bit integers, so the routine is cheap, stateless, and portable across JS engines. It is intended for utility-level procedural noise and pattern work — not for cryptographic or statistically perfect uniform random generation.

## Example
```typescript
// produce a small deterministic 2D grid of values with a chosen seed
const seed = 12345;
for (let y = 0; y < 5; y++) {
  const row: number[] = [];
  for (let x = 0; x < 5; x++) {
    row.push(hash2(x, y, seed));
  }
  console.log(row.map(v => v.toFixed(3)).join(' '));
}
```

## Notes
- Inputs (x, y, seed) are coerced to 32-bit signed integers via |0; fractional parts are truncated and large values wrap modulo 2^32. Provide integer seeds for predictable results.
- Not cryptographically secure and not guaranteed to have perfect statistical uniformity — suitable for visuals and simple procedural variation, not security-sensitive use.
- The return value is a JavaScript number in the closed interval [0, 1] (derived from a 32-bit unsigned value divided by 4294967295).

---

## pickPattern

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Returns a pattern definition together with its canonical PatternName. Provide an optional PatternName to request a specific pattern; when omitted the function selects a pattern using the provided Rand instance. Use this when you need both the chosen pattern's metadata (name) and its concrete Pattern object for rendering, serialization, or tests.

## Remarks
Consolidates the mapping from PatternName to Pattern and the selection logic that uses the project's random source. This keeps callers from duplicating lookup or selection code and ensures a single point of truth for how patterns are chosen or resolved by name.

## Example
```typescript
// pick a random pattern using the RNG
const { name, def } = pickPattern(rng);

// request a specific pattern by name
const chosen = pickPattern(rng, 'stripe');
console.log(chosen.name, chosen.def);
```

## Notes
- The signature does not reveal how unknown or invalid PatternName values are handled — check the implementation to see if it throws, falls back, or ignores the name.
- Determinism depends on how the provided Rand is seeded; use a seeded Rand for reproducible selection in tests.
- Prefer treating the returned def as immutable unless the Pattern type explicitly documents mutability.

---

## range

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Returns a floating-point value scaled to the interval between min and max using the provided RNG function. Reach for this helper when you have a Rand (a function that produces unit-range values) and need a uniformly distributed number mapped into an arbitrary numeric range instead of manually scaling the output each time.

## Remarks
This is a small convenience wrapper that converts a unit random value (typically in the range 0..1) into the desired numeric interval by linear interpolation: result = min + u * (max - min). It keeps calling code concise and centralizes the scaling logic so callers only provide the RNG and the target bounds.

## Example
```typescript
// Using the built-in Math.random as the RNG
const value = range(Math.random, 10, 20); // value in [10, 20) assuming Math.random() is in [0, 1)

// Using a seeded RNG that returns numbers in 0..1
function seededRng(): number {
  // ...implementation that returns a unit-range float
  return 0.42;
}
const seededValue = range(seededRng, -5, 5); // maps 0.42 into the -5..5 interval
```

## Notes
- The exact inclusivity of the endpoints depends on the behaviour of the provided rng: if rng() can return 1 the result may equal max; if rng() never returns 1 (e.g. Math.random) the result will be < max.
- If min > max the function still returns a value on the numeric line between those two values (because max - min is negative), which can be surprising — prefer giving min <= max for clarity.
- This function returns a floating-point value; use Math.round/Math.floor if an integer is required.

---

## smooth

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Maps a parameter t (typically in the 0..1 range) to a smoothed interpolation using the cubic Hermite polynomial t*t*(3-2*t). Reach for smooth when you need an ease-in/ease-out curve for animations, transitions, or value blending instead of a linear interpolation — it produces zero velocity at the endpoints so changes start and finish gently.

## Remarks
This function implements the common "smoothstep" easing curve (a cubic Hermite interpolant). It is a small, pure helper intended to shape pulses or animation envelopes so values accelerate away from 0 and decelerate toward 1 without instantaneous velocity at the ends. Because it is cheap and deterministic, it is suitable for per-frame use in rendering or UI update code.

## Example
```typescript
// Ease a linear interpolation between `a` and `b` using smooth
function lerp(a: number, b: number, t: number) {
  const eased = smooth(Math.max(0, Math.min(1, t))); // clamp t to [0,1]
  return a + (b - a) * eased;
}

// common checks
console.log(smooth(0));   // 0
console.log(smooth(0.5)); // 0.5
console.log(smooth(1));   // 1
```

## Notes
- Inputs are not clamped by this function; pass values outside [0,1] and the polynomial will extrapolate beyond the [0,1] range. Clamp externally if needed.
- The first derivative is zero at t=0 and t=1, which avoids abrupt starts/stops but also means no instantaneous velocity at the endpoints.
- The function is pure and has no side effects; it is appropriate for per-frame evaluation in animation loops.


---

## valueNoise

> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

Returns a 2D value-noise sample for the given coordinates and seed. The function looks up pseudo-random values at the four integer lattice corners surrounding (x, y) using hash2, then blends them with a smooth interpolation function to produce a continuous noise value. Use this when you need a simple, fast procedural noise field (e.g., textures, heightmaps, or as a building block for fractal noise) and you don't require gradient continuity like Perlin noise.

## Remarks
This implements classic value noise: the input coordinates are split into integer cell indices (xi, yi) and fractional offsets (xf, yf). The four corner values (a, b, c, d) come from hash2(xi, yi, seed) and neighbors; smooth is applied to the fractional offsets to generate interpolation weights, and the final result is a bilinear blend using those smoothstep weights. Compared with gradient-based noise, value noise is simpler and faster but can have discontinuous derivatives at lattice boundaries.

## Example
```typescript
// sample a single noise value
const n = valueNoise(12.34, 45.67, 1234);

// common pattern: combine octaves to produce fractal noise
let amplitude = 1, frequency = 1, sum = 0;
for (let octave = 0; octave < 4; octave++) {
  sum += amplitude * valueNoise(x * frequency, y * frequency, seed + octave);
  frequency *= 2;
  amplitude *= 0.5;
}
```

## Notes
- Depends on external helpers: hash2(xi, yi, seed) must return a deterministic numeric value; smooth(t) should return a smooth interpolation weight (commonly a cubic or quintic smoothstep). If those return ranges differ, the output range of valueNoise will vary.
- The function is continuous in value but its derivatives are generally discontinuous at integer lattice boundaries; avoid when smooth gradient continuity is required for shading or derivative-based algorithms.
- For large-scale patterns, scale coordinates (multiply x/y) rather than changing seed; use multiple octaves for more natural variation.

---