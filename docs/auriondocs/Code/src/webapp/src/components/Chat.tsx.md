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


ChatProps defines the props contract for the Chat component. It bundles the per-conversation identity (conversationId), a deterministic avatar appearance (avatarSeed) and an optional color override (paletteStops) to align the indicator with the active Gabriel Sequence, plus optional callbacks to surface key events to the parent (onMessageSent, onBusyChange, onConversationLoaded, onConversationMissing). Use this interface whenever you render Chat.tsx to ensure consistent visuals and to translate user actions and conversation state into the surrounding app state.

## Remarks
ChatProps acts as a lightweight boundary between the UI layer and the data/flow logic of a conversation. It preserves determinism in the avatar visuals through avatarSeed, enabling the same thinking-pulse pattern to render across renders and tab switches. PaletteStops let the parent drive color theming to match the actual conversation, while the callbacks decouple the Chat component from navigation and data-fetch concerns, reducing duplication and improving testability.

## Example
```typescript
import type { ChatProps } from './Chat';
const exampleProps: ChatProps = {
  conversationId: 'conv-123',
  avatarSeed: 42,
  paletteStops: null,
  onMessageSent: () => { /* handle post-send */ },
  onBusyChange: (busy) => console.log('Chat busy:', busy),
  onConversationLoaded: (conv) => { console.log('Conversation loaded', conv); },
  onConversationMissing: () => { console.log('Conversation was deleted'); },
};

// Usage example
// <Chat {...exampleProps} />
```

## Notes
- paletteStops is optional and, if null or undefined, the avatar colors derive from avatarSeed with seed-based defaults.
- avatarSeed should remain stable for a given conversation to preserve a deterministic visual rhythm.
- onConversationLoaded fires when conversation metadata finishes loading after a switch; keep its handler idempotent and inexpensive.

---

## EntryActions
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** interface

```typescript
interface EntryActions
```


EntryActions defines the per-entry action surface used by the chat UI. It exposes a busy flag and four callbacks for a specific message: onDelete(messageId), onRegenerate(messageId), onVariantSwitch(targetId), and onRemember(messageId). This interface is intended to be passed to components rendering per-message controls, enabling the UI to trigger business logic without depending on concrete implementations.

## Remarks
By encapsulating these actions behind a single interface, the UI remains decoupled from how deletion, regeneration, variant switching, or remembering are implemented. This makes components easily testable and swappable, as different environments can provide different action handlers while preserving the same contract. The targetId in onVariantSwitch signals that variant changes may refer to a different entity than the message itself, supporting multi-variant entries.

## Notes
- busy should gate interactions; disable controls while an operation is in progress to avoid race conditions.
- Callbacks return void; side effects are handled by the implementation. If asynchronous behavior is required, manage it outside this interface.
- Ensure IDs (messageId/targetId) align with the app's identity model to prevent referencing non-existent entities.

---

## ChatEntry
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type alias

```typescript
type ChatEntry =
  |
```


ChatEntry is a TypeScript discriminated union that models a single item in the chat feed used by the chat UI. It uses a kind field to differentiate variants, starting with the text variant { kind: 'text', ... } which represents plain text messages. This design enables rendering code to safely pattern-match on the entry’s kind and access the appropriate payload for each variant, while allowing the set of supported entry types to grow without breaking existing code.

## Remarks
By encoding all chat entry kinds as a discriminated union, Chat.tsx can render different visuals for text messages, system notices, or media entries in a type-safe way. It decouples payload structure from presentation; rendering can switch on kind and read only the fields guaranteed by that variant. As new kinds are introduced, the compiler can help surface missing handling paths, guiding UI evolution.

## Notes
- When adding new variants, update all pattern matches to handle the new kind; prefer exhaustive switches to catch omissions at compile time.
- Fields specific to a variant may be optional; always narrow to the variant before accessing them.
- Be mindful of runtime data that may not conform to the expected payload; validate or provide fallback rendering.

---

## VariantMeta
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** type alias

```typescript
type VariantMeta =
```


VariantMeta is a TypeScript type that describes the metadata for a single variant within a variant group. It carries a single field, variantGroupId: string, which links the variant to its group and enables UI logic to group, filter, or reason about variants by their group.

