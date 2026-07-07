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


FlowParams is a compact TypeScript interface that describes the numeric configuration for a flow-style visual pattern used in the pulse pattern module. It exposes seven properties: angle, bandWidth, bands, period, speed, cx, and cy. By packaging these related values in a single FlowParams object, callers can pass a complete configuration to pattern-generation logic without scattering individual numeric literals, enabling reuse, easier parameterization, and clearer intent when configuring different flow instances.

## Remarks
FlowParams acts as a stable data contract between UI/configuration code and the rendering subsystem. It isolates the concerns of "what parameters exist" from "how they're applied," providing a single place to adjust flow generation behavior and to reuse parameter sets across multiple patterns. It also improves type safety and discovery in IDEs.

## Example
```typescript
// Example usage: configure and render a flow pattern
const params: FlowParams = {
  angle: 30,
  bandWidth: 1.25,
  bands: 6,
  period: 120,
  speed: 1.0,
  cx: 320,
  cy: 240
};

renderFlowPattern(params);
```

## Notes
- Ensure the values are finite numbers; NaN or Infinity will break rendering.
- If consuming JSON at runtime, validate shapes since TypeScript types are erased at runtime and this interface is a compile-time contract.
- Be mindful of coordinate system origin and units when setting cx, cy to position the flow.

---

## NoiseParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface NoiseParams
```


NoiseParams is a lightweight configuration object that groups four numeric parameters used by the noise generation routine in pattern rendering. It encapsulates scale, loopR, seed, and octaves so callers can pass a single configuration object instead of multiple separate arguments, improving readability and reusability.

## Remarks
NoiseParams serves as a contract between the rendering code and its noise generators. It isolates how noise is configured from how it is consumed, enabling swapping implementations or reusing the same settings across multiple patterns.

## Example
```typescript
// Common usage: create a configuration object and pass it to a noise function
const params: NoiseParams = { scale: 0.8, loopR: 200, seed: 42, octaves: 5 };
const value = noiseAt(position, params);
```

## Notes
- Different noise algorithms may interpret "scale" and "octaves" differently; choose values compatible with the generator you use.
- Use a stable seed to reproduce results; changing the seed yields different noise patterns.
- If the object is mutated after creation, results may vary; prefer treating NoiseParams as immutable or creating a new object when changes are needed.

---

## Pattern
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
export interface Pattern<P = unknown>
```


`Pattern<P>` defines a contract for parameterized, spatial-temporal patterns. Implementations provide an init method that creates a parameter object of type P from a given size and a RNG, and a sample method that evaluates the pattern value at coordinates (t, x, y) using those parameters and a time value. This abstraction lets you separate how a pattern is parameterized from how it is evaluated, enabling interchangeable pattern strategies and easier testing.

## Remarks
Pattern acts as an architectural anchor for procedural content generation. By decoupling parameter creation from value computation, it enables swapping different pattern strategies without changing the rest of the system, and it makes testing easier by isolating the sampling logic from parameter state. The generic parameter type P lets each pattern carry its own specialized configuration.

## Example
```typescript
type Params = { value: number };
class ConstantPattern implements Pattern<Params> {
  init(size: number, rng: Rand): Params {
    return { value: size };
  }
  sample(t: number, x: number, y: number, params: Params, time: number): number {
    return params.value;
  }
}
```

## Notes
- Pattern is an interface; you cannot instantiate Pattern directly.
- The init method captures per-pattern state in P, which is then used by sample.
- Be mindful that sample should be pure with respect to input coordinates and time for a given params instance.

---

## PlasmaParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface PlasmaParams
```


PlasmaParams is a lightweight TypeScript interface that groups ten numeric parameters into a single, strongly-typed object used to drive plasma-pattern calculations in the Pulse patterns module. It provides a stable data contract for pattern generators and helpers that need multiple related numbers, reducing parameter clutter in function signatures. The ten properties a, b, c, d, sa, sb, sc, sd, cx, and cy are all numbers; together they describe the configuration state for a given plasma computation and are typically consumed by rendering or calculation logic that positions and shapes the pattern around a coordinate reference (cx, cy).

## Remarks
PlasmaParams exists to decouple pattern-configuration data from the logic that performs the computation. By consolidating related numeric knobs into a single interface, APIs become easier to read, test, and extend (for example, by adding new fields in the future without changing every call site). The cx and cy fields suggest a coordinate reference used by pattern calculations, reinforcing that this object represents spatial configuration as much as numeric tuning.

## Notes
- All fields are plain numbers; TypeScript enforces the shape at compile time, but there is no runtime validation inherent to the interface.
- All properties are required when constructing a PlasmaParams object; supply sensible defaults at call sites if you need optional behavior.
- Changing field names or inferred semantics would be a breaking change for call sites depending on this contract.

---

## PulseParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface PulseParams
```


PulseParams defines a compact contract for the numeric settings that drive a pulsing visual effect. It groups the center coordinates (cx, cy), the width of the wave to render (waveWidth), the number of ripple rings (ripples), and the maximum radius the pulse can expand to (maxRadius), enabling animation code to operate with a single, cohesive parameter object rather than scattering individual numbers.

## Remarks
PulseParams acts as a simple data carrier that abstracts the geometry and look of a pulse. It decouples the animation loop from the specific numeric details, enabling reuse and easier experimentation with different visual styles. This interface expresses intent: a geometric center and a radial expansion culminating at maxRadius, governing how many ripples and how wide each wave appears.

---

## ShimmerCell
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface ShimmerCell
```


ShimmerCell is a lightweight data contract describing a single shimmering segment within a pattern animation. It encodes where the segment appears horizontally (from and to) and when it appears in time (startTime and duration), enabling the rendering pipeline to compose a full shimmer effect from a collection of cells without coupling presentation logic to the data.

## Remarks
Separating the data shape from rendering logic, ShimmerCell acts as a building block for pattern shimmer effects. It lets the UI renderer consume a uniform list of cells to produce synchronized shimmering across a pattern without hardcoding layout details.

## Notes
- Ensure from <= to to avoid invalid ranges.
- Use a consistent time unit for startTime and duration across all cells in a given animation run (e.g., milliseconds or frames).

---

## ShimmerParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface ShimmerParams
```


ShimmerParams is a compact data container that aggregates the parameters required to drive a shimmer effect in the pulse pattern system. It groups a two-dimensional grid of ShimmerCell objects to be updated, a numeric duration window that bounds how long shimmer segments last, and a Rand instance used to introduce controlled randomness. Use this interface when you want to pass all shimmer-related configuration as a single, cohesive object rather than scattering values across multiple function parameters.

## Remarks

By encapsulating pixels, timing, and randomness together, ShimmerParams decouples the rendering logic from the exact values used to orchestrate a shimmer. It enables reuse of the same pixel grid with different timing or randomness seeds, and it makes testing easier by isolating the configuration from the rendering loop. It sits at the boundary between data (pixels) and behavior (durations and RNG), providing a stable contract for shimmer initialization.

## Notes

- minDur and maxDur are plain numbers; there is no compile-time enforcement of minDur <= maxDur; callers should validate before use.
- pixels is a 2D array; ensure rectangular shape if the consumer assumes uniform row lengths.
- rng must be a valid Rand instance; passing something else will likely cause runtime errors.

---

## SpiralParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface SpiralParams
```


SpiralParams is a small data contract that groups the numeric configuration used to render a spiral pattern within the Pulse patterns module. It exposes six parameters: arms (the number of spiral arms), tightness (how tightly the spiral winds), speed (the animation rate), sharpness (the curvature or edge crispness of the arms), and cx/cy (the spiral's center coordinates). This interface is intended as a plain data carrier consumed by the spiral rendering logic.

## Remarks
By isolating these related values, SpiralParams decouples the rendering configuration from the drawing code, enabling reuse across pattern presets and simplifying testing. It defines a stable API boundary that can evolve (e.g., by adding new parameters) without forcing changes on call sites.

## Notes
- No constraints are enforced by the type system; validate ranges at the call site if necessary (e.g., arms should be a positive value).
- cx and cy are interpreted in the drawing coordinate space; ensure they align with the renderer's origin or transforms.
- This is a plain data object; avoid adding methods or behavior; treat as immutable data if possible to simplify reasoning.

---

## WavesParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface WavesParams
```


