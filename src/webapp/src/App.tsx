import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { HiOutlineArrowPath } from 'react-icons/hi2';
import { Avatar } from './components/Avatar';
import { Chat } from './components/Chat';
import { GabrielSequenceView } from './components/GabrielSequenceView';
import { Sidebar } from './components/Sidebar';
import { DefaultLayout } from './layouts/DefaultLayout';
import { ConversationsService, type ConversationResponse } from './api/generated';
import { notifyError } from './lib/notify';
import { paletteForSeed, paletteVarsFromStops, type RGB } from './pulse/palettes';
import type { GabrielSequence } from './api/sequence';

const SIDEBAR_STORAGE_KEY = 'gabriel.sidebar.collapsed';
const ACTIVE_CONVERSATION_KEY = 'gabriel.activeConversationId';

// Placeholder seed used only during the brief gap between boot and the first
// conversation load. The avatar swaps to the conversation's real seed as soon
// as Chat fires onConversationLoaded (or startNewConversation resolves).
const FALLBACK_AVATAR_SEED = 1;

function loadSidebarCollapsed(): boolean {
  // Default to collapsed (closed overlay) — matches the EchoHub-style burger pattern.
  try {
    const v = localStorage.getItem(SIDEBAR_STORAGE_KEY);
    return v == null ? true : v === '1';
  } catch {
    return true;
  }
}

function loadActiveConversation(): string | null {
  try { return localStorage.getItem(ACTIVE_CONVERSATION_KEY); } catch { return null; }
}

function persistActiveConversation(id: string | null) {
  try {
    if (id) localStorage.setItem(ACTIVE_CONVERSATION_KEY, id);
    else localStorage.removeItem(ACTIVE_CONVERSATION_KEY);
  } catch {
    // ignore
  }
}

