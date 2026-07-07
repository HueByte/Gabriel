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


Props defines the props for the StreamingText component, describing how the streamed text is presented as it arrives from a server-sent events source. It includes the text to display, a flag to enable or disable the typewriter-style animation as the SSE appends, and two optional presentation toggles: caret and galactic mode. The animate flag is captured at mount; toggling it after mount does not abort the in-flight typewriter and the current reveal will complete before any change takes effect. When galactic is enabled, the leading edge of the reveal is shown in galactic cipher while the English translation lags behind by GALACTIC_LEAD characters.

## Remarks

This interface serves as a contract between the consumer of StreamingText and its rendering implementation, encapsulating presentation concerns (animation, caret, and galactic reveal) behind a single, stable type. Centralizing these options in Props decouples visual behavior from the component's core rendering logic, allowing different streaming or visualization modes to be swapped with minimal code changes. The mount-time capture semantics for animate (and the galactic reveal behavior) give predictable animation lifecycles, which helps avoid surprising mid-flight state changes.

## Example

```typescript
// Example usage of Props
const example: Props = {
  text: "Streaming data...",
  animate: true,
  galactic: true,
  caret: true
};
```

## Notes

- The animate flag is captured at mount; toggling it later will not interrupt an in-flight typewriter. If you need to restart the animation, remount the StreamingText component.
- When galactic mode is enabled, the reveal uses GALACTIC_LEAD as the offset for the English translation; ensure that GALACTIC_LEAD is defined in your environment so the offset behaves as intended.


---

## StreamingText
> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

```typescript
export function StreamingText(
```


StreamingText is a lightweight React (TSX) component that renders the provided text with an optional streaming, typing-like reveal. When animate is true, the text appears progressively to simulate typing; when false, the text renders immediately. The caret prop toggles a blinking cursor at the end to reinforce the typing illusion, while galactic switches to an alternate space-themed styling. Use this component when you want to present text with a dynamic, attention-guiding reveal (e.g., chat messages, onboarding steps, or terminal-like UIs) instead of rendering the string in one go.

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


Tick is the animation loop that progressively reveals a target string in the StreamingText component by advancing two cursors—gal (the leading index) and en (the end index)—at a rate derived from the current backlog and an optional galactic mode. It schedules itself via requestAnimationFrame until the entire target has been revealed, mutating shared refs and triggering a re-render when progress occurs.

## Remarks
Tick relies on several React refs (cursorsRef, targetRef, galacticRef) to track progress across frames and mutates timing state (nextRevealRef) along with the cursor counters. The reveal rate clamps with MAX_RATE and ramps with backlog using SPEEDUP_PER_BACKLOG_CHAR, enabling a smooth, backlog-driven pacing that adapts as more characters are queued. When progress happens, it signals a render via bump(n => n + 1); if the target is still incomplete, it schedules the next frame with requestAnimationFrame; otherwise it clears the RAF handle. This function is intended to be driven by the animation loop rather than called for synchronous, one-shot rendering.

## Example
```typescript
// Conceptual example: drive a streaming reveal in a requestAnimationFrame loop
const now = Date.now();
tick(now); // tick progresses the reveal; the function schedules the next frame internally
```

## Notes
- This function mutates internal state (refs) and has side effects beyond pure computation, so tests and usage should account for frame-based progression.
- Its behavior depends on external constants (MAX_RATE, BASE_RATE, SPEEDUP_PER_BACKLOG_CHAR, GALACTIC_LEAD); ensure these are defined in scope for predictable pacing and galactic mode handling.

---