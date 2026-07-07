# SkinPicker.tsx

> **Source:** `src/webapp/src/components/SkinPicker.tsx`

## Contents

- [SkinPickerProps](#skinpickerprops)
- [SkinPicker](#skinpicker)
- [handlePalette](#handlepalette)
- [handlePattern](#handlepattern)

---

## SkinPickerProps
> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** interface

```typescript
interface SkinPickerProps
```


SkinPickerProps defines the props for the SkinPicker component. It models a controlled skin-selection surface where pattern and palette can be strings or null to represent an unset state; changes are emitted via onChange with the new { pattern, palette }. If onReroll is supplied, a reroll button is rendered beside the pickers and the skin overrides remain pinned (pins survive a reroll; only seed-derived dimensions change). The disabled flag toggles user interaction to reflect loading or saving.

## Remarks
This interface enforces a clear boundary between presentation and state. It adopts a controlled-component pattern: the parent owns pattern and palette and reacts to onChange with a new value object. The optional onReroll captures an alternate UX path that can be provided to enable seed-based exploration without losing user pins.

## Notes
- pattern and palette may be null; treat null as "not selected" and handle in UI logic.
- onReroll, when provided, will not clear existing pins; only seed-derived dimensions are affected.
- The disabled flag disables interaction; ensure the consuming UI reflects this state and guards onChange invocations accordingly.

---

## SkinPicker
> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

```typescript
export function SkinPicker(
```


SkinPicker is a reusable React functional component that renders a control for selecting a skin pattern from a provided palette. It is a controlled UI unit: it receives the current pattern and the available palette via props, and communicates changes back to its parent through onChange. It also exposes onReroll to request a new, alternate pattern and respects the disabled flag to render in a non-interactive state. This symbol should be used when you need a compact, standardized skin-selection control in editors or customization screens, rather than building separate pattern and color pickers from scratch.

## Remarks
Skins are a common customization primitive; SkinPicker encapsulates the UX of choosing and iterating through skins. By isolating this behavior, the rest of the UI can rely on a single contract (pattern, palette, onChange, onReroll) and swap in different palettes or patterns without altering layout code. It also eases testing by making the interaction surface consistent across pages.

## Example
```typescript
<SkinPicker
  pattern={currentPattern}
  palette={palette}
  onChange={setPattern}
  onReroll={rollPattern}
  disabled={false}
/>
```

## Notes
- SkinPicker is intended to be a controlled component; avoid mutating internal state. Instead, update the parent state via onChange.
- The onReroll callback is a hint for generating a new pattern; the parent should apply the change so the pattern prop updates synchronously.
- When disabled is true, all interactive affordances should be inert to prevent user interaction.

---

## handlePalette
> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

```typescript
const handlePalette = (value: string) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `value` | `string` | — |


handlePalette is a small UI helper that updates the selected palette in the SkinPicker when the user changes the palette input. It takes a string value and forwards the new palette along with the existing pattern to the onChange callback; if the input is cleared (empty string), it sends null for palette to indicate 'no palette selected'.

## Remarks
Acts as a thin adaptor between the palette input and the parent state. By preserving the current pattern while normalizing the palette value, it keeps state transitions simple and centralized in one consumer (onChange) rather than scattering logic across multiple handlers.

## Notes
- Relies on onChange and pattern being in scope; missing them will cause runtime errors.
- Using value === '' to clear the palette means any non-empty string is treated as a valid palette; ensure downstream logic can handle the palette value type (string | null).

---

## handlePattern
> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

```typescript
const handlePattern = (value: string) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `value` | `string` | — |


handlePattern is a small helper used when the user edits the skin pattern in the SkinPicker. It updates the parent state by invoking onChange with an object that always contains the latest palette and a normalized pattern: if the input is an empty string, pattern is set to null to denote 'no pattern'; otherwise, it uses the provided string. This centralizes how pattern changes are propagated, ensuring the UI and the underlying data model stay in sync with a single payload.

## Remarks
By pooling pattern and palette updates into one call, this function reduces the risk of the UI diverging from the data model if pattern and palette occasionally fall out of sync. It also imposes a single representation for "no pattern" (null) across the codebase, which simplifies downstream checks.

## Example
```typescript
// Most common case: set a non-empty pattern
handlePattern('grid-pattern');

// User clears the field
handlePattern('');
```

## Notes
- Normalization: '' becomes null to represent 'no pattern'; downstream code should handle null accordingly.
- Palette dependency: the function reads palette from its closure; ensure palette is updated in the component state when necessary.
- No whitespace trimming: if you want to strip spaces, call handlePattern(value.trim()) before invocation.

---