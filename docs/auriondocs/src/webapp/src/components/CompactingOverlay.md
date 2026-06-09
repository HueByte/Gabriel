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

Props for the CompactingOverlay component — use this interface when rendering the compacted / folded message overlay that visually connects a collapsed message group to the conversation avatar. It supplies optional color stops for matching the overlay's swirl to the conversation palette and an optional messageCount to display how many messages were folded.

## Remarks
This surface-level props bag keeps the overlay component decoupled from the rest of the conversation model: paletteStops is provided so the overlay can compute or interpolate a swirl color that matches the conversation avatar, and messageCount is used only for the visible caption (for example, "5 messages"). Both properties are optional to allow the overlay to render in a minimal form when color data or counts are not available.

## Example
```typescript
// Typical usage inside a conversation UI
const palette: readonly RGB[] = [ { r: 240, g: 200, b: 100 }, { r: 180, g: 120, b: 40 } ];
<CompactingOverlay paletteStops={palette} messageCount={3} />
```

## Notes
- paletteStops may be null or undefined; components should treat both as "no palette provided" and fall back to a default color.
- The paletteStops array is readonly — do not attempt to mutate it in-place.
- messageCount is optional; treat undefined as "unknown" or 0 depending on the UX requirement.

---

## SwirlProps

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** interface

Represents properties that supply an optional color palette via paletteStops — a readonly array of RGB values. Use this interface when you need to provide a set of color stops to the swirl-related rendering/visual logic in this file; omit the property when no custom palette is required.

## Remarks
This small props shape makes palette injection explicit and enforces immutability for the stops at the type level. The paletteStops field is optional so callers can choose not to provide custom colors; code that consumes SwirlProps should therefore treat paletteStops as potentially undefined and avoid mutating the array if one is provided.

## Example
```typescript
// RGB is defined elsewhere in the codebase; treat these as RGB values
const myStops: readonly RGB[] = [
  { r: 255, g: 0, b: 0 } as RGB,
  { r: 0, g: 255, b: 0 } as RGB,
  { r: 0, g: 0, b: 255 } as RGB,
];

const props: SwirlProps = { paletteStops: myStops };

// pass `props` to the consumer that accepts SwirlProps
renderSwirl(props);
```

## Notes
- paletteStops is optional — consumers must handle the undefined case.
- The readonly modifier prevents in-place mutation at compile time; create a new array to change stops.
- The RGB type/shape is declared elsewhere; this interface only requires values conforming to that type.

---

## CompactingOverlay

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

An exported function (presumably a React component) named CompactingOverlay that accepts an object with the properties paletteStops and messageCount. The available source is a partial signature only, so implementation details (rendered output, prop types, and behavior) are not present here — inspect the component source before relying on specifics.

## Remarks
The name and parameters suggest this symbol provides a UI overlay related to a "compacting" operation: paletteStops likely controls visual styling (for example color stops in a gradient) and messageCount likely represents the number of messages being compacted or shown in the overlay. Because the function body is not available in the provided fragment, this remark is intentionally high-level and inferential; consult the full implementation to confirm exact responsibilities and hooks used.

## Notes
- Source code for the function implementation is missing from the provided fragment; prop types and return value (JSX, null, etc.) cannot be confirmed here.
- Do not assume the shape of paletteStops (array, objects, or simple values) or that messageCount is required — check the real props definition or TypeScript types in the component file.
- Accessibility behavior, side effects, and rendering conditions (e.g., conditional display, animations, server-side rendering compatibility) are unknown until the implementation is available.

---

## Core

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

```typescript
function Core(
```


A function (in a .tsx file) that destructures a single prop named `paletteStops` from its first argument. The provided snippet does not include the implementation or TypeScript types; consumers should pass a `paletteStops` value when invoking or rendering this symbol and consult the full CompactingOverlay.tsx file for exact behavior and the expected shape of `paletteStops`.

## Example
```typescript
// Likely usage as a React component in a .tsx file
<Core paletteStops={myPaletteStops} />

// Or invoked directly if treated as a plain function
Core({ paletteStops: myPaletteStops });
```

## Notes
- The implementation body and type definitions are not present in the provided source; inspect the full file to learn what `paletteStops` should contain and whether the symbol is a React component or a plain function.
- The .tsx extension suggests this is intended as a React functional component, but this cannot be confirmed from the truncated snippet.
- Do not assume `paletteStops` is optional, its exact type, or whether it is mutated internally without reviewing the complete source.

---

## Swirl

> **File:** `src/webapp/src/components/CompactingOverlay.tsx`  
> **Kind:** function

Swirl is a function declared in CompactingOverlay.tsx that takes a single destructured parameter with a paletteStops property. The provided source is truncated, so the function's behavior, return value, and usage details are not available from the supplied snippet; consult the full implementation to understand how paletteStops is consumed and what Swirl renders or returns.

## Remarks
This symbol appears in a .tsx file and therefore is likely a React function component or a UI-related helper that expects paletteStops (presumably a list of color stops or similar). Because the implementation is missing, callers should not assume how paletteStops is interpreted (shape, required/optional fields, or side effects) without inspecting the complete source.

## Notes
- The implementation and return type are not present in the provided source; treat this documentation as a placeholder until the full function body is available.
- The exact shape and expected contents of paletteStops are unspecified here; check the prop/type definition in the full file or related types.
- If this is a React component, ensure the component's props and lifecycle expectations are validated against the full source before use.

---