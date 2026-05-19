import { useEffect, useRef, useState, type FormEvent, type KeyboardEvent } from 'react';
import { HiOutlineArrowUp } from 'react-icons/hi2';
import {
  ApiError,
  ConversationsService,
  type ConversationResponse,
  type MessageResponse,
  type MessageToolCall,
} from '../api/generated';
import { toast } from 'react-toastify';
import { streamChat } from '../api/streamChat';
import { notifyError } from '../lib/notify';
import { useHideThinking, useHideToolCalls, useHideToolResults } from '../lib/userPrefs';
import { StreamingText } from './StreamingText';
import { ThinkingPulse } from './ThinkingPulse';

// Chat entries are heterogeneous - text bubbles (user/assistant), individual
// tool calls, tool results, and the model's "thoughts" (reasoning text that
// preceded a tool call) - so we model them as a discriminated union instead
// of trying to cram everything into a single message shape.
//
// The ReAct flow visualizes as:
//   User Query ➔ [ Thought ➔ Action(toolCall) ➔ Observation(toolResult) ]* ➔ Final Answer
//
// Detecting a thought is purely structural: assistant content that lives on a
// Message which ALSO carries tool calls is - by construction of the backend
// loop - the reasoning text the model emitted before requesting the tool.
type ChatEntry =
  | { kind: 'text'; id: string; role: 'user' | 'assistant'; content: string; streaming?: boolean }
  | { kind: 'thought'; id: string; content: string; streaming?: boolean }
  // `reasoning` carries the dedicated reasoning_content stream (Grok 4,
  // DeepSeek-R1, OpenAI o-series, Anthropic extended-thinking). Rendered the
  // same way as a `thought` - collapsed by default - but kept distinct because
  // a single turn can produce both: pre-tool reasoning text *and* a separate
  // chain-of-thought stream.
  | { kind: 'reasoning'; id: string; content: string; streaming?: boolean }
  | { kind: 'toolCall'; id: string; toolCallId: string; name: string; argumentsJson: string }
  | { kind: 'toolResult'; id: string; toolCallId: string; content: string };

function historyToEntries(messages: MessageResponse[]): ChatEntry[] {
  const entries: ChatEntry[] = [];
  for (const m of messages) {
    if (m.role === 'user') {
      if (m.content) entries.push({ kind: 'text', id: m.id, role: 'user', content: m.content });
    } else if (m.role === 'assistant') {
      // Reasoning stream (reasoning_content) renders first if the provider
      // captured one for this turn - it precedes both the model's regular
      // content and any tool calls.
      if (m.reasoningContent) {
        entries.push({ kind: 'reasoning', id: `${m.id}-reasoning`, content: m.reasoningContent });
      }
      // Assistant content + tool calls on the same message ⇒ that content is
      // the model's reasoning ("Thought"). Without tool calls, it's the final
      // answer (or an intermediate text-only iteration).
      const hasToolCalls = !!m.toolCalls && m.toolCalls.length > 0;
      if (m.content) {
        if (hasToolCalls) {
          entries.push({ kind: 'thought', id: m.id, content: m.content });
        } else {
          entries.push({ kind: 'text', id: m.id, role: 'assistant', content: m.content });
        }
      }
      if (m.toolCalls) {
        for (const tc of m.toolCalls) {
          entries.push(toolCallEntry(m.id, tc));
        }
      }
    } else if (m.role === 'tool' && m.toolCallId && m.content != null) {
      entries.push({ kind: 'toolResult', id: m.id, toolCallId: m.toolCallId, content: m.content });
    }
    // role === 'system' - not rendered.
  }
  return entries;
}

function toolCallEntry(messageId: string, tc: MessageToolCall): ChatEntry {
  return {
    kind: 'toolCall',
    id: `${messageId}-call-${tc.id}`,
    toolCallId: tc.id,
    name: tc.name,
    argumentsJson: tc.argumentsJson,
  };
}

