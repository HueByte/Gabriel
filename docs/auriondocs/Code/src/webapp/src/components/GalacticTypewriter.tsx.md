# GalacticTypewriter.tsx

> **Source:** `src/webapp/src/components/GalacticTypewriter.tsx`

## Contents

- [Props](#props)
- [GalacticTypewriter](#galactictypewriter)
- [toGalactic](#togalactic)

---

## Props

> **File:** `src/webapp/src/components/GalacticTypewriter.tsx`  
> **Kind:** interface

Configuration for the GalacticTypewriter component's two-pass typewriter animation. Use this interface to provide the text to render, to control timing (per-character speed and inter-pass pause, expressed in milliseconds), and to receive a completion callback when the animation finishes.

## Remarks
This interface keeps animation-related configuration separate from rendering logic: charMs specifies the per-character delay applied to both the initial "galactic" pass and the subsequent "english" pass so pacing is consistent, while pauseMs controls the gap between those two passes. onDone is an optional hook consumers can use to react after the component's animated sequence completes.

## Example
```typescript
// Typical usage in JSX
<GalacticTypewriter
  text="Hello, world!"
  charMs={45}       // 45 ms per character for both passes
  pauseMs={600}     // 600 ms pause between the galactic and english passes
  onDone={() => console.log('animation finished')}
/>
```

## Notes
- charMs and pauseMs are measured in milliseconds.
- Both charMs and pauseMs are optional; this interface does not define defaults — check the component implementation for the actual fallback values.
- onDone is optional; if provided, it will be invoked by the component when its animated sequence completes.

---

## GalacticTypewriter

> **File:** `src/webapp/src/components/GalacticTypewriter.tsx`  
> **Kind:** function

Renders a typewriter-style animated reveal of the provided text, exposing simple timing controls and an optional completion callback. Use this component when you want incremental, character-by-character text animation without wiring up timers yourself — control the per-character delay with charMs, the post-animation pause with pauseMs, and receive a notification via onDone.

## Remarks
This is a small presentational component that encapsulates the typewriter animation behavior (timing, incremental reveal, and completion notification). It exists to keep callers focused on what to show and when to respond to completion events, rather than how to schedule character-by-character rendering.

## Example
```typescript
// Typical usage in a React render function
<GalacticTypewriter
  text="Welcome to the bridge, commander."
  charMs={30}        // 30ms per character
  pauseMs={600}      // wait 600ms after finishing
  onDone={() => console.log('Typewriter finished')}
/>
```

## Notes
- If the component unmounts before the animation completes, onDone may never be called; ensure parent logic accounts for that if needed.
- Very small charMs values can make the animation appear instantaneous and increase render activity; choose a value that balances smoothness and performance.
- Consider accessibility: screen readers may not announce progressively revealed text as expected — test with assistive tech and provide alternatives if necessary (for example, exposing the full text to assistive-only elements).


---

## toGalactic

> **File:** `src/webapp/src/components/GalacticTypewriter.tsx`  
> **Kind:** function

```typescript
function toGalactic(ch: string): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `ch` | `string` | — |

**Returns:** `string`


Converts a single character into its corresponding "galactic" glyph by looking up an uppercase key in the global GAL_MAP. If no mapping exists for the character (after being uppercased), the original character is returned unchanged — use this helper when rendering or transforming text into the galactic script for UI components such as a typewriter effect.

## Remarks
This is a small, pure helper that centralizes the per-character mapping logic used by the typewriter component. It performs a case-insensitive lookup by uppercasing the input before checking GAL_MAP, so callers do not need to normalize character case themselves. The function expects that a GAL_MAP object is available in the surrounding scope.

## Example
```typescript
// Given a GAL_MAP like:
// const GAL_MAP = { A: "⍺", B: "β" };

toGalactic('a'); // returns "⍺" (lookup uses 'A')
toGalactic('B'); // returns "β"
toGalactic('?'); // returns "?" (no mapping, falls back to original)
```

## Notes
- The function uppercases the input, so mapping is effectively case-insensitive.
- Passing a multi-character string or a grapheme cluster will attempt to lookup the whole string in GAL_MAP; it is intended for single characters.
- It does not mutate state and has no side effects; behavior depends on the contents of the global GAL_MAP.

---