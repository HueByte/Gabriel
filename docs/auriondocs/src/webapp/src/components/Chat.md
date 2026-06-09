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

Properties for the Chat component that represent a single conversation view. Use this interface when rendering the Chat component to provide the conversation identifier, deterministic avatar seed (and optional server-driven color stops) plus lifecycle callbacks for message sends, busy-state changes, and conversation-loading events.

## Remarks
ChatProps centralizes the small surface area the parent needs to control or observe for a conversation: which conversation to render (conversationId), how the avatar/"thinking pulse" is colored (avatarSeed and optional paletteStops), and several event hooks. onConversationLoaded is intended as the single place the Chat component exposes fetched conversation metadata so the parent can avoid repeating that fetch. onConversationMissing is specifically raised when the initial history fetch returns 404 (the conversation was deleted elsewhere) so the parent can navigate away or show an error.

## Example
```typescript
// Typical usage inside a parent component
<Chat
  conversationId={currentConversationId}
  avatarSeed={42}
  paletteStops={serverPaletteStops} // optional: readonly RGB[] | null
  onMessageSent={() => { /* update local UI, analytics, etc. */ }}
  onBusyChange={(busy) => setIsBusy(busy)}
  onConversationLoaded={(conv) => { setAvatarSeedFromServer(conv.avatarSeed); }}
  onConversationMissing={() => navigate('/conversations')}
/>
```

## Notes
- paletteStops is nullable/optional; when absent the component falls back to a palette derived from avatarSeed.
- onConversationLoaded fires once per conversation switch when the component has loaded the conversation metadata — avoid duplicating that fetch in the parent.
- onBusyChange may be emitted frequently as the chat performs network or processing work; keep handlers lightweight or debounce if needed.

---

## EntryActions

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** interface

A small DTO describing the action handlers and the busy state that a chat entry row/component can use to perform user-driven operations (delete, regenerate, switch variant, remember). Use this interface when passing a single prop containing all entry-related callbacks and state instead of supplying each callback separately.

## Remarks
This groups together the UI-facing operations for a single chat message entry: a boolean "busy" that signals whether actions should be disabled, and four callbacks representing the common operations a user can perform on an entry. Keeping these handlers together simplifies prop passing to child components that render message controls and centralizes how callers indicate an in-progress operation.

## Example
```typescript
const actions: EntryActions = {
  busy: isOperationPending,
  onDelete: (messageId) => api.deleteMessage(messageId),
  onRegenerate: (messageId) => api.regenerateMessage(messageId),
  onVariantSwitch: (targetId) => setActiveVariant(targetId),
  onRemember: (messageId) => store.rememberMessage(messageId),
};

// Passing to a child chat-entry component
<ChatEntry message={msg} actions={actions} />
```

