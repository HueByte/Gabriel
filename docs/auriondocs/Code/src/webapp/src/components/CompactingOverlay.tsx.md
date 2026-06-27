# CompactingOverlay.tsx

> **Source:** `src/webapp/src/components/CompactingOverlay.tsx`

## Contents

- [CompactingOverlayProps](#compactingoverlayprops)
- [SwirlProps](#swirlprops)
- [CompactingOverlay](#compactingoverlay)
- [Core](#core)
- [Swirl](#swirl)

---

## CompactingOverlayProps

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** interface

A lightweight props shape for the CompactingOverlay component describing optional presentation data used when rendering a folded/compacted conversation overlay. Use this interface when supplying palette information for color matching (the swirl) and an optional count of how many messages are folded — typically rendered in the overlay caption.

## Remarks
This interface contains only presentation-related values and does not impose any runtime validation. "paletteStops" provides the conversation's color stops (used to match the overlay swirl to an avatar or conversation theme) and is allowed to be null to indicate that no palette information is available. The array is marked readonly to signal callers and consumers that the list should not be mutated in place.

## Example
```typescript
const props: CompactingOverlayProps = {
  // provide a readonly array of RGB stops (RGB is defined elsewhere in the codebase)
  paletteStops: [{ r: 250, g: 200, b: 0 }, { r: 240, g: 100, b: 50 }],

  // optional count of folded messages; can be omitted if unknown
  messageCount: 5,
};

// Pass props into the CompactingOverlay component
// <CompactingOverlay {...props} />
```

## Notes
- paletteStops may be undefined or null; consumers should check for both before reading the array.
- The paletteStops array is readonly — do not attempt to mutate it (create a new array if you need to transform stops).
- messageCount is not validated by the type and may be any number; callers should supply a non-negative integer if the value is shown to users.

---

## SwirlProps

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** interface

```typescript
interface SwirlProps
```


An interface describing an optional list of color stops for a swirl/gradient-like consumer: an ordered, readonly array of RGB values. Use this when a configuration or component prop may supply a color palette for rendering; the readonly annotation indicates the array itself should not be mutated by the consumer.

## Remarks
This small shape signals two things: the palette is optional, and callers should treat the array of stops as immutable. The readonly modifier communicates the intention that the list of stops is consumed as-is (read-only) rather than being appended to or altered in place; it does not, however, deep-freeze the objects stored in the array.

## Example
```typescript
// Construct a value conforming to SwirlProps
const props: SwirlProps = {
  paletteStops: [
    { r: 255, g: 100, b: 0 } as RGB,
    { r: 10, g: 200, b: 255 } as RGB
  ]
};

// Consumer code should handle the optional case:
function renderSwirl(p: SwirlProps) {
  const stops = p.paletteStops ?? defaultStops;
  // use `stops` (read-only) to create a gradient
}
```

## Notes
- readonly prevents array mutation methods (push/pop/splice) but does not make the RGB objects themselves immutable — element properties can still be changed if RGB is a mutable type.
- paletteStops is optional; callers and implementors must provide a fallback or handle undefined when rendering.
- TypeScript will accept a plain mutable RGB[] where a readonly RGB[] is expected, but using a readonly type documents intent and can help catch accidental mutations in strictly-typed code.

---

## CompactingOverlay

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

A React function component exported as CompactingOverlay that accepts at least two props: paletteStops and messageCount. It is responsible for producing the overlay UI associated with a "compacting" state and is intended to be used wherever that overlay should appear in the app.

## Remarks
Centralizes the presentation of the compaction-related overlay so callers only need to pass visual configuration (paletteStops) and the numeric count (messageCount). Keeping this UI in a single component makes it easier to reuse and test the overlay consistently across the application.

## Example
```typescript
import { CompactingOverlay } from 'src/webapp/src/components/CompactingOverlay';

// Render the overlay with a palette configuration and a message count
<CompactingOverlay paletteStops={[/* color stops or config */]} messageCount={12} />
```

## Notes
- The provided source is truncated: prop shapes and full behavior are not visible here — confirm the exact types for paletteStops and messageCount from the implementation or its type declarations before passing complex values.
- This component is a named export; import it with a named import (not a default import).
- The implementation may rely on specific shapes or stable object identities for paletteStops (e.g., arrays/objects); avoid recreating large prop objects inline on every render unless intended.

---

## Core

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Core(
```


A React function component named Core that receives a props object containing a paletteStops property. It is part of the CompactingOverlay implementation (declared in CompactingOverlay.tsx); consult that file for what UI this component renders and for the exact shape and typing of paletteStops.

## Notes
- The provided snippet contains only the start of the function signature; implementation and prop types are not available here. Check src/webapp/src/components/CompactingOverlay.tsx for full details.
- Do not assume the runtime shape or type of paletteStops (array, map, etc.) without verifying the component's prop types or TypeScript definitions.
- Verify whether Core is exported or intended as an internal helper before importing it from other modules.

---

## Swirl

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

Swirl is a function declared in CompactingOverlay.tsx that takes a single destructured parameter: { paletteStops }. The provided source snippet does not include the function body, so this documentation only records the visible signature and the fact that the function expects a paletteStops property on its argument.

## Remarks
The implementation is not available in the provided code fragment, so callers should inspect the CompactingOverlay.tsx source to determine what Swirl renders, the expected shape and types of paletteStops, and whether Swirl returns a React element or some other value. The name and file location suggest it's related to the CompactingOverlay UI, but that relationship and behavior must be confirmed from the full source.

## Notes
- The type and structure of paletteStops are not present in the snippet — confirm whether it is an array, object, or a stricter typed interface before passing data.
- It is unknown whether paletteStops is required or optional; check the implementation or PropTypes/TypeScript definitions in the file.
- Because the body is missing, do not assume thread-safety, side effects, or render behavior without reviewing the full implementation.

---