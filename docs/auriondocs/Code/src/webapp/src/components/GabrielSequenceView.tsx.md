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

Properties for configuring a GabrielSequenceView component: select which sequence feed to load, control a forced refetch, set the rendered square size in pixels, and receive the raw sequence when a fetch completes. Use this interface when embedding GabrielSequenceView so the parent can choose the data source, trigger refreshes after user actions, and derive shared UI accents from the server-driven sequence.

## Remarks
This small props bag intentionally separates view concerns from parent logic. "source" determines which backend endpoint the view will call (for example a conversation-specific sequence vs. a project-shared sequence). "refreshKey" is a manual change-detection mechanism: incrementing it causes the view to refetch. "size" controls the rendered square pixel dimensions (defaults to 200 to match the prior Three.js avatar). "onSequenceLoaded" lets the parent observe the raw GabrielSequence each time the view successfully fetches it so shared UI accents (gradients, pulse colors, link tints) can be derived from the same server-driven palette.

## Example
```typescript
// Typical usage in JSX
<GabrielSequenceView
  source={sequenceSource}
  refreshKey={fetchCounter}
  size={300}
  onSequenceLoaded={(sequence) => {
    const palette = derivePaletteFromSequence(sequence);
    setUiPalette(palette);
  }}
/>

function derivePaletteFromSequence(sequence: GabrielSequence) {
  // inspect server-provided sequence to pick gradient/colors
  return {/* ... */};
}
```

## Notes
- Changing refreshKey is the intended way to force the component to refetch; toggling other props may not trigger a fetch.
- size is a square pixel measurement; omit to use the default of 200.
- onSequenceLoaded is invoked once per successful fetch — it can be called multiple times over the component's lifetime, so avoid heavy synchronous work in the callback or debounce expensive operations.
- Treat the GabrielSequence passed to onSequenceLoaded as read-only; do not mutate it in-place.

---

## GabrielSequenceView

> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

Renders a visual view of a Gabriel sequence using the provided data source. Reach for this component when you need to display a Gabriel sequence in the UI and want a self-contained renderer that accepts a data source, an optional refresh trigger, a size hint, and a completion callback.

## Remarks
This component abstracts the UI concerns for presenting a Gabriel sequence so callers do not need to manage rendering details. It accepts a `source` (the origin of sequence data), a `refreshKey` to force reloading when external state changes, and a `size` hint (default 200) to control the rendered footprint. Use `onSequenceLoaded` to be notified when the component has obtained/initialized the sequence (for example to synchronize other parts of the UI).

## Example
```typescript
// Typical usage in JSX
<GabrielSequenceView
  source={mySequenceSource}
  refreshKey={cacheBuster}
  size={300}
  onSequenceLoaded={() => console.log('sequence ready')}
/>
```

## Notes
- `size` is a visual hint (defaults to 200) — treat it as the component's intended width/height rather than a data limit.
- Provide a stable `refreshKey` when you need the view to reload in response to external changes; changing the key should cause the component to refresh.
- `onSequenceLoaded` is invoked when the component finishes loading/initializing the sequence; treat it as an asynchronous notification rather than a synchronous return value.

---

## draw

> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

Animates and renders a palette-indexed sequence to a canvas by linearly interpolating between consecutive frames and writing the resulting RGB bytes into an ImageData buffer on each animation frame. Callers reach for this when they want a smooth, continuous cross-frame blend (rather than discrete frame stepping) and are already managing a canvas context, an ImageData/data buffer, and a sequence object referenced by sequenceRef.

## Remarks
Per-frame work: the function computes the current cycle position from the RAF timestamp and an anchored start time, picks the current and next frame indices, computes a linear interpolation factor t in [0,1), and then walks every pixel to blend the palette entries for those two frames into the ImageData byte array. It writes the RGB channels each frame (the alpha channel is intentionally left alone and is expected to be initialized once elsewhere), then pushes the result to the canvas with ctx.putImageData. The function re-schedules itself with requestAnimationFrame, forming a continuous animation loop.

## Example
```typescript
// Typical usage (simplified):
startTimeRef.current = performance.now();
raf = requestAnimationFrame(draw);

// To stop:
cancelAnimationFrame(raf);
```

## Notes
- The function mutates shared outer variables: sequenceRef.current, startTimeRef, imageData/data, ctx, raf and relies on constants FRAME_DURATION_MS, FRAMES and PIXEL_COUNT being defined and valid (> 0).
- The alpha channel (byte at offset +3) is not set here; ensure imageData.data alpha values are initialized once (e.g., 255) or you will get transparent pixels.
- This does a per-pixel loop every frame and rounds to integers; consider performance implications for large PIXEL_COUNT or slow devices.
- The animation runs until its RAF handle is cancelled externally (cancelAnimationFrame).

---