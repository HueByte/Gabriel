# ModeSelector.tsx

> **Source:** `src/webapp/src/components/ModeSelector.tsx`

## Contents

- [ModeSelectorProps](#modeselectorprops)
- [ModeSelector](#modeselector)

---

## ModeSelectorProps

> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** interface

Properties for the ModeSelector component — identifies which conversation the selector is for, carries the current mode (or null for the default "Chatty" mode), and provides a callback for when the user changes the mode. Use this interface when rendering a ModeSelector for a specific conversation so the component can display and update that conversation's mode.

## Remarks
This props shape keeps the ModeSelector decoupled from store logic: the parent provides the conversationId and current mode (initially from ConversationResponse.mode) and receives updates through onChanged. The value field is nullable: null represents the default/Chatty mode rather than an omitted or unknown state, so consumers should treat null as a meaningful value.

## Example
```typescript
// Parent component rendering a ModeSelector for a conversation
function ConversationHeader({ conversation }: { conversation: ConversationResponse }) {
  const [mode, setMode] = React.useState<GabrielMode | null>(conversation.mode ?? null);

  return (
    <ModeSelector
      conversationId={conversation.id}
      value={mode}
      onChanged={(next) => setMode(next)}
      disabled={false}
    />
  );
}
```

## Notes
- value === null is treated as the default "Chatty" mode; do not equate null with "unset" unless that is intended.
- onChanged may be called with null to indicate switching back to the default mode — ensure handlers accept null.
- conversationId ties the selector to a specific conversation; if conversation identity can change, update props accordingly to avoid mismatches.


---

## ModeSelector

> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** function

Renders a UI control for selecting a conversation's "mode" and reports changes to a parent component. Use this React functional component when you need to display or allow editing of a conversation's mode (for example in a conversation header or settings panel). It accepts the conversation identifier, the current selected value, a change callback, and a disabled flag to prevent user interaction.

## Remarks
This component centralizes the selection UI so callers don't need to reimplement mode selection across the app. The parent is responsible for providing the current value and for persisting or reacting to changes. The conversationId prop provides contextual information (for example, to scope analytics or to be forwarded by the change handler) but the exact behavior depends on the component's implementation.

## Example
```typescript
// Typical controlled usage: parent keeps the selected mode in state
function ConversationHeader({ id }: { id: string }) {
  const [mode, setMode] = useState<string>("default");

  return (
    <ModeSelector
      conversationId={id}
      value={mode}
      onChanged={(newMode) => setMode(newMode)}
      disabled={false}
    />
  );
}
```

## Notes
- The provided source snippet was truncated; confirm the exact prop types before wiring strong TypeScript types (e.g. whether `value` is a string, enum, or object).
- Verify what `onChanged` receives: a raw mode value, an event, or a richer payload — wiring may break if the signature is assumed incorrectly.
- Treat this as a controlled component: supply the current `value` and update it from `onChanged` to keep the UI in sync.

---