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

```typescript
interface PulseState
```


PulseState is a compact interface that describes the configuration for the ThinkingPulse visualization. It encapsulates the color scheme (palette), the animation pattern (pattern), and a generic params bag used by rendering logic to adjust behavior at render time.

## Remarks
PulseState acts as a presentation-focused state contract that decouples styling (Palette) and animation configuration (Pattern) from the rendering implementation. By aggregating these concerns, it lets ThinkingPulse.tsx swap palettes or patterns without changing its rendering code, and enables easier testing and reuse across different pulse variants.

## Notes
- Treat params as an opaque bag; do not rely on runtime structure unless validated.
- Changes to Palette or Pattern definitions may require updates to consumers of PulseState.
- If PulseState is shared across components, prefer immutable usage to avoid unnecessary re-renders.

---

## Bars
> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

```typescript
function Bars(
```


Bars is a React functional component that renders a small collection of animated vertical bars as part of the ThinkingPulse visualization. It receives a seed prop to drive deterministic animation behavior and a paletteStops prop to derive bar colors from a color ramp. Use Bars when you want the bar-based visual element encapsulated as a reusable UI fragment within ThinkingPulse, rather than inlining the rendering logic in the container component.

## Remarks
Bars encapsulates a single visual primitive used by the ThinkingPulse UI. It isolates concerns, allowing the bar animation and color mapping to be developed, tested, and reused independently from the surrounding layout. By accepting a seed, the component can produce repeatable animation patterns for the same seed, which is helpful for consistent visuals in demos or tests. The paletteStops parameter enables easy theming by swapping color ramps without changing the rendering logic.

---

## ThinkingPulse
> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

```typescript
export function ThinkingPulse(
```


ThinkingPulse is a compact React function that renders a pulsing indicator used to convey an ongoing thinking or processing state. It accepts a seed value and an array of paletteStops to drive deterministic variation in the pulse timing and color progression, enabling consistent visuals across renders and themes without relying on external state.

## Remarks
Encapsulating the pulse as its own component isolates animation concerns from layout and business logic, promoting reuse wherever a lightweight loading indicator is needed. The seed parameter enables per-instance uniqueness while remaining deterministic, which is helpful when rendering multiple indicators in a single view. paletteStops lets you tailor the color ramp to the active theme without altering the component implementation.

## Example
```tsx
// Example usage showing common case
import { ThinkingPulse } from './ThinkingPulse';

export function App() {
  return (
    <div>
      <ThinkingPulse seed={42} paletteStops={['#6366f1', '#8b5cf6', '#f472b6']} />
    </div>
  );
}
```

## Notes
- If the indicator is purely decorative, consider marking it as aria-hidden or otherwise provide an accessible live region if it communicates status (e.g., role="status", aria-live="polite").
- Changing the seed will restart or reseed the pulse; keep seed stable if you want a consistent look for a given view.
- Ensure the values in paletteStops are valid color strings with sufficient contrast for accessibility.


---

## buildState
> **File:** `src/webapp/src/components/ThinkingPulse.tsx`  
> **Kind:** function

```typescript
function buildState(seed: number, paletteStops?: readonly RGB[]): PulseState
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `seed` | `number` | — |
| `paletteStops` | `readonly RGB[]` | — |

**Returns:** `PulseState`


It creates a deterministic PulseState from a numeric seed and an optional set of color stops. The RNG order mirrors Avatar.tsx (palette, then pattern, then params) so that the same seed yields the same visual outcome as the old procedural avatar. If paletteStops are provided (and non-empty), the function uses a server-driven Gabriel Sequence palette by constructing a sequence palette with those stops; otherwise it falls back to a locally generated seed palette. The RNG is initialized with mulberry32(seed), a palette is selected via pickPalette(rng), a pattern via pickPattern(rng), and the pattern parameters are initialized with picked.def.init(BARS, rng). The returned PulseState contains the chosen palette, the selected pattern, and the generated params. 

## Remarks

This function centralizes the creation of ThinkingPulse state from a seed, enabling reproducible visuals across sessions. By allowing an optional paletteStops, it supports server-driven color schemes without changing the underlying pattern-selection logic, ensuring consistency between server-provided palettes and local rendering. The explicit RNG ordering provides predictable results that align with the legacy Avatar behavior while remaining flexible for testing and server-driven customization.

## Example

```typescript
// Basic usage with just a seed
const state = buildState(12345);

// With explicit color stops (server-driven Gabriel Sequence palette)
const stops: RGB[] = [
  { r: 255, g: 0, b: 0 },
  { r: 0, g: 255, b: 0 },
  { r: 0, g: 0, b: 255 }
];
const stateWithStops = buildState(12345, stops);
```

## Notes

- If you pass an empty array for paletteStops, the function ignores it and uses the seed-generated palette.
- paletteStops is shallow-copied when constructing the sequence palette to avoid mutating the original stops array.
- The function relies on mulberry32, pickPalette, pickPattern, and the pattern's init function (def.init) to produce the final params; changes to these helpers may affect determinism.

---