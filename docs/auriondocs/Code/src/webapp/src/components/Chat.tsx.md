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

```typescript
interface ChatProps
```


ChatProps is the props contract for the Chat component used by the web UI. It bundles the identifiers and callbacks that configure a chat session: the conversationId to load and persist context, an avatarSeed that deterministically drives the thinking-pulse animation, and an optional paletteStops override to align the pulse colors with the active Gabriel Sequence. Optional callbacks surface user actions and conversation lifecycle events to the parent: onMessageSent notifies when the user sends a message, onBusyChange communicates loading state, onConversationLoaded fires after metadata is loaded for a new conversation, and onConversationMissing signals that the conversation no longer exists (e.g., 404) so the parent can navigate away.

## Remarks
This interface acts as a stable boundary between the Chat component and its host, encapsulating the essential theming, identity, and lifecycle hooks needed for a chat session. It cleanly separates UI concerns (avatar visuals, pulse colors) from data-fetching logic and provides explicit hooks for hosts to react to events like message submission and conversation switches. The paletteStops option enables server-driven branding by overriding the seed-based palette to match the active Gabriel Sequence.

## Notes
- paletteStops is optional; when provided (and non-null), the thinking-pulse bars color scheme comes from these stops instead of the seed-derived palette, ensuring visual continuity with the active Gabriel Sequence.
- onConversationLoaded fires after the conversation metadata loads during a switch, enabling parents to sync state (e.g., avatarSeed) without re-fetching.
- onConversationMissing fires when the initial history fetch returns 404; the parent typically navigates away from the stale URL.

---

## EntryActions
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** interface

```typescript
interface EntryActions
```


EntryActions defines a compact contract for the actionable controls associated with a chat entry in the Chat.tsx UI. It groups a busy flag with four callbacks that drive user interactions: delete, regenerate, switch variants, and remember. Implementers supply concrete behavior for these actions, while UI components consume the interface to render and wire controls without depending on a specific implementation.

## Remarks
This interface encapsulates the interaction concerns for a single chat entry. The busy flag signals in-flight work so the UI can disable actions appropriately, avoiding overlapping operations. The onDelete, onRegenerate, onVariantSwitch, and onRemember callbacks all receive an identifier (messageId or targetId) that scopes the action to the correct entry, facilitating loose coupling between the UI and the underlying business logic.

## Example
```typescript
// Example: simple action handlers for a chat entry
const exampleActions: EntryActions = {
  busy: false,
  onDelete: (messageId) => {
    console.log(`Deleting message ${messageId}`);
    // trigger deletion logic here
  },
  onRegenerate: (messageId) => {
    console.log(`Regenerating response for ${messageId}`);
    // trigger regeneration logic here
  },
  onVariantSwitch: (targetId) => {
    console.log(`Switching to variant ${targetId}`);
  },
  onRemember: (messageId) => {
    console.log(`Remembering message ${messageId}`);
  },
};
```

## Notes
- When busy is true, consumers should disable related UI controls to prevent concurrent operations on the same entry.
- The handlers expect string identifiers and should rely on a consistent ID scheme across the UI to avoid misrouting actions.

---

## ChatEntry
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type alias

```typescript
type ChatEntry =
  | { kind: 'text'; key: string; id: string; role: 'user' | 'assistant'; content: string; streaming?: boolean; variant?: VariantMeta }
  | { kind: 'thought'; key: string; id: string; content: string; streaming?: boolean }
  | { kind: 'reasoning'; key: string; id: string; content: string; streaming?: boolean }
  | { kind: 'toolCall'; key: string; id: string; toolCallId: string; name: string; argumentsJson: string }
  | { kind: 'toolResult'; key: string; id: string; toolCallId: string; content: string };
```


ChatEntry represents the various forms a single chat item can take in the UI, modeled as a discriminated union so callers can treat every entry uniformly while branching on kind to render differently. It covers user/assistant text messages, internal thoughts, streams of reasoning, and tool interactions, all sharing common identifiers but exposing shape-specific fields as needed. This type is typically consumed by the chat rendering layer and storage components to keep a cohesive history of dialogue events.

