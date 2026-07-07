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


ChatProps encapsulates the props contract for the Chat component, describing the data it needs to render and the callbacks it exposes to coordinate with its container. It requires a conversationId to identify the active conversation and an avatarSeed that deterministically drives the thinking-pulse animation. If paletteStops are provided, the pulse bars recolor to match these stops; otherwise the seed-derived palette is used. The interface also exposes optional callbacks to react to user actions and lifecycle events: onMessageSent fires after a message is sent; onBusyChange notifies callers about the chat’s busy state; onConversationLoaded provides per-conversation metadata via a ConversationResponse once the history is loaded; and onConversationMissing signals that the conversation was removed (e.g., 404), inviting the parent to navigate away or refresh.

---

## EntryActions
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** interface

```typescript
interface EntryActions
```


EntryActions is a small contract that describes what actions an individual chat entry can trigger and whether it is currently busy. Implementers pass these callbacks to the chat UI so user interactions (delete, regenerate, variant switch, remember) are wired without the UI needing to know the underlying logic.

## Remarks
EntryActions serves as a boundary between the presentation layer (Chat.tsx) and the domain or data layer that performs actions on messages. By collecting action handlers in a single object, components can be tested easily with mocks and swapped with different backends without changing the UI code. The busy flag communicates a per-entry loading state, allowing the UI to disable relevant controls while an operation completes.

## Example
```typescript
const sampleActions: EntryActions = {
  busy: false,
  onDelete: (messageId: string) => console.log(`Deleting ${messageId}`),
  onRegenerate: (messageId: string) => console.log(`Regenerating ${messageId}`),
  onVariantSwitch: (targetId: string) => console.log(`Switching to variant ${targetId}`),
  onRemember: (messageId: string) => console.log(`Remembering ${messageId}`),
};
```

## Notes
- Keep a stable EntryActions reference across renders to prevent unnecessary re-renders of the chat entry.
- The busy flag should reflect in the UI by disabling actions while an operation is in flight.
- Callbacks must forward the correct messageId or targetId; ensure the caller binds the proper identifiers.

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


ChatEntry is a TypeScript discriminated union that models a single entry in the chat stream. It covers visible user/assistant text (the 'text' variant), internal commentary like pre-tool reasoning ('thought' and 'reasoning'), and tool interactions ('toolCall' and 'toolResult'). Each entry carries a shared identity through 'key' and 'id'; the 'text' variant requires a 'role' and 'content', and may optionally include 'streaming' and 'variant' metadata. The union is defined in src/webapp/src/components/Chat.tsx and is consumed by the chat UI to render messages and tool interactions in a consistent, type-safe way.

## Remarks
ChatEntry centralizes the different chronologically ordered pieces that appear in a chat session, enabling the UI to render user messages, assistant responses, and behind-the-scenes tool interactions from a single, type-safe source. The 'kind' discriminant lets render code switch behavior—e.g., showing a text bubble for 'text', inline progress for 'streaming' entries, or a placeholder and later replacement for 'toolCall'/'toolResult'. Keeping 'VariantMeta' as an optional knob on text entries allows optional per-entry presentation metadata without leaking it into other variants.

## Example
```typescript
// Example usages of ChatEntry variants
const userMessage: ChatEntry = {
  kind: 'text',
  key: 'k1',
  id: 'id-1',
  role: 'user',
  content: 'Hello!'
};

const toolCall: ChatEntry = {
  kind: 'toolCall',
  key: 'k2',
  id: 'id-2',
  toolCallId: 'tc-99',
  name: 'translate',
  argumentsJson: '{"to":"fr","text":"Hello"}'
};
```

## Notes
- Every entry has 'key' and 'id'; 'kind' determines which fields are present and how the entry is processed.
- The 'text' variant requires 'role' and 'content'; 'variant' is optional.
- Tool interaction variants use 'toolCallId' to pair requests with results.

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


VariantMeta is a small TypeScript type that captures the metadata for a single variant within a group of variants used by the Chat UI. It conveys which variant group it belongs to, its zero-based index within that group, the total number of variants in the group, and the IDs of the sibling variants. This shape is used whenever you need to pass around or reason about a specific variant alongside its peers, rather than scattering separate fields through components.

