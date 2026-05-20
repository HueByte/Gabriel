// Streaming client for the chat SSE endpoints.
//
// The generated openapi-typescript-codegen client can't model SSE, so this is
// hand-written. Native EventSource is also out (it only supports GET and can't
// send a body), so we use fetch + a manual ReadableStream parser.
//
// 401 handling: when the SSE pre-stream response is 401 (access JWT expired),
// call the shared refreshSession() and retry the SSE POST once. If refresh
// fails or the retry is still 401, signal session-expired so AuthContext can
// log the user out cleanly - the in-flight chat send is then a thrown error
// the caller toasts.
//
// Two endpoints share this transport:
//   - POST /messages/stream      (new turn, body { content })
//   - POST /messages/{id}/regenerate (regenerate variant, no body)
// The 401-refresh-and-retry path is shared via streamSse(); endpoint-specific
// helpers (streamChat / streamRegenerate) just construct the URL + body.

import { refreshSession, signalSessionExpired } from './authRefresh';

export type AgentEvent =
  // First event of every send-driven turn (not regenerate). Carries the real
  // DB id of the just-persisted user message so the client can swap its
  // optimistic `tmp-xxxxx` user-entry id for the real one without a
  // follow-up GET conversation round-trip.
  | { type: 'userMessagePersisted'; messageId: string }
  | { type: 'textDelta'; delta: string }
  | { type: 'reasoningDelta'; delta: string }
  | { type: 'toolCall'; messageId: string; toolCallId: string; name: string; argumentsJson: string }
  | { type: 'toolResult'; messageId: string; toolCallId: string; content: string }
  | { type: 'assistantMessage'; messageId: string; content: string; reasoningContent?: string | null }
  // Rolling-summary compaction is about to start. Emitted before the summary
  // LLM call so the UI can swap to a "Compacting…" overlay; paired with a
  // later `compactDone` (even on summary failure, so the overlay always clears).
  | { type: 'compactStart'; messageCount: number; currentTokens: number; thresholdTokens: number }
  | { type: 'compactDone'; messageCount: number; summaryTokens: number }
  | { type: 'error'; message: string }
  | { type: 'done' };

export interface StreamChatOptions {
  signal?: AbortSignal;
}

function doFetch(url: string, body: unknown, signal?: AbortSignal): Promise<Response> {
  return fetch(url, {
    method: 'POST',
    // Explicit so the access cookie travels even if the deployment ever
    // splits the webapp + API across origins (today's Vite proxy makes
    // them same-origin, but defaults won't save us if that changes).
    // Without this, a stale-cookie scenario in a cross-origin deploy
    // would 401 the SSE with no recovery path.
    credentials: 'include',
    headers: {
      // Only send Content-Type when there's a body to send - regenerate has none.
      ...(body !== null ? { 'Content-Type': 'application/json' } : {}),
      Accept: 'text/event-stream',
    },
    body: body !== null ? JSON.stringify(body) : undefined,
    signal,
  });
}

async function* streamSse(
  url: string,
  body: unknown,
  opts: StreamChatOptions = {},
): AsyncGenerator<AgentEvent> {
  let response = await doFetch(url, body, opts.signal);

  // Pre-stream 401 - try one refresh, then retry. If still unauthorized, tell
  // the rest of the app the session is dead and surface a clean error.
  if (response.status === 401) {
    const refreshed = await refreshSession();
    if (refreshed) {
      response = await doFetch(url, body, opts.signal);
    }
    if (response.status === 401) {
      signalSessionExpired();
      throw new Error('Session expired. Please sign in again.');
    }
  }

  if (!response.ok) {
    // 4xx/5xx surfaced before any streaming started - body is ProblemDetails JSON.
    let detail: string | undefined;
    try {
      const errBody = (await response.json()) as { detail?: string; title?: string };
      detail = errBody.detail ?? errBody.title;
    } catch {
      // ignore parse failure - fall back to status text
    }
    throw new Error(detail ?? `${response.status} ${response.statusText}`);
  }

  if (!response.body) {
    throw new Error('Response has no body.');
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = '';

  try {
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      buffer += decoder.decode(value, { stream: true });

      // SSE frames are separated by a blank line (`\n\n`). Each frame may have
      // multiple field lines; we only care about `data:`.
      let sepIdx: number;
      while ((sepIdx = buffer.indexOf('\n\n')) !== -1) {
        const frame = buffer.slice(0, sepIdx);
        buffer = buffer.slice(sepIdx + 2);

        for (const line of frame.split('\n')) {
          if (!line.startsWith('data:')) continue;
          const payload = line.slice(5).trimStart();
          if (!payload) continue;
          try {
            yield JSON.parse(payload) as AgentEvent;
          } catch {
            // Malformed frame - skip without crashing the stream.
          }
        }
      }
    }
  } finally {
    reader.releaseLock();
  }
}

export function streamChat(
  conversationId: string,
  content: string,
  opts: StreamChatOptions = {},
): AsyncGenerator<AgentEvent> {
  const url = `/api/conversations/${encodeURIComponent(conversationId)}/messages/stream`;
  return streamSse(url, { content }, opts);
}

export function streamRegenerate(
  conversationId: string,
  messageId: string,
  opts: StreamChatOptions = {},
): AsyncGenerator<AgentEvent> {
  const url = `/api/conversations/${encodeURIComponent(conversationId)}/messages/${encodeURIComponent(messageId)}/regenerate`;
  // Regenerate endpoint takes no body - pass null so the helper omits the
  // Content-Type header + JSON serialisation entirely.
  return streamSse(url, null, opts);
}
