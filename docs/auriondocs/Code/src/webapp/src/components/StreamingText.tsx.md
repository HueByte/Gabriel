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


Props describes the public input contract for StreamingText: the text to display, a flag indicating whether the server-sent events stream is still appending to text (captured at mount; toggling later does not abort the in-flight typewriter — it always finishes), and two optional flags that alter rendering (caret and galactic). The galactic flag renders the leading edge of the reveal in galactic cipher, with the English translation trailing by GALACTIC_LEAD characters. This interface is consumed by StreamingText to govern how and when the text appears during a streaming reveal.

## Remarks
Props acts as a small, focused contract that encapsulates the variation in how text is revealed. It separates concerns by letting StreamingText know when to animate, whether to show a caret, and whether to apply the galactic-cipher reveal, without embedding this logic in the component's internal state. The animate flag's mount-scoped semantics prevent mid-flight changes from interrupting an ongoing typewriter run, which helps keep the user experience smooth when the underlying data continues to flow.

## Example
```typescript
<StreamingText text="Connecting to server..." animate={true} galactic caret={true} />
```

## Notes
- Changing animate after mount does not abort an in-flight typewriter; to restart the animation you must remount StreamingText.

---

## StreamingText
> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

```typescript
export function StreamingText(
```


StreamingText is a UI component that renders the provided text with an optional streaming or typing animation. It accepts a text string and control flags: animate turns on the progressive reveal of characters, caret enables a cursor indicator at the end, and galactic toggles a space-themed styling variant. Use it when you want to convey dynamic, real-time text entry or storytelling in interfaces such as chat windows, terminals, or sci‑fi dashboards. When animate is false, the full text is shown immediately; when true, characters appear progressively according to the component's internal timing. The caret flag adds a blinking cursor at the end to imply ongoing streaming.

## Remarks
StreamingText centralizes the common pattern of dynamic text reveal, decoupling animation concerns from page composition. It provides a consistent API for enabling a typing-like effect and theming through a single prop set, helping maintainable, reusable UI components across the app.

## Notes
- If the text prop changes during streaming, the component may restart the reveal unless controlled explicitly.
- The galactic theme may introduce CSS animations or heavier styling; consider performance implications on lower-end devices.
- When using caret with animation, ensure sufficient contrast so the cursor remains visible against varying backgrounds.

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


Drives the incremental, frame-synced reveal of a streaming text display by advancing two cursors toward the target content and scheduling the next reveal based on a computed rate. Use this instead of manually managing timers to progressively render text; it encapsulates the timing and mode-specific logic (galactic vs. non-galactic) and stops when the target is fully revealed.

## Remarks
This function centralizes the streaming reveal logic, coordinating two cursors (gal and en) and a Galactic mode flag. It uses a time accumulator (nextRevealRef) and a RAF loop to keep the pace consistent with backlog-driven speed, so callers don't need to manage per-character timers. It mutates refs and schedules frames, becoming the single source of truth for the reveal cadence.

## Example
```typescript
// Start streaming reveal loop
tick(performance.now());
```

## Notes
- Be aware that the function depends on several refs existing in scope (cursorsRef, targetRef, galacticRef, nextRevealRef, rafRef, bump).
- The reveal rate is bounded by MAX_RATE and BASE_RATE; tuning these constants changes perceived speed and responsiveness.
- Inactive tabs may pause RAF; the loop resumes when tick is invoked again by the browser's RAF.
- In Galactic mode, English characters are capped by GALACTIC_LEAD relative to gal characters; ensure this matches intended UX.

---