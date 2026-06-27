# Chat.tsx

> **Source:** `src/webapp/src/components/Chat.tsx`

## Contents

- [ChatProps](#chatprops)
- [EntryActions](#entryactions)
- [ChatEntry](#chatentry)
- [VariantMeta](#variantmeta)
- [Chat](#chat)
- [Reasoning](#reasoning)
- [Thought](#thought)
- [ToolResult](#toolresult)
- [VariantPicker](#variantpicker)
- [advance](#advance)
- [applyAgentEvent](#applyagentevent)
- [hasActiveAssistantStream](#hasactiveassistantstream)
- [historyToEntries](#historytoentries)
- [lastIndexWhere](#lastindexwhere)
- [onKeyDown](#onkeydown)
- [onSubmit](#onsubmit)
- [prettyArgs](#prettyargs)
- [prev](#prev)
- [renderEntry](#renderentry)
- [send](#send)
- [toolCallEntry](#toolcallentry)
- [variantMetaOf](#variantmetaof)

---

## ChatProps

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** interface

Defines the properties accepted by the Chat component — use this interface when rendering or wrapping the Chat component so the parent can supply conversation identity, avatar determinism, optional palette overrides, and lifecycle callbacks (conversation loaded/missing, busy state, and message-sent notifications).

## Remarks
ChatProps centralizes the inputs and callbacks a parent needs to coordinate a single conversation view. It keeps UI concerns (conversationId, avatarSeed, optional paletteStops) separate from lifecycle/interaction concerns (onConversationLoaded, onConversationMissing, onMessageSent, onBusyChange). Notably, avatarSeed is used to produce a deterministic thinking-pulse animation and paletteStops — when provided by the server — will override the seed-derived colors so the indicator reflects server-driven visuals.

## Example
```typescript
import React from 'react';
import Chat from './Chat';
import { useNavigate } from 'react-router-dom';

function ConversationPage({ id }: { id: string }) {
  const navigate = useNavigate();

  return (
    <Chat
      conversationId={id}
      avatarSeed={42}
      paletteStops={null}
      onConversationLoaded={(conv) => {
        // pick up server-provided avatar seed or other metadata
        console.log('conversation loaded', conv.id);
      }}
      onConversationMissing={() => {
        // conversation deleted elsewhere — go back to list
        navigate('/conversations');
      }}
      onBusyChange={(busy) => {
        // reflect busy state in parent UI
        console.log('chat busy?', busy);
      }}
      onMessageSent={() => {
        // scroll, analytics, etc.
        console.log('message sent');
      }}
    />
  );
}
```

## Notes
- paletteStops can be undefined or null; when absent the component falls back to an avatarSeed-derived palette.
- onConversationLoaded is fired once per conversation switch when metadata becomes available — do not expect it to be called on every render.
- onConversationMissing is specifically emitted when the initial history fetch returns a 404 (conversation deleted); handle navigation or cleanup accordingly.

---

## EntryActions

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** interface

Represents the set of callbacks and a simple busy flag that a chat-entry component expects to receive from its parent. Use this interface when wiring per-message controls (delete, regenerate, switch variant, remember) so the entry UI can invoke standardized handlers and reflect an in-progress state.

## Remarks
This interface decouples entry-level UI from the implementation of those actions by centralizing the handler signatures. A parent controller (for example, a chat container) implements these callbacks and passes them down to each message/entry component, allowing consistent behavior, easier testing, and a single place to manage side effects and loading state.

## Example
```typescript
const actions: EntryActions = {
  busy: false,
  onDelete: (messageId) => {
    // remove message with id
  },
  onRegenerate: (messageId) => {
    // trigger message regeneration
  },
  onVariantSwitch: (targetId) => {
    // switch the displayed variant for the message
  },
  onRemember: (messageId) => {
    // mark message as remembered/saved
  },
};

// Passing to a chat entry component
// <ChatEntry id={msg.id} text={msg.text} actions={actions} />
```

## Notes
- All handlers expect a string identifier for the target message; callers should pass the correct message id.
- The busy flag is a simple signal for the UI to disable controls — the parent must toggle it around async operations.
- Consider memoizing these handlers (or the whole actions object) to avoid unnecessary re-renders of many entry components.

---

## ChatEntry

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type

Represents a single entry in the chat stream as a discriminated union. The union is discriminated by a literal `kind` field; the visible source shows at least a `kind: 'text'` variant. Use this type when consuming or rendering chat items and branch on `kind` to access variant-specific properties safely.

## Remarks
This type centralizes all possible chat entry shapes (messages, system notices, attachments, etc.) behind a single type so consumers can handle them via exhaustive narrowing. It enforces type-safe access to variant-specific fields and makes adding new entry kinds explicit — callers will get a type error if they forget to handle a new variant.

## Example
```typescript
function renderChatEntry(entry: ChatEntry) {
  switch (entry.kind) {
    case 'text':
      // entry is narrowed to the 'text' variant here — access text-specific fields safely
      return <div>{/* render text entry fields here */}</div>;
    default:
      // Exhaustiveness check: if a new variant is added, this will error at compile time
      const _exhaustiveCheck: never = entry;
      return null;
  }
}
```

## Notes
- The provided source is truncated; only the `kind: 'text'` branch is visible. Confirm the full type to learn all variants and their fields before accessing them. 
- Always narrow on `kind` (or use type guards) before reading variant-specific properties. 
- Use an exhaustive switch (or similar) to ensure new variants are handled at compile time.

---

## VariantMeta

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type

Small type describing metadata for a variant. It contains a single property, `variantGroupId` (string), which identifies the group or family the variant belongs to; use this type when a variant value needs to carry its group identifier.

---

## Chat

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Renders a chat user interface for a specific conversation and exposes callbacks so a parent can react to important events. Use this component when you want an encapsulated chat view that is tied to a conversation identifier and needs configurable visuals (avatar seed and palette) plus hooks for message, loading and busy-state events.

## Remarks
This component centralizes presentation and interaction for a single conversation: the parent supplies the conversationId and visual parameters, and receives lifecycle and interaction callbacks (message sent, busy state changes, conversation loaded/missing). Exposing these events keeps side effects and navigation in the parent while the Chat component focuses on UI and local behavior.

## Example
```typescript
<Chat
  conversationId="conversation-123"
  avatarSeed={42}
  paletteStops={["#0f172a", "#3b82f6", "#06b6d4"]}
  onMessageSent={(message) => {
    // persist message or update store
    console.log('message sent', message);
  }}
  onBusyChange={(isBusy) => {
    // show/hide global loading indicator
    setGlobalLoading(isBusy);
  }}
  onConversationLoaded={(conversation) => {
    // navigate or update state based on loaded conversation
  }}
  onConversationMissing={() => {
    // show not-found UI or redirect
  }}
/>
```

## Notes
- Prefer passing stable (memoized) callback functions to avoid unnecessary re-renders of the Chat component.
- Keep visual props (avatarSeed, paletteStops) stable when you want consistent appearance across renders; changing them will typically alter avatar generation or theming.

---

## Reasoning

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Reasoning(
```


A function declared with a single destructured parameter exposing properties `content` and `streaming`. The provided source is only the declaration fragment, so the implementation, return type, and exact prop types are not available in the snippet; callers should consult the full source to confirm behavior before using it.

## Remarks
This symbol appears in Chat.tsx and is therefore part of the chat UI codepath; it likely centralizes logic for handling or rendering "reasoning" output produced by the system and may behave differently when the `streaming` prop indicates incremental output. Because the body is not present in the snippet, do not assume whether it returns JSX (a React component), a plain string/value, or has side effects — verify in the full file.

## Notes
- The implementation/body was not included in the snippet; behavior and return type are unknown.
- Confirm the expected types for `content` (e.g., string, React.ReactNode, or structured data) and for `streaming` (likely a boolean) in the actual source before integrating.
- If this is a UI component that renders streaming content, ensure callers handle incremental updates and re-renders appropriately.

---

## Thought

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Thought(
```


A presentational React component named `Thought` that destructures a single `content` prop from its arguments. The implementation body is not present in the provided source, so the exact rendering, returned JSX, and side effects are unknown; based on the filename (`Chat.tsx`) and the symbol name, this component is intended to render a single chat message or "thought".

## Remarks
This symbol likely exists to encapsulate rendering and styling for an individual chat entry, keeping message-specific markup, formatting, and interaction (e.g., reactions, timestamps) isolated from the higher-level chat container. Placing per-message rendering in a small component helps the chat UI stay modular and makes it easier to change presentation without touching chat list management logic.

## Notes
- The implementation is missing/truncated in the provided source; the exact output (HTML structure, CSS classes, event handlers) cannot be determined from the signature alone.
- The type and shape of `content` are not specified; it could be a string, a ReactNode, or a structured object. Consumers should confirm the prop type or add TypeScript typings before use.
- If `content` is rendered as HTML or inserted into the DOM, ensure proper sanitization to avoid XSS vulnerabilities.

---

## ToolResult

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function ToolResult(
```


A React functional component that renders the output of a tool inside the chat UI. The component accepts a prop named `content` (the source visible to the documentation generator shows the signature fragment `function ToolResult({ content }`), and is intended to be used whenever tool-generated output needs to be displayed as part of a chat message. The implementation details are not available in the provided excerpt, so callers should not assume formatting or sanitization behavior.

## Remarks
This component isolates presentation concerns for tool results from the rest of the chat message rendering. Centralizing tool-output rendering makes it easier to apply consistent styling, formatting, or accessibility behavior for all tool-generated content in the chat UI.

## Example
```typescript
// Pass the tool output (string or node) to ToolResult when rendering a message
<ToolResult content={toolOutput} />
```

## Notes
- The full implementation and prop types are not present in the available source; confirm whether `content` expects a string, ReactNode, or a structured value before passing rich/HTML content.
- Do not assume the component sanitizes HTML or untrusted input — if you may render HTML, sanitize it first or verify the component's behavior in the codebase.


---

## VariantPicker

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

A function-style React component signature that accepts a destructured props object with three properties: `variant`, `disabled`, and `onSwitch`. The implementation and explicit prop types are not present in the provided source, so only the intent inferred from the parameter names can be documented: use this component when you need a UI control to represent or change the active "variant" (for example, a mode, option, or conversation variant) and to notify the parent via a callback when the user switches.

## Remarks
This symbol centralizes variant-selection concerns behind a single component API so callers can render a single control and react to changes via `onSwitch`. The lack of implementation in the provided source means callers should consult the real implementation or type declarations to confirm exact prop shapes and callback signature before relying on runtime behaviour.

## Example
```typescript
// minimal usage example — adjust types to match the real implementation
function Parent() {
  const [variant, setVariant] = React.useState<string>('default');
  const handleSwitch = (next: string) => setVariant(next);

  return (
    <VariantPicker
      variant={variant}
      disabled={false}
      onSwitch={handleSwitch}
    />
  );
}
```

## Notes
- The source provided is incomplete: the component body, return value, and prop types are missing. Verify the actual implementation for details such as the accepted type of `variant` and the exact signature of `onSwitch`.
- Treat `onSwitch` as a callback that will be called when the active variant should change; confirm whether it receives the new variant, an event object, or nothing in the real implementation.

---

## advance

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Advances the current variant selection to the next variant (wrapping to the first after the last) and notifies the consumer via onSwitch. Use this when you want a simple cyclic "next" action — for example as a click handler for a "next variant" control or as part of automated stepping logic.

## Remarks
This small helper captures the logic for moving from the current variant to the next one in the surrounding component's state. It computes the next index using modulo arithmetic so the selection wraps around from the final variant back to the first, then calls the onSwitch callback with the corresponding sibling id. Centralising this behaviour avoids duplicating wrap-around logic wherever a "next" action is needed.

## Example
```typescript
// Typical use inside a React component (closure provides variantIndex, variantCount, variantSiblingIds, onSwitch)
<button onClick={advance}>Next variant</button>
```

## Notes
- If variantCount is 0 this function will compute NaN for the next index; ensure variantCount > 0 before calling.
- The function assumes variantSiblingIds[next] exists; keep variantSiblingIds length in sync with variantCount to avoid undefined being passed to onSwitch.
- Calling advance has a side-effect (it invokes onSwitch), so consumers should expect potential navigation or state changes as a result.

---

## applyAgentEvent

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Updates the chat entries state in response to streaming agent events (textDelta, reasoningDelta, toolCall, etc.). Use this when you receive incremental AgentEvent objects from the backend or model stream and need to merge those tokens into the React state-managed ChatEntry list so the UI can render streaming assistant output, interim reasoning, and tool interactions.

## Remarks
This function centralizes the logic for turning incremental agent events into stable ChatEntry rows. It preserves React-friendly immutable updates, keeps streaming bubbles using stable keys/ids so the same React instance continues to render while content grows, and converts transient streaming bubbles into final entry types (for example, reclassifying a trailing streaming assistant text as a collapsed `thought` when a tool is called). It relies on small helpers (for example, lastIndexWhere and toolCallEntry) to find or construct entries and expects the provided setEntries dispatcher from useState/useReducer.

## Example
```typescript
// inside a React component that holds entries state
const [entries, setEntries] = useState<ChatEntry[]>([]);

// when receiving events from a WebSocket or model stream:
function onAgentEvent(evt: AgentEvent) {
  applyAgentEvent(evt, setEntries);
}
```

## Notes
- The function uses crypto.randomUUID() to create synthetic streaming ids; ensure the runtime environment supports it (modern browsers/node versions).
- setEntries must be the React Dispatch returned by useState/useReducer; the function always updates state immutably via the updater form (prev => ...).
- The implementation depends on helper utilities (e.g. lastIndexWhere, toolCallEntry). Those must be available in the same module scope to construct or find entries correctly.
- Streaming entries are sometimes dropped if empty (e.g., an empty streaming assistant bubble before a tool call will be omitted).

---

## hasActiveAssistantStream

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Returns true when the most recent chat entry is an assistant-produced text message that is currently streaming. Reach for this helper when the UI or control logic needs a quick check of whether the assistant is mid-stream (for example to show a streaming indicator, disable inputs, or decide whether to stop the stream).

## Remarks
This function performs a minimal, constant-time check against only the last element of the provided entries array — it treats the latest entry as the authoritative source of streaming state. It exists to keep streaming-related UI logic simple and cheap rather than scanning or inferring state from older entries.

## Example
```typescript
const entries: ChatEntry[] = [
  { kind: 'text', role: 'user', streaming: false, text: 'Hello' },
  { kind: 'text', role: 'assistant', streaming: true, text: 'Thinking...' }
];

if (hasActiveAssistantStream(entries)) {
  // show streaming indicator, prevent new requests, etc.
}

// empty array -> false
hasActiveAssistantStream([]); // false
```

## Notes
- The function only checks the last entry; a streaming flag on an earlier entry is ignored.
- It assumes a non-null array is passed. Passing null or undefined will throw when accessing length.
- The check is structural: it expects entries to have kind, role and streaming properties and tests kind === 'text', role === 'assistant', and a truthy streaming value.

---

## historyToEntries

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Converts an array of MessageResponse objects (the server/API message history) into an ordered list of ChatEntry objects suitable for rendering in the chat UI. Use this when you need to turn a persisted conversation history into the UI model; it preserves server-provided IDs as entry keys for stable history rendering.

## Remarks
This function encodes the mapping rules the UI expects for historical messages: user messages become text entries, assistant messages may produce up to three kinds of entries (a reasoning block, a thought block or final text, and zero or more tool call entries), and tool messages become toolResult entries. The function intentionally sets key === id for history-loaded entries because those ids are stable database ids; the key/id separation is only relevant for in-flight optimistic entries whose id may be replaced later.

## Example
```typescript
// Input: two messages from the API
const messages: MessageResponse[] = [
  { id: 'm1', role: 'user', content: 'Hi' },
  {
    id: 'm2',
    role: 'assistant',
    reasoningContent: 'Considering tools...',
    content: 'Result after thinking',
    toolCalls: [{ id: 'tc1', name: 'search', input: 'x' }]
  }
];

const entries = historyToEntries(messages);
/* entries will look like:
[
  { kind: 'text', key: 'm1', id: 'm1', role: 'user', content: 'Hi', variant: /*...*/ },
  { kind: 'reasoning', key: 'm2-reasoning', id: 'm2-reasoning', content: 'Considering tools...' },
  { kind: 'thought', key: 'm2', id: 'm2', content: 'Result after thinking' },
  // plus one or more toolCall entries created by toolCallEntry(m2.id, tc1)
]
*/
```

## Notes
- The mapping treats assistant content as a "thought" (internal reasoning) when the message has tool calls; without tool calls the assistant content becomes a regular assistant text entry.
- For user and assistant entries the code requires m.content to be truthy; tool-result entries use a null-check (m.content != null), so empty-string content may still produce a toolResult.
- System-role messages are intentionally ignored and do not produce ChatEntry items.

---

## lastIndexWhere

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Returns the index of the last element in the given array that satisfies the provided predicate. Scans the array from end to start and returns the index of the first element (from the right) for which pred(element) is true; returns -1 if no element matches. Use this helper when you need the last matching index without allocating a reversed copy of the array.

## Remarks
This is a small, allocation-free utility that performs a reverse linear scan. It accepts a readonly array and does not mutate it. The predicate receives only the element (not its index or the array), so callers that need position information should capture it differently or use a different helper.

## Example
```typescript
// Find the last even number
const nums = [1, 3, 4, 6, 7];
const lastEven = lastIndexWhere(nums, n => n % 2 === 0); // 3 (element 6)

// Find the last message from a specific user
type Message = { id: string; userId: string; text: string };
const messages: readonly Message[] = [
  { id: 'a', userId: 'u1', text: 'hi' },
  { id: 'b', userId: 'u2', text: 'hello' },
  { id: 'c', userId: 'u1', text: 'bye' }
];
const lastFromU1 = lastIndexWhere(messages, m => m.userId === 'u1'); // 2
```

## Notes
- Returns -1 when no element satisfies the predicate.
- Time complexity is O(n) in the length of the array; the predicate may be invoked up to arr.length times.
- The predicate is called with the element only; it does not receive the index or the array.

---

## onKeyDown

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Handles keydown events for a chat textarea: when the Enter key is pressed without the Shift modifier it prevents the default newline behavior and triggers a send action. Use this handler to submit a message with Enter while preserving Shift+Enter for inserting line breaks.

## Remarks
This function is a small helper used by a chat input component to centralize the Enter-to-send behavior. It deliberately calls send with the void operator (fire-and-forget) so the handler doesn't await the asynchronous send operation; error handling or UI state updates for the send operation should be implemented by the send function itself or its surrounding logic.

## Example
```typescript
// inside a React component
const send = async () => {
  // perform send logic (POST, state updates, etc.)
};

return (
  <textarea
    onKeyDown={onKeyDown}
    // other props: value, onChange, placeholder...
  />
);
```

## Notes
- Preventing the default behavior stops the textarea from inserting a newline when Enter is pressed without Shift.
- Holding Shift while pressing Enter allows a newline to be inserted (the handler ignores Shift+Enter).
- The handler uses `void send()` and does not await or handle errors; ensure send handles its own errors or that calling code manages send's lifecycle if you need feedback or retries.


---

## onSubmit

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Prevents the browser's default form submission and triggers the component's send() action. Use when wiring a React form's onSubmit to run client-side submission logic instead of allowing a full-page navigation.

## Remarks
This handler intercepts the native submit event (via e.preventDefault()) and invokes send() to perform the actual submission work. The expression void send() intentionally discards the returned promise so the handler remains synchronous (not declared async) and avoids unawaited-promise lint errors. Choose this pattern when you do not need to await send() inside the event handler.

## Example
```typescript
// inside a React component
const onSubmit = (e: FormEvent) => {
  e.preventDefault();
  void send();
};

return (
  <form onSubmit={onSubmit}>
    {/* form fields */}
  </form>
);
```

## Notes
- void send() drops the Promise: errors from send() will not be propagated to this handler. If you need to handle errors or await completion, make the handler async and use await send().
- e.preventDefault() is required to stop the browser from performing a full-page form submit/navigation.
- Ensure send is defined in the same component scope (and not a stale closure) before calling it.

---

## prettyArgs

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function prettyArgs(json: string): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `json` | `string` | — |

**Returns:** `string`


Converts a JSON string into a normalized, display-friendly form and suppresses empty arguments.

This helper returns an empty string for falsy inputs or the empty object literal "{}" so callers can avoid rendering empty argument lists. For non-empty input it attempts to parse the string as JSON and re-serializes it with JSON.stringify to produce a stable (compact) representation. If parsing fails the original input is returned unchanged.

## Remarks
This exists primarily for UI display: it normalizes argument payloads so they appear as a single-line JSON string (without extra whitespace) and hides empty objects. It is not intended to pretty-print with indentation; instead it produces a compact, canonical JSON string when the input is valid JSON.

## Example
```typescript
prettyArgs(undefined); // ""
prettyArgs(''); // ""
prettyArgs('{}'); // ""
prettyArgs('{"a": 1, "b": 2}'); // '{"a":1,"b":2}'
prettyArgs('not json'); // 'not json'  (fallback — returns original input)
```

## Notes
- The function returns a compact (minified) JSON string, not a multi-line indented "pretty" format — the name can be misleading. 
- Malformed JSON is silently passed through (the original string is returned); callers that require strict validation should parse separately.
- The special-case for '{}' means an objectively empty object will be rendered as an empty string; this choice is deliberate for UI suppression but may be surprising in other contexts.

---

## prev

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Selects the previous variant index (wrapping to the end when necessary) and invokes the component's onSwitch callback with the corresponding sibling id. Use this helper when you need a reusable "move to previous variant" action inside the Chat component (for example as an onClick handler or keyboard shortcut) instead of recomputing the wrapped index each time.

## Remarks
This closure reads variantIndex, variantCount, variantSiblingIds and onSwitch from the surrounding scope and centralizes the wrap-around arithmetic: (variantIndex - 1 + variantCount) % variantCount ensures the index cycles correctly. It exists so UI handlers can simply call prev to perform consistent, cyclic navigation and trigger the same onSwitch side-effect.

## Example
```typescript
// inside the Chat component's render/return
<button onClick={prev}>Previous</button>
```

## Notes
- variantCount must be a positive integer; if it is 0 the modulo operation yields NaN and indexing will fail.
- Ensure variantSiblingIds contains at least variantCount entries — otherwise the indexed lookup may be undefined.
- prev is a closure that uses captured values; if you memoize it (e.g. with useCallback) include the relevant dependencies so it uses up-to-date state.
- onSwitch is invoked synchronously; any side-effects it performs will run immediately when prev is called.

---

## renderEntry

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Renders a ChatEntry into the appropriate React markup for the chat UI, including assistant/user text, streaming assistant behavior, tool calls/results, thoughts/reasoning, and the per-entry action controls. Use this when you need a single, central renderer that turns a ChatEntry + EntryActions into the correct DOM for display and user interaction rather than manually branching on entry kinds in multiple places.

## Remarks
This function centralizes all presentation rules for different ChatEntry kinds (text, toolCall, toolResult, thought, reasoning, etc.) and the UI logic that surrounds persisted vs. streaming messages. It determines when to show action buttons (regenerate, remember, delete), when to render assistant text with streaming/typing affordances (StreamingText with animate/caret/galactic props), and how variant state affects the availability of regeneration and variant picking. Keeping this logic here ensures consistent behavior across the chat UI and isolates id/streaming/variant heuristics in one place.

## Notes
- Streaming and animation semantics: StreamingText captures the animate prop at mount, so switching an entry from streaming → done after mount will not interrupt the typing loop; the initial animate value controls the first render only.
- Persistence check relies on id prefixes: entries whose id starts with "tmp-" or "streaming-" are treated as non-persisted and will not show the action overlay; this is an important convention for correct button visibility.
- Regenerate availability: the regenerate action is only enabled for assistant entries that are persisted and represent the latest variant (variantIndex === variantCount - 1). Calling regenerate on older variants is intentionally prevented.
- UI disabling: action buttons use actions.busy to disable interaction; callers must set this flag while async operations are in progress to prevent concurrent actions.
- Keys and role handling: the renderer uses e.key for React keys and differentiates assistant vs. user roles to choose between StreamingText and static rendering.

---

## send

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Sends the current trimmed input as a user message to the conversation streaming API, appending an optimistic user entry to the chat, and then consumes streaming events (assistant content, persistence notices, compacting signals, errors). Use this as the chat component's send handler so the optimistic UI, busy/abort lifecycle, stick-to-bottom behavior, and compacting overlays are handled consistently instead of calling the streaming API directly.

## Remarks
This function encapsulates several UI and networking concerns so the component can present low-latency optimistic updates while the server responds incrementally. It: creates a temporary optimistic user entry (tmp-UUID) that is patched when persistence events arrive; forces the chat to stick to the bottom so the new turn is visible during streaming; uses an AbortController stored in abortRef so the UI can cancel the stream; and listens for compacting events to show/hide a compacting overlay. It intentionally avoids refetching the entire conversation on success to prevent remounting streaming UI (typewriter) when ids are patched — callers should expect messagesRef to be stale immediately after a successful send.

## Example
```typescript
// Typical use inside a React component: call send from a button or Enter key handler.
<button onClick={() => void send()}>Send</button>

// Or from a keyboard handler
const onKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    void send();
  }
};
```

## Notes
- send enforces a single outstanding send via the busy check; calling it while busy is a no-op.
- The function sets isAtBottomRef.current = true, so invoking send will scroll the view to the bottom on the next layout even if the user was reading older messages.
- After a successful send the in-memory messagesRef may be stale (it reflects the conversation before this turn); handleRegenerate is expected to lazily refetch when it needs up-to-date state.
- On network or streaming errors the code removes any assistant streaming placeholder entries and clears compacting/busy state; abortRef is cleared in finally so callers should not assume it remains set.

---

## toolCallEntry

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Creates a ChatEntry object representing a tool invocation that originated from a particular chat message. Use this when converting a MessageToolCall (the structured data for a tool call) into the ChatEntry shape consumed by the chat UI or storage layer so the call can be rendered/identified alongside other chat entries.

## Remarks
This helper centralizes the construction of a 'toolCall' ChatEntry so the id and key format is consistent across the codebase. It composes the entry id and key from the messageId and the tool call's id to provide a stable, message-scoped identifier for rendering and reconciliation.

## Example
```typescript
const messageId = 'msg-123';
const tc: MessageToolCall = {
  id: '42',
  name: 'translate',
  argumentsJson: '{"text":"hello","target":"es"}'
};

const entry = toolCallEntry(messageId, tc);
// entry => {
//   kind: 'toolCall',
//   key: 'msg-123-call-42',
//   id: 'msg-123-call-42',
//   toolCallId: '42',
//   name: 'translate',
//   argumentsJson: '{"text":"hello","target":"es"}'
// }
```

## Notes
- The returned entry.key and entry.id are derived by concatenating messageId and tc.id; if multiple calls share the same messageId and tc.id they will produce identical keys/ids.
- argumentsJson is forwarded as-is (no validation or parsing is performed by this function).
- The function returns a plain object and does not deep-clone nested data referenced by argumentsJson.

---

## variantMetaOf

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Creates a VariantMeta object by projecting the variant-related fields from a MessageResponse. Use this when you need just the variant identification data (group id, index, count and sibling ids) rather than the entire message object — for example when passing only variant metadata to UI components or analytics code.

## Remarks
This small adapter centralizes the mapping from MessageResponse to VariantMeta so callers don't need to repeat the same field selection. It keeps variant-related concerns separate from the rest of the message payload and makes intent explicit when only variant metadata is required.

## Example
```typescript
const message: MessageResponse = getMessageResponse();
const meta = variantMetaOf(message);
renderVariantControls(meta);
```

## Notes
- The returned object is a shallow projection: arrays (variantSiblingIds) and other reference types are not deep-cloned.
- Fields may be undefined if the source MessageResponse does not include variant information; the function does not validate or normalize those values.

---