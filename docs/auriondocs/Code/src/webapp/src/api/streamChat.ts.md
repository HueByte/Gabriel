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


StreamChatOptions defines optional configuration for streaming chat calls. By supplying a signal, callers can cancel an in-progress stream using an AbortSignal instead of managing custom cancellation logic.

## Remarks
This option follows the standard AbortSignal cancellation pattern used widely in web APIs. It decouples cancellation from the operation's implementation, allowing consumers to initiate cancellation from their own controllers. Because the interface is minimal (only signal), it keeps the surface area small while enabling cooperative cancellation across collaborators that participate in the streaming process.

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


AgentEvent is a discriminated union that models all streaming events emitted during a send-driven chat turn. It encompasses persistence, incremental updates, tool interactions, assistant content, and lifecycle signals that drive the client UI in real time.

## Remarks

These events provide a single, typed stream that decouples the UI from the underlying streaming protocol. They include: persisting the user message (where the real DB id is exposed as messageId to swap optimistic UI ids), delta updates (textDelta and reasoningDelta), tool invocations and results (toolCall and toolResult), and the lifecycle signals for compaction (compactStart and compactDone) as well as error and completion (error, done). The compact lifecycle enables the UI to show a "Compacting…" overlay during the rolling summary, and to clear it reliably on completion or failure.

## Example

```typescript
function handleEvent(e: AgentEvent) {
  switch (e.type) {
    case 'userMessagePersisted':
      // e.messageId is the real persisted ID; replace tmp UI id
      break;
    case 'textDelta':
      // apply e.delta to the current text
      break;
    case 'reasoningDelta':
      // apply e.delta to the reasoning portion
      break;
    case 'toolCall':
      // begin tool invocation identified by e.toolCallId
      break;
    case 'toolResult':
      // render tool result content
      break;
    case 'assistantMessage':
      // display assistant content, including optional reasoningContent
      break;
    case 'compactStart':
      // show "Compacting…" overlay with counts
      break;
    case 'compactDone':
      // update summary and hide overlay
      break;
    case 'error':
      // surface error.message
      break;
    case 'done':
      // stream finished
      break;
  }
}
```

## Notes

- Multiple events (for example, textDelta and reasoningDelta) can arrive for the same turn; the UI should apply updates in a way that preserves overall coherence.
- toolCallId in toolCall and toolResult must be used to correlate requests with their corresponding results; mismatched IDs should be treated as out-of-band data.
- argumentsJson is carried as an opaque JSON string; decode or pass through as needed without assuming a particular shape unless you know the schema from your tool definition.

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


doFetch is a small fetch wrapper that issues a POST request to the provided URL and returns the resulting Response promise. It always includes credentials to ensure cookies are sent across origins, and it only sets Content-Type when a body is actually sent, JSON-stringifying the body when present. The function also sets Accept to text/event-stream to align with endpoints that stream data (SSE). Call it when you need to post data and begin consuming a streaming response while keeping the user’s cookies in sync with the API.

## Remarks
doFetch centralizes a couple of cross-cutting concerns: cookie propagation across origins and the streaming-oriented Accept header, so callers don’t have to repeat these defaults. It also encapsulates the conditional body handling in one place, reducing the risk of mismatched headers and bodies across the codebase.

## Example
```typescript
const controller = new AbortController();

doFetch('/api/streamChat', { room: 'general', since: 0 }, controller.signal)
  .then(res => {
    if (!res.ok) throw new Error(`Request failed with status ${res.status}`);
    // For streaming endpoints, you typically read from res.body or use EventSource on a separate URL.
    console.log('Content-Type:', res.headers.get('Content-Type'));
  })
  .catch(err => {
    if (err.name === 'AbortError') {
      console.log('Request aborted');
    } else {
      console.error(err);
    }
  });

// Cancel after 5 seconds as an example
setTimeout(() => controller.abort(), 5000);
```

## Notes
- Passing undefined as the body can inadvertently cause a Content-Type header to be sent without an actual payload; pass null to indicate no body.
- Aborting the request throws an AbortError; ensure callers handle this case if cancellation is part of the UX.
- This is a thin wrapper around fetch; to actually consume streaming data you’ll typically read from response.body or switch to a streaming/event mechanism on the caller side.

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


Streams chat content into a running conversation, identified by conversationId, by delivering the provided content chunk through a streaming channel. Use this when you want incremental delivery of messages (for example, rendering a bot response as it's produced) rather than waiting for the full message before displaying it. The opts parameter accepts a StreamChatOptions object to tailor the streaming behavior (such as lifecycle events, error handling, or progress callbacks) without changing the call site; this keeps the UI code clean and decoupled from transport details.

## Remarks
This symbol acts as a streaming primitive that decouples transport concerns from chat composition. It coordinates with a conversation-scoped stream and with the consumer of the stream to present content as it arrives. By taking content as a separate parameter and exposing a configurable opts object, it supports flexible streaming strategies—from streaming partial chunks of a reply to delivering live updates in a chat interface.

## Notes
- When multiple streams share the same conversationId, ensure you preserve the order of chunks to avoid interleaved content.
- Cancel/clean up the stream when the consumer is finished to prevent resource leaks and unnecessary network activity.

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


streamRegenerate initiates a regeneration operation for the streaming updates associated with a specific message in a given conversation. It requires the conversationId and messageId to identify the target, and accepts an optional StreamChatOptions to tailor the regeneration behavior. Call this when you need to refresh or recover the live stream for a message, for example after a transient error or to re-sync the stream in the UI.

## Remarks

streamRegenerate acts as a focused abstraction around the regeneration workflow for a single message within a conversation. By capturing the target identifiers (conversationId and messageId) and the optional configuration, it decouples UI concerns from the underlying streaming infrastructure and enables consistent retry or refresh semantics across components that subscribe to the chat stream.

## Notes

- The opts parameter defaults to an empty object, so callers may omit it entirely.
- IDs are strings; ensure they come from trusted sources to avoid mis-targeting messages.
- Return type and side effects depend on the implementation; this documentation does not expose the return value.

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


streamSse is an asynchronous generator that initiates a server-sent events (SSE) stream from the specified URL, sending the provided body and applying the optional StreamChatOptions. It yields each incoming event as soon as it arrives, letting consumers process updates incrementally with a for-await-of loop. Use it when you need real-time updates from the server (for example, chat messages or presence events) without buffering the entire payload in memory.

## Remarks
streamSse provides a focused, typed surface for consuming SSE in this chat-oriented API. By encapsulating the transport and options handling behind a single function, it promotes consistent configuration and error handling across all callers that need streaming data. It also centralizes how StreamChatOptions influence streaming behavior, reducing duplication across modules.

## Example
```typescript
// Example
async function listen() {
  const url = "/api/chat/stream";
  const body = {};
  const opts = {} as StreamChatOptions;

  for await (const event of streamSse(url, body, opts)) {
    console.log("event:", event);
  }
}
```

## Notes
- The body parameter is typed as unknown; ensure the server accepts and processes the payload you send.
- Errors during streaming propagate to the consumer; wrap the for-await-of loop in try/catch to handle network or parsing errors.

---