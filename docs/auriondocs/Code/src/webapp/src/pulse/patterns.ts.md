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


FlowParams is a lightweight data contract that describes the configuration for a flow-style decorative pattern used by the pulse rendering system. It groups numeric parameters such as the direction (angle), band characteristics (bandWidth and bands), the animation cadence (period), movement speed (speed), and the origin for placement (cx, cy). By aggregating these fields into a single FlowParams object, code can pass pattern configuration between functions cleanly and reuse the same parameter set across different parts of the rendering pipeline.

## Remarks
FlowParams exists to encapsulate related configuration for a flow-like visual pattern, separating concerns between shape definition and rendering logic. It helps keep interfaces stable when pattern computations evolve, and supports reuse across components that consume these parameters. The interface acts as a pure data carrier; it does not implement behavior or enforce validation, so callers should apply any necessary checks at the boundaries of the system.

## Notes
- The interface does not enforce value ranges or units; consumers should validate values as needed for their rendering context.
- FlowParams is a plain object; if you share it across asynchronous boundaries or multiple renderers, consider immutability (e.g., freezing the object or copying before mutation) to avoid subtle bugs.
- No default values are provided; ensure all required fields are supplied by the caller before use.

---

## NoiseParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface NoiseParams
```


NoiseParams is a compact data contract that groups four numeric parameters used to configure a noise-based pattern generation routine. It provides a single, strongly-typed object containing scale, loopR, seed, and octaves so callers can pass consistent noise configuration to the pattern logic without scattering individual numbers.

## Remarks
NoiseParams acts as a shared configuration object that decouples the noise-generation algorithm from its callers, enabling reuse and predictable variation by adjusting a single seed. Centralizing scale and octaves promotes consistent texture across patterns, while loopR is a numeric parameter whose exact interpretation depends on the consuming implementation. Seed ensures reproducible results across renders.

---

## Pattern
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
export interface Pattern<P = unknown>
```


Pattern defines a pluggable contract for generating numeric samples over a temporal-spatial domain. Implementations provide an init method that derives a parameter object P from a given size and a RNG, and a sample method that computes a numeric value using the coordinates (t, x, y), the stored parameters, and a time value. Consumers reach for Pattern when they want interchangeable, parameterized sampling strategies—precomputing parameters once via init and then evaluating samples efficiently via sample across frames or tiles.

## Remarks
Pattern serves as a strategy boundary for procedural patterns in the system. By separating parameter initialization from per-sample evaluation, it allows the same consumer code to switch between different pattern algorithms without changes to the calling site. The generic P enables implementations to embed whatever state is necessary (e.g., seed, frequency, modulation settings) while keeping the interface stable.

---

## PlasmaParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface PlasmaParams
```


PlasmaParams is a TypeScript interface that defines the shape of a plain object carrying ten numeric values used to configure plasma-pattern calculations in the web application's pulse pattern module. It serves as a single, strongly-typed container for related parameters so functions that generate or manipulate plasma visuals can accept one argument rather than ten separate numbers.

## Remarks
By grouping these fields (a, b, c, d; sa, sb, sc, sd; cx, cy) into a single parameter object, the interface reduces boilerplate, clarifies intent, and makes parameter sets reusable across different plasma computations. The exact meaning of each field is algorithm-dependent, but together they express primary coefficients, secondary modifiers, and the pattern's center coordinates, enabling flexible, testable configurations.

## Example
```typescript
// Example: construct a plasma parameter set
const params: PlasmaParams = {
  a: 1.0, b: 0.5, c: -0.25, d: 2.0,
  sa: 0.8, sb: 0.2, sc: 1.1, sd: -0.5,
  cx: 320, cy: 240
};
```

## Notes
- All ten fields are required; the interface does not permit optional properties. Ensure all values are provided at compile time.
- This interface represents a data contract; introducing new fields would require updates to call sites and dependent code.

---

## PulseParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface PulseParams
```


