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


ModeSelectorProps defines the props expected by the ModeSelector component to manage a per-conversation mode setting. It includes a conversationId that scopes the setting, a value that is either GabrielMode or null (null indicates the default, treated as Chatty), an onChanged callback that is invoked with the next mode value when the user makes a selection (GabrielMode or null), and an optional disabled flag to render the control non-interactive. The initial value is sourced from ConversationResponse.mode on load.

## Remarks
ModeSelectorProps embodies a controlled component pattern: the parent owns the mode in state and passes it via value while updates flow back through onChanged. Using null to denote the default mode provides a clean reset path without introducing another sentinel value. By tying the value to a specific conversation through conversationId and initializing from ConversationResponse.mode, this component can be reused across chats while preserving the contextual mode.

## Notes
- Pass null to onChanged to reset to the default mode.
- Ensure your consumer maps null to the default GabrielMode when storing state.
- When disabled is true, avoid rendering interactive controls; onChanged should not be used to indicate intent.

---

## ModeSelector
> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** function

```typescript
export function ModeSelector(
```


ModeSelector is a React functional component that renders a control for selecting a mode associated with a particular conversation. It is designed to be controlled by its parent: the currently selected mode is supplied via value, and any user changes are reported back through onChanged. The conversationId prop ties the control to a specific conversation context, while disabled toggles interactivity.

## Remarks
By encapsulating the mode-selection UI, ModeSelector promotes reuse across the app and keeps higher-level components focused on data flow rather than presentation. It defines a stable interface for mode values and their change semantics, making it easy to swap the underlying presentation (e.g., a dropdown, segmented control) without altering its callers. The prop contract implies that the parent owns the authoritative mode state and persists changes.

## Example
```typescript
// Example usage of ModeSelector
function ConversationToolbar({ conversation }) {
  const [mode, setMode] = React.useState(conversation.mode);
  return (
    <ModeSelector
      conversationId={conversation.id}
      value={mode}
      onChanged={newMode => setMode(newMode)}
      disabled={conversation.isArchived}
    />
  );
}
```

## Notes
- The parent component owns the mode value; this control reflects that state via the value prop and reports changes through onChanged.
- If the available mode options change, ensure the parent updates the value prop accordingly to keep the UI in sync.

---