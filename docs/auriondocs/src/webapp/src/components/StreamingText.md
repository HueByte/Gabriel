# StreamingText.tsx

> **Source:** `src/webapp/src/components/StreamingText.tsx`

## Contents

- [Props](#props)
- [StreamingText](#streamingtext)
- [tick](#tick)

---

## Props

> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** interface

Props describes the properties accepted by the StreamingText component. Use it to provide the string to render plus visual/animation flags: `animate` controls the typewriter-like reveal state captured at mount, `caret` enables an optional caret, and `galactic` switches the leading-edge reveal into a galactic-cipher display with the English translation trailing behind.

## Remarks
This interface exists to separate the streaming/animation concerns from content: the component expects the full text to render and a snapshot of whether an in-flight SSE-based append/animation is active at mount. `animate` is intentionally captured at mount so the component can complete any ongoing reveal even if the parent later toggles that prop.

## Example
```typescript
// Typical usage in JSX
<StreamingText
  text={message}
  animate={isStreaming}
  caret={true}
  galactic={showGalactic}
/>
```

## Notes
- `animate` is captured at mount; changing it after the component mounts will not abort or rewind an in-progress typewriter reveal — the reveal always finishes once started.
- When `galactic` is true the component renders the leading edge in a galactic cipher and the English translation trailing behind by GALACTIC_LEAD characters; account for that offset when aligning or measuring visible characters.

---

## StreamingText

> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

```typescript
export function StreamingText(
```


Renders provided text with optional streaming/animated presentation and a couple of appearance flags. Use this component when you want a text value displayed either statically or as a progressive/animated reveal, and when you need an optional caret or a special “galactic” styling without implementing those behaviors at each call site.

## Remarks
This component centralizes streaming/animation behaviour and simple appearance toggles (caret and galactic) so callers can opt in to animation and visual variants via props. The boolean props have defaults (caret and galactic default to false), allowing simple static usage by passing only the text and animate flags.

## Example
```typescript
// Basic animated streaming text
<StreamingText text={"Hello, world"} animate={true} />

// Animated with caret
<StreamingText text={message} animate={true} caret={true} />

// Static text with "galactic" styling
<StreamingText text={title} animate={false} galactic={true} />
```

## Notes
- caret and galactic default to false; omit them for the simplest behavior.
- animate toggles whether the text is presented progressively; pass false to render the text immediately.
- If the displayed text changes frequently, consider how updates should affect the animation (e.g., restarting vs. continuing); behaviour depends on the component implementation.

---

## tick

> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

Advances the reveal cursors for a streaming text animation and schedules the next animation frame. Callers use this when driving a character-by-character reveal: tick consumes the RAF timestamp (now), increments the two cursor counters stored in refs (cursorsRef.current), updates the next reveal timestamp, and re-schedules requestAnimationFrame until both cursors reach the target length.

## Remarks
This function is part of the rendering loop for a two-stream reveal ("gal" and "en"). It repeatedly advances cursors while the current time meets or exceeds the next scheduled reveal, accelerating the reveal rate based on backlog (characters remaining) and applying a configurable lead/lag between the two streams when in galactic mode. It mutates several refs (cursorsRef, targetRef, galacticRef, nextRevealRef, rafRef) and calls a bump function to trigger React re-renders only when the cursors actually advanced.

## Example
```typescript
// Start the animation loop (typical usage inside an effect or event handler)
rafRef.current = requestAnimationFrame(tick);
```

## Notes
- tick mutates ref objects directly; ensure those refs (cursorsRef, targetRef, nextRevealRef, galacticRef, rafRef) are initialized and remain valid for the lifetime of the animation.
- The reveal rate depends on external constants (BASE_RATE, MAX_RATE, SPEEDUP_PER_BACKLOG_CHAR, GALACTIC_LEAD); large backlogs increase rate up to MAX_RATE.
- Remember to cancel the RAF (cancelAnimationFrame(rafRef.current)) on unmount or when stopping the animation to avoid dangling callbacks.

---