PulseParams encapsulates the configuration for a ripple/pulse pattern, specifying the center coordinates (cx, cy), the visual width of the wave (waveWidth), the number of ripples to render (ripples), and the maximum radius the pattern may reach (maxRadius). It is intended to be passed as a single object to rendering logic that draws or animates pulses, rather than forwarding five separate numeric arguments.

## Remarks
This interface serves as a compact data contract between the pulse-rendering component and the UI layer. By grouping related numeric parameters into one PulseParams object, developers gain reusable presets and cleaner call sites, and can extend the configuration with additional properties without touching multiple call sites.

## Notes
- All fields are numeric values representing rendering-space quantities; validate non-negativity as appropriate for the consumer.
- This interface defines data shape only and does not implement behavior or rendering logic.

---

## ShimmerCell
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface ShimmerCell
```


ShimmerCell describes a single segment of a shimmer animation, carrying numeric bounds and timing information. It defines four values: from, to, startTime, and duration, which together model the position range and when the segment should begin and how long it lasts. A developer would create or pass around ShimmerCell objects when constructing or configuring the shimmer pattern in the Pulse UI, such as assembling multiple cells to form a complete shimmer effect or transferring timing data between pattern logic and rendering code.

## Remarks
ShimmerCell acts as a lightweight data contract used by the pattern engine to represent a shimmer segment. By keeping timing (startTime, duration) separate from presentation, it supports composing complex shimmer animations from multiple cells without embedding rendering details.

---

## ShimmerParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface ShimmerParams
```


ShimmerParams is a configuration object that describes how a shimmering effect should be rendered on a grid of pixels. It bundles together a 2D array of ShimmerCell instances (pixels), a non-negative duration range for shimmer transitions (minDur and maxDur), and a random-number generator (rng) to drive per-cell variation. Use this interface when you need to pass a single, cohesive shimmer configuration to a generator or renderer instead of supplying multiple scattered parameters.

## Remarks
ShimmerParams isolates data shape from rendering logic, enabling reusable shimmer generators to operate with a single payload. It combines the per-cell state container (ShimmerCell[][]) with the timing and randomness controls, so changes to the grid or the timing model don't require changes to the consumer code. It relies on ShimmerCell to express per-cell visual state and on Rand to provide deterministic randomness when seeded.

## Notes
- Validate duration bounds (minDur <= maxDur and minDur >= 0) before use; the interface does not enforce these constraints.
- The rng is a shared randomness source; reuse a single Rand instance for reproducible shimmer visuals; swapping it mid-flight may produce inconsistent results.

---

## SpiralParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface SpiralParams
```


SpiralParams is a compact configuration object that describes a multi-arm spiral's geometry and animation, including how many arms it has, how tight the spiral is, how fast it animates, the perceived sharpness of the arms, and the center coordinates. Developers reach for this interface when parameterizing a spiral pattern in the Pulse pattern renderer, allowing a single, typed payload to drive rendering logic rather than passing multiple discrete numbers around.

## Remarks
SpiralParams serves as a data contract between the UI-driven configuration and the rendering pipeline. By consolidating these numeric controls into one object, it makes it easy to swap different spiral configurations at runtime, experiment with visual styles, or animate changes smoothly. It also decouples the drawing routine from hard-coded constants, improving testability and reusability across patterns.

## Notes
- The interface carries no validation; callers should enforce sensible ranges (e.g., arms > 0) to avoid unexpected rendering.
- Be mindful of object mutability if SpiralParams is shared across animation frames; prefer creating new instances when tweaking values to avoid unintended drift.

---

## WavesParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface WavesParams
```


