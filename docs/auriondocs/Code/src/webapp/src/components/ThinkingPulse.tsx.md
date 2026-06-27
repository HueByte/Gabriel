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

Represents the state needed to render a single "thinking" pulse: the selected color palette, the pulse pattern, and any pattern-specific parameters. Use this interface when constructing, passing, or updating the pulse configuration consumed by the ThinkingPulse rendering logic.

## Remarks
This type consolidates three pieces of data that are commonly passed together into the ThinkingPulse component or its helper functions: Palette selects colors, Pattern describes the animation/shape behavior, and params carries any extra configuration that a particular Pattern implementation might require. Keeping them in one object simplifies prop passing and makes it straightforward to swap or serialize the whole pulse state.

## Example
```typescript
// Example placeholders — replace with real Palette/Pattern instances
const myPalette: Palette = /* ... */;
const myPattern: Pattern = /* ... */;
const myParams = { speed: 1.2, intensity: 0.8 };

const state: PulseState = {
  palette: myPalette,
  pattern: myPattern,
  params: myParams
};

// Pass into a renderer or component
// <ThinkingPulse state={state} />
```

## Notes
- params is typed as unknown by design: callers must narrow or validate its shape before using it (type guards or runtime checks). 
- Palette and Pattern objects may be shared; avoid mutating them in-place if they are reused elsewhere — treat them as immutable or clone when needed.
- If you need stronger typing for params for a specific pattern, create a union or a pattern-specific state type that refines params to the expected shape.

---

## Bars

> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

```typescript
function Bars(
```


Bars is a function declared in ThinkingPulse.tsx that accepts a single object parameter with properties `seed` and `paletteStops`. The provided source contains only the parameter list; the function body, return type and concrete behavior are not available in the supplied code.

## Remarks
This symbol appears to be a small, purpose-specific abstraction that takes a randomness or determinism seed and a set of color/palette stops — likely to drive some visual output (for example, a bar visualization) inside the ThinkingPulse component. Because the implementation is missing, callers should consult the component's definition or its usages to confirm expected prop types and rendering behavior.

## Notes
- The implementation and return value are not present in the provided source; documentation cannot assert whether this is a React component, utility function, or something else.
- Prop types are unknown; reasonable guesses are that `seed` is a number or string used to seed deterministic visuals and `paletteStops` is an array describing color stops, but these are assumptions and should be verified in the full source.
- Check call sites in ThinkingPulse.tsx (or repository-wide usages) to determine lifecyle, prop validation, and side effects before relying on this symbol.

---

## ThinkingPulse

> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

A React functional component exported from src/webapp/src/components/ThinkingPulse.tsx that accepts a props object with the keys `seed` and `paletteStops`.

## Remarks
The provided source is incomplete, so the concrete behavior, return value (JSX shape), prop types, and side effects are not available in this snippet. From the prop names alone: `seed` is likely used to control deterministic randomness and `paletteStops` to supply color/gradient configuration, but callers should consult the component implementation or its type definitions to confirm expected types and semantics.

## Example
```typescript
// Example usage — adapt types/shapes according to the component's actual implementation
<ThinkingPulse seed={12345} paletteStops={[{ offset: 0, color: '#fff' }, { offset: 1, color: '#000' }]} />
```

## Notes
- Implementation and prop type information were not provided in the source snippet; verify the exact prop shapes in src/webapp/src/components/ThinkingPulse.tsx before using.
- If `seed` controls deterministic animation or randomness, changing it frequently may force visual resets or re-renders.
- The shape and expected entries of `paletteStops` are unknown here — commonly this is an array of color-stop objects or tuples; passing an unexpected shape may cause runtime errors or no visible effect.

---

## buildState

> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

Constructs a deterministic PulseState from a numeric seed and an optional sequence palette. Use this when you need the same visual pattern that the legacy Avatar generator would produce for a given seed, but optionally want to replace the palette with server-provided Gabriel Sequence color stops.

## Remarks
This function mirrors Avatar.tsx's RNG ordering (palette, then pattern, then params) so the same numeric seed yields the same pattern as the old procedural avatar generator. When paletteStops is provided and non-empty, those stops replace the locally chosen palette so the component's colors can follow a server-driven Gabriel Sequence while still keeping pattern determinism. The paletteStops array is shallow-copied into the returned Palette to avoid directly exposing the provided array reference.

## Example
```typescript
// Use the default palette derived from the seed
const stateA = buildState(123456);

// Override the palette with server-provided RGB stops
const sequenceStops = [ { r: 255, g: 0, b: 0 }, { r: 0, g: 128, b: 255 } ];
const stateB = buildState(123456, sequenceStops);
```

## Notes
- Passing an empty array ([]) as paletteStops will cause the function to fall back to the seed-derived palette; only a non-empty array triggers the override.
- The provided paletteStops array is shallow-copied (spread) into the returned Palette; the copy prevents direct mutation of the original reference but does not deep-clone nested objects.
- The RNG used (mulberry32) is deterministic but not cryptographically secure; use it only for reproducible visuals, not for security-sensitive randomness.
- seed should be a numeric value; identical seeds produce identical returned PulseState objects (assuming the same code/path).

---