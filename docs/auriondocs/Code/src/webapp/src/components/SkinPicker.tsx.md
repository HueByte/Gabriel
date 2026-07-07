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


SkinPickerProps defines the props for a skin-selection UI that combines a pattern and a color palette. The two primary selections, pattern and palette, are strings or null, representing the current selections and allowing an explicit unselected state. The onChange callback reports the full next state as { pattern, palette } whenever the user updates either dimension, so the parent can synchronize state atomically. If you supply an onReroll callback, the UI will render a reroll button beside the pickers; the reroll action changes only seed-derived dimensions and preserves any pinned pattern or palette. The disabled flag disables all interaction when true.

## Remarks

By isolating the skin state into a single props interface, this symbol decouples the rendering logic from how the app stores skin state. The presence of onReroll conveys a design choice: allow exploration of variations without losing user selections, which simplifies coordinating with parent components and any seed-based UI behavior. It also clarifies ownership: the SkinPicker component handles input events and communicates the result upward via onChange, while the parent remains the source of truth for the actual data.

## Notes

- Null values are meaningful: pattern: string | null and palette: string | null indicate no selection; ensure consumer handles nulls gracefully.
- onChange provides a new composite object; avoid mutating the provided next object and rely on the callback to update external state.

---

## SkinPicker
> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

```typescript
export function SkinPicker(
```


SkinPicker is a reusable React component that presents a UI for selecting or generating a skin variant based on a given pattern and color palette. It accepts a pattern that encodes the design constraints, a palette of colors to apply, and callbacks for when a skin is changed or a new variation is requested, plus a disabled flag to prevent interaction. Developers reach for this component when they need a consistent, declarative control to choose or randomize skins across parts of the UI (e.g., avatars, characters, or themed UI elements) without implementing their own selection logic.

## Remarks
SkinPicker encapsulates the common interaction model of skin selection: it consumes a pattern and palette and exposes two hooks to its parent: onChange for the chosen skin, and onReroll to request a new variation. By centralizing this behavior in a single component, you ensure consistent UX and theming across features that share the same skin concept. It also isolates the skin generation policy from the rest of the app—patterns and palettes can evolve independently, while the consumer only handles the resulting skin data via onChange.

## Notes
- When disabled is true, user interactions should be blocked and callbacks should not fire.
- If the provided palette changes while a skin is selected, you may want to reset or revalidate the current selection to avoid mismatches.
- Ensure onChange handles the structure of the skin data consistently with the rest of the app.

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


handlePalette is an event handler used to update the palette selection for the SkinPicker. It forwards a payload to onChange that includes the current pattern and a palette value derived from the input: if the value is an empty string, palette is set to null; otherwise, the value is passed through. This is invoked when a user selects or clears a palette in the UI, ensuring the parent receives a consistent shape for updates while preserving the existing pattern.

## Remarks
By centralizing the transformation from the raw input value to the onChange payload, this helper keeps the component’s render logic lean and reduces duplication. It also encodes the convention that an empty input means "no palette" by using null, which simplifies downstream logic and state handling.

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


It updates the pattern selection in SkinPicker by normalizing the input value: when the value is an empty string, the pattern is set to null; otherwise the raw string is used. The updated payload always includes the current palette alongside the pattern, and is sent via onChange. This makes it clear in the state whether a pattern is explicitly chosen or cleared.

---