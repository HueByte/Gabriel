# ModeSelector.tsx

> **Source:** `src/webapp/src/components/ModeSelector.tsx`

## Contents

- [ModeSelectorProps](#modeselectorprops)
- [ModeSelector](#modeselector)

---

## ModeSelectorProps
> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** interface

```typescript
interface ModeSelectorProps
```


ModeSelectorProps defines the properties for the ModeSelector control in the web UI. It binds the selector to a specific conversation, exposes the current mode (GabrielMode or null for default), provides a callback to request changes, and supports disabling the control.

## Remarks
This interface enables the ModeSelector to be a controlled, stateless UI component. By passing the current value and an onChanged callback, the parent maintains authority over mode selection and can synchronize it with server data (e.g., via ConversationResponse on load). The conversationId ties the selection to a particular conversation context, allowing different conversations to have independent mode states while reusing the same component.

## Notes
- Null value indicates the default mode (treated as Chatty); callers should handle null both for the value and for the next value in onChanged.


---

## ModeSelector
> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** function

```typescript
export function ModeSelector(
```


ModeSelector is a React functional component that renders a control for selecting the mode associated with a specific conversation. It takes a conversationId to scope the control to a particular conversation, a value representing the currently selected mode, an onChanged callback invoked with the new mode when the user makes a selection, and a disabled flag that renders the control in a non-interactive state when true.

## Remarks
By encapsulating mode presentation behind ModeSelector, the UI remains consistent across the app and any changes to how modes are displayed or validated can be localized here. It acts as a specialized presentation component that communicates user-initiated changes back to its parent via onChanged, while keeping the parent free from implementation details of the underlying control.

## Example
```typescript
// Typical usage within a React component
function ExampleUsage() {
  const [mode, setMode] = useState<string>("default");
  return (
    <ModeSelector
      conversationId="conv-42"
      value={mode}
      onChanged={setMode}
      disabled={false}
    />
  );
}
```

## Notes
- Ensure the consumer provides valid mode values to avoid runtime errors.
- If ModeSelector is disabled, onChanged should not be called.


---