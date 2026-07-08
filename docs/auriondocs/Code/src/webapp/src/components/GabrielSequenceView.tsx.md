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


GabrielSequenceViewProps is the props contract for GabrielSequenceView. It lets you choose which sequence to render via source (SequenceSource), optionally force a refresh with refreshKey, control the square display size in pixels with size (defaulting to 200 to match the prior Three.js avatar), and receive the raw GabrielSequence after a successful load through onSequenceLoaded so the parent can derive a server-driven palette for UI accents.

## Remarks
GabrielSequenceViewProps exists to decouple the rendering of a Gabriel sequence from the data-fetching concerns and to present a compact, typed surface for consumers. The refreshKey acts as a cache-busting knob; changing its value triggers a refetch of the sequence, which is useful after mutations such as sending a message. If you need visuals to react to the loaded data, implement onSequenceLoaded to receive the raw GabrielSequence and drive shared accents consistently across components.

## Notes
- The size prop is square in pixels and defaults to 200 to align with the prior Three.js avatar.
- The onSequenceLoaded callback is optional; omit it if you don't need to surface the raw data.
- If you rely on refreshKey, ensure you provide a new value whenever you intend to trigger a fetch; passing the same value won't cause a refresh.

---

## GabrielSequenceView
> **File:** `src/webapp/src/components/GabrielSequenceView.tsx`  
> **Kind:** function

```typescript
export function GabrielSequenceView(
```


GabrielSequenceView is a React function component that renders a visualization of a Gabriel sequence derived from the provided source data. It accepts a source input, a refreshKey to force reinitialization, an optional size (default 200) to control the rendered dimension, and an onSequenceLoaded callback that fires when the sequence visualization has finished loading.

## Remarks
GabrielSequenceView encapsulates the visualization concern for Gabriel sequences, exposing a simple prop surface so callers don't need to manage drawing logic directly. The refreshKey prop is a deliberate hook for reloading the visualization when the underlying data or view requirements change, without mutating props. The onSequenceLoaded callback provides a hook for post-load actions (e.g., analytics, tooltips, or synchronization with other components). This component typically renders into a container whose size is controlled by the size prop and may coordinate with responsive layout behavior elsewhere in the app.

## Notes
- Be mindful that large or rapidly changing 'source' data may cause expensive re-renders; consider debouncing or memoization strategies to mitigate work.
- If the parent layout does not constrain width/height, provide a sensible size or wrap the component in a container that enforces layout to avoid overflow.

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


draw is the per-frame animation callback responsible for rendering the GabrielSequenceView canvas. On each invocation, it blends the colors for every pixel by linearly interpolating between two consecutive frames of the underlying sequence, based on the current position within a looping cycle derived from a shared start time and fixed frame duration. The interpolated RGB values are written into the canvas ImageData; the alpha channel is left unchanged per frame (initialized elsewhere). The resulting image data is then painted to the canvas and the callback re-schedules itself with requestAnimationFrame to continue the animation.

---