WavesParams is a lightweight TypeScript interface that describes the numeric parameters controlling a wave-based visual pattern in the Pulse module. It encapsulates angle (orientation), freq (cycles per unit), speed (motion pace), sharpness (edge steepness), and the center coordinates cx and cy, enabling functions that render or animate waves to receive a single, typed configuration object rather than a scattered set of primitive arguments.

## Remarks
WavesParams acts as a simple data contract that decouples the parameter source from the rendering logic. By bundling all wave-related knobs into one object, the code that consumes these values stays generic and testable, and different patterns or effects can be configured by swapping this object. The cx/cy fields define the wave's center in the rendering space, while angle, freq, speed, and sharpness tune the direction, rate, velocity, and crispness of the wave.

## Notes
- Angle unit is not explicit in the interface; ensure the consumer uses a consistent unit (e.g., radians or degrees) with the rest of the rendering pipeline.
- cx and cy refer to the wave's center within the coordinate system used by the pattern; mismatch with the canvas/SVG coordinate space can produce offset visuals.
- All fields are plain numbers with no defaults; callers should provide meaningful values and validation should occur at the call site or in the consuming function.

---

## PatternName
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** type alias

```typescript
export type PatternName = keyof typeof PATTERNS
```


PatternName is a TypeScript type alias that represents the set of valid pattern names by selecting the keys of PATTERNS. It yields a string literal union of the PATTERNS object's property names, providing a compile-time contract for any API that accepts a pattern name. Use PatternName wherever you want to constrain values to known patterns defined in PATTERNS, rather than relying on arbitrary strings.

## Remarks
PatternName centralizes the domain of pattern identifiers. By deriving its values from PATTERNS, it stays in sync with the supported patterns across the codebase; adding or removing a pattern in PATTERNS automatically updates all typings that use PatternName. This reduces drift between runtime data and type-level constraints and helps developers discover valid pattern names via autocomplete.

## Example
```typescript
// Example usage demonstrating the type-safe pattern name
function renderPattern(name: PatternName) {
  // ... implementation
}

// Assuming PATTERNS has a key named 'dash', the following is valid:
const p: PatternName = "dash";
```

## Notes
- There is no runtime representation for PatternName; it's a compile-time type alias derived from PATTERNS.
- For maximum reliability, ensure PATTERNS is declared as a value with literal keys (e.g., const PATTERNS = { ... } as const) so keyof typeof PATTERNS yields stable string literals.

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


Fractal Brownian motion (fBM) is produced by layering multiple octaves of value noise: each octave samples noise at a higher frequency, scales its contribution by a decreasing amplitude, and the results are averaged to yield a smooth, natural-looking value. This function orchestrates that pattern for the given coordinates (x, y) and a seed, returning a normalized, multi-octave noise value. Use fbm when you need richer, multi-scale variation in textures, terrain, or procedural patterns without manually composing the octave loop at each call site.

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


Computes a deterministic pseudo-random value in [0,1] from two integer inputs x and y and a numeric seed. It coerces the inputs to 32-bit integers, mixes them with the seed, and applies a small bitwise scramble to produce a stable, hash-like number suitable for coordinate-based procedural variation without relying on a global RNG.

## Remarks
 hash2 is intentionally stateless: the same (x, y, seed) triple always yields the same output, making it ideal for tile-based patterns, noise generation, or other procedural content that must be reproducible across runs or across different parts of a rendering pipeline. It serves as a building block for higher-level pattern generators by providing a compact, fast, deterministic source of pseudo-random numbers derived from spatial coordinates.

