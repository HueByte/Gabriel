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


FlowParams is a TypeScript interface that describes the configuration object used by the flow pattern in the Pulse web app. It collects seven numeric values (angle, bandWidth, bands, period, speed, cx, cy) into a single parameter bundle that pattern renderers can consume to compute layout, timing, and motion.

## Remarks
This interface centralizes the configuration for the flow primitive, ensuring that any consumer of a flow pattern receives a complete, typed set of controls. By bundling angle, dimensional parameters, and animation tuning into one object, it reduces API surface area and makes it easy to swap in alternate visual parameters without modifying rendering code.

## Example
```typescript
const defaultFlow: FlowParams = {
  angle: 0,
  bandWidth: 4,
  bands: 5,
  period: 1000,
  speed: 1,
  cx: 0,
  cy: 0
};
```

## Notes
- Values are plain numbers; there are no inherent units encoded in the interface. Consult broader documentation or consumers to determine expected units.
- This interface has no defaults; callers or consuming functions must provide all fields or merge in defaults elsewhere.

---

## NoiseParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface NoiseParams
```


NoiseParams represents the configuration for procedural noise used by the pulse-pattern system. It aggregates four numeric parameters: scale, loopR, seed, and octaves. scale controls the spatial granularity of the noise; loopR defines the looping radius or period to produce seamless repetitions; seed makes noise deterministic across runs; and octaves controls how many layers of noise are combined to produce richer texture.

## Remarks
NoiseParams serves as a stable, self-describing configuration bundle that decouples the noise engine from its callers. It promotes reuse and makes it easier to tweak the appearance of patterns by adjusting a single object rather than modifying calls at many sites.

## Notes
- Ensure values are within sensible ranges before use: non-negative scale and octaves, and a positive loopR. Invalid values may cause runtime errors or nonsensical visuals.

---

## Pattern
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
export interface Pattern<P = unknown>
```


`Pattern<P>` is a generic contract for stateful, parameterized pattern generators. It provides init(size: number, rng: Rand): P to create the pattern's internal state and sample(t: number, x: number, y: number, params: P, time: number): number to produce a value from that state at a given position and moment.

## Remarks
Pattern serves as a pluggable generator boundary in rendering or simulation pipelines. By separating initialization from sampling, it lets different pattern implementations share the same usage surface while carrying their own internal state (P). Because init consumes a Rand, the randomness shaping the pattern is captured at initialization, enabling repeatable runs when the RNG is controlled by the caller.

---

## PlasmaParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface PlasmaParams
```


PlasmaParams is a compact TypeScript interface that bundles ten numeric configuration values used to render a plasma-like pattern in the pulse patterns subsystem. It exists to pass related parameters as a single object rather than as a scattered set of arguments, improving readability and reusability across rendering and calculation functions. All fields are plain numbers, representing coefficients and coordinates that downstream code applies when generating the visual effect.

## Remarks
Design-wise, PlasmaParams serves as a cohesive parameter bag that isolates the plasma configuration from the logic that consumes it. It helps decouple pattern configuration from rendering code, enabling easier testing and reuse across different plasma patterns that share the same parameter shape. By grouping a–d, sa–sd, and center coordinates cx, cy, it clarifies which values belong to a plasma-style pattern.

## Example
```typescript
const example: PlasmaParams = {
  a: 1,
  b: 1,
  c: 1,
  d: 1,
  sa: 0.5,
  sb: 0.5,
  sc: 0.5,
  sd: 0.5,
  cx: 320,
  cy: 240
};
```

## Notes
- All properties are required; you must supply values for all ten fields.
- The names a,b,c,d, sa,sb,sc,sd, cx, cy are domain-specific and not self-describing; consult the consuming code to understand their meaning.
- There is no runtime validation implied by the interface; ensure values are within expected ranges before use.

---

## PulseParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface PulseParams
```


PulseParams defines the configuration for a single pulse pattern used by the web app's pulse rendering system. It groups the center coordinates (cx, cy), the visual width of the waveform (waveWidth), the number of ripple rings to render (ripples), and the maximum reach of the pulse (maxRadius), allowing rendering code to produce consistent ripple animations from a given point.

## Remarks
PulseParams acts as a simple data container that decouples geometry from animation logic. By passing this object, the rendering layer can render different pulse patterns without changing the drawing code; it also makes it straightforward to validate and reuse the same parameter shape across multiple patterns. If the design later requires more attributes (e.g., color, duration), they can extend this interface while keeping existing consumers intact.

## Example
```typescript
const pulse: PulseParams = {
  cx: 100,
  cy: 100,
  waveWidth: 8,
  ripples: 3,
  maxRadius: 60
};
// The rendering system consumes PulseParams to draw a ripple pattern from (cx, cy)
```

## Notes
- Values are numeric; ensure non-negative and appropriate ranges to avoid invalid visuals.
- Treat PulseParams as immutable; create new instances rather than mutating existing ones during animation.
- cx/cy refer to the render target's coordinate space; ensure alignment with the view or canvas.

---

