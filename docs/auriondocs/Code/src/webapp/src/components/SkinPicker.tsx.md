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


SkinPickerProps defines the props contract for the SkinPicker UI component that lets users choose a skin pattern and a palette. Both selections are represented as nullable strings, signaling an explicit 'not selected' state. The interface requires an onChange callback that receives the next combined selection, allowing the consumer to update the app state in one step. An optional onReroll callback, when provided, causes a reroll action to render next to the pickers; note that rerolling does not clear the pins for pattern or palette, it only changes seed-derived dimensions. A disabled flag can disable interaction during operations like saving.

## Remarks
This interface serves as a clean boundary between presentation and state management for skin selection. It centralizes the idea that a SkinPicker mutates or reports the next selection via onChange while optionally offering a non-destructive reroll action via onReroll. By using nullable fields, it accommodates situations where a user hasn't pinned a particular attribute yet, leaving the rest unchanged.

## Example
```typescript
// Example usage of SkinPickerProps
const example: SkinPickerProps = {
  pattern: null,
  palette: null,
  onChange: (next) => {
    // apply next to your state-tracking skin
  },
  onReroll: () => {
    // trigger a reroll; pins remain intact
  },
  disabled: false
};
```

## Notes
- Null values indicate 'no pin'; consumer code should handle them gracefully and avoid assuming non-null values.
- Provide onReroll only if your UI actually renders a reroll control; otherwise, omit this prop to keep the API surface minimal.
- When disabled is true, ensure all interactive elements are non-editable and focus is managed for accessibility.

---

## SkinPicker
> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

```typescript
export function SkinPicker(
```


SkinPicker is a React component that renders a UI for selecting a skin/pattern from a supplied palette. It delegates state management to its parent: when a user chooses a new pattern, it calls onChange with the selection; onReroll requests a new random option; and disabled toggles interactivity.

## Remarks
SkinPicker serves as a reusable, presentational control that encapsulates the skin-selection UX. It keeps business logic out of the UI by relying on callbacks (onChange, onReroll) and a controlled pattern prop, enabling consistent behavior across pages and making the underlying rendering easy to swap without touching consumer code.

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


handlePalette updates the skin configuration by calling onChange with the current pattern and the new palette value; if the user clears the selection (value is an empty string), the palette is represented as null.

## Remarks
Acts as a focused handler for palette changes within the SkinPicker component. It preserves the existing pattern while updating only the palette, delegating state management to the onChange callback. Representing an empty selection as null standardizes the payload, avoiding ambiguous empty strings in downstream logic.

## Notes
- Relies on the surrounding scope to provide a defined pattern value and a working onChange callback; undefined pattern could lead to unexpected payloads.
- Passing null for no palette assumes downstream logic accepts null; if null is not supported, adjust the shape or validation accordingly.

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


handlePattern is a small change-handler that normalizes the user input for the pattern field and forwards it to the shared onChange callback. It passes along the current palette and sets pattern to null when the input is empty, or to the literal value when non-empty. This is intended for use as the input change handler in SkinPicker.tsx so downstream logic consistently sees a string|null for pattern rather than an empty string.

## Remarks
handlePattern relies on a closure over onChange and palette from the surrounding component. It centralizes the normalization of the pattern value, ensuring the exposed update shape remains stable across calls and removing the need for callers to handle empty strings themselves.

## Notes
- The function does not mutate local state itself; it only invokes onChange with a new payload. If onChange is asynchronous, callers should handle potential race conditions at a higher level.
- Empty input becomes null, which is meaningful to downstream logic that distinguishes “no pattern” from an empty string.

---