interface ChatProps {
  conversationId: string;
  /** Avatar seed - drives the thinking-pulse pattern so motion stays
   *  deterministic per conversation. Colors come from `paletteStops` when
   *  available, otherwise fall back to the seed-derived palette. */
  avatarSeed: number;
  /** Optional server-driven palette stops. When provided, the thinking pulse
   *  recolors its bars from this instead of the seed-derived palette so the
   *  indicator matches the active Gabriel Sequence's actual colors. */
  paletteStops?: readonly import('../pulse/palettes').RGB[] | null;
  onMessageSent?: () => void;
  onBusyChange?: (busy: boolean) => void;
  // Fired once per conversation switch as soon as the conversation metadata is
  // loaded - lets the parent pick up things like the avatar seed without doing
  // its own duplicate fetch.
  onConversationLoaded?: (conv: ConversationResponse) => void;
  // Fired when the initial history fetch returns 404 - the conversation was
  // deleted out from under us (e.g. on another tab). The parent typically
  // navigates away from this stale URL.
  onConversationMissing?: () => void;
}

// Composer cap (~5 lines at 14.5px / 1.5 line-height + padding). Beyond this
// the textarea grows scroll-internally instead of pushing the messages list.
const MAX_COMPOSER_HEIGHT = 140;
// Forgiveness around the bottom - the user is treated as "at bottom" until
// they're more than this many px away from it. Implemented via rootMargin on
// the IntersectionObserver: positive bottom margin extends the root's
// effective bottom edge, keeping the sentinel "intersecting" for a bit of
// slop above the true bottom.
const PIN_SLOP_PX = 80;