## Remarks
VariantMeta acts as a concise contract that ties a variant to its group and to its peers. It enables UI patterns like variant selectors, next/previous navigation, or conditional styling to be implemented against a single object rather than multiple scattered values. The variantGroupId ensures variants are grouped correctly, while variantSiblingIds provides a stable reference to related variants for lookup or navigation purposes.

## Example
```typescript
const sampleVariant: VariantMeta = {
  variantGroupId: "theme",
  variantIndex: 1,
  variantCount: 3,
  variantSiblingIds: ["theme-dark", "theme-light", "theme-system"]
};
```

## Notes
- The variantSiblingIds array is readonly, signaling that the collection should be treated as immutable by consumers.
- This type does not enforce runtime invariants; callers should validate or enforce relationships (e.g., 0 <= variantIndex < variantCount) when constructing VariantMeta.

---

## Chat
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
export function Chat(
```


Chat is a React function component that renders the chat UI for a given conversation identified by conversationId. It accepts avatarSeed and paletteStops to customize the avatar appearance and provides callbacks for key interactions: onMessageSent when the user sends a message, onBusyChange to report transient loading states, onConversationLoaded when the conversation data has been loaded, and onConversationMissing when the conversation could not be found. Use it when you want an embedded, self-contained chat surface whose data handling is orchestrated by the surrounding page.

## Remarks
Chat acts as the presentation layer that delegates data loading and persistence to its consumer. By exposing the onMessageSent and lifecycle callbacks, it remains reusable across different conversations and data sources. It focuses on UX and rendering while the app supplies data and side effects.

## Notes
- onMessageSent is invoked when the user sends a message; ensure the parent updates the conversation state accordingly.
- onBusyChange signals transient UI loading state; avoid performing heavy work in this callback.
- onConversationLoaded / onConversationMissing signal loading outcomes; wire your UI to reflect success or missing conversation.

---

## Reasoning
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Reasoning(
```


Reasoning is a React function component that renders the reasoning content for a chat message. It accepts a content prop containing the reasoning text or structure and a streaming flag that indicates whether the content arrives progressively, enabling incremental updates in the UI.

## Remarks
Reasoning encapsulates the presentation of the model's internal reasoning separate from the final answer, enabling consistent styling and easier testing. The streaming flag allows progressive rendering, improving perceived latency as results arrive. This component lives alongside the main answer renderer in Chat.tsx, and keeping reasoning isolated makes it easier to swap in alternate renderers or apply specialized animations.

## Example
```typescript
<Reasoning content="The model considered A, then B, and concluded C." streaming={true} />
```

---

## Thought
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function Thought(
```


Thought is a React function component that accepts a content prop and is used within the chat interface to render a discrete piece of text or content as a 'thought'. The signature shows destructuring of the content prop, indicating a simple, presentational role rather than a data-fetching or stateful behavior.

## Remarks
Thought encapsulates the presentation concerns of a 'thought' item, allowing consistent styling and reusability across the chat UI. By isolating this rendering, the Chat component can compose thoughts alongside messages without duplicating layout or styles. The exact rendering (HTML structure and styling) is not visible in the snippet, but the naming convention and prop shape imply a lightweight, content-driven display.

## Example
```typescript
// Example usage within Chat.tsx
<Thought content="This is a meta-thought about the next reply." />
```

## Notes
- The snippet omits type annotations for the props and the render output, so the exact typings and DOM structure are not visible from the snippet alone.

---

## ToolResult
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function ToolResult(
```


The ToolResult function is a small, presentation-oriented React component designed to display the outcome of a tool or operation within the chat interface. It accepts a single prop, content, which represents the rendered content or message to present to the user. When a developer considers using ToolResult, they would reach for it to standardize the visual presentation of tool outputs, ensuring consistent typography, spacing, or surrounding chrome across different tool results, instead of rendering raw strings or bespoke blocks directly in the chat UI.

## Remarks
ToolResult encapsulates the presentation concern for tool outputs so that other components can rely on a uniform look-and-feel for results. This abstraction helps maintain a single place to adjust how tool results appear (e.g., padding, borders, or typography) without affecting the business logic that computes or fetches the tool data. It fits into the chat/UI layer as a specialized renderer that can be swapped out or extended if the platform introduces new result formats.

## Notes
- The component is intentionally lean to avoid pulling in additional dependencies; any complex formatting should be delegated to the content passed in or composed by higher-level components.
- If the UI ever needs to support non-text tool results (e.g., rich cards, actions), ToolResult can evolve to render children or a richer content model, while preserving its contract for the single content prop.


---

## VariantPicker
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
function VariantPicker(
```


VariantPicker is a small, stateless UI component intended to let a user choose among different visual or behavioral variants within the Chat UI. It accepts three props: a current variant value, a disabled flag to prevent interaction, and an onSwitch callback to notify the parent component when the user selects a different variant. Use this component when you want to expose a simple, consistent way to toggle between predefined variants (for example, different chat layouts or rendering modes) without embedding variant-switching logic directly inside the consuming page. When disabled is true, the control should render in a non-interactive state, signaling to users that switching is temporarily unavailable.

## Remarks
VariantPicker encapsulates the concept of a variant-selection control, enabling consistent presentation across the app by centralizing how variants are displayed and switched. It decouples the presentation of variant options from the business logic that applies a chosen variant, allowing parent components to react to onSwitch with whatever state-management strategy they prefer.

## Notes
- The component is intentionally lightweight and relies on the parent to manage the actual variant state and side effects.
- If the parent needs to disable user interaction temporarily (e.g., during data fetch), pass disabled = true; the component should render an inert control.


---

## advance
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const advance = () =>
```


advance is a compact UI helper in Chat.tsx that advances the currently displayed variant to the next one in a circular sequence. It calculates the next index as (variantIndex + 1) % variantCount and then triggers the switch by invoking onSwitch with the ID at variantSiblingIds[next]. This pattern is used when wiring a 'Next' control in a multi-variant chat interface, centralizing the wraparound logic so individual handlers don't need to replicate indexing or modulo arithmetic.

## Remarks
By encapsulating the navigation step, advance keeps the Chat.tsx logic focused on rendering while the navigation rule stays in one place. It relies on outer-scoped values (variantIndex, variantCount, variantSiblingIds) and a callback (onSwitch) to perform the actual transition, enabling easier refactoring and potential reuse across different UI controls that navigate among the same variant set.

## Notes
- Modulo by zero risk: variantCount must be greater than 0 to avoid runtime errors.
- Bounds risk: variantSiblingIds[next] must exist for the computed next index; otherwise onSwitch may receive undefined.
- Stale closures: if advance is stored and invoked after its captured scope changes, ensure the captured values reflect the current state or recompute those values on invocation.

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


applyAgentEvent is the client-side event handler that updates the chat UI in response to streaming AgentEvent objects. It accepts the event and a React state setter for ChatEntry[] and dispatches updates to render the model’s turns as they arrive, including text, thinking, and tool-oriented actions.

- textDelta: If there is an active streaming assistant bubble, the delta is appended to its content; otherwise a new streaming bubble is started with a synthetic streaming id (streaming-<uuid>). This preserves a continuous typing experience in the UI.
- reasoningDelta: Thinking tokens arrive before the final text. The function maintains a single streaming reasoning entry ahead of the text bubble, updating its content as tokens arrive so the UI reads top-down: thinking → answer.
- toolCall: In-flight streaming reasoning is frozen, and the trailing streaming bubble (if any) is reclassified as a collapsed thought. A toolCall entry is appended with the tool’s metadata, allowing the UI to render the action and its observation alongside prior thinking.
- toolResult: The snippet shows a case for toolResult but it is truncated in the provided code; the full implementation would integrate the tool’s result into the chat stream.

Implementation notes: the function generates client-side streaming ids using crypto.randomUUID(), and those ids are intended to be patched to real database ids later (on assistantMessage) so the same React instance preserves typing state across updates. A helper like lastIndexWhere is used to locate the appropriate position for updates (e.g., the latest streaming reasoning entry).

In short, applyAgentEvent centralizes the streaming choreography between the agent’s thinking, the streamed text, and tool-based actions, ensuring the chat UI remains coherent as multi-part turns unfold.


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


Determines whether the most recent entry in a chat transcript represents an actively streaming assistant message. It examines the last ChatEntry in the provided entries array and returns true when that entry exists and is of kind 'text' with role 'assistant' and a truthy streaming flag; otherwise returns false. This utility is useful whenever UI logic needs to know if the assistant is currently streaming a response so the UI can show a live indication or continue buffering chunks.

## Remarks
By encapsulating this logic, the function reduces duplication in UI code paths that need to decide whether to keep streaming indicators active. It models a simple, well-scoped decision: is the very last entry a streaming assistant text?

## Example
```typescript
// Minimal example demonstrating common usage
const entries: ChatEntry[] = [
  { kind: 'text', role: 'user' },
  { kind: 'text', role: 'assistant', streaming: true }
];

const hasStreaming = hasActiveAssistantStream(entries);
console.log(hasStreaming); // true
```

## Notes
- It only checks the last entry; earlier entries with streaming true are ignored.
- Returns false for empty arrays (no last entry to inspect).
- Relies on the ChatEntry shape as used by the dependency API: kind, role, and streaming are the fields checked by the function.

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


historyToEntries takes an array of MessageResponse and builds a flat list of ChatEntry items suitable for rendering a chat transcript. It treats history-loaded messages as immutable history by using the message id as both key and id, while in-flight entries use the same value for key and id until a real ID replaces the optimistic one. The function translates each API message into the UI-facing entry types and attaches per-entry metadata to support rendering variants.

- For user messages with content, a text entry is created with kind: 'text', role: 'user', and both key and id set to the message id, plus the message content and a variant derived from variantMetaOf(m).
- For assistant messages, any reasoningContent is rendered first as a dedicated reasoning entry with a key of `${m.id}-reasoning` and the reasoning text. This precedes the model’s content.
- If the assistant has content, the function distinguishes between tool-assisted turns and plain ones: when toolCalls exist, the content is emitted as a 'thought' entry (indicating the model’s reasoning or intermediate content). If there are no tool calls, the content becomes a standard text entry with role 'assistant' and a variant.
- Any toolCalls for the message are expanded into individual entries via toolCallEntry, appended after the content entries.
- If a message has role 'tool' and a toolCallId with content, a toolResult entry is emitted to link the result to its call.
- System messages are not rendered in the UI.

The function returns the assembled ChatEntry[] in the input order, forming a coherent sequence that the chat UI can render directly.

## Remarks
historyToEntries centralizes API-to-UI translation, isolating how raw MessageResponse data becomes renderable chat entries. It encapsulates the nuanced sequencing of reasoning streams, assistant content, and tool-call results so the UI can present a faithful transcript of both user and model activity, including any intermediate reasoning. By treating history IDs as stable keys and only using the key/id split for in-flight messages, it cleanly supports streaming scenarios where IDs may be patched later.

## Notes
- The presence of reasoningContent does not guarantee accompanying content; a reasoning entry may appear independently of the final assistant text.
- Tool calls are emitted after the related content in the message and are represented via toolCallEntry; ensure that toolCallEntry is defined for the given toolCall shape to avoid missing entries.


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


Finds the last index in a read-only array for which the provided predicate returns true, scanning from the end toward the start. If no element satisfies the predicate, it returns -1. Use this helper when you need the position of the most recent item that matches a condition without mutating the input or writing a backward loop yourself.

## Remarks
Encapsulates a common backward-lookup pattern into a tiny, reusable utility. It operates on a readonly array and performs a single linear pass from the end, yielding O(n) time with O(1) extra space. The result is the index of the last matching element, or -1 if none match; callers should rely on -1 to indicate “no match.” If you need the element itself, access arr[idx] after ensuring idx >= 0. This function is a light-weight alternative to a manual reverse loop when the goal is simply locating a position.

## Example
```typescript
const nums: readonly number[] = [1, 2, 3, 2];
const idx = lastIndexWhere(nums, x => x === 2);
// idx === 3
```


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


onKeyDown is a keyboard event handler intended for use on a textarea in the chat UI. It sends the current message when Enter is pressed without Shift, while allowing Shift+Enter to insert a newline.

## Remarks
It relies on the DOM KeyboardEvent and targets a HTMLTextAreaElement, making it a natural fit for textarea-based chat inputs. The handler tests `e.key === 'Enter'` and `!e.shiftKey` to distinguish a send action from a request to insert a newline. By calling `e.preventDefault()` and then `void send()`, it prevents the default newline behavior and initiates message delivery, explicitly ignoring any promise returned by `send()`; `send` is expected to be defined in the surrounding scope.

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


onSubmit is a form submission handler intended for a chat input form. It receives a FormEvent, calls e.preventDefault() to stop the browser's default submit action, and then triggers the sending action by calling send(). The explicit void operator ensures any promise returned by send() is intentionally ignored, signaling a fire-and-forget side effect.

## Remarks
By encapsulating preventDefault and the non-blocking send invocation, this symbol provides a consistent, reusable entry point for submitting messages from a form. It keeps UI logic lean and decouples the form handler from the details of how sending is implemented. If you need to react to the outcome of sending, convert this handler to async and await send, or attach completion/error handling at the call site.

## Notes
- The promise returned by send is intentionally ignored due to the void operator; don’t rely on its resolution inside this handler.
- Ensure that send is in scope; otherwise this function will fail at runtime.

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


Converts a JSON-encoded string into a compact, display-friendly representation. Use it when you want to render user-provided arguments in a UI without preserving original formatting, or when you need a single-line, normalized JSON for display. If the input is falsy or represents an empty object '{}', it returns an empty string, signaling that there are no arguments. If the input is valid JSON, it parses it and re-serializes with JSON.stringify to collapse extraneous whitespace; if parsing fails, it falls back to returning the original string unchanged.

## Remarks
This abstraction centralizes the logic for turning raw argument payloads into a stable, UI-friendly form. It helps prevent formatting glitches in the chat component by guaranteeing a consistent single-line JSON, while gracefully handling non-JSON inputs.

## Example
```typescript
// Example usage
prettyArgs('{"b":1,"a":2}'); // -> '{"b":1,"a":2}'
prettyArgs('{}'); // -> ''
prettyArgs('not json'); // -> 'not json'
```

## Notes
- If json is invalid, returns the original string (no exception thrown).
- '{}' or falsy input yields '' to indicate "no arguments".
- Output is a compact single-line JSON string (no indentation).

---

## prev
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const prev = () =>
```


prev is a navigation helper that moves to the previous variant in a circular collection. It computes the previous index using (variantIndex - 1 + variantCount) % variantCount and then switches to that variant by calling onSwitch with the corresponding id from variantSiblingIds.

## Remarks
This function encapsulates wraparound navigation, delegating the actual switch to the onSwitch callback. It relies on the surrounding state (variantIndex, variantCount, variantSiblingIds) to remain coherent, allowing a UI control (such as a Previous button) to trigger a stateful transition without duplicating index arithmetic.

## Notes
- Assumes variantCount > 0; if variantCount is zero, the modulo operation would produce an undefined result, so callers should guard against this scenario.
- variantIndex should be within [0, variantCount - 1] to ensure variantSiblingIds[next] resolves to a valid id.


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


Renders a ChatEntry into the chat UI by pattern matching on the entry kind and its runtime state. For text entries, it differentiates between live, streaming assistant messages—rendered with a typewriter-like StreamingText—and static messages from other roles. It computes whether action controls should appear (based on persistence and variant status), whether regenerating the assistant’s response is allowed (only for the latest variant), and whether there are multiple variants to switch between. For non-text entries, it delegates to specialized renderers: tool calls render a short command badge and code snippet, tool results render a ToolResult, and thought/reasoning entries render their respective blocks. The function returns a React element tree keyed by the entry’s key, suitable for inclusion in a chat thread.

## Remarks
Rationale: centralizes the presentation logic for chat entries in a single place, translating from the normalized ChatEntry model to the concrete UI. It isolates concerns about persistence, streaming animation, and user actions (regeneration, bookmarking, deletion) from higher-level chat orchestration. It also coordinates variant-switching UI when an entry has multiple variants, ensuring the UI state remains consistent with the underlying data.

## Notes
- Actions are only shown for persisted messages; streaming or synthetic entries (tmp- or streaming- ids) do not display action controls.
- Regenerate is only available for the latest variant of an assistant message.
- Deleting an entry removes that message and everything after it; ensure the surrounding state remains consistent.

---

## send
> **File:** `src/webapp/src/components/Chat.tsx`  
> **Kind:** function

```typescript
const send = async () =>
```


The function initiates sending a user message by trimming the input, guarding against concurrent sends, and then streaming the assistant’s reply while updating the chat UI in real time. It inserts a temporary user entry immediately for instant feedback, starts a streaming request, and patches that entry with real database IDs as persisted events arrive. It also enforces UI state (busy, compacting) and relies on a ResizeObserver-driven layout step to scroll after the new content renders. If the stream fails, it cleans up the placeholder and notifies the user. It invokes an optional onMessageSent callback after the streaming completes to signal completion to external observers.

## Remarks
This abstraction exists to deliver a responsive, streaming chat experience without requiring a full refetch after every user message. The code uses optimistic UI updates by inserting a temporary user entry with a provisional ID (tempUserId) and then patches that entry with the real DB id when AgentUserMessagePersisted arrives. By avoiding a follow-up GET, it prevents React remounts caused by ID changes and keeps the typewriter-style reveal smooth; the actual ID changes are applied in-place via the streaming events (e.g., patching the temporary entry). The AbortController stored in abortRef allows cancellation of in-flight streams, and a final cleanup step ensures UI state (compacting, busy) is reset regardless of success or failure. A ResizeObserver will perform the scroll-to-bottom adjustment on the next layout, so the user’s view tracks the newest content without forcing a rerender mid-turn.

## Notes
- Optimistic update with a temporary entry: the user sees their message immediately, with a provisional key (temp-*) and id; real IDs are patched later from streaming events. If the stream errors early, the placeholder is dropped.
- No refetch on the happy path: the implementation avoids a GET after sending to preserve the live reveal and prevent remounts; downstream consumers must rely on in-place patches and any lazy lookups if they need the latest IDs.
- Cleanup and cancellation: an AbortController is stored for potential cancellation; the finally block guarantees compacting state and busy state are reset, and any in-flight placeholder is cleared if needed.


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


Converts a MessageToolCall into a ChatEntry that represents a tool invocation within the chat UI. Use this helper when you need to render or store a tool call alongside normal messages, rather than constructing the ChatEntry object by hand.

## Remarks
By centralizing the mapping, the function ensures a consistent ToolCall entry shape across the UI. It derives a stable identity by combining messageId and tc.id for the key and id, which helps React's keying and downstream processing. The returned object preserves the toolCallId, name, and argumentsJson so the UI can display the tool name and pass the raw arguments to the executor when needed.

## Example
```typescript
const messageId = "m1";
const tc = { id: "tc42", name: "SearchTool", argumentsJson: '{"query":"cats"}' };
const entry = toolCallEntry(messageId, tc);
// entry: {
//   kind: 'toolCall',
//   key: 'm1-call-tc42',
//   id: 'm1-call-tc42',
//   toolCallId: 'tc42',
//   name: 'SearchTool',
//   argumentsJson: '{"query":"cats"}'
// }
```

## Notes
- The function is pure: it does not mutate its inputs; it only constructs and returns a new object.
- The key and id are deterministic: if either messageId or tc.id change, the resulting entry identity changes accordingly.

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


variantMetaOf extracts the variant metadata from a MessageResponse and returns it as a VariantMeta. It simply copies four fields: variantGroupId, variantIndex, variantCount, and variantSiblingIds. A developer would reach for it when a component or function needs a VariantMeta object derived from a MessageResponse, rather than using the MessageResponse directly. It acts as a small, explicit mapper to provide a plain VariantMeta type from the message data.

## Remarks
This function serves as a concise adapter that isolates the VariantMeta shape from MessageResponse, helping UI code stay decoupled from the full message type by centralizing the mapping logic.

---