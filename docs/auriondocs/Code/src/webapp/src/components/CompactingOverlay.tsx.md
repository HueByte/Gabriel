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


CompactingOverlayProps is the props interface for the CompactingOverlay component. It exposes two optional fields: paletteStops, which is a readonly array of RGB values or null, used for color matching the swirl to the avatar; and messageCount, a number indicating how many messages are folded and displayed in the caption.

## Remarks

CompactingOverlayProps exists to keep presentation details decoupled from the component's behavior. It acts as a small data carrier that callers can populate to influence visuals without changing logic. paletteStops ties the overlay's color palette to the conversation's avatar swirl, enabling a consistent visual identity. The messageCount value controls how many messages are represented as folded in the caption, providing a concise indicator of chat activity.

---

## SwirlProps
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** interface

```typescript
interface SwirlProps
```


SwirlProps is a TypeScript interface that defines the props contract for a swirl‑style overlay component. Its optional paletteStops property accepts a readonly array of RGB color specifications; when provided, these stops configure the color progression used by the swirl rendering. Because the property is optional, consumers may omit it to rely on the component's default color palette.

## Remarks
SwirlProps encapsulates color configuration separate from rendering logic, enabling reuse of the swirl overlay with different color schemes without mutating internal state. The `ReadonlyArray<RGB>` type communicates that the consumer should treat the palette stops as an immutable input and signals to downstream code that the palette is provided from outside. This abstraction makes it easy to swap color schemes by swapping props rather than mutating internal state of the rendering component.

## Notes
- Mutability trap: ReadonlyArray prevents structural mutations but does not freeze the RGB objects themselves. If the RGB instances are shared, changes to their properties could affect other consumers.
- For React usage, pass a stable array reference to avoid unnecessary re-renders when the prop changes are not meaningful.

---

## CompactingOverlay
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
export function CompactingOverlay(
```


CompactingOverlay is a React functional component that accepts a props object with paletteStops and messageCount and renders a compact overlay UI. It is intended to be used when you need a lightweight overlay that conveys color-palette context alongside a small numeric indicator rather than a full, multi-pane component.

## Remarks
A UI primitive for overlay presentation that consolidates color-stop visualization with a message count. It helps centralize the styling and behavior of compact overlays across the app, reducing duplication and ensuring visual consistency.

---

## Core
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Core(
```


Core renders the central portion of the CompactingOverlay UI by consuming the paletteStops prop. It encapsulates the color-stop visuals, so callers can provide palette configuration without pulling in the overlay's surrounding layout.

## Remarks
Core isolates the color-stop rendering logic from the overlay chrome, enabling easier testing and potential reuse in other components that share the same color-stop visuals. By concentrating palette-driven rendering in one component, changes to how color stops are represented stay local to Core and do not affect the overlay structure.

## Notes
- The snippet exposes only the function signature fragment; confirm the full prop types and implementation in the surrounding module to avoid misusing Core.

---

## Swirl
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Swirl(
```


Swirl is a small function component defined in CompactingOverlay.tsx that renders a swirl-shaped visual using a paletteStops prop. By encapsulating the swirl rendering in its own helper, the overlay can reuse this decorative element without duplicating drawing logic, keeping the overlay's layout focused on structure.

## Remarks

Swirl serves as a presentational helper that isolates the swirl-drawing concern from the surrounding layout. This separation makes the overlay easier to test and maintain, and aligns with a UI pattern of composing complex visuals from small, focused components.

## Notes

- Be mindful that paletteStops can trigger re-renders; avoid creating new arrays on every render. If the swirl visual is static, consider memoization or stable references to paletteStops to minimize unnecessary updates.

---