export function Chat({ conversationId, avatarSeed, paletteStops, onMessageSent, onBusyChange, onConversationLoaded, onConversationMissing }: ChatProps) {
  const [entries, setEntries] = useState<ChatEntry[]>([]);
  const [input, setInput] = useState('');
  const [busy, setBusy] = useState(false);
  // User preferences: per-kind ReAct visibility. Each toggle independently
  // suppresses one category of scaffolding in the transcript:
  //   - thinking:   `thought` + `reasoning` entries (chain-of-thought)
  //   - tool calls: `toolCall` entries (action badge)
  //   - tool results: `toolResult` entries (observation badge)
  // The actual assistant text bubble (kind='text', role='assistant') always
  // shows so the typewriter still works.
  const [hideThinking] = useHideThinking();
  const [hideToolCalls] = useHideToolCalls();
  const [hideToolResults] = useHideToolResults();
  const scrollRef = useRef<HTMLDivElement | null>(null);
  const messagesContentRef = useRef<HTMLDivElement | null>(null);
  const bottomSentinelRef = useRef<HTMLDivElement | null>(null);
  const textareaRef = useRef<HTMLTextAreaElement | null>(null);
  const abortRef = useRef<AbortController | null>(null);
  // Whether the bottom sentinel is currently visible to the user. Updated by
  // an IntersectionObserver. Ref because we read from non-React callbacks and
  // don't need re-renders on every change.
  const isAtBottomRef = useRef(true);

  // Load history on conversation switch.
  useEffect(() => {
    let cancelled = false;
    setEntries([]);
    // Re-engage auto-anchor on conversation switch so we land at the bottom.
    isAtBottomRef.current = true;
    ConversationsService.getApiConversations1({ id: conversationId })
      .then(conv => {
        if (cancelled) return;
        setEntries(historyToEntries(conv.messages ?? []));
        onConversationLoaded?.(conv);
      })
      .catch((e: unknown) => {
        if (cancelled) return;
        // 404 = the conversation no longer exists. Let the parent decide
        // what to do (typically: clear the stored id and redirect).
        if (e instanceof ApiError && e.status === 404) {
          onConversationMissing?.();
          return;
        }
        notifyError(e);
      });
    return () => {
      cancelled = true;
      // If a stream was in flight from the previous conversation, abort it.
      abortRef.current?.abort();
      abortRef.current = null;
    };
  }, [conversationId]);

  // Track whether the user is at the bottom via an IntersectionObserver on a
  // sentinel placed at the very end of the message content. This is the
  // canonical pattern for chat stick-to-bottom - no scroll-event race, no
  // scrollHeight/scrollTop arithmetic, and the browser handles edge cases like
  // sub-pixel zoom and content shrinking for us.
  useEffect(() => {
    const root = scrollRef.current;
    const sentinel = bottomSentinelRef.current;
    if (!root || !sentinel) return;
    const io = new IntersectionObserver(
      ([entry]) => { isAtBottomRef.current = entry.isIntersecting; },
      { root, rootMargin: `0px 0px ${PIN_SLOP_PX}px 0px`, threshold: 0 },
    );
    io.observe(sentinel);
    return () => io.disconnect();
  }, []);

  // When content grows (new messages, streaming text) OR the container shrinks
  // (composer expanding), re-anchor to the bottom - but only if the user
  // hasn't scrolled away. Smooth scroll for small deltas (the common streaming
  // case feels gentle), instant for large jumps (conversation switch, big
  // catch-ups) where smooth would noticeably lag.
  useEffect(() => {
    const root = scrollRef.current;
    const content = messagesContentRef.current;
    if (!root || !content) return;
    const ro = new ResizeObserver(() => {
      if (!isAtBottomRef.current) return;
      const delta = root.scrollHeight - root.scrollTop - root.clientHeight;
      root.scrollTo({
        top: root.scrollHeight,
        behavior: delta < 600 ? 'smooth' : 'auto',
      });
    });
    ro.observe(content); // fires on content growth (streaming, new messages)
    ro.observe(root);    // fires on container resize (composer growing)
    return () => ro.disconnect();
  }, []);

  // Auto-grow the textarea up to MAX_COMPOSER_HEIGHT; beyond that, the
  // textarea's own overflow-y handles scrolling.
  useEffect(() => {
    const ta = textareaRef.current;
    if (!ta) return;
    ta.style.height = 'auto';
    ta.style.height = `${Math.min(ta.scrollHeight, MAX_COMPOSER_HEIGHT)}px`;
  }, [input]);

  // Surface thinking/streaming state to the parent so the avatar can react.
  useEffect(() => {
    onBusyChange?.(busy);
  }, [busy, onBusyChange]);

  // Restore textarea focus after a send completes. The `disabled={busy}` attr
  // blurs the textarea when the stream starts; without this the user has to
  // click back into the input to send the next message. We watch for the
  // busy→idle transition specifically so we don't steal focus on mount or on
  // every unrelated re-render. `preventScroll` avoids jumping the page if the
  // chat is somehow scrolled away from the composer.
  const prevBusyRef = useRef(false);
  useEffect(() => {
    if (prevBusyRef.current && !busy) {
      textareaRef.current?.focus({ preventScroll: true });
    }
    prevBusyRef.current = busy;
  }, [busy]);

  const send = async () => {
    const text = input.trim();
    if (!text || busy) return;

    setInput('');
    setBusy(true);

    // Sending always re-engages stick-to-bottom, even if the user was scrolled
    // up reading older content. The ResizeObserver below will scroll on the
    // next layout (the new user entry + incoming streaming reply).
    isAtBottomRef.current = true;

    const tempUserId = `tmp-${crypto.randomUUID()}`;
    const userEntry: ChatEntry = { kind: 'text', id: tempUserId, role: 'user', content: text };
    setEntries(prev => [...prev, userEntry]);

    const controller = new AbortController();
    abortRef.current = controller;

    try {
      for await (const evt of streamChat(conversationId, text, { signal: controller.signal })) {
        applyAgentEvent(evt, setEntries);
        if (evt.type === 'done' || evt.type === 'error') break;
      }
      onMessageSent?.();
    } catch (e: unknown) {
      // Network failure / pre-flight 4xx - drop the streaming placeholder if any.
      notifyError(e);
      setEntries(prev => prev.filter(e2 => !(e2.kind === 'text' && e2.role === 'assistant' && e2.streaming)));
    } finally {
      setBusy(false);
      abortRef.current = null;
    }
  };

  const onSubmit = (e: FormEvent) => {
    e.preventDefault();
    void send();
  };

  const onKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      void send();
    }
  };

  return (
    <div className="chat">
      <div className="messages" ref={scrollRef}>
        <div className="messages-content" ref={messagesContentRef}>
          {entries.length === 0 && !busy && (
            <div className="empty">Say hi to get started.</div>
          )}
          {/* Per-kind ReAct visibility filter. Each toggle independently
              drops one scaffolding kind from the transcript while leaving
              the user + assistant text bubbles intact. `text` always passes
              through so the streaming typewriter keeps working. */}
          {entries.filter(e => {
            if (e.kind === 'thought' || e.kind === 'reasoning') return !hideThinking;
            if (e.kind === 'toolCall') return !hideToolCalls;
            if (e.kind === 'toolResult') return !hideToolResults;
            return true;
          }).map(renderEntry)}
          {/* Thinking indicator - shows once the user has submitted and before
              the first delta arrives. The condition checks that no assistant
              bubble is currently streaming, which means tokens haven't started
              flowing yet. Once a delta lands the streaming bubble takes over. */}
          {busy && !hasActiveAssistantStream(entries) && (
            <div className="thinking-pulse" aria-label="Thinking">
              <ThinkingPulse seed={avatarSeed} paletteStops={paletteStops ?? undefined} />
            </div>
          )}
          {/* Stick-to-bottom sentinel - IntersectionObserver above watches it. */}
          <div ref={bottomSentinelRef} className="messages-bottom" aria-hidden="true" />
        </div>
      </div>
      <form className="composer" onSubmit={onSubmit}>
        <div className="composer-shell">
          <textarea
            ref={textareaRef}
            value={input}
            onChange={e => setInput(e.target.value)}
            onKeyDown={onKeyDown}
            placeholder="Message Gabriel…"
            disabled={busy}
            rows={1}
          />
          <button type="submit" disabled={busy || input.trim().length === 0} aria-label="Send">
            <HiOutlineArrowUp aria-hidden="true" />
          </button>
        </div>
      </form>
    </div>
  );
}