## Remarks
VariantMeta serves as a lightweight contract that keeps variant-related data decoupled from presentation concerns. By standardizing how a variant references its group, components and utilities can reason about grouping, validation, and rendering consistently across the Chat UI.

## Example
```typescript
const meta: VariantMeta = { variantGroupId: "theme" };

const metas: VariantMeta[] = [
  { variantGroupId: "theme" },
  { variantGroupId: "layout" }
];
```

## Notes
- VariantMeta is a plain object type; at runtime there is no corresponding value beyond what you create.
- If you introduce additional metadata about variants, prefer extending this type or composing with other types rather than mutating its shape.

---

## Chat
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
export function Chat(
```


Chat is a React functional component that renders a chat interface bound to a single conversation identified by conversationId. It coordinates the presentation and lifecycle of that conversation and exposes callbacks for outbound messages and lifecycle changes, allowing a parent to react to user actions and data availability without embedding lower-level logic.

## Remarks
Chat acts as a small orchestration layer around a conversation UI: it selects which conversation to display, emits onMessageSent when the user sends a message, and reports its busy state and data readiness via onBusyChange, onConversationLoaded, and onConversationMissing. The avatarSeed prop enables deterministic avatar generation for participants, while paletteStops provides theming hints that the component can apply without requiring external styling. This separation keeps the chat UI focused on interaction while letting the parent coordinate broader app state.

## Example
```typescript
<Chat
  conversationId="conv-123"
  avatarSeed="seed-001"
  paletteStops={["#ff7a7a", "#7a7aff", "#7affc4"]}
  onMessageSent={(message) => console.log("Message sent:", message)}
  onBusyChange={(busy) => console.debug("Busy:", busy)}
  onConversationLoaded={() => console.log("Conversation loaded")} 
  onConversationMissing={() => console.warn("Conversation missing")} 
/>
```

## Notes
- Changing conversationId switches the displayed conversation; callers should account for any UI transitions or cleanup needed when the target conversation changes.
- avatarSeed should remain stable for the same participants to keep avatars consistent across renders.
- Callbacks may be invoked asynchronously; avoid assuming synchronous execution when updating surrounding UI state.

---

## Reasoning
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Reasoning(
```


Reasoning renders the model's reasoning content within the chat UI by taking a content payload and a streaming flag. It is a small, focused React component that you reach for when you want to present the underlying chain-of-thought behind an answer, either all at once or progressively as data becomes available.

## Remarks
This abstraction separates the presentation of reasoning from the higher-level chat orchestration and model results. It enables a consistent, testable styling for reasoning output and supports a streaming UX to reduce perceived latency.

## Example
```typescript
// Full reasoning displayed at once
<Reasoning content='The steps: A -> B -> C' streaming={false} />

// Streaming reasoning as steps arrive
<Reasoning content={steps} streaming={true} />
```

## Notes
- React escapes strings by default, so content is rendered safely as text. If you pass raw HTML or JSX, sanitize or pass React nodes instead of strings.
- If streaming is enabled, ensure the parent provides content in a way that represents incremental updates to avoid jarring UI changes.

---

## Thought
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Thought(
```


Thought is a small, presentational React component used inside the chat UI to render a short, introspective snippet of text. It accepts a single prop, content, and renders that content with a distinct styling that sets it apart from regular chat messages. Use Thought when you want to display a user’s internal reflection or a system-generated thought fragment in the conversation flow, without plumbing it through the standard message rendering logic.

## Remarks
Thought serves as a UI primitive that encapsulates styling concerns for a 'thought' state. By isolating this presentation in its own component, the chat UI can consistently render thoughts across different contexts (e.g., user thinking, AI deliberation) and swap themes without touching the message components.

## Example
```typescript
<Thought content="I need to fetch the latest data before replying." />
```

## Notes
- Content is rendered as provided; React escapes strings by default, so plain text is safe from HTML injection. If you pass React nodes, ensure they conform to the design system.
- As a presentational primitive, Thought has no side effects and should be free of data-fetching logic; changes to its typography or colors should be centralized in the app theming.

---

## ToolResult
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function ToolResult(
```


ToolResult is a React functional component that renders the output produced by an interactive tool within the chat UI. It accepts a single prop, content, which provides the tool's result, enabling a consistent presentation of tool outputs across the chat interface rather than scattering raw values into messages.

## Remarks
ToolResult centralizes how tool outputs are displayed, allowing consistent styling and formatting across different tools. It isolates rendering concerns from chat orchestration, so new tools can be integrated without reworking the surrounding UI.

## Example
```typescript
<ToolResult content="Calculation result: 42" />
```

## Notes
- If content may contain user-provided data, ensure that rendering is safe (avoid injecting raw HTML).
- If you expect non-string content (e.g., React nodes or structured data), ensure the component type supports it or wrap it in a renderer.

---

## VariantPicker
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function VariantPicker(
```


VariantPicker is a function component that renders a small UI control for selecting among predefined variants within the chat interface. It is a controlled component: the currently selected variant is read from the variant prop, and user-initiated changes are reported to the parent via the onSwitch callback. The disabled prop disables interaction when true. Use this symbol when you need a reusable, isolated variant-switching control rather than inlining variant logic in every caller.

## Remarks
VariantPicker encapsulates the presentational and event-bridge aspects of variant selection, allowing Chat.tsx to compose higher-level behavior without embedding switch logic. It assumes the parent owns the variant state and reacts to onSwitch to update it, enabling predictable, testable state management.

## Notes
- Treat variant as a controlled value; do not mutate it within VariantPicker. 
- When disabled is true, ensure no interactions are possible and the UI communicates a non-interactive state (accessibility considerations encouraged).
- Ensure onSwitch is called with the new variant value when the user selects a different option; avoid calling onSwitch if the selected option is unchanged.

---

## advance
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const advance = () =>
```


advance is a small navigation helper that moves to the next variant in a sequence and wraps back to the start when it reaches the end. It computes the next index as (variantIndex + 1) % variantCount and then calls onSwitch with the ID of the next variant from variantSiblingIds. Use this as a handler for a Next action in a variant viewer (for example, a button or keyboard shortcut) to cycle through available variants.

## Remarks
By centralizing the wrap-around logic, this function keeps navigation consistent across UI components and decouples the navigation decision from the rendering. It relies on the surrounding component to provide variantIndex, variantCount, and variantSiblingIds and to handle the actual switch via onSwitch.

## Example
```typescript
// Typical usage inside a React component
<button onClick={advance}>Next</button>
```

## Notes
- Ensure variantCount > 0 to avoid NaN and undefined lookups when computing the next index; consider disabling the control or guarding with a precondition.
- Keep variantIndex and variantSiblingIds in sync with the actual list of variants to prevent out-of-bounds or undefined IDs being passed to onSwitch.

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


applyAgentEvent is a small state updater that translates agent events into chat entries for a streaming chat UI. It orchestrates how incoming streaming tokens (text and reasoning) and tool interactions are reflected in the UI, creating or mutating chat bubbles as tokens arrive and aligning the thinking/acting flow for the user.

## Remarks

Centralizes the streaming UI semantics for agent interactions: textDelta appends or starts a new streaming assistant bubble, reasoningDelta maintains a single streaming reasoning entry ahead of the answer, and toolCall collapses the reasoning into a compact thought while inserting a tool call entry. This abstraction keeps the React state updates consistent across token streams, tool invocations, and tool results, so the chat UI can render a coherent, top-down narrative (thinking → answer) without scattering logic across multiple components.

## Example
```typescript
// Typical usage inside a chat handler that receives agent events
applyAgentEvent(evt, setEntries);
```

## Notes

- Streaming bubbles are given synthetic IDs (e.g., streaming-<uuid>) and kept stable until a real DB id patches them later; this stabilizes React component identity during typing.
- When a toolCall arrives, in-flight streaming reasoning is frozen and the trailing streaming text bubble may be reclassified as a collapsed thought to reflect the model's internal transition from thinking to action.
- The code relies on a helper (lastIndexWhere) and crypto.randomUUID(); ensure the environment provides these APIs, or polyfills/polyfills provided by the project.


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


This function checks the latest entry in a chat history to determine whether an assistant is currently streaming a reply. It returns true only if there is a last entry, that entry is a text message authored by the assistant, and its streaming flag is set. Use this predicate in the UI to decide whether to show a streaming indicator or to gate user input while the assistant is delivering a streamed response.

## Remarks
This small predicate centralizes the concept of an "active assistant stream" so multiple UI components share a single source of truth. By requiring kind === 'text' and role === 'assistant' in addition to streaming, it avoids false positives from other message types or roles. It relies on the chat history being updated in real time as streaming progresses; as a result, any mutation or reordering of entries should keep the last entry in sync to preserve correctness.

## Notes
- Empty entries array yields false.
- Only detects streaming on the last entry; if a previous entry was streaming but the last one is not, it returns false.
- Relies on the ChatEntry shape (kind, role, streaming) remaining stable across updates.

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


Converts an API-provided history (array of MessageResponse) into a render-friendly sequence of ChatEntry items, preserving order and categorizing content by role and type. It handles user text, assistant replies, streaming reasoning content, and tool interactions, emitting distinct entry kinds such as text, reasoning, thought, and tool results. In-flight entries are distinguished from history-loaded ones by using a special reasoning key (id-reasoning) so streaming content can render before final IDs are assigned.

## Remarks
This abstraction decouples the UI rendering model from the raw history payload, enabling progressive rendering of model thinking and tool interactions while keeping a stable display structure. The key/id strategy supports optimistic updates: historical entries use the real IDs (key = id), whereas in-flight entries derive a separate reasoning key to avoid clashing as IDs are patched. By emitting separate kinds for reasoning (reasoning), interim content (thought), and final user/assistant text (text), along with explicit tool-call entries, the UI can present a coherent, temporally ordered story of each turn, including any tool interactions.

## Example
```ts
// Example usage showing common case with reasoning and tool calls
const messages: MessageResponse[] = [
  { id: '1', role: 'user', content: 'What is the capital of France?' },
  {
    id: '2',
    role: 'assistant',
    reasoningContent: 'Considering geography and political capitals...',
    content: 'Paris is the capital of France.',
    toolCalls: [{ name: 'lookup', arguments: { q: 'capital of France' } }]
  }
];
const entries = historyToEntries(messages);
```

## Notes
- System messages are not rendered in the output; they are ignored by this transformer.
- The function relies on helpers like variantMetaOf and toolCallEntry to enrich entries with variant metadata and to format tool-call results; ensure these helpers exist where this function is used.
- Tool call results are emitted after the corresponding content for the same message, preserving the natural order of the user/assistant turn followed by any tool interactions.

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


Finds the index of the last element in a readonly array that satisfies the given predicate. It iterates from the end of the array toward the beginning, evaluating pred on each element until a match is found and returns that element's index. If no element matches, it returns -1.

## Remarks
It encapsulates a reverse search, returning the index of the last element that satisfies the predicate. This is handy when you need the most recent match, rather than the first, without manually looping from the end. It complements other array utilities by providing a concise, reusable pattern for end-focused condition checks.

## Example
```typescript
const values = [1, 3, 4, 6, 7];
const lastEvenIndex = lastIndexWhere(values, v => v % 2 === 0); // 3
```

## Notes
- The predicate is invoked for each element starting from the end until a match is found; in the worst case, every element is evaluated. This means the function has O(n) time complexity.
- The input array is treated as readonly and is not mutated by this operation.

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


Intercepts keydown events on the chat textarea to implement the common “press Enter to send” behavior. When Enter is pressed without Shift, it prevents the default newline action and calls send() to dispatch the current message; Shift+Enter remains usable for adding newlines.

## Remarks
Centralizing this UX behavior in a small handler keeps input concerns separate from sending logic, and makes the Enter-to-send rule explicit wherever the textarea is used. It provides a consistent, predictable experience for chat input, preventing accidental sends when a newline is intended (Shift+Enter). The void operator signals that the asynchronous result of send() is intentionally not awaited at this call site.

## Example
```typescript
<textarea onKeyDown={onKeyDown} />
```

## Notes
- If send() can fail, consider handling errors in send() or at the call site, since this handler discards the Promise.
- Attach onKeyDown to the specific textarea to avoid interfering with other controls.
- Be mindful that this only affects Enter without Shift; other keys are passed through to the textarea.

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


onSubmit is a form submission handler that wires the form event to the chat message-sending flow. When the user submits the form, it prevents the browser’s default submission behavior (preventing a page reload) and then invokes send() to perform the actual sending operation. The explicit void before send() signals that the handler intentionally does not await or handle the promise returned by send(), making this a fire-and-forget interaction.

## Remarks
This function acts as the UI boundary between the form submission and the message-sending logic. Keeping e.preventDefault() here ensures the page stay intact, while delegating to send() encapsulates the transmission behavior. The void prefix expresses a fire-and-forget intent; if the UI needs to reflect send status or handle errors, consider awaiting send() and adding error handling at this level or higher.

## Notes
- If you care about success/failure feedback, remove the void and await the send() promise, adding appropriate error handling (e.g., try/catch) and UI updates.
- Ensure this handler is attached to the form’s onSubmit prop so it’s invoked on submission (e.g., <form onSubmit={onSubmit}>).</n

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


Parses a JSON string and re-serializes it to a canonical, compact JSON representation; if the input is empty or the literal '{}', it returns an empty string. If the string isn’t valid JSON, it returns the original input unchanged. Use prettyArgs in UI code (for example in Chat.tsx) when you want to normalize and safely display JSON arguments instead of showing raw, potentially noisy input.

## Remarks
Encapsulates a small formatting concern for JSON payloads behind a single function, making testing and maintenance easier and keeping UI rendering logic free of ad-hoc JSON handling. This centralization ensures consistent behavior when displaying or logging user-provided JSON in the chat UI, and isolates changes to formatting from the rest of the codebase.

## Example
```typescript
// Common cases
prettyArgs('{"b":2,"a":1}') // '{"b":2,"a":1}'
prettyArgs('{}') // ''
prettyArgs('not json') // 'not json'
```

## Notes
- Returns '' for '{}' to signal "no args" rather than a value.
- Non-JSON inputs are passed through unchanged, avoiding runtime errors at the call site.
- The output is a compact JSON representation (whitespace is removed); this may affect the exact character sequence if the input order of keys differs.

---

## prev
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const prev = () =>
```


prev is a navigation helper that computes the previous variant in a circular sequence and switches to it. It subtracts one from the current variantIndex, wraps around with a modulo against variantCount, and then invokes onSwitch with the id located at the resulting index in variantSiblingIds. This is typically used by a Previous button in a variant switcher to move backward through variants without hitting the beginning or end.

## Remarks
Encapsulates the wrap-around logic so UI controls can simply trigger prev without duplicating boundary checks. It relies on the alignment between variantIndex, variantCount, and variantSiblingIds to map an index to the corresponding variant id, keeping navigation concerns separate from presentation.

## Example
```typescript
// In a React component
return (
  <button onClick={prev} aria-label="Previous variant">Prev</button>
);
```

## Notes
- variantCount must be > 0 to avoid a NaN/undefined target from modulo 0.
- Assumes variantIndex is in [0, variantCount-1] and that variantSiblingIds has a valid entry for the computed index.
- This function does not mutate state; it computes the target id and delegates to onSwitch.

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


Renders a ChatEntry inside the chat UI by selecting the appropriate presentation based on the entry's kind and state, and by wiring per-entry actions when appropriate. It handles text entries with live-assistant streaming versus static history, supports multi-variant assistant responses through a VariantPicker, and delegates rendering to specialized subcomponents for tool calls/results, thoughts, and reasoning; action buttons (regenerate, save as memory, delete) are exposed only for persisted messages and when the user's context permits.

## Remarks
Centralizes the rendering logic for all ChatEntry variants, isolating presentation concerns from data handling. It coordinates with the Actions interface to reflect busy states, ensures synthetic IDs used for streaming or temporary entries do not leak into user interactions, and provides a consistent extension point for new entry kinds or interaction patterns.

## Notes
- Buttons are disabled when actions.busy is true to prevent overlapping operations.
- Streaming assistant messages use synthetic IDs (e.g., 'tmp-' or 'streaming-') to distinguish transient UI state from persisted messages.
- VariantPicker is shown only when an entry reports multiple variants.

---

## send
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const send = async () =>
```


Executes the send action for the chat input. It trims the input, exits early if the text is empty or a send is already in progress, and then inserts an optimistic user message. It starts a streaming session against the backend and applies incoming stream events to progressively render the agent's reply, patching temporary IDs with real database IDs as they arrive. It also respects UI state (busy/compacting, auto-scroll) and cleans up by clearing the in-flight abort controller and resetting state after completion or failure, avoiding a full refetch to preserve the typewriter reveal.

## Remarks
This function encapsulates optimistic UI updates with streaming results to keep the chat responsive while persisting the final persisted state in the background. It coordinates with stream events to patch temporary identifiers to real IDs and to reflect UI cues (like compacting) during the agent's reply. The design also relies on a ResizeObserver-driven layout sequence to ensure the newest content is scrolled into view after the turn begins. The AbortController integration provides a clear cancellation path for in-flight sends, which helps prevent leaks and inconsistent UI when navigation or rapid repeats occur.

## Notes
- Temporary IDs: The user message is created with a temporary id (prefixed with tmp-); real IDs are applied later as persistence events arrive.
- Cancellation and cleanup: The in-flight AbortController is stored in abortRef.current and reset in finally to avoid leaks; this path ensures the UI is left in a consistent state even on errors.
- No automatic refetch: To preserve the typewriter-like reveal, the code avoids triggering a full GET fetch after streaming; downstream events patch IDs in place, which keeps the reveal smooth but may leave stale local state unless events arrive.


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


Converts a MessageToolCall into a chat entry of kind toolCall by composing a unique key from the parent messageId and the tool call's own id, and by wiring the toolCallId, name, and serialized arguments into the ChatEntry. It is used when inserting a tool invocation into the chat timeline so the UI can render and interact with tool-based responses in a consistent structure.

## Remarks
By isolating this mapping, the chat renderer can treat tool invocations generically alongside user and assistant messages. The function ensures a stable key for React lists (the key is messageId-call-id) and exposes the essential identifiers (toolCallId and name) to UI components and any reducers or middleware that need to react to tool usage.

## Example
```typescript
// Concrete usage showing the most common case
toolCallEntry('msg1', { id: 'tc42', name: 'Translate', argumentsJson: '{"text":"Hello"}' });
// Returns
// {
//   kind: 'toolCall',
//   key: 'msg1-call-tc42',
//   id: 'msg1-call-tc42',
//   toolCallId: 'tc42',
//   name: 'Translate',
//   argumentsJson: '{"text":"Hello"}'
// }
```

## Notes
- argumentsJson must be a string containing valid JSON; passing a non-string or improperly escaped JSON can cause runtime errors.
- The function relies on tc.id to generate the key; if id is missing or undefined, the resulting key/id may be invalid.
- The returned value is a plain ChatEntry tailored for tool calls; avoid mutating its fields after creation to preserve identity semantics in rendering.

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


variantMetaOf extracts the variant-related metadata from a MessageResponse and returns a VariantMeta object containing the group ID, index, total count, and the list of sibling variant IDs. Use this helper when UI or logic needs a stable, simplified view of a message's variant information rather than reading those fields directly from MessageResponse.

## Remarks
Centralizes the mapping from MessageResponse to VariantMeta so consumers share the same shape and naming. If the MessageResponse structure changes, only this function may need updating while callers continue to rely on the consistent VariantMeta interface. It is a pure, side-effect-free mapper that is ideal for UI components that render variant navigation or indicators.

## Example
```typescript
const m: MessageResponse = fetchLatestMessage();
const meta = variantMetaOf(m);
// meta.variantGroupId, meta.variantIndex, meta.variantCount, meta.variantSiblingIds
```

## Notes
- The function is pure: it does not mutate the input and simply returns a new object referencing the input's values.
- The variantSiblingIds array is returned by reference; clone if you need an immutable copy.


---