## Notes
- The "busy" flag should reflect pending work for the entry (e.g., network request) so the UI can disable controls and prevent duplicate invocations.
- onVariantSwitch uses a parameter named `targetId` (not `messageId`) — ensure callers supply the correct identifier (typically the variant's id) to avoid mix-ups.
- Handlers are invoked with string IDs; callers should validate/normalize IDs if your app accepts empty or numeric identifiers.

---

## ChatEntry

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type

Represents a discriminated-union type for entries shown in the chat UI. Use this type when modeling or handling items in the chat stream so callers can narrow behavior based on the entry's 'kind' discriminator.

## Remarks
This type centralizes the different shapes of chat entries behind a single alias and relies on the "kind" literal property for safe narrowing. Callers (renderers, event handlers, serializers) can switch or if-check on entry.kind to obtain a precise subtype.

## Example
```typescript
function handleEntry(entry: ChatEntry) {
  if (entry.kind === 'text') {
    // TypeScript narrows `entry` to the 'text' variant here.
    // Access properties specific to the 'text' variant and handle rendering or processing.
  } else {
    // Handle other variants (if any) or treat as fallback.
  }
}
```

## Notes
- Always narrow on the `kind` property rather than relying on structural checks; this keeps code resilient as variants are extended.
- When adding new variants, update switches/if-chains and any exhaustive checks to avoid missing cases at compile time.
- If entries are created from untrusted input (e.g., JSON), validate the shape before treating it as a ChatEntry to avoid runtime errors.

---

## VariantMeta

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type

Represents minimal metadata for a variant; currently this type exposes a single required field, `variantGroupId`, which holds the identifier of the group to which the variant belongs. Use this type when you need to pass or store a variant's grouping identifier (for example in props, state, or mapping structures).

## Remarks
This type isolates grouping information from a variant's full payload so callers can depend on a lightweight, explicit shape when only the group identity is needed. Keeping this as a small alias makes it easy to extend later if additional metadata fields become necessary without changing call sites that only require the group id.

## Example
```typescript
const meta: VariantMeta = {
  variantGroupId: 'group-123'
};

function handleSelection(m: VariantMeta) {
  console.log('selected group:', m.variantGroupId);
}

handleSelection(meta);
```

## Notes
- `variantGroupId` is required and typed as `string`.
- This is a compile-time TypeScript type only; there is no runtime validation enforced by the type itself.
- If new metadata fields are added later, update usages that assume this is a single-property shape.

---

## Chat

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
export function Chat(
```


Renders a conversation-centered chat UI and exposes lifecycle and interaction hooks for embedding contexts. Use this component when you need a ready-made conversation view tied to a specific conversationId and want to customize avatar appearance and color palette while receiving notifications for key events (message sent, busy state changes, conversation load or missing).

## Remarks
This is a functional React component intended to act as the conversation surface: it accepts a conversation identifier and visual customization props (avatarSeed and paletteStops) and reports state changes and events back to the parent via callbacks. Keep parent-side state minimal by reacting to the provided callbacks rather than probing the component internals.

## Example
```typescript
// Typical usage inside a React component
<Chat
  conversationId="conv-123"
  avatarSeed="alice@example.com"
  paletteStops={["#FF7A7A", "#7ABFFF"]}
  onMessageSent={(message) => console.log('message sent', message)}
  onBusyChange={(isBusy) => setProcessing(isBusy)}
  onConversationLoaded={(meta) => console.log('conversation loaded', meta)}
  onConversationMissing={() => navigate('/conversations')}
/>
```

## Notes
- Keep conversationId stable when possible; frequent changes will typically cause the component to reload or remount the conversation view.
- onBusyChange may be invoked multiple times during network or processing activity — prefer lightweight handlers or debounce heavy work.
- paletteStops is intended to supply color values for UI theming (e.g., hex or CSS color strings); ensure values are valid CSS colors to avoid styling inconsistencies.


---

## Reasoning

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Reasoning(
```


Renders a piece of "reasoning" or explanatory content in the chat UI and supports a streaming mode. Use this component when you need to display intermediate explanation, chain-of-thought, or progressively arriving text in a chat conversation; it accepts props named content and streaming to control what is shown and whether the content should be presented incrementally.

## Remarks
This component isolates reasoning/explanation output from other chat message types so the UI can style and handle it differently (for example, enabling progressive rendering when content is streamed). Keeping reasoning rendering in a dedicated component makes it easier to apply different semantics, animations, or accessibility behavior without affecting standard chat message rendering.

## Example
```typescript
// Render static reasoning content
<Reasoning content={"The model concluded X because..."} streaming={false} />

// Render while streaming incremental updates (implementation-specific)
<Reasoning content={partialText} streaming={true} />
```

## Notes
- The source implementation was not provided; only the prop names (content, streaming) are known. Exact prop types and runtime behavior (e.g., whether content is a string, ReactNode, or a stream/generator) are not available.
- If streaming is true, the surrounding code may expect incremental updates or a specific shape for content — verify the concrete implementation before integrating.
- Sanitize or otherwise validate content if it can contain HTML or user-provided data to avoid injection issues.

---

## Thought

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Function named Thought that declares a single destructured parameter ({ content }). The implementation (function body and return) is missing from the available source, so the symbol's behavior, return value, and intended use cannot be determined from this repository snapshot. Do not rely on this symbol until its implementation is restored.

## Remarks
This symbol is incomplete in the current source — the file appears truncated or the function was left unfinished. The repository snapshot does not provide any details about what 'content' should contain or how it should be handled.

## Notes
- The source is syntactically incomplete; as-is the file will not compile until the function body/return are added.
- Only the parameter name (content) is visible; its expected type, shape, and side effects are unknown.

---

## ToolResult

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function ToolResult(
```


Renders the output produced by an external tool inside the chat UI. Use this presentational component when a chat message needs to display a tool-generated result provided via the `content` prop.

## Remarks
This component acts as a thin, focused renderer for tool results so the chat layout can treat tool output uniformly. Keeping tool-specific rendering in a single component makes it easier to apply consistent styling, accessibility attributes, or special formatting for tool responses.

## Example
```typescript
// Simple string content
<ToolResult content="Spellcheck completed: 3 issues found" />

// JSX content for preformatted output
<ToolResult content={<pre>{outputText}</pre>} />
```

## Notes
- The source provided was truncated; the concrete prop types and internal behavior are not available here. Verify the expected type of `content` (string, ReactNode, object, etc.) before use.
- Check how this component handles large or untrusted content (escaping, sanitization) if tool output may include HTML or user-supplied data.

---

## VariantPicker

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

A presentational React component that lets the user pick or switch between named "variants." Use this when you need a small controlled UI to display the current variant, optionally disable interaction, and react to user-initiated variant changes via a callback.

## Remarks
This component is a controlled input: the current selection is provided by the variant prop and any change should be reflected by updating that prop from the parent. The disabled prop prevents user interaction, and onSwitch is invoked when the user requests a different variant (the exact argument shape passed to onSwitch depends on the component implementation).

## Example
```typescript
// Render VariantPicker as a controlled component
function Parent() {
  const [variant, setVariant] = useState<string>('default');

  return (
    <VariantPicker
      variant={variant}
      disabled={false}
      onSwitch={(newVariant) => setVariant(newVariant)}
    />
  );
}
```

## Notes
- The component appears controlled: you must update the variant prop in response to onSwitch to reflect changes in the UI.
- Provide a stable onSwitch handler to avoid unnecessary re-renders in parent components.

---

## advance

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Computes the next variant index in a circular list and triggers the provided switch callback with the corresponding sibling id. Use this helper when you need to move forward by one position through a set of variants (for example, in a "Next" button or an automatic carousel) and ensure wrap-around from the last item back to the first.

## Remarks
This function encapsulates the wrap-around arithmetic ((variantIndex + 1) % variantCount) and the side effect of invoking onSwitch with the next variant's id. Keeping this logic in a small helper avoids duplicating index math and makes it easy to wire a single action to UI controls (buttons, timers) that should advance the active variant.

## Example
```typescript
// inside a React component where these values are in scope
const variantIndex = 0;
const variantCount = variants.length; // must be > 0
const variantSiblingIds = variants.map(v => v.id);
const onSwitch = (id: string) => setActiveVariantId(id);

const advance = () => {
  const next = (variantIndex + 1) % variantCount;
  onSwitch(variantSiblingIds[next]);
};

// usage in JSX
<button onClick={advance}>Next</button>
// or automatic advance
// useEffect(() => { const t = setInterval(advance, 3000); return () => clearInterval(t); }, [advance]);
```

## Notes
- Ensure variantCount > 0 before calling; modulo by zero yields NaN and will produce an invalid index.
- variantSiblingIds should contain at least variantCount entries and be indexed by the computed next value; otherwise the lookup may return undefined.
- onSwitch is invoked as a side effect; callers must provide a valid function to handle the id (and be prepared for it to be called synchronously).

---

## applyAgentEvent

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Applies an incoming AgentEvent to a React chat entries state updater, transforming streaming tokens and agent actions into the appropriate ChatEntry objects. Use this when integrating a streaming agent/event source with a chat UI so that partial (streaming) text, interleaved reasoning tokens, and tool calls/results are rendered and transitioned correctly in state rather than handling those transitions inline where events arrive.

## Remarks
This function centralizes the UI-state logic for streaming assistant output and agent reasoning: it appends or updates a streaming assistant text bubble for textDelta events, maintains a single streaming reasoning entry for reasoningDelta tokens, and converts in-flight streaming state into concrete entries when a toolCall arrives (freezing reasoning and reclassifying the trailing streaming assistant text as a collapsed "thought" if present). IDs/keys for streaming entries are generated so the React component instance can remain stable while content is patched (the code patches the synthetic streaming id to a real DB id later on). The function expects to be called with React's setState updater form (setEntries(prev => ...)) so it operates on the latest state.

## Example
```typescript
// inside a React component
const [entries, setEntries] = useState<ChatEntry[]>([]);
// evt is an AgentEvent received from a streaming source
applyAgentEvent(evt, setEntries);
```

## Notes
- The implementation relies on helpers/types not shown here (e.g. lastIndexWhere, toolCallEntry and the ChatEntry shape from your codebase). Ensure those exist and match the expected semantics.
- Uses crypto.randomUUID() to create stable synthetic ids/keys for streaming entries — ensure the runtime environment provides this API.
- setEntries is used with the functional updater to avoid stale-state races when events arrive rapidly or concurrently.

---

## hasActiveAssistantStream

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Returns true when the most recent chat entry is an assistant-generated text entry that is currently streaming. Reach for this predicate when deriving UI state from the chat entries (for example, to show a streaming indicator, prevent sending a new user message, or adjust controls while a response is being received).

## Remarks
This is a small, pure predicate used by the chat UI to detect an in-progress assistant response. It inspects only the last entry in the provided array and does not modify the input. Because it operates on the last element, it intentionally models the common chat pattern where only the latest assistant entry can be actively streaming.

## Example
```typescript
const entries: ChatEntry[] = [
  { kind: 'text', role: 'user', text: 'Hello' },
  { kind: 'text', role: 'assistant', text: 'Thinking...', streaming: true }
];

if (hasActiveAssistantStream(entries)) {
  // show streaming indicator, disable send button, etc.
}
```

## Notes
- If `entries` is empty the function returns false (it safely handles the missing last element).
- The check is strict about `kind === 'text'` and `role === 'assistant'`; other entry shapes or streaming sources will not be considered streaming by this predicate.
- It treats any truthy `streaming` value as active; if the streaming flag can be non-boolean, ensure callers set it predictably.

---

## historyToEntries

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Converts an array of MessageResponse objects (server/API chat history) into a flat list of ChatEntry objects suitable for the chat UI renderer. Use this when you need to build the chat view from persisted history — it preserves message ordering and expands assistant messages into one or more renderable entries (reasoning, thought, text, tool results).

## Remarks
This function exists to normalize heterogeneous history messages into a consistent sequence of UI entries. Assistant messages can produce multiple entries: a reasoning entry (if reasoningContent exists) is emitted first, then the assistant's main content is emitted either as a "thought" (if the message contains tool calls) or as a rendered assistant text entry, and finally any tool call results are appended. For history-built entries the function keeps key === id because those ids are the real database ids; the key/id distinction only matters for optimistic/in-flight entries handled elsewhere.

## Example
```typescript
// Input: one assistant message with reasoning, content and a tool call
const messages: MessageResponse[] = [
  {
    id: 'msg1',
    role: 'assistant',
    reasoningContent: 'I think we should...',
    content: 'Final answer text',
    toolCalls: [{ id: 'tc1', /* ... */ }]
  }
];

const entries = historyToEntries(messages);
// entries will be (conceptually):
// [
//   { kind: 'reasoning', key: 'msg1-reasoning', id: 'msg1-reasoning', content: 'I think we should...' },
//   { kind: 'thought', key: 'msg1', id: 'msg1', content: 'Final answer text' },
//   { kind: 'toolResult', key: '...', id: '...', toolCallId: 'tc1', content: '...' }
// ]
```

## Notes
- The reasoning entry uses a synthetic key/id of `${id}-reasoning` (so consumers should not expect that id to match a database id).
- Tool result entries are emitted only when a toolCall has a toolCallId and non-null content; tool calls are appended after assistant content.
- Messages with role === 'system' are intentionally ignored and not rendered.
- For history entries this function sets key === id; callers should not rely on a separate key/id mapping for these archived messages.

---

## lastIndexWhere

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Searches an array from the end and returns the index of the first element (from the end) that satisfies the provided predicate, or -1 if none match. Use this when you need the position of the last element meeting a condition; it is a small utility alternative to Array.prototype.findLastIndex (or when you want an explicit, dependency-free implementation).

## Remarks
This generic function accepts a readonly array and a predicate that receives each element (not index or array) and tests it. It iterates from the last element toward the first and returns as soon as a matching element is found, so it avoids checking earlier elements once a match is discovered.

## Example
```typescript
const items = ['apple', 'banana', 'cherry', 'banana'];
const lastBanana = lastIndexWhere(items, e => e === 'banana');
// lastBanana === 3

const numbers = [1, 3, 5, 6, 7];
const lastEven = lastIndexWhere(numbers, n => n % 2 === 0);
// lastEven === 3

const none = lastIndexWhere(numbers, n => n > 10);
// none === -1
```

## Notes
- Predicate is called with only the element value; it does not receive index or the array. If you need index-aware logic, change the predicate accordingly.
- Time complexity is O(n) in the worst case; the loop stops early on a match.
- Returns -1 when no element satisfies the predicate.
- Accepts readonly arrays and does not mutate the input.

---

## onKeyDown

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Runs when a key is pressed inside a textarea used for chat input. If the Enter key is pressed without Shift, the handler prevents the textarea from inserting a newline and triggers the send() action (Shift+Enter still allows a newline). Use this on textareas where Enter should submit the message while Shift+Enter inserts a line break.

## Remarks
This is a small event handler that centralizes the Enter-vs-Shift+Enter behavior for chat-style text input. It prevents the default browser newline insertion for plain Enter presses and delegates the actual send operation to a send() function in the same scope. The implementation uses `void send()` to intentionally ignore the returned promise (avoiding a floating-promise lint error) rather than awaiting it here.

## Example
```typescript
const send = async () => {
  // perform send logic (POST, WebSocket send, etc.)
};

const onKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    void send();
  }
};

return <textarea onKeyDown={onKeyDown} />;
```

## Notes
- Calling `e.preventDefault()` stops the textarea from inserting a newline for plain Enter presses.
- `void send()` does not await the send operation; if you need to handle errors or ensure ordering, await/send inside the handler or handle errors inside `send`.
- The handler checks `!e.shiftKey` to allow Shift+Enter to insert line breaks as expected.

---

## onSubmit

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Prevents the browser's default form submission behavior and invokes the surrounding component's send function to handle the form data. Use this as a React form onSubmit handler when you want to handle submission client-side without a page reload.

## Remarks
This function calls e.preventDefault() to stop the native navigation/POST and then calls send(). The call is prefixed with void to intentionally ignore the returned Promise (i.e. the handler does not await send). That pattern is used here to avoid treating the returned Promise as the return value of the event handler.

## Example
```typescript
// inside a React component
<form onSubmit={onSubmit}>
  <!-- form fields -->
  <button type="submit">Send</button>
</form>
```

## Notes
- Because send() is not awaited, any errors it throws must be handled inside send itself; otherwise they will be unobserved.
- The parameter is a React FormEvent; ensure the handler is passed to a <form> element (not a button) so preventDefault prevents the form submission.

---

## prettyArgs

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Normalize a JSON argument string for display in the UI: produce a compact JSON representation when the input is valid JSON, return an empty string for falsy input or the literal "{}", and return the original input unchanged if it is not valid JSON.

## Remarks
This small helper is used to make argument payloads concise when rendering in the chat UI — it hides empty objects and ensures valid JSON is represented in a consistent, compact form. It deliberately does not attempt to pretty-print with indentation; its goal is normalization and brevity rather than multiline formatting.

## Example
```typescript
prettyArgs('')           // returns ''
prettyArgs('{}')         // returns ''
prettyArgs('{"a":1}')  // returns '{"a":1}' (parsed then re-stringified)
prettyArgs('not json')   // returns 'not json' (left unchanged)
```

## Notes
- The function returns an empty string for the exact input "{}" — this special-case can be surprising if callers expect an explicit empty-object representation.
- Valid JSON is re-parsed and re-stringified without added whitespace (compact form); it does not produce indented/pretty-printed JSON.
- Inputs that are non-empty but not valid JSON are returned verbatim (no trimming or sanitation is performed).

---

## prev

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Moves the current selection to the previous variant in a circular list and calls the surrounding onSwitch callback with that variant's sibling id. Use this helper when wiring a "previous" control (button, keyboard shortcut, etc.) so callers don't need to implement wrap-around index math.

## Remarks
prev is a small closure that reads variantIndex, variantCount, variantSiblingIds and onSwitch from its surrounding scope. It uses modular arithmetic to compute the prior index with wrap-around (so the previous of index 0 becomes the last index) and then invokes onSwitch with the id found at that computed index. Centralizing this behavior avoids duplicating the wrap-around logic at each call site.

## Notes
- Ensure variantCount is a positive integer; if variantCount is 0 the modulo yields an invalid index and behavior will be incorrect.
- variantSiblingIds should be an array (or indexable collection) with at least variantCount entries so that variantSiblingIds[next] is valid.
- onSwitch is expected to be a callable function; if it's missing or not a function a runtime error or unexpected behavior may occur. The function is synchronous and not debounced or throttled.

---

## renderEntry

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Renders a single ChatEntry as React elements appropriate to its kind. Use this when mapping chat history into the UI so each entry (assistant/user text, tool calls/results, thoughts, reasoning, etc.) is displayed with the correct visual components and action affordances (regenerate, remember, delete, variant picker).

## Remarks
Centralizes the presentation logic for every ChatEntry variant so the chat list rendering code can remain simple (e.g. entries.map(e => renderEntry(e, actions))). For text entries it decides whether to use StreamingText (assistant streaming state) or static content, and it computes visibility/enablement for contextual actions based on persistence, streaming state, role, and variant metadata.

## Example
```typescript
// Typical usage inside a React component's render/return
return (
  <div className="chat-history">
    {entries.map(e => renderEntry(e, actions))}
  </div>
);
```

## Notes
- An entry is considered persisted only when it is not streaming and its id does not start with "tmp-" or "streaming-"; action buttons are hidden for non-persisted entries.
- The `animate` prop for StreamingText is captured at mount, so toggling an entry from streaming → done after mount does not restart the streaming animation.
- Regenerate is enabled only for assistant entries that are the latest variant of a turn; regenerating older variants is intentionally disallowed.
- UI action buttons are disabled while `actions.busy` is true; callers should set that flag to prevent concurrent operations.

---

## send

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Sends the trimmed contents of the chat input as a new user message, performs an optimistic UI update, and streams the agent's response. Use this when the user submits a message (for example, on Enter or Send); it guards against duplicate sends, attaches a temporary optimistic message ID, and drives the streaming update lifecycle until completion, error, or cancellation.

## Remarks
This function coordinates the end-to-end send flow inside the Chat component: it creates a temporary user entry (tmp-<uuid>) and appends it to entries for an immediate optimistic UI update, sets the UI to stick-to-bottom, then consumes the event stream from streamChat. Incoming events are applied via applyAgentEvent; special compacting events update a compacting overlay counter. The implementation intentionally avoids refetching the whole conversation after a successful send because stream events patch optimistic IDs to real DB IDs and remounting the typewriter (due to id/key changes) would interrupt the streaming reveal. An AbortController is stored in abortRef.current so the stream can be cancelled from other code paths.

## Notes
- The function returns immediately if the trimmed input is empty or the component is busy; callers should expect no action in those cases.
- Optimistic entries use a temporary id/key (tmp-...) for their whole optimistic lifetime; stream events are expected to replace those with real DB ids — other code should not assume entries are immediately consistent with the server state (messagesRef may be stale until subsequent events arrive).
- abortRef.current is replaced with a new AbortController for each send; to cancel an in-flight send call abortRef.current.abort() from outside. The compacting counter is cleared in the finally block to avoid leaving the overlay visible after errors or cancellation.

---

## toolCallEntry

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Creates a ChatEntry object representing a tool invocation by combining a parent messageId with a MessageToolCall.

Use this helper when converting a MessageToolCall into the ChatEntry shape consumed by the chat UI or state, ensuring a consistent id and key format for React rendering and lookups.

## Remarks
This function centralizes how tool-call entries are represented in the chat: it builds both the stable entry id and the React list key from the parent message id and the tool-call id, and copies the main fields (name, argumentsJson) into the ChatEntry shape. That keeps rendering and identity logic consistent across the codebase and avoids repeating string formatting where tool-call entries are created.

## Example
```typescript
const messageId = "msg-123";
const tc: MessageToolCall = {
  id: "456",
  name: "search",
  argumentsJson: '{"q":"typescript"}'
};

const entry = toolCallEntry(messageId, tc);
// entry => {
//   kind: 'toolCall',
//   key: 'msg-123-call-456',
//   id: 'msg-123-call-456',
//   toolCallId: '456',
//   name: 'search',
//   argumentsJson: '{"q":"typescript"}'
// }
```

## Notes
- The generated id/key uses `${messageId}-call-${tc.id}`; ensure tc.id is defined and stable to avoid duplicate or `...-call-undefined` keys.
- argumentsJson is passed through without validation or cloning; if it contains large payloads or mutable objects, handle accordingly before calling this helper.
- This function does not guarantee global uniqueness beyond combining messageId and tc.id — collisions are possible if those inputs are not unique.

---

## variantMetaOf

> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

Creates a VariantMeta object by picking the variant-related fields from a MessageResponse (variantGroupId, variantIndex, variantCount, variantSiblingIds). Use this helper when you need the compact variant metadata derived from a full MessageResponse rather than the entire message.

## Remarks
This is a tiny mapping helper that centralizes the logic for extracting variant metadata from a MessageResponse. It keeps callers from repeating the same property selection and makes intent clearer when only variant metadata is required (for example, when storing or forwarding variant identity separate from the message body).

## Example
```typescript
const msg: MessageResponse = getMessageResponse();
const meta: VariantMeta = variantMetaOf(msg);
// meta now contains variantGroupId, variantIndex, variantCount, variantSiblingIds
```

## Notes
- The function performs a shallow copy: nested structures such as variantSiblingIds are passed through by reference (not deep-cloned).
- No validation or normalization is performed — if the MessageResponse is missing these fields they will be copied as-is (possibly undefined).

---