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

Properties for configuring a GabrielSequenceView — a React view that fetches and renders a server-driven "sequence" (either a conversation-specific sequence or a project-shared sequence). Use these props to choose which backend sequence to load (source), control refetch behavior (refreshKey), set the rendered size in pixels, and receive the raw fetched sequence for deriving shared UI accents.

## Remarks
This interface isolates view configuration from fetch and styling concerns. The `source` decides which server endpoint the view will call (conversation vs. project shared). `refreshKey` is a simple numeric way for a parent to force the component to refetch (bumping the number triggers a reload). `onSequenceLoaded` lets the parent observe the exact server-driven payload so it can compute consistent UI accents (gradients, pulse colors, tints) from the same data the view uses.

## Example
```typescript
// Typical usage inside a parent component
const [refreshKey, setRefreshKey] = useState(0);
const handleSequenceLoaded = (sequence: GabrielSequence) => {
  // derive shared accents from server palette
  const palette = derivePaletteFromSequence(sequence);
  setAccents(palette);
};

return (
  <GabrielSequenceView
    source={{ kind: 'project', id: 'proj_123' }}
    refreshKey={refreshKey}
    size={240}
    onSequenceLoaded={handleSequenceLoaded}
  />
);

// To force a refetch after sending a message:
setRefreshKey(k => k + 1);
```

## Notes
- Bumping `refreshKey` (a new numeric value) is the supported mechanism to force the view to refetch; simply mutating objects passed as `source` may not trigger reloads.
- `size` defaults to 200 pixels when omitted to maintain compatibility with the prior Three.js avatar sizing.
- `onSequenceLoaded` is invoked after each successful fetch with the raw [`GabrielSequence`](../../../api/Gabriel.Engine/Sequence/GabrielSequence.cs.md) payload; treat that object as a read-only snapshot from the server.

---

## GabrielSequenceView

> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

```typescript
export function GabrielSequenceView(
```


Renders a visual view for a Gabriel sequence sourced from the provided `source` prop. Use this component when you want a self-contained UI element that displays a Gabriel sequence and notifies the parent when the sequence data has been loaded. The `refreshKey` prop can be changed to force the component to reload; `size` controls the rendered dimension in pixels (default 200); `onSequenceLoaded` is invoked once the component has the sequence available.

## Remarks
This component acts as a presentation/adapter around a Gabriel sequence data source: it accepts an external `source` descriptor and exposes a simple way to refresh and receive the loaded sequence. Consumers are expected to drive reloads by changing `refreshKey` when underlying source data changes, and to handle the loaded sequence via `onSequenceLoaded` (for caching, further processing, or syncing parent state).

## Example
```typescript
// Typical usage inside a parent component
<GabrielSequenceView
  source={{ id: 'sequence-123' }}
  refreshKey={version}
  size={300}
  onSequenceLoaded={(sequence) => {
    // store or inspect the loaded sequence
    setSequence(sequence);
  }}
/>
```

## Notes
- Changing `refreshKey` is the intended mechanism to trigger a reload; keeping the same value will not cause a refresh.
- `size` is interpreted as pixels for the rendered view; keep values reasonable for the available layout.
- `onSequenceLoaded` may be invoked asynchronously after the component resolves or derives the sequence — do not assume synchronous execution.

---

## draw

> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

Renders and animates a sequence onto a canvas by linearly interpolating between two adjacent frames' palette colors for each pixel and writing the resulting RGB values into an ImageData buffer. Use this as the requestAnimationFrame callback to produce a continuously looping animation based on startTimeRef, FRAME_DURATION_MS and FRAMES.

## Remarks
This function reads the current sequence from sequenceRef.current and computes the current frame pair and interpolation factor from the elapsed time since startTimeRef.current. It writes per-pixel RGB values into the already-prepared imageData.data buffer and blits it to the canvas via ctx.putImageData. The implementation intentionally uses simple linear interpolation (t) instead of eased interpolation to produce a continuous flow between palette indices.

## Example
```typescript
// Typical usage: start the animation and later stop it.
startTimeRef.current = performance.now();
raf = requestAnimationFrame(draw);

// ...later to stop:
if (raf) cancelAnimationFrame(raf);
```

## Notes
- The alpha channel (data[offset + 3]) is not updated on each frame; it must be initialized once when imageData is created.
- The per-pixel loop runs on the main thread and may be CPU-heavy for large PIXEL_COUNT; consider using an OffscreenCanvas or reducing pixel count if you observe jank.
- If sequenceRef.current is falsy, nothing is drawn for that frame but the animation loop continues (requestAnimationFrame is still scheduled).
- Colors are rounded with Math.round, which quantizes interpolated values to integer byte channels.

---