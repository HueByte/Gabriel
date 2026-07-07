# streamChat.ts

> **Source:** `src/webapp/src/api/streamChat.ts`

## Contents

- [StreamChatOptions](#streamchatoptions)
- [AgentEvent](#agentevent)
- [doFetch](#dofetch)
- [streamChat](#streamchat)
- [streamRegenerate](#streamregenerate)
- [streamSse](#streamsse)

---

## StreamChatOptions
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** interface

```typescript
export interface StreamChatOptions
```


StreamChatOptions provides optional configuration for chat streaming operations. It currently exposes a single optional member, signal, that accepts an AbortSignal to allow cancellation of an in-flight stream or request.

## Remarks
This interface serves as a lightweight cancellation helper for streaming-related APIs. By isolating the cancellation concern into StreamChatOptions, you can pass cancellation semantics alongside other options without embedding AbortSignal logic directly into the calling code. It promotes a consistent pattern for cancelling long-running or continuous stream operations, while remaining unobtrusive for simple, synchronous usage.

## Example
```typescript
const controller = new AbortController();
const options: StreamChatOptions = { signal: controller.signal };

// Pass `options` to any API that accepts StreamChatOptions to enable cancellation via the AbortController.
```

## Notes
- If you provide a signal, ensure you handle the abort event on the consumer side and terminate the operation cleanly to free resources.
- AbortSignal/Cancellation is only effective with APIs that actively support and observe the signal; otherwise passing a signal has no effect.


---

## AgentEvent
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** type alias

```typescript
export type AgentEvent =

  | { type: 'userMessagePersisted'; messageId: string }
  | { type: 'textDelta'; delta: string }
  | { type: 'reasoningDelta'; delta: string }
  | { type: 'toolCall'; messageId: string; toolCallId: string; name: string; argumentsJson: string }
  | { type: 'toolResult'; messageId: string; toolCallId: string; content: string }
  | { type: 'assistantMessage'; messageId: string; content: string; reasoningContent?: string | null }

  | { type: 'compactStart'; messageCount: number; currentTokens: number; thresholdTokens: number }
  | { type: 'compactDone'; messageCount: number; summaryTokens: number }
  | { type: 'error'; message: string }
  | { type: 'done' };
```


AgentEvent is a TypeScript discriminated union that encodes every server-to-client streaming event emitted during a send-driven chat turn, including message persistence, incremental content deltas, tool interactions, and lifecycle signals. It exists so consumers can render updates incrementally and swap optimistic user-entry IDs for the real database IDs without extra round-trips.

## Remarks
This abstraction centralizes the chat-stream protocol and decouples the UI from the underlying transport. By modeling text and reasoning deltas, tool calls and results, and compacting progress as distinct events, it enables precise, non-blocking rendering and progress indicators. The inclusion of userMessagePersisted, compactStart/compactDone, and error/done signals provides clear lifecycle hooks for the client to manage overlays, status messages, and stream termination.

## Example
```typescript
function renderEvent(ev: AgentEvent) {
  switch (ev.type) {
    case 'userMessagePersisted':
      console.log(`Persisted user message ${ev.messageId}`);
      // swap tmp- ids to ev.messageId in the UI
      break;
    case 'textDelta':
      // append delta to the current user message draft
      console.log(`Text delta: ${ev.delta}`);
      break;
    case 'reasoningDelta':
      console.log(`Reasoning delta: ${ev.delta}`);
      break;
    case 'toolCall':
      // show a running tool call in progress
      console.log(`Tool call: ${ev.name} (id=${ev.toolCallId})`);
      break;
    case 'toolResult':
      console.log(`Tool result: ${ev.content}`);
      break;
    case 'assistantMessage':
      console.log(`Assistant: ${ev.content}`);
      break;
    case 'compactStart':
      console.log(`Compact started: ${ev.messageCount} msgs, tokens=${ev.currentTokens}/${ev.thresholdTokens}`);
      break;
    case 'compactDone':
      console.log(`Compact done: ${ev.summaryTokens} tokens`);
      break;
    case 'error':
      console.error(`Error: ${ev.message}`);
      break;
    case 'done':
      console.log('Stream complete');
      break;
  }
}
```

## Notes
- The UI should always clear the compact overlay on compactDone, even if the summary failed (as indicated by the comment in the source).
- ToolCall and ToolResult share a stable toolCallId to correlate invocations with their results; store that mapping in the UI to present progress per tool.
- Reasoning content on assistant messages is optional and may be omitted; handle undefined reasoningContent gracefully.

---

## doFetch
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
function doFetch(url: string, body: unknown, signal?: AbortSignal): Promise<Response>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `url` | `string` | — |
| `body` | `unknown` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<Response>`


doFetch is a small HTTP helper that issues a POST request to the given URL, optionally sending an JSON-encoded body and returning the raw Response promise. It always includes credentials to carry cookies across origins, which is important when the web app and API are or may become cross-origin in deployment. It signals to servers that a streaming response is expected by setting Accept to text/event-stream. The Content-Type header is only added when there is a body to send, avoiding unnecessary headers for empty payloads, and the body is JSON-stringified when present. An optional AbortSignal can be supplied to cancel the request via fetch.

## Remarks
This encapsulates the streaming handshake pattern (SSE-style) used by this codebase, guaranteeing cookie propagation and consistent content negotiation across endpoints that stream data. Centralizing the header and body decisions reduces the risk of accidentally omitting credentials or misconfiguring Content-Type on calls that initiate streams. It also clearly expresses cancellation as a first-class concern via the optional AbortSignal.

## Example
```ts
// Example: initiate a streaming POST and observe a response
const ctrl = new AbortController();
doFetch('/api/streamChat', { room: 'lobby' }, ctrl.signal)
  .then(res => {
    if (!res.ok) throw new Error(`Request failed: ${res.status}`);
    // Consume the response as text for illustration; a streaming consumer may attach to res.body
    return res.text();
  })
  .then(text => {
    console.log('First chunk/text:', text);
  })
  .catch(err => {
    if (err.name === 'AbortError') {
      console.log('Fetch aborted');
    } else {
      console.error('Fetch error', err);
    }
  });

// Cancel after 10 seconds if still pending
setTimeout(() => ctrl.abort(), 10000);
```

## Notes
- JSON.stringify will throw if the body contains circular references or non-serializable values; ensure the payload is JSON-serializable.
- The function always uses credentials: 'include'; if you must avoid cookies, use a different helper or adjust configuration at call site.


---

## streamChat
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
export function streamChat(
  conversationId: string,
  content: string,
  opts: StreamChatOptions
```


Streams chat content into a specified conversation, delivering updates as they become available instead of a single completed payload. The function takes a conversationId, the initial content chunk, and an optional StreamChatOptions object to configure streaming behavior. Use this when you want real-time or incremental delivery of chat content—for example to power live-updating UIs or long-running generation flows—rather than posting a single message.

## Remarks
StreamChatOptions on the dependency surface provides a typed contract for how streaming should behave (e.g., cancellation, pacing, or progress reporting). By isolating streaming logic behind streamChat, the rest of the chat infrastructure can swap in different streaming backends or testing stubs without touching call sites. This separation clarifies that the operation is asynchronous and likely interacts with the transport layer over time, which has implications for error handling and UI state management.

---

## streamRegenerate
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
export function streamRegenerate(
  conversationId: string,
  messageId: string,
  opts: StreamChatOptions
```


streamRegenerate is a function that triggers the regeneration workflow for a single message within a specific conversation in the streaming chat context. It takes a conversationId and a messageId to identify the target, plus an optional StreamChatOptions bag to tailor the regeneration behavior.

## Remarks
This abstraction encapsulates the regeneration behavior behind a focused, reusable API. By taking the conversation and message identifiers, it cleanly expresses intent and reduces the surface area callers must understand about the streaming pipeline. It sits at the boundary between higher-level chat logic and the lower-level streaming implementation, enabling consistent regeneration semantics across different parts of the UI or data layer.

## Notes
- Ensure conversationId and messageId refer to existing entities; misreferenced IDs may result in no-ops or errors depending on the runtime.
- Because opts defaults to an empty object, callers can omit it for default regeneration behavior; supply specific options only if you need to alter streaming behavior.

---

## streamSse
> **File:** `src/webapp/src/api/streamChat.ts`  
> **Kind:** function

```typescript
async function* streamSse(
  url: string,
  body: unknown,
  opts: StreamChatOptions
```


Streams chat updates from a Server-Sent Events endpoint by yielding each incoming event as it arrives. This async generator is designed to be consumed with for-await-of so you can process messages incrementally rather than buffering a full payload.

## Remarks
This abstraction encapsulates the streaming transport for chat data, allowing higher-level chat logic to focus on rendering and business rules rather than the intricacies of SSE parsing and fetch setup. It emits a stream of event chunks as they arrive, enabling backpressure-friendly processing and incremental updates.

## Example
```typescript
for await (const event of streamSse("/api/chat/stream", { channelId: "abc" }, {} as StreamChatOptions)) {
  // Handle the streamed event (e.g., decode and render)
  console.log(event);
}
```

## Notes
- The endpoint must support Server-Sent Events (text/event-stream); otherwise iteration may fail or yield no data.
- Consume with a for-await-of loop; this is an async generator, not a Promise.
- To cancel the stream, terminate the consumer (e.g., via AbortController) when the stream is no longer needed.

---