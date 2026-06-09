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

Props for the SkinPicker component: supplies the currently selected pattern and palette (each may be a string or null), a required onChange handler to receive updates, an optional onReroll action that adds a reroll button, and an optional disabled flag to disable interaction. Use this interface when you want the SkinPicker to be controlled by parent state rather than internal state.

## Remarks
This is a simple controlled-component props shape. pattern and palette represent "overrides" (explicit selections) and may be null to indicate no override. onReroll — when provided — triggers a reseed of any seed-derived dimensions but does not clear pattern/palette overrides: pinned selections survive a reroll. The component expects the parent to apply changes received via onChange to keep the control in sync.

## Example
```typescript
// Parent component using SkinPicker as a controlled input
function Parent() {
  const [skin, setSkin] = React.useState<{ pattern: string | null; palette: string | null }>({ pattern: null, palette: null });

  const handleChange = (next: { pattern: string | null; palette: string | null }) => {
    setSkin(next);
  };

  const handleReroll = () => {
    // trigger reseed logic elsewhere (doesn't clear pattern/palette)
    console.log('reroll requested');
  };

  return (
    <SkinPicker
      pattern={skin.pattern}
      palette={skin.palette}
      onChange={handleChange}
      onReroll={handleReroll}
      disabled={false}
    />
  );
}
```

## Notes
- pattern and palette may be null; onChange receives both fields and callers should handle null values.
- onReroll is optional — if omitted, no reroll button is rendered.
- Reroll does not clear pattern/palette overrides (pins survive reroll).
- disabled should disable all interactive elements (including reroll when present).

---

## SkinPicker

> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

Renders a UI for selecting an avatar "skin" by choosing a pattern and a color palette. Use this component when you need a reusable, form-friendly control that displays the current pattern and palette, notifies the host of changes, and optionally supports a "reroll" action to randomize or choose an alternative.

## Remarks
Treats the provided pattern and palette as the source of truth and reports user selections through the supplied callbacks. It is intended as a controlled component: callers supply the current values and update them in response to onChange. The onReroll callback provides a convenient hook for a host to replace the current values (for example, with a random choice) without the picker itself making those decisions.

## Example
```typescript
// Parent component maintains selection and passes handlers
function AvatarEditor() {
  const [pattern, setPattern] = useState<PatternType>(initialPattern);
  const [palette, setPalette] = useState<PaletteType>(initialPalette);

  return (
    <SkinPicker
      pattern={pattern}
      palette={palette}
      onChange={({ pattern: p, palette: pal }) => {
        if (p !== undefined) setPattern(p);
        if (pal !== undefined) setPalette(pal);
      }}
      onReroll={() => {
        // choose new pattern/palette and update state
      }}
      disabled={false}
    />
  );
}
```

## Notes
- Ensure your onChange handler updates both pattern and palette as needed; the component forwards selections but does not persist them itself.
- If disabled is true, the picker should be treated as non-interactive; provide the same value props to avoid visual or state mismatches.

---

## handlePalette

> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

Updates the parent's state with a new palette selection while preserving the current pattern. When given an empty string it treats that as "no palette" and passes null for the palette field; otherwise it forwards the provided string.

## Remarks
Used as a change handler inside the SkinPicker component to propagate palette changes up via the onChange callback. It intentionally converts an empty selection to null so the parent can distinguish between "no palette chosen" and an actual string value. The function captures the current pattern and onChange values from its enclosing scope and only modifies the palette.

## Example
```typescript
// Typical usage inside a component render
<select onChange={e => handlePalette(e.target.value)}>
  <option value="">(none)</option>
  <option value="light">Light</option>
  <option value="dark">Dark</option>
</select>
```

## Notes
- An empty string is converted to null; callers should expect palette to be either a string or null.
- handlePalette closes over pattern and onChange from its containing scope — ensure those are the intended values when the function is invoked.
- onChange is invoked synchronously; if it performs heavy work consider debouncing at the caller side.

---

## handlePattern

> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

Normalizes a pattern string and notifies the parent via the onChange callback. Treats an empty string as absence of a pattern (null) and forwards the current palette value unchanged. Use this inside the SkinPicker component as the handler for user input that updates the selected pattern.

## Remarks
This function is a small adapter that lives in the SkinPicker component's closure: it maps the UI representation (an empty string) to the canonical model representation (null) and delegates the actual update to the provided onChange callback along with the component's current palette. It centralizes the normalization logic so callers don't need to duplicate the empty-string → null mapping.

## Example
```typescript
// Typical usage in a TSX input handler
<input
  value={pattern ?? ''}
  onChange={e => handlePattern(e.target.value)}
/>
```

## Notes
- handlePattern relies on onChange and palette from the outer scope; it performs a side-effect by calling onChange rather than returning a new value.
- It treats only the exact empty string as "no pattern" (null); it does not trim or otherwise validate the input.
- Downstream code must expect pattern to be either a non-empty string or null (not an empty string).

---