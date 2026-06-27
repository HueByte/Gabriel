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

Properties for the SkinPicker component. Use this interface when a parent component needs to provide the current pattern and palette overrides and receive updates; it also exposes an optional reroll action and a disabled flag for temporarily disabling interaction.

## Remarks
This interface models externally controlled skin overrides (pattern and palette) so the parent fully controls the selected values. The optional onReroll callback causes the picker to render a reroll button when provided; invoking reroll changes seed-derived dimensions only and does not clear any explicit pattern/palette overrides.

## Example
```typescript
// inside a parent React component
const [pattern, setPattern] = useState<string | null>(null);
const [palette, setPalette] = useState<string | null>(null);
const [saving, setSaving] = useState(false);

const props: SkinPickerProps = {
  pattern,
  palette,
  onChange: ({ pattern: nextPattern, palette: nextPalette }) => {
    setPattern(nextPattern);
    setPalette(nextPalette);
  },
  onReroll: () => {
    // trigger a reroll of seed-derived dimensions; does not clear pattern/palette
  },
  disabled: saving,
};

// JSX
// <SkinPicker {...props} />
```

## Notes
- The onChange callback receives an object with pattern and palette which may be null; treat null as "no override".
- If onReroll is omitted, no reroll button is rendered.
- Reroll does not clear pinned overrides: explicit pattern/palette values survive a reroll (reroll only affects seed-derived aspects).

---

## SkinPicker

> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

Renders a UI control for selecting a "skin" composed of a pattern and a color palette. Use this component when you need a reusable, likely controlled widget that displays the current pattern/palette and lets the user change or randomize that selection; it also supports disabling user interaction.

## Remarks
This component appears to be intended as a controlled input: the current selection is supplied via the pattern and palette props and changes are reported back via onChange. An onReroll callback is provided so callers can implement a "randomize" or alternate-selection action. Passing disabled should prevent user interaction (verify exact behavior and visual states against the implementation).

## Example
```typescript
// Typical controlled usage (adapt to the real onChange/onReroll signatures in the implementation):
const [pattern, setPattern] = useState(initialPattern);
const [palette, setPalette] = useState(initialPalette);

<SkinPicker
  pattern={pattern}
  palette={palette}
  onChange={(next) => {
    // `next` will contain the new selection — check the real shape in source
    setPattern(next.pattern ?? pattern);
    setPalette(next.palette ?? palette);
  }}
  onReroll={() => {
    // request a new random skin from caller logic
  }}
  disabled={false}
/>
```

## Notes
- The source provided for this symbol was truncated; confirm the exact prop types and the shape of the onChange/onReroll callbacks in the implementation before integrating.
- Treat it as a controlled component: updating pattern/palette in response to onChange is likely required for changes to be reflected.
- If you pass callbacks, consider memoizing them to avoid unnecessary re-renders depending on the component's implementation.

---

## handlePalette

> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

Updates the selected palette for the current skin and notifies the parent via the onChange callback. It converts an empty-string selection into null (representing "no palette") and preserves the existing pattern value from the surrounding scope. Reach for this helper when wiring a palette <select> or similar input to the SkinPicker's change pipeline so the parent receives a consistent { pattern, palette } object.

## Remarks
This small adapter exists to normalize UI input values into the shape expected by the onChange consumer: empty selections become null while non-empty values are forwarded as-is. It keeps the pattern value from the surrounding closure untouched and only changes the palette field, so callers don't need to construct the full update object themselves.

## Example
```typescript
// Typical usage inside a React component's JSX
// <select onChange={e => handlePalette(e.target.value)}>
//   <option value="">(none)</option>
//   <option value="warm">Warm</option>
//   <option value="cool">Cool</option>
// </select>
```

## Notes
- handlePalette relies on `pattern` and `onChange` from the containing scope; ensure they are defined and have the expected types.
- It maps an empty string to `null`. If consumers expect undefined or an empty string to represent "no palette," adapt accordingly.
- This function builds and sends a new object to onChange but does not mutate any external state itself.

---

## handlePattern

> **File:** `src/webapp/src/components/SkinPicker.tsx`  
> **Kind:** function

Normalizes a user-provided pattern string and propagates it via the component's onChange callback. Empty strings are converted to null to represent the absence of a pattern, and the current palette value from the surrounding scope is forwarded unchanged.

## Remarks
This small handler centralizes the normalization rule (empty -> null) so callers and the rest of the component receive a consistent representation for "no pattern". It captures `onChange` and `palette` from the enclosing scope and delegates the actual update to the provided `onChange` function rather than mutating state itself.

## Example
```typescript
// typical usage inside the SkinPicker component JSX
<input
  value={pattern ?? ''}
  onChange={e => handlePattern(e.target.value)}
  placeholder="Enter pattern or leave empty"
/>
```

## Notes
- handlePattern relies on `palette` and `onChange` from the surrounding scope; ensure those are defined and have the expected shape.
- The conversion of an empty string to null is intentional; if an empty string should be preserved as a meaningful value, remove or adjust this normalization.
- This function is synchronous and not debounced; if input rate is high or onChange triggers expensive work, consider debouncing at the caller level.


---