export function App() {
  const [conversationId, setConversationIdState] = useState<string | null>(loadActiveConversation);
  const [avatarSeed, setAvatarSeed] = useState<number>(FALLBACK_AVATAR_SEED);
  const [sidebarCollapsed, setSidebarCollapsed] = useState<boolean>(loadSidebarCollapsed);
  // Two refresh signals so each refetch only fires when its target actually
  // changed. sidebarRefresh bumps on anything that reorders/recolors the
  // conversation list (rename/delete/new-chat/send/reroll). sequenceRefresh
  // bumps only when the Gabriel Sequence's inputs change (send/reroll) —
  // renaming or deleting a sibling conversation does NOT change the active
  // conversation's sequence, so we don't waste an HTTP call there.
  const [sidebarRefresh, setSidebarRefresh] = useState(0);
  const [sequenceRefresh, setSequenceRefresh] = useState(0);
  const [thinking, setThinking] = useState(false);
  // Server-driven palette pulled from the latest GabrielSequence response. Used
  // as the source of truth for accent / gradient / thinking-pulse colors when
  // present; falls back to the seed-derived pulse palette before the first
  // sequence fetch resolves.
  const [sequenceStops, setSequenceStops] = useState<readonly RGB[] | null>(null);
  const bootRef = useRef(false);

  // Single setter that also persists. Always use this — never setConversationIdState directly.
  const setConversationId = useCallback((id: string | null) => {
    setConversationIdState(id);
    persistActiveConversation(id);
  }, []);

  const bumpSidebar = useCallback(() => setSidebarRefresh(n => n + 1), []);
  const bumpSequence = useCallback(() => setSequenceRefresh(n => n + 1), []);

  // Convenience for "a chat turn just completed" — both list ordering AND
  // sequence Live State are affected. Stable identity so Chat doesn't see a
  // new prop on every parent render.
  const onTurnComplete = useCallback(() => {
    setSidebarRefresh(n => n + 1);
    setSequenceRefresh(n => n + 1);
  }, []);

  const toggleSidebar = useCallback(() => {
    setSidebarCollapsed(prev => {
      const next = !prev;
      try { localStorage.setItem(SIDEBAR_STORAGE_KEY, next ? '1' : '0'); } catch { /* ignore */ }
      return next;
    });
  }, []);

  // Stable handler so Sidebar's onSelect prop reference doesn't change on
  // every App re-render (the previous inline `id => setConversationId(id)`
  // was creating a fresh closure each pass).
  const selectConversation = useCallback((id: string) => {
    setConversationId(id);
  }, [setConversationId]);

  // Chat fires this once it loads the conversation's metadata — lets us pick up
  // the avatar seed without doing a duplicate fetch.
  const handleConversationLoaded = useCallback((conv: ConversationResponse) => {
    setAvatarSeed(conv.avatarSeed);
  }, []);

  // GabrielSequenceView fires this once per successful sequence fetch. The
  // sequence's palette is the canonical visual identity now that the avatar is
  // server-driven — capture it so accent / gradient / thinking-pulse colors
  // match what the user actually sees on the avatar.
  const handleSequenceLoaded = useCallback((seq: GabrielSequence) => {
    // The wire format is number[][] (each row 3 numbers). Map to RGB tuples so
    // the rest of the pipeline keeps strong typing.
    const stops: RGB[] = seq.palette.map(c => [c[0], c[1], c[2]] as const);
    setSequenceStops(stops);
  }, []);

  // When the active conversation changes, clear the previous server palette so
  // we fall back to seed-derived colors until the new sequence resolves.
  // Without this the old conversation's palette would briefly bleed into the
  // new one's UI on switch.
  useEffect(() => {
    setSequenceStops(null);
  }, [conversationId]);

  const startNewConversation = useCallback(() => {
    setConversationId(null);
    ConversationsService.postApiConversations({ requestBody: { title: 'New chat' } })
      .then(conv => {
        setConversationId(conv.id);
        setAvatarSeed(conv.avatarSeed);
        bumpSidebar();
      })
      .catch(notifyError);
  }, [setConversationId, bumpSidebar]);

  const renameConversation = useCallback(async (id: string, title: string) => {
    try {
      await ConversationsService.patchApiConversations({ id, requestBody: { title } });
      bumpSidebar();
    } catch (e: unknown) {
      notifyError(e);
    }
  }, [bumpSidebar]);

  const deleteConversation = useCallback(async (id: string) => {
    try {
      await ConversationsService.deleteApiConversations({ id });
      if (conversationId === id) {
        // Active conversation was just removed — fall back to a fresh chat.
        setConversationId(null);
        startNewConversation();
      }
      bumpSidebar();
    } catch (e: unknown) {
      notifyError(e);
    }
  }, [conversationId, setConversationId, startNewConversation, bumpSidebar]);

  const rerollAvatar = useCallback(async () => {
    if (!conversationId) return;
    try {
      const conv = await ConversationsService.postApiConversationsAvatarReroll({ id: conversationId });
      setAvatarSeed(conv.avatarSeed);
      // Reroll changes the seed → the sequence's palette + pattern change.
      // Both signals fire: sidebar list reflects the updated updatedAt,
      // sequence refetches with the new visual identity.
      bumpSidebar();
      bumpSequence();
    } catch (e: unknown) {
      notifyError(e);
    }
  }, [conversationId, bumpSidebar, bumpSequence]);

  // Boot: if we have nothing stored, create a fresh conversation. If a stored
  // id exists we leave it — Chat will load it; on 404 the user can pick another
  // from the sidebar or start a new one.
  useEffect(() => {
    if (bootRef.current) return;
    bootRef.current = true;
    if (!conversationId) startNewConversation();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const sidebar = (
    <Sidebar
      activeId={conversationId}
      collapsed={sidebarCollapsed}
      onToggleCollapsed={toggleSidebar}
      onSelect={selectConversation}
      onNewChat={startNewConversation}
      onRename={renameConversation}
      onDelete={deleteConversation}
      refreshKey={sidebarRefresh}
    />
  );

  // Active palette stops, in priority order:
  //   1) The server-driven Gabriel Sequence palette — canonical visual identity
  //      once the avatar finishes its first fetch.
  //   2) The seed-derived pulse palette — fallback during boot / between
  //      conversation switch and the new sequence arriving.
  // Both feed paletteVarsFromStops so .palette-scope's CSS vars (--palette-accent,
  // --palette-accent-soft, --palette-gradient) are recomputed every time the
  // active source changes. Galactic text, thinking-pulse glow, links, blockquote
  // borders all read from those vars and re-tint automatically.
  const activeStops = useMemo<readonly RGB[]>(
    () => sequenceStops ?? paletteForSeed(avatarSeed).stops,
    [sequenceStops, avatarSeed],
  );
  const paletteVars = useMemo(
    () => paletteVarsFromStops(activeStops) as React.CSSProperties,
    [activeStops],
  );

  return (
    <DefaultLayout sidebar={sidebar}>
      <div style={paletteVars} className="palette-scope">
        <div className={`avatar-wrap${thinking ? ' thinking' : ''}`}>
          {conversationId ? (
            // Server-driven Gabriel Sequence. refreshKey is the dedicated
            // sequenceRefresh signal — only bumps on send + reroll, so renames
            // / deletes / new-chat creations don't trigger pointless refetches.
            // (A conversationId change naturally remounts the component, which
            // already triggers the initial fetch.)
            <GabrielSequenceView
              conversationId={conversationId}
              refreshKey={sequenceRefresh}
              onSequenceLoaded={handleSequenceLoaded}
            />
          ) : (
            // Booting / no active conversation yet — show the procedural avatar
            // as a placeholder so the wrap doesn't collapse.
            <Avatar seed={avatarSeed} />
          )}
          <button
            type="button"
            className="reroll"
            onClick={() => void rerollAvatar()}
            disabled={!conversationId}
            aria-label="Reroll avatar"
            title="Reroll avatar"
          >
            <HiOutlineArrowPath aria-hidden="true" />
          </button>
        </div>

        {conversationId && (
          <Chat
            conversationId={conversationId}
            avatarSeed={avatarSeed}
            paletteStops={sequenceStops}
            // onTurnComplete bumps BOTH sidebar (list re-sort by updatedAt)
            // AND the Gabriel Sequence (Live State just changed) in one stable
            // callback. Chat sees a single stable prop instead of two.
            onMessageSent={onTurnComplete}
            onBusyChange={setThinking}
            onConversationLoaded={handleConversationLoaded}
          />
        )}
      </div>
    </DefaultLayout>
  );
}