function hasActiveAssistantStream(entries: ChatEntry[]): boolean {
  const last = entries[entries.length - 1];
  return !!(last && last.kind === 'text' && last.role === 'assistant' && last.streaming);
}

// Array.prototype.findLastIndex exists but lacks the type narrowing we want;
// a tiny helper keeps the stream handlers tidy without leaking `any`.
function lastIndexWhere<T>(arr: readonly T[], pred: (e: T) => boolean): number {
  for (let i = arr.length - 1; i >= 0; i--) {
    if (pred(arr[i])) return i;
  }
  return -1;
}

function renderEntry(e: ChatEntry) {
  switch (e.kind) {
    case 'text': {
      // Assistant text gets the two-cursor typewriter (galactic lead + english
      // translation trailing). User text and history render statically.
      // `animate` is captured at mount in StreamingText, so it only matters at
      // first render - switching from streaming → done doesn't abort the loop.
      // `caret` stays true for assistant entries; StreamingText hides it once
      // typing has caught up to text.
      const isLiveAssistant = e.role === 'assistant' && e.streaming === true;
      return (
        <div key={e.id} className={`message ${e.role}${e.streaming ? ' streaming' : ''}`}>
          {e.role === 'assistant' ? (
            <StreamingText text={e.content} animate={isLiveAssistant} caret galactic />
          ) : (
            e.content
          )}
        </div>
      );
    }
    case 'toolCall':
      return (
        <div key={e.id} className="react-step tool-call">
          <span className="tool-badge action-badge">action</span>
          <code>{e.name}({prettyArgs(e.argumentsJson)})</code>
        </div>
      );
    case 'toolResult':
      return <ToolResult key={e.id} content={e.content} />;
    case 'thought':
      return <Thought key={e.id} content={e.content} />;
    case 'reasoning':
      return <Reasoning key={e.id} content={e.content} streaming={e.streaming} />;
  }
}

