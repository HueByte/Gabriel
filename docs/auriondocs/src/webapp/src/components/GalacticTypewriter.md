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

Properties for the GalacticTypewriter component — use to provide the string to animate and to control timing and completion behavior. Prefer this over manually wiring timers when you want a reusable, two-pass typewriter effect controlled by a single prop bag.

## Remarks
This interface models the inputs required for a two-pass typing animation: the component runs an initial "galactic" pass and then an "english" pass over the same text. charMs controls the per-character timing for both passes, pauseMs inserts a delay after the first pass finishes and before the second begins, and onDone is invoked once the whole animation sequence completes.

## Example
```typescript
// Typical usage in JSX/TSX
<GalacticTypewriter
  text={"We come in peace."}
  charMs={40}       // 40 ms per character for both passes
  pauseMs={600}     // 600 ms pause between passes
  onDone={() => console.log('animation finished')}
/>
```

## Notes
- charMs applies to both the galactic and the english pass; there is no separate speed for each pass.
- pauseMs is optional; when omitted the second pass starts immediately after the first finishes.
- onDone is optional; if provided it will be called once after both passes complete.

---

## GalacticTypewriter

> **File:** `src/webapp/src/components/GalacticTypewriter.tsx`  
> **Kind:** function

```typescript
export function GalacticTypewriter(
```


Renders the given text with a typewriter-like animation, revealing characters over time. Use this component for decorative or attention-grabbing text reveals in a UI where a sequential character-by-character reveal improves clarity or aesthetics; supply the text to display and optionally tune the character delay, completion pause, and a completion callback.

## Remarks
This is a presentational React component that drives a timed reveal of the provided text. The charMs and pauseMs parameters let callers trade off speed vs. drama — charMs is the per-character delay and pauseMs is a short delay after the full text has been revealed before the component considers the animation finished and triggers the onDone callback. Prefer this component for non-critical, decorative text: if the content must be immediately available to assistive technologies or for users who disable animations, provide an accessible fallback or expose the full text elsewhere.

## Example
```typescript
// Default behavior
<GalacticTypewriter text="Welcome to the galaxy" />

// Customized timing and completion handler
<GalacticTypewriter
  text="Prepare for launch..."
  charMs={45}
  pauseMs={700}
  onDone={() => console.log('typing finished')}
/>
```

## Notes
- charMs and pauseMs are specified in milliseconds.
- Very long text or very large charMs values can produce long animations; prefer shorter passages for this effect.
- If the component is unmounted before the animation completes, the onDone callback may not be invoked — avoid relying on onDone for critical flow control without guarding for unmounts.

---

## toGalactic

> **File:** `src/webapp/src/components/GalacticTypewriter.tsx`  
> **Kind:** function

Convert a single character to its "Galactic" representation by looking up the character's uppercase form in GAL_MAP; if no mapping is found the original input is returned. Use this helper when rendering characters for the Galactic typewriter UI so callers don't need to handle case normalization or fallback behavior themselves.

## Remarks
This small adapter centralizes the lookup and fallback logic for the GalaticTypewriter component: it normalizes input to uppercase before consulting GAL_MAP and guarantees a safe fallback to the original character when a mapping is missing. The function assumes GAL_MAP provides the canonical mappings and keeps callers unaware of the map's shape or absence of entries.

## Example
```typescript
// example GAL_MAP (defined elsewhere)
// const GAL_MAP = { A: 'Λ', B: '฿' };

toGalactic('a'); // 'Λ'  (looks up 'A')
toGalactic('B'); // '฿'  (looks up 'B')
toGalactic('#'); // '#'  (no mapping => returns original)
```

## Notes
- The function uppercases the input using ch.toUpperCase() before lookup; this makes mappings case-insensitive but means the lookup key is the entire uppercased string, so multi-character inputs are treated as a single key. 
- It uses the nullish coalescing operator (??) to fall back to the original ch only when the map entry is undefined or null; a mapped empty string ("") is returned as-is.
- Unusual Unicode uppercasing behavior (where toUpperCase can change length or produce unexpected sequences) can affect lookup keys for non-ASCII input.

---