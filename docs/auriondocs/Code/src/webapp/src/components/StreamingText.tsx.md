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

```typescript
interface Props
```


Props defines the property contract for StreamingText: the text to render, whether the SSE-driven in-flight reveal should animate, and optional visuals such as a caret indicator and the galactic-cipher reveal.

## Remarks
Encapsulating these options in an interface gives StreamingText a clean contract and lets its parent control when animation starts and which embellishments are shown. The animate flag is captured at mount and, once the in-flight typewriter begins, toggling animate later does not abort it — it always completes. The optional caret and galactic fields let callers opt into subtle UI modes without expanding the core rendering logic.

## Example
```typescript
const example: Props = {
  text: 'Connecting to server...',
  animate: true,
  caret: true,
  galactic: true
};
```

## Notes
- The animate property is captured at mount and will not be interrupted by changing it mid-flight.
- Optional props default to undefined; treat as disabled if not provided.
- The galactic option is opt-in and adds a leading-edge reveal; callers should ensure the consuming rendering path supports this mode if used.

---

## StreamingText
> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

```typescript
export function StreamingText(
```


StreamingText is a React component that renders the provided text with optional streaming animation and presentation cues. It accepts a text string and three boolean toggles: animate, caret, and galactic, which control whether the text reveals itself progressively, whether a caret cursor is shown, and whether a galactic styling variant is applied. Use this component when you want to present text in a dynamic, newsfeed-like or chat-like fashion without implementing the animation and cursor UI yourself; it also lets you switch to a static rendering or a themed look by adjusting props.

## Remarks
StreamingText encapsulates a common UI pattern—progressive text reveal—so callers can opt into animation and decorations in one place instead of scattering this logic throughout the codebase. It serves as a lightweight abstraction over the visual behaviors of text streams, allowing teams to maintain consistent UX by toggling simple props rather than wiring up bespoke animation code in every usage site.

## Example
```typescript
<StreamingText text="Fetching data..." animate={true} caret={true} galactic={false} />
```


---

## tick
> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

```typescript
function tick(now: number)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `now` | `number` | — |


tick is an internal animation helper used by StreamingText.tsx to progressively reveal a target string over time. It drives a frame-based reveal loop, updating internal counters (gal and en) and scheduling subsequent frames via requestAnimationFrame until the entire text is shown. The reveal cadence is computed from a backlog-aware rate, with a separate path for Galactic mode that can adjust how many characters are exposed per step. This function encapsulates the timing and pacing logic separate from render cycles, ensuring a smooth, frame-rate-independent streaming experience.

## Remarks
tick centralizes the time-based pacing for the streaming reveal, decoupling character emission from React re-renders and letting the UI remain responsive even under frame drops. It mutates state through refs and only signals progress when something actually advances (via bump), which minimizes unnecessary renders while preserving correct timing. The exact pacing is governed by constants (BASE_RATE, MAX_RATE, SPEEDUP_PER_BACKLOG_CHAR) and Galactic behavior (GALACTIC_LEAD), so tweaking those values directly influences the perceived speed of the streaming text.

## Notes
- The animation is driven by the browser's requestAnimationFrame; in environments with throttled or suspended tabs, the reveal pace may slow or pause.
- The function mutates shared refs (e.g., c.gal, c.en) and relies on a single RAF loop; introducing concurrent callers or multiple loops can produce unexpected pacing unless carefully coordinated.


---