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


This interface defines the props contract for the ModeSelector component. It exposes the conversationId to scope the mode selection to a specific conversation, a value that represents the current GabrielMode or null to indicate the default (Chatty) behavior, an onChanged callback for reporting user-driven changes, and an optional disabled flag to render the control non-interactive when needed.

## Remarks

This abstraction decouples the UI from state management by standardizing how a caller supplies the current mode and receives updates. The initial mode is sourced from on-load data (ConversationResponse.mode), ensuring consistency with persisted conversation state. By tying the mode to GabrielMode and scoping it with a conversationId, this interface supports per-conversation configuration and straightforward wiring to higher-level state managers.

## Example

```tsx
<ModeSelector
  conversationId="conv-123"
  value={null}
  onChanged={_next => { /* update local/global state with the next GabrielMode or null to reset to default */ }}
  disabled={false}
/>
```

## Notes

- value={null} denotes the default mode (treated as Chatty); supply a GabrielMode to override the default.
- onChanged is required and should update the parent state so the ModeSelector remains a controlled component.
- The disabled flag should be used to reflect non-editable states (e.g., during loading or when permissions prevent changes).


---

## ModeSelector
> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** function

```typescript
export function ModeSelector(
```


ModeSelector is an exported React function component from ModeSelector.tsx that destructures its props to include conversationId, value, onChanged, and disabled. The snippet provides only the function signature, so its rendering logic and side effects are not visible here. Developers would reach for it when they need a control tied to a specific conversation that exposes a value and an onChanged callback, with an optional disabled flag.

---