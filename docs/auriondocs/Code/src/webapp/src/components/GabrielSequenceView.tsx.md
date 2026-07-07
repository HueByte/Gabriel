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


GabrielSequenceViewProps defines the props consumed by the GabrielSequenceView component to render a sequence. It specifies the source sequence to fetch (conversations or project-wide shares), an optional refreshKey to trigger a refetch after updates, an optional size for a square display, and an optional onSequenceLoaded callback that exposes the raw GabrielSequence to the parent so visual accents can be derived from the server-driven palette.

## Remarks
GabrielSequenceViewProps acts as a thin bridge between data-fetching concerns and presentation concerns. By isolating the source and refresh mechanism from layout and by providing a hook (onSequenceLoaded) for the parent to align theming, it enables reuse of the same view for different sequence sources while keeping concerns decoupled.

## Example
```typescript
<GabrielSequenceView
  source={SequenceSource.Conversation}
  size={240}
  refreshKey={refreshCounter}
  onSequenceLoaded={(sequence) => applyPaletteFromSequence(sequence)}
/>
```

## Notes
- Changing refreshKey forces a refetch; pass a changing value (e.g., a counter) after actions like sending a message.
- size defaults to 200 if omitted; adjust to fit your layout, keeping in mind this controls the visual footprint of the square render.


---

## GabrielSequenceView
> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

```typescript
export function GabrielSequenceView(
```


GabrielSequenceView is a React functional component that renders a visual representation of a Gabriel sequence from the provided source data. It offers a compact, embeddable surface for UI pages, with a configurable render size and an optional post-load hook. Pass the sequence data via source, use refreshKey to force a re-render when the data or display conditions change, control the visualization footprint with size, and optionally respond to load completion with onSequenceLoaded.

## Remarks
GabrielSequenceView acts as a presentation-layer abstraction that isolates the details of how a Gabriel sequence is drawn from the rest of the application. By taking a raw source and a refreshKey, it lets parent components trigger re-renders without mutating internal state. It also provides a callback hook (onSequenceLoaded) so callers can chain actions (e.g., hide loading UI or start downstream processing) once the sequence is ready. It fits alongside other sequence-related components that share a common prop shape for consistent behavior.

## Notes
- Ensure that source is stable and serializable; if it changes, update refreshKey to guarantee the view reinitializes.
- onSequenceLoaded might fire multiple times if the component remounts or the sequence is reloaded; guard accordingly.

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


Draw is the per-frame render callback that drives a palette-based, frame-sequence animation on a canvas. It computes a looping cycle position from the elapsed time since startTime and, for the current frame, linearly interpolates each pixel's color between the current and next frame using their palette entries, then writes the result into a shared ImageData buffer and paints it with putImageData. Alpha remains fixed from initialization and is not updated on every frame; only the RGB channels are recomputed, producing smooth color transitions across frames. The function then re-schedules itself via requestAnimationFrame to continue the animation.

## Remarks
This function encapsulates the timing, frame selection, and color interpolation for a palette-driven frame sequence, enabling smooth morphing without constructing new textures per frame. By operating on a common ImageData buffer and a single canvas context, it keeps rendering lightweight and predictable, while remaining agnostic to the higher-level component structure that provides the sequence and palette data.

## Notes
- Alpha channel is initialized once and not updated per frame; ensure the initial alpha value matches the desired opacity.
- Interpolated color components are rounded per pixel; this can introduce tiny color deviations over frames but preserves performance and visual stability.


---