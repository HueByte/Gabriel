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


CompactingOverlayProps defines the props accepted by the CompactingOverlay component. It carries optional presentation data: paletteStops supplies the conversation's color stops used to color-match the swirl to the avatar, and messageCount indicates how many messages are folded and should be reflected in the overlay caption. Both properties are optional, allowing the overlay to render with sensible defaults when these details are not provided.

## Remarks
This interface isolates theming data (paletteStops) from content state (messageCount) and the rendering logic of CompactingOverlay. By making paletteStops optional and readonly, it supports theme-aware overlays that can visually align with different avatars without requiring callers to mutate color data. The messageCount field, when provided, communicates folding state to the caption without forcing content-level changes.

## Notes
- paletteStops may be undefined or null; callers should handle absence gracefully.
- paletteStops is readonly; callers must not mutate the array.
- messageCount is optional; when present it should be reflected in the overlay's caption; otherwise, the caption behavior is left to the component's defaults.

---

## SwirlProps
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** interface

```typescript
interface SwirlProps
```


SwirlProps describes the props for a swirl visualization in the CompactingOverlay component. It declares a single optional field, paletteStops, which is a readonly array of RGB values. When provided, these values establish the sequence of color stops the swirl will use; when omitted, the component relies on its default color configuration.

## Remarks
SwirlProps encapsulates color configuration separate from rendering logic, enabling reuse of the swirl visual with different palettes. The use of readonly on the array communicates that callers should not mutate the palette list; while the array is immutable, the contained RGB objects may still be mutable depending on the RGB type definition.

## Notes
- The paletteStops property is optional; the consumer should handle undefined if not provided.
- The array is readonly; do not mutate the array or rely on mutating its elements to alter the palette. If a different palette is needed, pass a new array.
- Ensure the RGB values you supply conform to the project's RGB type definition.

---

## CompactingOverlay
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
export function CompactingOverlay(
```


CompactingOverlay is a React function component that accepts two props: paletteStops and messageCount. It provides a compact, overlay-style UI element intended to visualize a compaction or consolidation phase without taking the user to a new view. The paletteStops prop implies a color-based indicator (such as a gradient or segmented bar) while messageCount conveys the scale of work involved, allowing the component to communicate progress or workload succinctly within the current screen.

## Remarks
By isolating this visual pattern into its own component, the codebase gains a single, reusable representation of the 'compacting' state. It reduces duplication where multiple views need to signal ongoing cleanup or consolidation, and it centralizes any tweaks to color ramps or sizing in one place.

## Notes
- Ensure accessibility: provide appropriate ARIA attributes and avoid trapping focus if the overlay is non-modal.
- Validate props at compile-time and runtime: ensure paletteStops is an array-like structure and messageCount is a non-negative integer; handle missing values gracefully.

---

## Core
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Core(
```


Core is a React function component within the CompactingOverlay UI. It accepts a single prop named paletteStops and is responsible for rendering the color-stop visuals that illustrate the current palette used by the overlay.

## Remarks
By isolating the palette-stop rendering into Core, the UI code keeps concerns separated: Core concentrates on the palette visualization, while the surrounding overlay handles layout and interaction. This makes the palette rendering easier to test, reuse, and swap with alternative representations (for example, static stops or dynamically computed stops) without touching the parent component.

## Notes
- The shape and type of paletteStops aren't shown in the snippet; ensure proper typings to avoid runtime errors.
- If paletteStops changes frequently, consider memoization or stable keys to minimize re-renders.

---

## Swirl
> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Swirl(
```


Swirl is a React function component defined in the CompactingOverlay context. The snippet indicates it accepts a single destructured prop named paletteStops, but the function body is not shown. Because only the signature is available, the actual rendered output, interactions, and side effects cannot be determined from this fragment alone. To understand what Swirl renders or how paletteStops influences its behavior, you would need the full implementation.



---