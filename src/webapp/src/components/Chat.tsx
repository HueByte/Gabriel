import { useEffect, useRef, useState, type FormEvent, type KeyboardEvent } from 'react';
import { HiOutlineArrowUp } from 'react-icons/hi2';
import {
  ConversationsService,
  type ConversationResponse,
  type MessageResponse,
  type MessageToolCall,
} from '../api/generated';
import { toast } from 'react-toastify';
import { streamChat } from '../api/streamChat';
import { notifyError } from '../lib/notify';
import { StreamingText } from './StreamingText';
import { ThinkingPulse } from './ThinkingPulse';

// Chat entries are heterogeneous — text bubbles (user/assistant), individual
// tool calls, and tool results — so we model them as a discriminated union
// instead of trying to cram everything into a single message shape.
type ChatEntry =
  | { kind: 'text'; id: string; role: 'user' | 'assistant'; content: string; streaming?: boolean }
  | { kind: 'toolCall'; id: string; toolCallId: string; name: string; argumentsJson: string }
  | { kind: 'toolResult'; id: string; toolCallId: string; content: string };

function historyToEntries(messages: MessageResponse[]): ChatEntry[] {
  const entries: ChatEntry[] = [];
  for (const m of messages) {
    if (m.role === 'user') {
      if (m.content) entries.push({ kind: 'text', id: m.id, role: 'user', content: m.content });
    } else if (m.role === 'assistant') {
      if (m.content) entries.push({ kind: 'text', id: m.id, role: 'assistant', content: m.content });
      if (m.toolCalls) {
        for (const tc of m.toolCalls) {
          entries.push(toolCallEntry(m.id, tc));
        }
      }
    } else if (m.role === 'tool' && m.toolCallId && m.content != null) {
      entries.push({ kind: 'toolResult', id: m.id, toolCallId: m.toolCallId, content: m.content });
    }
    // role === 'system' — not rendered.
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
  /** Avatar seed — drives the thinking-pulse pattern so motion stays
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
  // loaded — lets the parent pick up things like the avatar seed without doing
  // its own duplicate fetch.
  onConversationLoaded?: (conv: ConversationResponse) => void;
}

// Composer cap (~5 lines at 14.5px / 1.5 line-height + padding). Beyond this
// the textarea grows scroll-internally instead of pushing the messages list.
const MAX_COMPOSER_HEIGHT = 140;
// Forgiveness around the bottom — the user is treated as "at bottom" until
// they're more than this many px away from it. Implemented via rootMargin on
// the IntersectionObserver: positive bottom margin extends the root's
// effective bottom edge, keeping the sentinel "intersecting" for a bit of
// slop above the true bottom.
const PIN_SLOP_PX = 80;

export function Chat({ conversationId, avatarSeed, paletteStops, onMessageSent, onBusyChange, onConversationLoaded }: ChatProps) {
  const [entries, setEntries] = useState<ChatEntry[]>([]);
  const [input, setInput] = useState('');
  const [busy, setBusy] = useState(false);
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
        if (!cancelled) notifyError(e);
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
  // canonical pattern for chat stick-to-bottom — no scroll-event race, no
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
  // (composer expanding), re-anchor to the bottom — but only if the user
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
      // Network failure / pre-flight 4xx — drop the streaming placeholder if any.
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
          {entries.map(renderEntry)}
          {/* Thinking indicator — shows once the user has submitted and before
              the first delta arrives. The condition checks that no assistant
              bubble is currently streaming, which means tokens haven't started
              flowing yet. Once a delta lands the streaming bubble takes over. */}
          {busy && !hasActiveAssistantStream(entries) && (
            <div className="thinking-pulse" aria-label="Thinking">
              <ThinkingPulse seed={avatarSeed} paletteStops={paletteStops ?? undefined} />
            </div>
          )}
          {/* Stick-to-bottom sentinel — IntersectionObserver above watches it. */}
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

function renderEntry(e: ChatEntry) {
  switch (e.kind) {
    case 'text': {
      // Assistant text gets the two-cursor typewriter (galactic lead + english
      // translation trailing). User text and history render statically.
      // `animate` is captured at mount in StreamingText, so it only matters at
      // first render — switching from streaming → done doesn't abort the loop.
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
        <div key={e.id} className="tool-call">
          <span className="tool-badge">tool</span>
          <code>{e.name}({prettyArgs(e.argumentsJson)})</code>
        </div>
      );
    case 'toolResult':
      return (
        <div key={e.id} className="tool-result">
          <span className="tool-badge">→</span>
          <pre>{e.content}</pre>
        </div>
      );
  }
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

    case 'toolCall':
      setEntries(prev => {
        // Finalize the current streaming bubble (the iteration that asked for tools).
        // Keep its id stable so the StreamingText component instance survives
        // the streaming → !streaming transition and can finish typing.
        const finalized = prev.map((e, i) => {
          if (i === prev.length - 1 && e.kind === 'text' && e.role === 'assistant' && e.streaming) {
            return { ...e, streaming: false };
          }
          return e;
        });
        return [...finalized, toolCallEntry(evt.messageId, {
          id: evt.toolCallId,
          name: evt.name,
          argumentsJson: evt.argumentsJson,
        })];
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
        const last = prev[prev.length - 1];
        if (last?.kind === 'text' && last.role === 'assistant' && last.streaming) {
          // Keep id stable so the in-flight StreamingText keeps typing.
          return [
            ...prev.slice(0, -1),
            { ...last, content: evt.content, streaming: false },
          ];
        }
        return [
          ...prev,
          { kind: 'text', id: evt.messageId, role: 'assistant', content: evt.content },
        ];
      });
      break;

    case 'error':
      // In-stream error — surface via toast. Pre-flight errors are caught at
      // the top of send() and toasted there.
      toast.error(evt.message);
      break;

    case 'done':
      // Loop finished — just make sure no entry is still marked streaming.
      setEntries(prev => prev.map(e =>
        e.kind === 'text' && e.streaming ? { ...e, streaming: false } : e,
      ));
      break;
  }
}