## ShimmerCell
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface ShimmerCell
```


Represents a single shimmer segment used in a UI shimmer animation. ShimmerCell captures both the horizontal range (from and to) that the shimmer spans and the timing (startTime and duration) for when that segment lights up. Use it when composing a shimmer pattern, by listing multiple cells to describe the full sequence instead of embedding timing logic in rendering code.

## Remarks

ShimmerCell decouples animation timing from rendering. It enables declarative pattern construction: you can reorder, merge, or stagger cells without touching the drawing code. By modeling shimmer as data, patterns can be precomputed or animated with consistent timing across platforms.

## Example

```typescript
// A minimal shimmer cell defining a short glow interval
const cell: ShimmerCell = { from: 0, to: 0.25, startTime: 0, duration: 120 };
```

```typescript
// A tiny pattern composed of two adjacent shimmer cells
const pattern: ShimmerCell[] = [
  { from: 0, to: 0.25, startTime: 0, duration: 120 },
  { from: 0.25, to: 0.5, startTime: 120, duration: 120 }
];
```

---

## ShimmerParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface ShimmerParams
```


ShimmerParams is a compact configuration object that bundles the inputs required to run a shimmer animation over a grid of pixels. It carries the 2D pixel grid, a minimum and maximum duration for shimmer cycles, and a random number generator to introduce per-cell variation.

## Remarks
This interface serves as a single, cohesive parameter bag for shimmer pattern generation, decoupling animation configuration from rendering logic. By accepting a shared Rand instance, it enables consistent randomness across cells and reusability of timing bounds, which helps keep the shimmer behavior predictable when the same seed is used.

## Example
```ts
const grid: ShimmerCell[][] = [];
const params: ShimmerParams = { pixels: grid, minDur: 50, maxDur: 200, rng: rand() };
```

---

## SpiralParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface SpiralParams
```


SpiralParams is a lightweight, strongly-typed parameter bag that describes a spiral pattern: how many arms, how tight the twist is, animation speed, arm sharpness, and the center coordinates. This interface is consumed by the pattern rendering logic to produce a spiral, so developers reach for it when configuring or mutating a spiral-based pattern as a single cohesive object.

## Remarks
By encapsulating these values in a dedicated type, SpiralParams decouples pattern configuration from the rendering implementation, making it easier to reuse across components and to swap in different spiral configurations without touching the generator code. It also provides a single place to reason about defaults and validation rules for spiral-based visuals. The interface is intentionally a plain data contract—behaviors are implemented elsewhere, walking the values into the math that draws the spiral.

## Notes
- No runtime constraints are enforced by the type itself; validate values before use (e.g., arms > 0, non-negative numeric fields).
- cx and cy specify the spiral's center in the rendering coordinate system; ensure they align with the target canvas or view dimensions.

---

## WavesParams
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** interface

```typescript
interface WavesParams
```


WavesParams is a compact parameter bag that groups the numeric controls for a wave-based pattern: angle, frequency, speed, sharpness, and the center coordinates cx and cy. Use it when you want to pass multiple related settings as a single object to a generator or renderer, instead of threading six separate numbers through the call sites.

## Remarks
By bundling these related values into a single interface, WavesParams provides a stable contract between the pattern consumer and the generator. It improves reusability and readability by keeping pattern configuration cohesive and interchangeable across different components that implement the same visual effect.

## Example
```typescript
const params: WavesParams = {
  angle: 0,
  freq: 1.5,
  speed: 0.75,
  sharpness: 0.6,
  cx: 320,
  cy: 240
};
```

## Notes
- All fields are plain numbers with no runtime constraints; ensure downstream logic validates them if needed.
- This interface describes data shape only and does not prescribe behavior; it should be consumed by a function or class that implements the actual wave-based rendering or computation.

---

## PatternName
> **File:** `src/webapp/src/pulse/patterns.ts`  
> **Kind:** type alias

```typescript
export type PatternName = keyof typeof PATTERNS
```


PatternName is a TypeScript type that represents the set of valid keys defined on PATTERNS. It is defined as keyof typeof PATTERNS, producing a union of string literals corresponding to the PATTERNS keys; developers use PatternName to constrain inputs to actual pattern names and to gain editor autocomplete and compile-time safety when accessing PATTERNS.

## Remarks
By deriving PatternName from PATTERNS keys, this type keeps the consumer in sync with the available patterns without manual maintenance. It acts as a thin, type-safe bridge between the runtime PATTERNS map and its consumers, preventing accidental usage of non-existent pattern names. This abstraction helps centralize the source of truth for pattern identifiers.

## Notes
- Type-level only: PatternName does not exist at runtime; there is no corresponding value.
- Renaming or removing a key in PATTERNS automatically changes the allowed values in PatternName after recompile.


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


Computes fractal Brownian motion by summing several octaves of 2D value noise with progressively higher frequency and lower amplitude, then normalizing by the total amplitude to produce a stable multi-scale value for procedural patterns. Given coordinates x and y, a seed to diversify the noise, and octaves to control detail, fbm yields a smooth, natural variation suitable for textures, terrains, or patterned effects.

## Remarks
fbm blends multiple scales of valueNoise into a cohesive variation rather than relying on a single noise scale. Each octave doubles the frequency and halves the amplitude, with seeds offset by i * 17 to decorrelate octaves and reduce artifacts. Normalizing by the cumulative amplitude keeps outputs in a stable range across different octave counts.

## Example
```typescript
// Example usage: common case
const v = fbm(12.3, 45.6, 7, 6);
```

## Notes
- octaves is treated as the loop bound; non-integer values are effectively truncated due to the loop condition (i < octaves).
- The per-octave seed offset (i * 17) decorrelates octaves; changing this constant changes the texture character.

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


Computes a deterministic, pseudo-random value from two coordinates and a seed. It uses 32-bit integer arithmetic and bitwise mixing to blend the inputs into a single nonnegative value, which is then normalized to the [0, 1] range. Use it when you need a repeatable, seedable random-like value for procedural patterns (textures, noise) without pulling from a global RNG.

## Remarks
This helper is designed for deterministic content generation: the same x, y, and seed always yield the same result, making it ideal for repeatable noise in patterns. The mixing uses integer multiplications and bitwise shifts to avoid simple correlations between input values, while remaining compact.

## Example
```typescript
const r = hash2(12, 34, 7);
console.log(r);
```

## Notes
- Inputs x, y, and seed are coerced to 32-bit integers via bitwise OR with zero (x | 0, y | 0, seed | 0); non-integer inputs are truncated.
- Output is a number in the range [0, 1], with the full interval achievable depending on the packed value.
- Not suitable for cryptographic purposes; this is a fast, non-cryptographic hash intended for procedural-generation uses rather than security-sensitive randomness.

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


pickPattern selects a Pattern using the supplied RNG and an optional PatternName, returning an object with both the chosen name and its corresponding Pattern definition. Call it when you need a ready-to-use pattern and either want to constrain the choice to a known name or prefer a deterministic, RNG-driven random selection for testing, seeding, or UI presentation.

## Remarks
pickPattern centralizes the relationship between pattern identifiers (PatternName) and their concrete definitions (Pattern). It hides the internal storage or registry of available patterns behind a single API, making it easier to swap implementations without changing call sites. By returning both the name and def, it avoids a second lookup and ensures consumers always have the identifier that produced the definition.

## Example
```ts
const { name, def } = pickPattern(rng);
console.log(name, def);
```

## Notes
- If a name is provided, the function will yield the corresponding Pattern for that name; the return value will reflect the chosen name.
- For reproducible results, seed the RNG before calling pickPattern, since randomness affects which pattern is selected when name is not provided.
- The RNG may advance its internal state as a side effect of the call; be mindful of call order if deterministic sequencing matters.

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


Generates a random floating-point number within the specified range from `min` (inclusive) to `max` (exclusive) using the provided random number generator function `rng`. This function scales the output of `rng()`, which is expected to produce a value between 0 and 1, to the desired numeric interval.

## Remarks
This abstraction allows for flexible random number generation within any numeric range while decoupling the source of randomness. By accepting a `Rand` function as input, it supports custom or seeded random generators, facilitating reproducible or specialized random sequences beyond the default global random source.

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


Implements the standard smoothstep easing function: it takes a normalized progress t in [0, 1] and returns t^2(3 - 2t), producing a smooth, S-shaped interpolation with zero slope at both ends. Use it to smoothly interpolate between two values rather than relying on linear progress in animations, UI transitions, or procedural patterns.

## Remarks
This function is clean and deterministic, with no side effects, so it can be freely composed into animation or interpolation code. It encapsulates a widely used easing curve (3t^2 - 2t^3), enabling consistent smoothing across values without repeating math. By plugging smooth(t) into a linear interpolate, you can produce natural-feeling transitions throughout the UI and graphics layers.

## Example
```typescript
// Example: use smooth to interpolate between two numbers with easing
const a = 0;
const b = 100;
const t = 0.25;
const value = a + (b - a) * smooth(t); // ≈ 15.625
```

## Notes
- Assumes 0 <= t <= 1. Outside this range, the result may lie outside [0, 1] and the easing behavior is not guaranteed.

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


Computes a deterministic, seeded 2D value noise value for coordinates (x, y). It locates the containing unit cell by flooring x and y, samples the four corner values using hash2 at (xi, yi), (xi + 1, yi), (xi, yi + 1), and (xi + 1, yi + 1), then blends them with a bilinear interpolation after applying a smoothing function to the fractional offsets (xf, yf). The resulting number is a smooth, continuous value that varies with position and seed, making it suitable for stable procedural textures or patterns in 2D space.

## Remarks
ValueNoise is a deterministic function for a given seed: the same inputs always yield the same output. The bilinear interpolation, combined with the smoothing of the fractional offsets, minimizes artifacts and yields smooth transitions between lattice points. Because the corner values come from hash2, the exact distribution of results depends on hash2's implementation while remaining consistent for the same seed and coordinates.

## Notes
- The output range and distribution depend on the implementations of hash2 and smooth; do not assume the result is confined to [0, 1].
- This function is deterministic and side-effect-free for a fixed seed, but its characteristics can vary with different hash2/smooth definitions.
- If you require more complex textures, consider combining valueNoise with additional octaves or other noise sources to enrich the pattern.

---