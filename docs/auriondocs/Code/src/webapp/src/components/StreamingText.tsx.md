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

Describes the properties accepted by the StreamingText component — a small DTO that carries the text to display and flags that control a typewriter-style reveal, an optional caret, and an optional "galactic cipher" leading-edge reveal. Use this interface when constructing props for the StreamingText component or when writing tests that assert streaming/animation behavior.

## Remarks
The animate flag is sampled at component mount: if true the component performs an in-flight typewriter-style reveal and that reveal is not aborted or restarted by toggling animate afterward. The galactic flag switches the rendering behavior so the visible leading edge is shown in a "galactic cipher" while the English translation trails behind by a fixed offset (referred to in code as GALACTIC_LEAD). caret is optional and controls whether a caret is shown; if omitted the component's internal default behavior applies.

## Example
```typescript
// Object form (useful in tests or programmatic construction)
const props: Props = {
  text: "We come in peace.",
  animate: true,
  caret: true,
  galactic: false,
};

// JSX usage (StreamingText is the component that consumes Props)
// <StreamingText text="Hello" animate caret galactic={false} />
```

## Notes
- animate is captured at mount; toggling it after the component mounts does not abort or restart the current reveal — remount to restart.
- galactic causes the visual lead to be shown in cipher with the English text trailing by GALACTIC_LEAD characters; consumers expecting synchronous translation should account for this lag.
- caret is optional; absence means the component will apply its default caret behavior.

---

## StreamingText

> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

A UI component that renders the provided text and exposes options to enable animation, show a caret, and apply a "galactic" visual variant. Reach for this component when you want a single reusable element to display text with optional streaming/animated presentation and small visual variants controlled via props.

## Remarks
This function is the single entry point for presenting text with a few display modes controlled by props. It centralizes the flags that affect rendering (animate, caret, galactic) so callers don't need to implement the animation or styling logic inline.

## Example
```typescript
// Render static text
<StreamingText text="Hello, world!" />

// Render with animation and a caret
<StreamingText text="Typing…" animate caret />

// Render with a themed/variant style (galactic) without animation
<StreamingText text="Starfield" galactic />
```

## Notes
- The caret and galactic props default to false (caret = false, galactic = false).
- The animate prop is a boolean flag provided to the component; omit it or pass false to avoid animation.

---

## tick

> **File:** `src/webapp/src/components/StreamingText.tsx`  
> **Kind:** function

Advances the two reveal cursors used by the streaming text animation and reschedules animation frames until the target text is fully revealed. It is intended to be called from requestAnimationFrame (it expects the RAF timestamp in now) and will increment internal cursor refs (cursorsRef.current) for the "galactic" and "english" layers according to pacing rules, speed up when backlog grows, and trigger a component update via bump when progress was made.

## Remarks
This function is the driver for a staggered/dual-layer text reveal: one cursor (gal) always leads, while the other (en) follows and — when in "galactic" mode — is constrained to trail the galactic cursor by a configurable lead (GALACTIC_LEAD). The reveal rate adapts based on how many characters remain (leadBacklog) and is clamped by MAX_RATE. It mutates several refs (cursorsRef, targetRef, nextRevealRef, rafRef) and relies on external constants (BASE_RATE, SPEEDUP_PER_BACKLOG_CHAR, MAX_RATE, GALACTIC_LEAD) and the bump callback to cause React re-renders.

## Example
```typescript
// Start the streaming reveal loop (typical usage in an effect or event handler)
rafRef.current = requestAnimationFrame(tick);
```

## Notes
- tick mutates refs directly (cursorsRef.current, nextRevealRef.current, rafRef.current) and is not a pure function; callers should ensure those refs are initialized and stable.
- The now parameter must be the DOMHighResTimeStamp provided by requestAnimationFrame; calling tick without that timestamp may cause timing logic to behave incorrectly.
- The function schedules itself with requestAnimationFrame while there is unread text; when the text is fully revealed it stops rescheduling and sets rafRef.current to null. Ensure bump is a stable updater to avoid unexpected re-renders.

---