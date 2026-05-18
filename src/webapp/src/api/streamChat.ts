// Streaming client for POST /api/conversations/{id}/messages/stream.
//
// The generated openapi-typescript-codegen client can't model SSE, so this is
// hand-written. Native EventSource is also out (it only supports GET and can't
// send a body), so we use fetch + a manual ReadableStream parser.

export type AgentEvent =
  | { type: 'textDelta'; delta: string }
  | { type: 'toolCall'; messageId: string; toolCallId: string; name: string; argumentsJson: string }
  | { type: 'toolResult'; messageId: string; toolCallId: string; content: string }
  | { type: 'assistantMessage'; messageId: string; content: string }
  | { type: 'error'; message: string }
  | { type: 'done' };

export interface StreamChatOptions {
  signal?: AbortSignal;
}

export async function* streamChat(
  conversationId: string,
  content: string,
  opts: StreamChatOptions = {},
): AsyncGenerator<AgentEvent> {
  const response = await fetch(
    `/api/conversations/${encodeURIComponent(conversationId)}/messages/stream`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Accept: 'text/event-stream' },
      body: JSON.stringify({ content }),
      signal: opts.signal,
    },
  );

  if (!response.ok) {
    // 4xx/5xx surfaced before any streaming started — body is ProblemDetails JSON.
    let detail: string | undefined;
    try {
      const body = (await response.json()) as { detail?: string; title?: string };
      detail = body.detail ?? body.title;
    } catch {
      // ignore parse failure — fall back to status text
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
            // Malformed frame — skip without crashing the stream.
          }
        }
      }
    }
  } finally {
    reader.releaseLock();
  }
}
