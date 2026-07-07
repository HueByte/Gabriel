# CompactingOverlay.tsx

> **Source:** `src/webapp/src/components/CompactingOverlay.tsx`

## Contents

- [CompactingOverlayProps](#compactingoverlayprops)
- [SwirlProps](#swirlprops)
- [CompactingOverlay](#compactingoverlay)
- [Core](#core)
- [Swirl](#swirl)

---

## CompactingOverlayProps
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** interface

```typescript
interface CompactingOverlayProps
```


CompactingOverlayProps defines the optional presentation parameters for the CompactingOverlay component. It exposes two concerns: a color palette (paletteStops) used to color-match the swirl to the avatar, and a count of folded messages (messageCount) displayed in the overlay caption.

## Remarks

This interface exists to separate visual presentation from behavior: it offers a typed contract for optional UI refinements that can be supplied by consumers or higher-level components without changing the underlying overlay logic. By making both properties optional, the component can render with sensible defaults when they are not provided, while allowing precise customization when needed.

## Notes

- paletteStops is optional and may be null. When provided, it is a readonly array of RGB values; callers should not mutate it.
- messageCount is optional and, if present, indicates how many messages are folded and should be reflected in the caption.
- There is no runtime contract here about the range or validation of the values (e.g., non-negativity for messageCount); callers should ensure values align with the UI’s expectations and perform validation if necessary.

---

## SwirlProps
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** interface

```typescript
interface SwirlProps
```


SwirlProps defines the props for rendering the swirl portion of the compacting overlay. It exposes an optional paletteStops property—a read-only array of RGB values—that specifies the color stops used to drive the swirl's gradient when rendering.

## Remarks
Palette configuration is kept separate from rendering logic to allow reusing the swirl across different overlays with different color schemes. By using a readonly array, SwirlProps communicates that callers should not mutate the color stops after they are created, preserving immutability in rendering pipelines.

## Example
```typescript
// Example usage with a custom color palette
const customStops: readonly RGB[] = [
  { r: 255, g: 0, b: 0 },
  { r: 255, g: 165, b: 0 },
  { r: 255, g: 255, b: 0 },
  { r: 0, g: 128, b: 255 }
];

const props: SwirlProps = {
  paletteStops: customStops
};
```

## Notes
- The paletteStops property is optional; provide it when you want to customize the swirl colors.
- Treat the RGB values as opaque color stops; do not mutate the values after creation.

---

## CompactingOverlay
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
export function CompactingOverlay(
```


CompactingOverlay is a React functional component that receives a props object containing paletteStops and messageCount. The signature indicates it renders UI that depends on these two values, typically overlaying a visualization of color stops alongside a counter of messages. Without the full implementation, the precise rendering, interactivity, and styling cannot be determined from the signature alone.

---

## Core
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Core(
```


Core is an internal render helper inside CompactingOverlay.tsx that receives a paletteStops prop and participates in rendering the portion of the UI that reflects those color stops. Use it when you need to understand or adjust how color stops influence the overlay’s visuals without altering the public API of the surrounding component.

## Remarks
Core keeps color-stop rendering isolated from layout, making the visual logic easier to reason about and test. It’s a private implementation detail of CompactingOverlay.tsx, so changes to Core can evolve without affecting external consumers of the component.

## Example
```typescript
// Most common usage inside CompactingOverlay
<Core paletteStops={paletteStops} />
```

## Notes
- Do not mutate paletteStops; treat props as immutable to preserve predictable rendering.
- If paletteStops is large or expensive to compute, consider memoizing its source to avoid unnecessary re-renders of Core.


---

## Swirl
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Swirl(
```


Swirl is a React function component defined in CompactingOverlay.tsx. It accepts a single prop named paletteStops, which by convention carries color stop data used to render a swirl-style visual. In this codebase, Swirl is likely used to render a decorative gradient or animation within the CompactingOverlay UI, allowing callers to customize its appearance by supplying different color stops rather than hardcoding colors.

## Remarks
Swirl encapsulates a visual motif as a reusable unit, enabling consistent styling across the overlay while isolating color-stop logic from layout concerns. It fits alongside other overlay components by exposing a simple, domain-specific prop (paletteStops) to control its rendering.

## Notes
- The exact shape and type of paletteStops are not visible in the snippet; verify the corresponding type/interface in the source to avoid mismatches.

---