// Dedicated reasoning-stream panel. Distinct badge ("thinking") so the user
// can tell a true chain-of-thought stream from `thought` (pre-tool reasoning
// text emitted on the regular content channel). Same collapsed disclosure
// pattern so long monologues don't dominate the chat.
function Reasoning({ content, streaming }: { content: string; streaming?: boolean }) {
  const lines = content.length > 0 ? content.split('\n') : [''];
  const firstLine = lines[0];
  const extraLines = lines.length - 1;
  const preview = firstLine.length > 0 ? firstLine : (streaming ? '(thinking…)' : '(no reasoning)');
  return (
    <details className="react-step reasoning" open={streaming}>
      <summary>
        <span className="tool-badge reasoning-badge">thinking</span>
        <span className="thought-preview">{preview}</span>
        {extraLines > 0 && (
          <span className="thought-lines">+{extraLines} {extraLines === 1 ? 'line' : 'lines'}</span>
        )}
      </summary>
      <div className="thought-body">{content}</div>
    </details>
  );
}

// Reasoning text the model emitted before requesting a tool. Same disclosure
// pattern as ToolResult - collapsed by default so the chat doesn't fill up
// with chain-of-thought monologues, expandable when the user wants to peek.
function Thought({ content }: { content: string }) {
  const lines = content.length > 0 ? content.split('\n') : [''];
  const firstLine = lines[0];
  const extraLines = lines.length - 1;
  const preview = firstLine.length > 0 ? firstLine : '(thinking…)';
  return (
    <details className="react-step thought">
      <summary>
        <span className="tool-badge thought-badge">thought</span>
        <span className="thought-preview">{preview}</span>
        {extraLines > 0 && (
          <span className="thought-lines">+{extraLines} {extraLines === 1 ? 'line' : 'lines'}</span>
        )}
      </summary>
      <div className="thought-body">{content}</div>
    </details>
  );
}

// Tool outputs can be huge (file listings, search results). Render collapsed
// by default with a one-line preview + line-count hint; expand reveals the
// full content capped at 30 lines with internal scroll (styles.css). Uses
// the native <details>/<summary> pattern for accessibility + zero JS state.
function ToolResult({ content }: { content: string }) {
  const lines = content.length > 0 ? content.split('\n') : [''];
  const firstLine = lines[0];
  const extraLines = lines.length - 1;
  const preview = firstLine.length > 0 ? firstLine : '(empty result)';
  return (
    <details className="react-step tool-result">
      <summary>
        <span className="tool-badge observation-badge">observation</span>
        <span className="tool-result-preview">{preview}</span>
        {extraLines > 0 && (
          <span className="tool-result-lines">+{extraLines} {extraLines === 1 ? 'line' : 'lines'}</span>
        )}
      </summary>
      <pre>{content}</pre>
    </details>
  );
}

function prettyArgs(json: string): string {
  if (!json || json === '{}') return '';
  try {
    return JSON.stringify(JSON.parse(json));
  } catch {
    return json;
  }
}