## Notes
- Inputs are coerced to 32-bit signed integers (x|0, y|0, seed|0); values outside that range wrap around.
- The result is in [0, 1], but 1.0 can occur due to the final division by 4294967295.
- Not suitable for cryptographic use or security-sensitive randomness.

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


PickPattern selects a Pattern definition from the available catalog, using the provided RNG to pick randomly when no name is supplied, or returning the explicitly requested named pattern. It returns an object with the selected PatternName and its corresponding Pattern definition. This helper centralizes pattern selection so UI and logic can either request a specific pattern by name or rely on a reproducible random pattern for demonstrations, tests, or procedurally generated content.

## Remarks
By returning both the name and the definition, the function keeps the caller from needing to cross-reference a separate catalog. It also isolates randomness from the rest of the system, allowing deterministic testing by injecting a seeded Rand. This pattern of decoupled selection is helpful wherever a catalog of named patterns is consumed by UI components or procedural generators.

## Example

```typescript
// Random pattern using a seeded RNG
const rng = new Rand(/* seed */ 42);
const { name, def } = pickPattern(rng);

// Specific named pattern
const { name: chosen, def: pattern } = pickPattern(rng, 'Stripes');
```

## Notes
- Invalid PatternName may throw at runtime.
- Seed the RNG to achieve reproducible results.


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


Computes a random value in the half-open interval [min, max) by invoking the supplied Rand function. It returns min + rng() * (max - min), assuming rng yields a number in [0, 1). Rand is a type alias for a zero-argument function that returns a number, enabling you to inject different RNG implementations or deterministic generators for testing.

## Remarks
By abstracting the randomness behind Rand, this function decouples range generation from a specific RNG implementation. It makes testing easier—provide a deterministic Rand—and allows swapping RNG strategies without changing the logic that computes the range.

## Notes
- Assumes rng returns a value in [0, 1). If not, results may fall outside [min, max).
- If max <= min, the expression can produce degenerate or inverted results; validate inputs or clamp accordingly.

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


Smooth is a classic smoothstep easing function implemented as t^2(3−2t). It maps an input t in [0,1] to an eased value that begins at 0 when t = 0 and ends at 1 when t = 1, with gradual acceleration and deceleration to avoid abrupt motion. Use it when you want a non-linear interpolation that preserves endpoints, such as easing animation progress or blending between states instead of applying a linear proportion.

## Remarks
Zero slope at t=0 and t=1 gives a smooth start and end, avoiding sudden changes in velocity when easing between states. This makes it a go-to choice for non-linear progress curves where endpoints must be preserved.

## Example
```typescript
smooth(0) // 0
smooth(0.25) // 0.15625
smooth(0.5) // 0.5
smooth(1) // 1
```

## Notes
- Intended input range is [0,1]; for t outside that interval, the output may fall outside [0,1].
- The derivative is zero at the endpoints, yielding a gentle ease-in and ease-out behavior.
- It is the canonical smoothstep curve and is widely used for graphics, UI transitions, and interpolations where a natural-looking ramp is desired.

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


Produces a smooth, deterministic 2D value-noise sample at (x, y) using the provided seed. It divides space into unit cells, samples the four corners of the cell containing (x, y) via hash2, then blends those corner values with bilinear interpolation guided by the smoothed fractional offsets. The result is a single scalar noise value suitable for procedural textures, height maps, or other patterns that require repeatable, natural-looking variation.

## Remarks

This abstraction isolates the lower-level hash-based corner sampling and interpolation from higher-level pattern composition. By combining hash2 with smooth interpolation, callers can layer multiple frequencies or seeds (e.g., for fractal terrain) without reimplementing the interpolation logic. The function is deterministic: the same x, y, and seed always yield the same output, making it ideal for reproducible rendering.

## Example

```typescript
// Common usage: sample a value-noise value at a point with a given seed
const n0 = valueNoise(2.3, 5.7, 42);
```

## Notes

- Pure function with no side effects; safe to call in tight loops.
- Relies on hash2 and smooth; ensure those utilities behave consistently.
- Output range depends on hash2; not normalized.


---