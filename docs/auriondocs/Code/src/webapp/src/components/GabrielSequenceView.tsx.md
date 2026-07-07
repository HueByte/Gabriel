# GabrielSequenceView.tsx

> **Source:** `src/webapp/src/components/GabrielSequenceView.tsx`

## Contents

- [GabrielSequenceViewProps](#gabrielsequenceviewprops)
- [GabrielSequenceView](#gabrielsequenceview)
- [draw](#draw)

---

## GabrielSequenceViewProps
> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** interface

```typescript
interface GabrielSequenceViewProps
```


GabrielSequenceViewProps defines the configuration the GabrielSequenceView consumes to render a Gabriel sequence. It selects which sequence to render via source (a specific conversation’s sequence or a project’s shared sequence), allows forcing a refetch via refreshKey, sets the render size with size, and exposes onSequenceLoaded to let the parent derive UI accents from the loaded sequence.

## Remarks

By separating rendering configuration from data-fetching concerns, this interface clarifies intent and makes it easy to swap endpoints or drive theming without touching the view’s internal logic. The onSequenceLoaded callback is the integration point for server-driven theming: after a successful fetch, you can derive a shared palette (gradients, thought-pulse colors, link tints) from the sequence data.

## Notes

- Changing refreshKey triggers a refetch; accumulate or debounce updates to avoid unnecessary network requests.
- The default display size is 200 pixels (square) to align with the prior Three.js avatar; if you provide a different size, ensure your surrounding layout accommodates the change.

---

## GabrielSequenceView
> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

```typescript
export function GabrielSequenceView(
```


GabrielSequenceView is a React functional component that renders a visualization of a Gabriel sequence from the provided source data. It accepts a source prop, a refreshKey to trigger updates, a size prop (defaulting to 200), and an onSequenceLoaded callback to signal when the sequence visualization has finished loading. Use this component when you want a reusable, declarative UI element to display Gabriel sequence data within the web application instead of embedding rendering logic directly in pages or containers.

## Remarks
GabrielSequenceView encapsulates the presentation concerns of the Gabriel sequence visualization, isolating data input (source) and post-load notification (onSequenceLoaded) from layout concerns. It provides a stable, reusable UI primitive that can be composed with other components or included in higher-level sequence dashboards. The default size of 200 ensures a compact footprint by default, while callers can adjust size to fit different layouts.

---

## draw
> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

```typescript
const draw = (now: number) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `now` | `number` | — |


Renders a single frame of the Gabriel sequence visualization by interpolating colors between two consecutive palette-indexed frames. Given the current timestamp, it determines the two frames to blend, computes a linear interpolation factor, and writes the resulting RGB values to the image data buffer before presenting it on the canvas. This per-frame callback is intended to run inside a requestAnimationFrame loop to create a smooth, looping color morph across the sequence.

## Remarks
Isolates per-pixel color math into a compact, high-frequency render step. By blending between frames rather than snapping to the nearest frame, it produces a fluid motion that still respects the palette-driven color map.

Alpha channel is not updated on every frame and is assumed to be initialized elsewhere; if per-frame transparency is required, update the alpha channel inside the loop.

Frame wrap-around is achieved with modular arithmetic on frame indices, enabling a seamless loop across FRAMES frames.

## Notes
- Alpha channel is static per frame; ensure the imageData alpha values are initialized prior to starting the loop.
- This function mutates shared buffers (data, imageData) on every frame; avoid allocations inside the loop for performance.
- It relies on several external refs and constants (sequenceRef, startTimeRef, FRAME_DURATION_MS, FRAMES, PIXEL_COUNT, data, imageData, ctx); ensure they exist and are initialized before calling draw.

---