// Mutates the entries array based on a single streamed agent event.
function applyAgentEvent(
  evt: import('../api/streamChat').AgentEvent,
  setEntries: React.Dispatch<React.SetStateAction<ChatEntry[]>>,
) {
  switch (evt.type) {
    case 'textDelta':
      setEntries(prev => {
        const last = prev[prev.length - 1];
        if (last?.kind === 'text' && last.role === 'assistant' && last.streaming) {
          // Append to the active streaming bubble.
          const updated: ChatEntry = { ...last, content: last.content + evt.delta };
          return [...prev.slice(0, -1), updated];
        }
        // Start a fresh streaming bubble.
        return [
          ...prev,
          {
            kind: 'text',
            id: `streaming-${crypto.randomUUID()}`,
            role: 'assistant',
            content: evt.delta,
            streaming: true,
          },
        ];
      });
      break;

    case 'reasoningDelta':
      // Reasoning tokens arrive interleaved with (or strictly before) regular
      // content tokens. Keep a single streaming `reasoning` entry per turn -
      // it sits ahead of the streaming text bubble so the UI reads top-down:
      // thinking → answer.
      setEntries(prev => {
        // Find the most recent streaming reasoning entry; reasoning arrives
        // before content, so it'll typically be at the tail. If we already
        // started a streaming text bubble, the reasoning still wraps around it.
        const idx = lastIndexWhere(prev, e => e.kind === 'reasoning' && !!e.streaming);
        if (idx >= 0) {
          const existing = prev[idx] as Extract<ChatEntry, { kind: 'reasoning' }>;
          const updated: ChatEntry = { ...existing, content: existing.content + evt.delta };
          return [...prev.slice(0, idx), updated, ...prev.slice(idx + 1)];
        }
        return [
          ...prev,
          {
            kind: 'reasoning',
            id: `streaming-reasoning-${crypto.randomUUID()}`,
            content: evt.delta,
            streaming: true,
          },
        ];
      });
      break;

    case 'toolCall':
      setEntries(prev => {
        // Freeze any in-flight streaming reasoning - the model is moving on
        // from thinking to executing tools, so the reasoning panel should
        // collapse to its done state.
        const frozenReasoning = prev.map<ChatEntry>(e =>
          e.kind === 'reasoning' && e.streaming ? { ...e, streaming: false } : e,
        );
        // The trailing streaming assistant bubble is the model's reasoning that
        // preceded this tool call - reclassify it as a `thought` so the UI
        // renders it as a collapsed ReAct step alongside the action/observation.
        // Empty buffers are dropped (some providers emit no reasoning before
        // calling a tool). Keep its id stable so any in-flight StreamingText
        // doesn't remount.
        const last = frozenReasoning[frozenReasoning.length - 1];
        const head = (last && last.kind === 'text' && last.role === 'assistant' && last.streaming)
          ? frozenReasoning.slice(0, -1)
          : frozenReasoning;
        const thoughtFromStreaming: ChatEntry[] = (last && last.kind === 'text' && last.role === 'assistant' && last.streaming)
          ? (last.content.trim().length > 0
              ? [{ kind: 'thought', id: last.id, content: last.content }]
              : [])
          : [];
        return [
          ...head,
          ...thoughtFromStreaming,
          toolCallEntry(evt.messageId, {
            id: evt.toolCallId,
            name: evt.name,
            argumentsJson: evt.argumentsJson,
          }),
        ];
      });
      break;

    case 'toolResult':
      setEntries(prev => [
        ...prev,
        { kind: 'toolResult', id: evt.messageId, toolCallId: evt.toolCallId, content: evt.content },
      ]);
      break;

    case 'assistantMessage':
      setEntries(prev => {
        // First: reconcile any streaming reasoning entry against the canonical
        // reasoningContent the server persisted. If we never streamed one but
        // the server has reasoning (e.g. provider buffered all reasoning before
        // text), inject it now so reloads-vs-live-view stay consistent.
        let next = prev.map<ChatEntry>(e =>
          e.kind === 'reasoning' && e.streaming
            ? { ...e, streaming: false, content: evt.reasoningContent ?? e.content }
            : e,
        );
        const hasStreamedReasoning = next.some(e => e.kind === 'reasoning');
        if (!hasStreamedReasoning && evt.reasoningContent) {
          // Insert before the final assistant text bubble (if any), else append.
          const tail = next[next.length - 1];
          const insertEntry: ChatEntry = {
            kind: 'reasoning',
            id: `${evt.messageId}-reasoning`,
            content: evt.reasoningContent,
          };
          next = tail?.kind === 'text' && tail.role === 'assistant'
            ? [...next.slice(0, -1), insertEntry, tail]
            : [...next, insertEntry];
        }

        const last = next[next.length - 1];
        if (last?.kind === 'text' && last.role === 'assistant' && last.streaming) {
          // Keep id stable so the in-flight StreamingText keeps typing.
          return [
            ...next.slice(0, -1),
            { ...last, content: evt.content, streaming: false },
          ];
        }
        return [
          ...next,
          { kind: 'text', id: evt.messageId, role: 'assistant', content: evt.content },
        ];
      });
      break;

    case 'error':
      // In-stream error - surface via toast. Pre-flight errors are caught at
      // the top of send() and toasted there.
      toast.error(evt.message);
      break;

    case 'done':
      // Loop finished - just make sure no entry is still marked streaming.
      setEntries(prev => prev.map(e => {
        if (e.kind === 'text' && e.streaming) return { ...e, streaming: false };
        if (e.kind === 'reasoning' && e.streaming) return { ...e, streaming: false };
        return e;
      }));
      break;
  }
}

