# Avatar.tsx

> **Source:** `src/webapp/src/components/Avatar.tsx`

## Contents

- [PulseConfig](#pulseconfig)
- [PulseState](#pulsestate)
- [Avatar](#avatar)
- [PulsePlane](#pulseplane)
- [createPulse](#createpulse)

---

## PulseConfig
> **File:** `src/webapp/src/components/Avatar.tsx`  
> **Kind:** interface

```typescript
interface PulseConfig
```


PulseConfig is a small configuration contract for the avatar pulsing effect. It exposes a required numeric seed which drives deterministic variation, and two optional knobs: patternName to select a predefined pulse pattern and paletteName to choose the color palette. Use it when you want repeatable, customizable pulse visuals without hardcoding values in the rendering logic.

## Remarks
PulseConfig decouples the customization surface for the pulsing visuals from the rendering logic, enabling repeatable experiments by varying the seed while keeping patterns and palettes stable. It also provides a simple, forward-compatible surface for introducing new visual variants without changing the existing call sites.

## Example
```typescript
// Minimal configuration
const config: PulseConfig = { seed: 7 };

// Full configuration with pattern and palette (assuming a valid PatternName value is available)
const configFull: PulseConfig = {
  seed: 42,
  patternName: (PatternName as any).Wave,
  paletteName: "Sunset"
};
```

## Notes
- seed is required and must be a finite number; it governs deterministic variation of the pulse.
- patternName and paletteName are optional; when omitted, sensible defaults are used.
- ensure the PatternName value you supply is valid for the current set of available patterns.

---

## PulseState
> **File:** `src/webapp/src/components/Avatar.tsx`  
> **Kind:** interface

```typescript
interface PulseState
```


PulseState is a TypeScript interface that captures the runtime state needed by the avatar's pulsing effect in the Avatar.tsx component. It pairs a current Pattern with its runtime params and color Palette and exposes the actual binary data buffer and the GPU texture used for rendering. This abstraction allows the rendering code to treat a single object as the source of truth for a given pulse instance, simplifying updates and handoffs between stages of the render loop.

## Remarks
PulseState acts as a lightweight data contract between pulse-generation logic (Pattern and params) and the rendering path (palette, data, texture). It decouples pattern computation from visualization, enabling reuse of the same PulseState across frames or avatar instances while the underlying pattern or palette evolves. The interface does not prescribe mutation semantics, leaving updates to the surrounding system.

## Notes
- The params field is of type unknown; callers must narrow its type before use to avoid runtime errors.
- If data is mutated, callers should refresh or upload the corresponding texture to keep rendering in sync.

---

## Avatar
> **File:** `src/webapp/src/components/Avatar.tsx`  
> **Kind:** function

```typescript
export function Avatar(
```


Renders a deterministic user avatar as a React functional component. Avatar accepts seed, pattern, and palette props to influence its appearance; the seed drives the generation so the same value produces the same avatar across renders. Use this component when you want a per-user avatar without storing images, and you wish to vary visuals with pattern and color palette.

## Remarks
Avatar encapsulates the avatar-generation logic behind a reusable UI unit. By centralizing avatar rendering, the app ensures consistent visuals for the same seed across the UI and hides the generation algorithm behind a simple API. This makes it easy to swap out the underlying rendering strategy without changing callers.

## Example
```typescript
<Avatar seed="user-123" pattern="grid" palette="cool" />
```

## Notes
- Changing the seed changes the avatar; it’s intended to be deterministic per seed.
- If the component renders in large lists, consider memoization or stable keys to minimize re-renders.

---

## PulsePlane
> **File:** `src/webapp/src/components/Avatar.tsx`  
> **Kind:** function

```typescript
function PulsePlane(
```


PulsePlane is a small React functional component that renders a pulsing plane animation as part of the avatar UI. It accepts a single prop, config, which likely governs visual aspects such as size, color, and animation duration. Use PulsePlane when you want a reusable, consistent pulsing indicator around avatars instead of duplicating animation code in multiple components.

## Remarks

By encapsulating the pulse effect, PulsePlane decouples decorative animation from layout logic, making it easy to theme and reuse across the app. It serves as a visual attention cue that doesn't carry state beyond presentation, so it should be used where a lightweight, non-interactive glow is desired.

## Example

```tsx
import PulsePlane from './PulsePlane';

function AvatarWithPulse() {
  return <PulsePlane config={{ size: 40, color: '#3b82f6', durationMs: 1500 }} />;
}
```

## Notes

- Decorative only: PulsePlane conveys a visual effect and should not be relied on for conveying status; consider adding ARIA labels if you repurpose it into a functional control.
- Ensure the pulse does not block interactions; place it behind interactive content or configure pointer events accordingly.
- If many avatars render PulsePlane simultaneously, monitor performance and reuse the component where possible.

---

## createPulse
> **File:** `src/webapp/src/components/Avatar.tsx`  
> **Kind:** function

```typescript
function createPulse(
```


Creates a pulsing animation configuration for avatars or avatar-like UI elements. It accepts a seed for deterministic variation, a patternName to select the pulse pattern, and a paletteName to choose the color tokens. Use this helper when you want consistent, themed pulsing visuals across Avatar components instead of duplicating animation setup logic inline.

## Remarks
By encapsulating the calculation of animation timing, scale, and color through seed/pattern/palette, this function centralizes the visual vocabulary for pulsing effects. It helps maintain consistency across avatars and makes it easy to swap patterns or palettes from design tokens without touching rendering code.

## Example
```typescript
// Most common usage
const pulse = createPulse({ seed: 42, patternName: 'breathing', paletteName: 'emerald' });
// Use pulse to configure the Avatar's pulsing style (e.g., via CSS variables or a style prop)
```

## Notes
- Keep seed stable per user/session to preserve visuals across renders.
- If patternName or paletteName are unsupported, a sensible default is used.
- This function is pure and has no side effects.

---