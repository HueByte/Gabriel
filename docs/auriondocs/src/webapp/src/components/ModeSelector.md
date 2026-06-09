# ModeSelector.tsx

> **Source:** `src/webapp/src/components/ModeSelector.tsx`

## Contents

- [ModeSelectorProps](#modeselectorprops)
- [ModeSelector](#modeselector)

---

## ModeSelectorProps

> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** interface

Props for the ModeSelector component — use when rendering a mode picker that is scoped to a specific conversation. This interface defines a controlled component API: the parent provides the current mode (or null for the default) and receives updates via onChanged. conversationId ties the selector to a conversation; disabled can be used to render the control inert.

## Remarks
The interface implements a common controlled-component pattern: the parent component owns the selected GabrielMode and passes it down via value, while the ModeSelector reports changes through onChanged. A value of null represents the default mode (treated as "Chatty"); the initial value typically comes from ConversationResponse.mode when a conversation is loaded.

## Example
```typescript
// Parent component managing the selected mode for a conversation
function ConversationHeader({ conversationId, initialMode }: { conversationId: string; initialMode: GabrielMode | null }) {
  const [mode, setMode] = useState<GabrielMode | null>(initialMode);

  return (
    <ModeSelector
      conversationId={conversationId}
      value={mode}
      onChanged={setMode}
      // disabled={true} // optional
    />
  );
}
```

## Notes
- value === null is treated as the default (Chatty); consumers should handle null in their state.
- onChanged may be called with null to reset to the default mode — ensure the handler accepts GabrielMode | null.
- conversationId is required to associate the selector with a particular conversation instance.
- disabled is optional; when omitted the control is expected to be interactive.

---

## ModeSelector

> **File:** `src/webapp/src/components/ModeSelector.tsx`  
> **Kind:** function

```typescript
export function ModeSelector(
```


A UI component that renders a selectable "mode" control for a given conversation and exposes the chosen mode to its parent via a change callback. Use this when you need a reusable, controlled selector to display or change the operational mode for a specific conversation rather than building the selector inline each time.

## Remarks
This component is intended as a thin, reusable abstraction over whatever UI is used to choose a conversation mode (radio buttons, a dropdown, segmented control, etc.). It is controlled by the parent: the current selection is provided via the value prop and changes are reported through onChanged. conversationId ties the selector to a particular conversation instance (for the parent to scope state, persistence, or telemetry) and disabled can be used to make the control non-interactive while work is in progress.

## Example
```typescript
import React, { useState } from 'react';
import { ModeSelector } from './ModeSelector';

function ConversationSettings({ conversationId }: { conversationId: string }) {
  const [mode, setMode] = useState<string>('default');

  return (
    <div>
      <h3>Conversation mode</h3>
      <ModeSelector
        conversationId={conversationId}
        value={mode}
        onChanged={(next) => setMode(next)}
        disabled={false}
      />
    </div>
  );
}
```

## Notes
- ModeSelector is a controlled component: keep the parent state in sync with the value prop and update it in onChanged.
- Ensure the value you pass matches the set of modes the component expects; passing unknown values may result in no option being selected.
- When disabled is true the control should not call onChanged; handle any asynchronous updates in the parent to re-enable it as needed.

---