## Remarks

The union’s discriminant, kind, enables exhaustive handling and clear rendering paths for each entry kind (text, thought, reasoning, toolCall, toolResult). All variants share the generic identifiers (key and id) while presenting shape-specific data—such as role and content for text entries, toolCallId and argumentsJson for tool interactions, or content for reasoning streams—so a single consumer can switch on kind and access the appropriate fields without casting. This abstraction is designed to accommodate the evolution of the chat pipeline (e.g., adding new entry kinds) without breaking existing renderers.

## Example

```typescript
const userEntry: ChatEntry = { kind: 'text', key: 'u1', id: '1', role: 'user', content: 'Hello' };
const toolCallEntry: ChatEntry = { kind: 'toolCall', key: 't1', id: '2', toolCallId: 'tc1', name: 'Search', argumentsJson: '{"query":"openai"}' };
```

## Notes

- Treat ChatEntry as a discriminated union; ensure exhaustive handling when switching on kind to avoid missing a variant.
- All variants share the common metadata (key, id), but their required fields differ; handle narrowing carefully when consuming a value.


---

## VariantMeta
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type alias

```typescript
type VariantMeta = {
  variantGroupId: string;
  variantIndex: number;
  variantCount: number;
  variantSiblingIds: readonly string[];
};
```


Defines the shape of metadata for a single variant within a group of related variants. It carries the group identifier, the variant’s position within that group, the total number of variants in the group, and the identifiers of the other variants in the same group. This lightweight descriptor is used by UI layers (for example in the chat interface) to render and navigate variant options without loading the full variant payload.

## Remarks

VariantMeta isolates variant-collection concerns from the full variant data, enabling lightweight transfer and predictable UI behavior when presenting variant selectors. By carrying only grouping and positional metadata, components can reason about enabled states, navigation order, and sibling relationships without depending on concrete variant details. Treat this object as an immutable descriptor that can be freely composed and passed through UI boundaries.

## Example

```typescript
const example: VariantMeta = {
  variantGroupId: 'color',
  variantIndex: 1,
  variantCount: 4,
  variantSiblingIds: ['color-red', 'color-green', 'color-blue']
};
```

## Notes

- The variantSiblingIds array is readonly; avoid mutating it in place.
- The type does not enforce invariants like variantIndex < variantCount; callers should validate as needed.

---

