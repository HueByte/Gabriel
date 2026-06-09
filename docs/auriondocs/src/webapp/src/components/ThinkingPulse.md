# ThinkingPulse.tsx

> **Source:** `src/webapp/src/components/ThinkingPulse.tsx`

## Contents

- [PulseState](#pulsestate)
- [Bars](#bars)
- [ThinkingPulse](#thinkingpulse)
- [buildState](#buildstate)

---

## PulseState

> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** interface

Represents the configuration/state of a single "pulse": it groups the Palette and Pattern objects together with an opaque params field for pattern-specific options. Use this interface when passing, storing, or serializing a pulse's configuration so consumers have a single, predictable shape to work with.

## Remarks
This interface separates visual concerns (palette) and structural/behavioral concerns (pattern) from arbitrary runtime parameters. The params field is intentionally typed as unknown to allow different patterns to define their own shapes without coupling the shared state type to every possible parameter set; callers are expected to perform type-narrowing or casting for their specific pattern.

## Example
```typescript
// Constructing a PulseState for a pattern that expects { speed: number }
type SpeedParams = { speed: number };
const state: PulseState = {
  palette: myPalette,
  pattern: myPattern,
  params: { speed: 120 } as unknown,
};

// Narrowing before use
const maybeParams = state.params;
if (typeof (maybeParams as any).speed === 'number') {
  const params = maybeParams as SpeedParams;
  console.log('speed =', params.speed);
}
```

## Notes
- params is typed as unknown: always narrow or validate it before reading properties to avoid runtime errors.
- Equality/identity: PulseState is a plain object; comparing states for changes should use deep equality or explicit field checks rather than relying on structural inference unless you control immutability.


---

## Bars

> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

Renders a small decorative sequence of vertical bars (commonly used as a "thinking" or progress pulse) whose appearance is derived from a provided seed and color stops. Reach for this component when you want a compact, repeatable bar-based visual tied to a deterministic seed and configurable palette.

## Remarks
Bars centralizes the logic for producing a palette-driven bar visualization that can be reproduced from a seed. The seed is intended to influence the generated layout/animation so the same seed yields the same pattern; paletteStops supplies the colors or gradient stops used to paint the bars. This keeps color mapping and deterministic layout in one reusable component rather than scattering that logic at call sites.

## Notes
- The exact shapes/types of seed and paletteStops are not specified here; inspect the implementation to confirm whether seed should be a number or string and what structure paletteStops expects (e.g., array of color strings or objects).
- Deterministic output depends on the PRNG/derivation algorithm used inside the implementation; if you need cross-platform stability, ensure the seed and algorithm are stable across environments.
- Embedding context (SVG vs. HTML, sizing, CSS) matters for layout and accessibility — check the component's markup if you need specific sizing or ARIA behavior.

---

## ThinkingPulse

> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

```typescript
export function ThinkingPulse(
```


An exported React functional component declared in a TSX file that accepts a props object with two named properties: `seed` and `paletteStops`. The component's implementation (not included in the provided snippet) defines how these props are interpreted and what JSX is returned; consult the source for precise prop types and visual behavior. Use this exported symbol when you need to include the ThinkingPulse UI element in a React tree and supply the required configuration via `seed` and `paletteStops`.

## Remarks
This symbol exposes a small surface for configuring a reusable presentation component through two props. Because the implementation is not present in the provided snippet, the documentation here intentionally avoids assumptions about prop types or rendering details — the file and its implementation should be consulted to learn how `seed` and `paletteStops` are validated and applied.

## Example
```typescript
import { ThinkingPulse } from './components/ThinkingPulse';

// Example usage — verify real prop types in the component source
<ThinkingPulse seed={/* seed value */} paletteStops={/* palette stops array */} />
```

## Notes
- The provided source only includes the function signature; confirm the exact types and semantics of `seed` and `paletteStops` in the implementation before use.
- This is a named export (not a default export); import it with curly braces as shown above.
- The file is a TSX component and therefore returns JSX; ensure usage happens within a React render context.

---

## buildState

> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

Constructs a PulseState deterministically from a numeric seed and an optional override palette. Use this when you need the same visual pulse (pattern, its init parameters, and palette) to be reproducible from a seed — for example to match the legacy procedural avatar generation — and when you may optionally want to force the palette to a server-driven set of RGB stops.

## Remarks
This function mirrors the RNG ordering used by Avatar.tsx (palette, then pattern, then params) so that a shared seed produces the same pattern as the older procedural avatar code. When paletteStops is provided and non-empty it replaces the randomly-picked palette so the resulting PulseState can track an externally supplied (e.g. server-driven) color sequence. The returned object contains: the chosen Palette, the pattern definition (pattern.def) and the params produced by that pattern's init function.

## Example
```typescript
// Use a fixed seed and a server-provided palette override
const seed = 123456;
const serverPaletteStops = [ { r: 255, g: 200, b: 0 }, { r: 10, g: 120, b: 220 } ];
const state = buildState(seed, serverPaletteStops);
// state.palette, state.pattern, and state.params are ready to drive the pulse renderer
```

## Notes
- paletteStops overrides the RNG-picked palette only when it's defined and has length > 0; an empty array falls back to the seeded palette.
- paletteStops is shallow-copied into the returned Palette (stops are spread into a new array), so the caller's array can be mutated afterwards without changing the returned Palette array reference.
- The seed determines all randomness via an internal mulberry32 PRNG; use the same numeric seed to reproduce the same PulseState.
- The pattern init is called with BARS and the RNG (pattern.def.init(BARS, rng)); callers cannot change the BARS value from this function call.


---