WavesParams is a compact interface that groups six numeric parameters used to configure a wave-based pattern in the web app. It exposes angle (the wave direction in radians), freq (spatial frequency), speed (propagation speed), sharpness (waveform contour), and cx/cy (the pattern's center coordinates). This interface is intended to be passed to rendering routines that draw or animate waves, giving them a single cohesive parameter object rather than a scattered set of values.

## Remarks
WavesParams acts as a parameter bag that decouples the rendering engine from calling-site internals, enabling reuse across different patterns and components. Centralizing these related settings makes it easier to swap in new configurations, test variations, and serialize parameters for storage or transmission.

## Notes
- No runtime validation is included; ensure values are finite numbers and within expected ranges before feeding them to rendering code.
- It's a plain data container (no methods). If you need behavior, wrap it or create a helper function to create parameter objects.

---

## PatternName
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** type alias

```typescript
export type PatternName = keyof typeof PATTERNS
```


PatternName is a TypeScript type that represents the set of valid pattern identifiers by taking the keys of the PATTERNS registry. It lets APIs accept a pattern name in a type-safe way, preventing typos and mismatches by constraining values to known PATTERNS keys rather than using a plain string.

## Remarks
PatternName serves as a semantic alias over the runtime PATTERNS object. By deriving its values from the actual keys of PATTERNS, it stays in sync with the available patterns and reduces duplication. This promotes safer, self-documenting code when a function or component accepts a pattern name.

## Notes
- This type is a string-literal union derived from PATTERNS; adding or removing keys updates PatternName automatically.
- It does not create new runtime values; it's only a compile-time type that constrains values to known keys.

---

## fbm
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

```typescript
function fbm(x: number, y: number, seed: number, octaves: number): number
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `x` | `number` | — |
| `y` | `number` | — |
| `seed` | `number` | — |
| `octaves` | `number` | — |

**Returns:** `number`


Fractal Brownian motion (fbm) noise is produced by layering multiple octaves of valueNoise at increasing frequency and decreasing amplitude, then normalizing by the sum of amplitudes. For each octave i (0-based), the function samples valueNoise at coordinates (x * freq, y * freq) with seed offset by i * 17, scales the sample by the current amplitude, and accumulates the result. After all octaves, it divides the total by the sum of amplitudes to produce a single value that reflects detail across scales. This yields smoother, more natural textures or terrain-like variations compared to a single octave of noise. Use fbm when you want richer patterns and you need deterministic results for the same inputs by fixing seed; adjust octaves to trade detail for performance.

## Remarks
fbm is a convenient abstraction that encapsulates the standard multi-octave sampling pattern over valueNoise. It hides the per-octave loop and normalization, making it easy to reuse stable fractal noise behavior across different parts of the codebase. The implementation decorrelates octaves by using distinct seeds per octave via seed + i * 17, which helps reduce repetitive artifacts. Be mindful that more octaves increase CPU cost; choose octaves to balance detail with performance. The output range depends on valueNoise and the chosen seed; the normalization keeps the result in a consistent range assuming valueNoise is reasonably bounded.

## Example
```typescript
// Example: 2D fractal noise at position (12.34, 56.78) with 6 octaves and a fixed seed
const n = fbm(12.34, 56.78, 12345, 6);
```

## Notes
- Octaves must be >= 1; passing 0 octaves yields NaN due to division by zero.
- More octaves increase computation; use the minimum needed to achieve the desired roughness.
- The pattern is deterministic for a given (x, y, seed, octaves); to animate noise, vary x/y over time or change the seed per frame.

---

## hash2
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

```typescript
function hash2(x: number, y: number, seed: number): number
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `x` | `number` | — |
| `y` | `number` | — |
| `seed` | `number` | — |

**Returns:** `number`


Computes a deterministic, non-cryptographic value from three integers (x, y, seed). The return value is a pseudo-random-looking number in the range [0, 1), intended for per-coordinate variation in procedural patterns without pulling in a full RNG.

## Remarks
Provides a lightweight, repeatable seed per coordinate to drive pattern variation without external dependencies. It relies on 32-bit integer arithmetic (Math.imul) to keep results stable across JavaScript engines. Given the same inputs, the output is always the same; changing any input changes the result. This is not suitable for cryptographic purposes or security-critical randomness.

## Notes
- Not cryptographically secure; suitable only for non-security-critical randomness in visuals.
- The value is in the inclusive range [0, 1]; edge cases may yield 0 or, rarely, 1 depending on intermediate results.


---

## pickPattern
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

```typescript
export function pickPattern(rng: Rand, name?: PatternName):
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `rng` | `Rand` | — |
| `name` | `PatternName` | — |


pickPattern selects a Pattern for the pulse pattern system. It accepts a RNG (Rand) and an optional PatternName, and returns an object containing the chosen name and its corresponding Pattern definition. If you supply a name, the function resolves and returns that specific pattern; if you omit the name, it uses the provided RNG to pick a random available pattern and returns both the name and its definition.

## Remarks
By centralizing the mapping from PatternName to Pattern, pickPattern isolates pattern selection from callers and other generation logic. It enables deterministic, testable generation when seeded RNGs are provided, and serves as the primary hook for obtaining a concrete Pattern given either a requested name or a random choice. It relies on the Rand instance, the PatternName type, and the Pattern interface, coordinating them to deliver a ready-to-use pattern description and metadata in a single return value.

---

## range
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

```typescript
const range = (rng: Rand, min: number, max: number) => min + rng() * (max - min)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `rng` | `Rand` | — |
| `min` | `number` | — |
| `max` | `number` | — |


Produces a random value uniformly distributed within [min, max) by applying a linear interpolation scaled by the provided Rand function. Pass a Rand implementation to range when you want randomness that is pluggable and testable, rather than using a global RNG directly.

## Remarks
This function is a tiny utility around linear interpolation: it decouples the randomness source (the Rand function) from the numerical interval you want to sample. By injecting RNGs, you can achieve reproducible results for patterns or tests, and swap in different distributions if needed without changing the caller.

## Example
```ts
// Example: deterministic RNG that always returns 0.5
const half: Rand = () => 0.5;
const value = range(half, 0, 10); // 0 + 0.5 * (10 - 0) = 5
```

## Notes
- Be aware that the result is scaled by rng() and thus depends on the RNG’s typical output range. With a standard Rand that yields values in [0, 1), the result lies in [min, max]. If rng() can return 1, max is reachable as well.
- If your Rand implementation ever returns values outside [0, 1], the produced value may fall outside the [min, max] interval.


---

## smooth
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

```typescript
const smooth = (t: number) => t * t * (3 - 2 * t)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `t` | `number` | — |


Implements a classic smoothstep easing function. Given a normalized input t in [0,1], it returns t^2*(3-2t), yielding a smooth 0→1 progression that provides a gentle start and end for interpolations.

## Remarks
This function offers a simple, monotone ease-in-out profile with zero slope at both ends, making it ideal for smoothly interpolating values without abrupt changes. It is self-contained and deterministic, requiring only the input t to produce a predictable eased value.

## Example
```typescript
const t = 0.25;
const value = smooth(t); // 0.15625
```

## Notes
- Input values outside [0,1] are not constrained; clamp t to [0,1] if you require a strict 0→1 mapping.

---

## valueNoise
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** function

```typescript
function valueNoise(x: number, y: number, seed: number): number
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `x` | `number` | — |
| `y` | `number` | — |
| `seed` | `number` | — |

**Returns:** `number`


valueNoise computes a deterministic 2D noise value at coordinates (x, y) by sampling the four lattice corners of the unit cell that contains (x, y) using a seed-based hash function, then smoothly interpolating those corner values with a smoothing function applied to the fractional parts of x and y. The resulting scalar is suitable for generating procedural textures or patterns where a gently varying field is required, and the seed parameter allows varying the pattern while preserving determinism for identical inputs.

## Remarks
Used as the fundamental 2D noise primitive in procedural textures and pattern systems, valueNoise provides a simple, deterministic surface from which more complex textures can be built by layering octaves or combining with other pattern generators. It encapsulates the standard approach of hashing cell corners and interpolating to produce a smooth value across space, keeping the implementation local to the coordinates and seed. Consumers can swap the hash or interpolation function to change the noise characteristics without altering the call sites.

## Notes
- Not a cryptographic random source; suitable for visuals but not security.
- The result depends on the concrete implementations of hash2 and smooth used by the surrounding code base; changes to those affect outputs.
- For richer textures, layer multiple octaves or adjust input scale; this function provides the basic primitive that higher-level noise combines.

---