## Chat
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
export function Chat(
```


Chat is a React function component that renders the chat UI for a specific conversation and reports user actions and lifecycle events back to its parent via callbacks. It accepts a conversationId to identify the conversation, avatarSeed for deterministic avatar visuals, and paletteStops to drive theming; the onMessageSent, onBusyChange, onConversationLoaded, and onConversationMissing props are used to communicate user actions and data-loading states upward.

## Remarks
Chat encapsulates the presentation and interaction logic of a single conversation, decoupling it from the data-loading and persistence concerns handled by the parent. By exposing explicit callbacks for message sending and lifecycle events, it gives the rest of the app flexibility to manage server requests, loading indicators, and error handling while reusing a consistent chat surface across conversations. The avatarSeed and paletteStops props enable stable, conversation-specific visuals, helping users recognize and stay oriented within a long-running thread.

---

## Reasoning
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Reasoning(
```


Reasoning is a TypeScript React function component defined in the chat UI (src/webapp/src/components/Chat.tsx). It accepts a props object with two fields: content and streaming. From this partial signature, the symbol appears to be involved in handling the reasoning portion of a chat interaction, though the actual rendering implementation is not visible in the provided excerpt.

---

## Thought
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Thought(
```


Thought is a React function component that takes a props object with a content property. As suggested by its declaration and its location in src/webapp/src/components/Chat.tsx, it serves as a presentation primitive for rendering the provided content within the chat interface, encapsulating the rendering concerns of a 'thought' so callers can render it consistently without duplicating markup.

## Remarks
Thought acts as a small, reusable UI building block for displaying thought content in chat conversations. By isolating its rendering, you can swap visuals, adjust styling, or apply accessibility considerations in one place, while leaving higher-level chat composition unchanged.

## Notes
- The code snippet is incomplete—the function body and return value are not shown, so exact rendering, props shape, and behavior are not visible.
- The actual prop types for content are not included in the fragment; rely on the implementation to determine accepted types (e.g., string vs. ReactNode).
- If this component is part of a larger chat theming system, ensure it is wired to the theme and layout conventions used by Chat.tsx.

---

## ToolResult
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function ToolResult(
```


ToolResult is a React function component in Chat.tsx that destructures a single prop named content. The snippet here shows only the function signature and does not reveal how content is rendered, so the exact output or styling cannot be inferred from this excerpt. A developer would reach for ToolResult when they need to display or integrate the content produced by a tool within the chat interface, as implied by its name.

---

## VariantPicker
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function VariantPicker(
```


VariantPicker is a small React functional component that presents a user interface for selecting among content variants in the chat UI. It takes three props: variant (the currently selected variant), disabled (a boolean that disables user interaction), and onSwitch (a callback invoked with the new variant when the user changes selection). Use VariantPicker when you want a reusable, testable control to switch between variants instead of embedding variant-switching logic directly in Chat.tsx.

## Remarks
VariantPicker abstracts the variant-switching behavior behind a clean, controlled API. It allows the parent component to own the variant state and respond to user input via onSwitch, which makes it straightforward to reuse and to test in isolation. It also promotes a consistent UX for variant selection across the chat UI by providing a single, centralized control.

## Notes
- When disabled is true, the component should not respond to user input or invoke onSwitch.
- If variant is a complex object, ensure its identity is stable to avoid unnecessary re-renders; prefer primitive or memoized values when possible.

---

## advance
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const advance = () =>
```


Advances to the next variant in a circular list by computing the next index and switching to that variant’s ID. It uses (variantIndex + 1) % variantCount to wrap around, then calls onSwitch with variantSiblingIds[next] to perform the transition. This is the kind of tiny navigator you wire to a Next control to let users cycle through available chat variants without manually handling bounds.

## Remarks
Encapsulates wrap-around and ID lookup in a single, reusable handler, keeping UI event wiring simple and less error-prone. It relies on surrounding state (variantIndex, variantCount, variantSiblingIds) and a callback (onSwitch), making it a lightweight navigation primitive that can be swapped or mocked in tests without touching the UI logic.

## Notes
- variantCount must be > 0 to avoid a division-by-zero in the modulo operation.
- Keep variantIndex, variantCount, and variantSiblingIds in sync with the actual variant list; a mismatch could yield an undefined ID.
- Since this function invokes onSwitch, ensure the callback can handle rapid or reentrant calls without causing inconsistent state.

---

## applyAgentEvent
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function applyAgentEvent(
  evt: import('../api/streamChat').AgentEvent,
  setEntries: React.Dispatch<React.SetStateAction<ChatEntry[]>>,
)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `evt` | `import('../api/streamChat').AgentEvent` | — |
| `setEntries` | `React.Dispatch<React.SetStateAction<ChatEntry[]>>` | — |


applyAgentEvent updates the chat entries state in response to AgentEvent messages, orchestrating live streaming bubbles for assistant text, interleaved reasoning, and tool invocations. It appends to an active streaming text bubble on textDelta, maintains a single streaming reasoning entry for reasoningDelta, and on toolCall, freezes the current reasoning, converts the trailing streaming content into a collapsed 'thought', and then inserts the tool invocation entry.

## Remarks
Centered as the UI glue between the agent stream and the chat display, this function encapsulates the rules for when to show ongoing thinking versus finished content. By using synthetic streaming IDs and stable keys, it lets React preserve DOM nodes during typing without remounting. The special handling of tool calls—promoting the trailing streaming text to a 'thought' and appending a tool entry—enables a clean ReAct-like narrative: think, act, observe.

## Notes
- Be aware that it relies on browser crypto APIs to generate streaming IDs; in environments without crypto, ensure a polyfill or fallback ID strategy.
- The function mutates state via a functional updater; callers must ensure setEntries is the React state setter from useState.

## Dependencies
- React
- ChatEntry

## Dependency APIs
- type ChatEntry (src/webapp/src/components/Chat.tsx)

---

## hasActiveAssistantStream
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function hasActiveAssistantStream(entries: ChatEntry[]): boolean
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `entries` | `ChatEntry[]` | — |

**Returns:** `boolean`


hasActiveAssistantStream reports whether the most recent chat entry is a streaming text message authored by the assistant. Call it to detect an in-progress assistant reply and drive UI decisions (e.g., showing a streaming indicator) instead of inspecting the entries array manually.

## Remarks
Its role is to centralize the streaming-detection logic to the last-entry condition, preventing duplicated checks across components. It also guards against false positives by requiring the entry to be a text-type with role 'assistant' and a truthy streaming flag, ensuring only genuine streaming outputs are treated as active.

## Notes
- The function only inspects the last entry; if a non-streaming entry is appended after a streaming one, the result will be false until that entry becomes the last element.
- If the entries array is empty, the function returns false; callers should handle empty input gracefully.

---

## historyToEntries
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function historyToEntries(messages: MessageResponse[]): ChatEntry[]
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `messages` | `MessageResponse[]` | — |

**Returns:** `ChatEntry[]`


historyToEntries translates a list of MessageResponse objects into a renderable sequence of ChatEntry items for the chat UI. It iterates the API history and emits entries that reflect each message's role, streaming state, and any associated tool calls, including a dedicated reasoning entry when reasoningContent is present and separate toolResult entries for tool calls; for history-built messages, key and id are identical, while in-flight entries may have their id patched later during streaming.

## Remarks
The function decouples the API payload from UI rendering by converting the raw message stream into a consistent, UI-friendly sequence of entries. It explicitly handles reasoning streams, tool interactions, and the final textual content, ensuring the chat UI can render partial thinking and tool usage in the correct order. By emitting distinct entry kinds (reasoning, text, thought, toolResult), it provides the UI with rich structure to progressively display messages as they arrive.

## Notes
- System messages are not rendered by this transformer; surface any system-level context in a separate UI layer if needed.
- A message without content yields no entry; user/assistant turns contribute entries only when content exists, and tool-calls are appended as separate toolResult entries in the order they appear.

---

## lastIndexWhere
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function lastIndexWhere<T>(arr: readonly T[], pred: (e: T) => boolean): number
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `arr` | `readonly T[]` | — |
| `pred` | `(e: T) => boolean` | — |

**Returns:** `number`


lastIndexWhere finds the last index in a read-only array where the provided predicate evaluates to true. It walks from the end of the array toward the start and returns the index of the first element that satisfies pred, or -1 if none match. This is useful when you need the most recent element meeting a condition without scanning from the front.

## Remarks
Viewed as a tiny backwards-search utility, it complements forward-search patterns like findIndex by performing a single backward pass and never mutating the input. The generic T keeps it usable for any element type, and the readonly arr emphasizes that no mutation occurs during the search. If your environment offers Array.prototype.findLastIndex, lastIndexWhere provides a self-contained alternative with the same backward-search semantics.

## Example
```ts
// Basic backward search on a plain array
const xs = [1, 2, 3, 4, 2];
const lastIndex = lastIndexWhere(xs, x => x === 2);
console.log(lastIndex); // 4

// If no element matches
console.log(lastIndexWhere([1, 3, 5], x => x > 10)); // -1
```

## Notes
- Return value -1 when no element matches; callers must handle this case.
- The predicate is evaluated from the end; predicates with side effects will run for elements from the tail until a match is found.

---

## onKeyDown
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const onKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `KeyboardEvent<HTMLTextAreaElement>` | — |


This onKeyDown handler is attached to the chat input textarea to enable sending messages with Enter. When Enter is pressed without Shift, it prevents the default newline behavior and calls send(); pressing Shift+Enter inserts a newline as usual.

## Remarks
This small abstraction centralizes Enter-to-send behavior in one place, decoupling the UI from the sending logic. It ensures Shift+Enter still yields a newline, preserving common chat UX expectations, while Enter alone reliably submits the current message.

## Notes
- Using void send() intentionally ignores the returned value (often a Promise). If you rely on send()'s result or handle errors, consider awaiting or adding error handling at the call site.
- Ensure this handler is bound to the textarea's onKeyDown; associating it with a different key event or element may alter UX expectations.


---

## onSubmit
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const onSubmit = (e: FormEvent) =>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `FormEvent` | — |


onSubmit is a compact form-submission handler that prevents the default browser submission and delegates to send() to perform the actual submission. It’s designed to be attached to a form’s onSubmit, so the UI stays responsive and the page does not reload when a user submits a message; the result of send() is intentionally not awaited, as indicated by the explicit void call.

## Remarks
onSubmit acts as a thin bridge between the UI layer and the sending workflow, decoupling the event handling from the business logic. It relies on a send function available in scope; by using void, it signals that the asynchronous result is intentionally ignored, which keeps the handler small but places responsibility for error handling elsewhere if needed. It is a common pattern in React/TypeScript forms where the submission should trigger async work without blocking the UI.

## Notes
- The promise returned by send is not awaited; if you need error feedback to users, handle the promise elsewhere or modify the handler to await with try/catch.

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


Transforms a string that should be JSON into a compact, canonical JSON representation. If the input is empty or exactly '{}', it returns an empty string to indicate 'no arguments'. Otherwise it attempts to parse the string as JSON and immediately stringify the result; if parsing fails, it returns the original input unchanged. This makes it safe to render or log user-provided argument strings without throwing, while normalizing valid JSON.

## Remarks
This is a small, pure helper that centralizes how UI code renders argument payloads. It relies on the native JSON.parse / JSON.stringify behavior, so it benefits from the language's standard JSON semantics and avoids reimplementing formatting rules. Returning the original input for invalid JSON preserves user input rather than producing misleading or broken displays.

## Example
```typescript
// Common case: normalize a valid JSON string
prettyArgs("{\"a\":1,\"b\":2}");

// Empty input yields an empty string
prettyArgs("");

// Non-JSON input is returned as-is
prettyArgs("not json");
```

---

## prev
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const prev = () =>
```


prev is a small navigation helper that moves the UI to the previous variant in a circular list. It wraps around from the first variant to the last by computing the target index as (variantIndex - 1 + variantCount) % variantCount and then switching to the corresponding variant using onSwitch with the ID from variantSiblingIds.

## Remarks
Encapsulates wrap-around navigation logic and keeps the variant-switching flow focused in one place. It depends on surrounding state (variantIndex, variantCount, variantSiblingIds) and the callback onSwitch to perform the actual switch. This abstraction makes wiring a Previous control in the Chat.tsx variant selector straightforward without duplicating boundary checks in the UI code.

## Notes
- If variantCount is 0 this will yield NaN; guard against zero variants or disable the control when there are none.

---

## renderEntry
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function renderEntry(e: ChatEntry, actions: EntryActions)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `ChatEntry` | — |
| `actions` | `EntryActions` | — |


Renders a single chat entry as a React node, selecting the appropriate visual rendering and interaction controls based on the entry's kind and state. It handles text messages (with special behavior for live, streaming assistant entries), tool interactions, and non-message entries by delegating to specific subcomponents. This centralizes per-entry UI decisions so the chat UI can consistently compose the conversation feed.

## Remarks

renderEntry acts as the central renderer for chat rows, coordinating how different entry kinds are displayed and interacted with. It couples the content with UI affordances (such as variant switching, regeneration, memory saving, and deletion) in a way that respects the entry's persistence and streaming status. By encapsulating this logic in a single function, the surrounding chat container can render a heterogeneous stream of messages without duplicating conditional rendering logic, while still enabling fine-grained behavior (e.g., hiding actions for ephemeral streaming messages and enabling regeneration only on the latest assistant variant).

## Example

```typescript
// Example usage demonstrating a persisted assistant text entry with a single variant
const entry: ChatEntry = {
  kind: 'text',
  role: 'assistant',
  streaming: false,
  id: 'msg-1',
  content: 'Hello, world!',
  variant: { variantIndex: 0, variantCount: 1 }
};

const actions: EntryActions = {
  busy: false,
  onVariantSwitch: (_v: any) => {},
  onRegenerate: (_id: string) => {},
  onRemember: (_id: string) => {},
  onDelete: (_id: string) => {}
};

renderEntry(entry, actions);
```


---

## send
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const send = async () =>
```


The send function is an asynchronous handler that submits the current user input as a new chat message, creates an optimistic chat entry, and then streams the backend reply to progressively render the assistant’s message. It guards against empty submissions and concurrent sends, clears the input, raises a busy state, and marks the UI to stay scrolled to the bottom. It establishes an AbortController to cancel the streaming session if needed, assigns a temporary id for the optimistic entry, and patches that entry with the real database id as server events arrive. If the stream completes or errors, it finalizes the UI state and invokes onMessageSent on success, or cleans up placeholders on failure. Finally, it resets compacting and busy indicators and clears the abort handle to avoid leaks, ensuring cleanup even when the stream is interrupted. The function relies on streamChat, applies agent events to update the chat log, and uses auxiliary state like isAtBottomRef and entries to coordinate the live, streaming user experience.

---

## toolCallEntry
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function toolCallEntry(messageId: string, tc: MessageToolCall): ChatEntry
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `messageId` | `string` | — |
| `tc` | [`MessageToolCall`](../../../api/Gabriel.API/Contracts/Messages/MessageResponse.cs.md) | — |

**Returns:** `ChatEntry`


Converts a MessageToolCall into a ChatEntry that represents a tool invocation inside a chat transcript. Given a messageId and a ToolCall descriptor tc, it computes a stable identifier by composing messageId and tc.id for both the key and the id, and returns a ChatEntry with kind 'toolCall' and fields populated from the descriptor: toolCallId from tc.id, name from tc.name, and argumentsJson from tc.argumentsJson.

## Remarks
By encapsulating the transformation from tool payload to chat entry, this helper decouples tool interactions from UI rendering and guarantees a uniform representation of tool calls across the chat interface. It also centralizes the deterministic key generation so the same tool call yields the same entry wherever constructed.

## Notes
- The function is pure; it does not mutate inputs and returns a new ChatEntry object.
- The key and id are derived from messageId and tc.id; ensure those values remain stable to avoid collisions or duplicate entries.

---

## variantMetaOf
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function variantMetaOf(m: MessageResponse): VariantMeta
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `m` | [`MessageResponse`](../../../api/Gabriel.API/Contracts/Messages/MessageResponse.cs.md) | — |

**Returns:** `VariantMeta`


Maps a MessageResponse to a VariantMeta by extracting the variantGroupId, variantIndex, variantCount, and variantSiblingIds. Use it whenever you need a concrete VariantMeta derived from a message payload, avoiding repetition of the field-mapping logic.

## Remarks

Centralizes the mapping so that if the shape or source of variant data changes, update happens in one place. It also makes the intent explicit: this function is solely responsible for projecting variant details from a MessageResponse into a VariantMeta, decoupling UI concerns from the API model.

## Example

```typescript
// Example: derive VariantMeta from an existing MessageResponse `m`
const meta: VariantMeta = variantMetaOf(m);
```

## Notes

- The function copies references for nested structures (e.g., variantSiblingIds). If you mutate the input array after obtaining the Meta, those changes will be visible through the Meta as well. Consider cloning if a defensive copy is required: `variantSiblingIds: [...m.variantSiblingIds]`.
- The mapping is intentionally narrow: only the four variant-related fields are carried forward; any additional fields on MessageResponse are ignored by this helper.

---