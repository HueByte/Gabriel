import { useCallback, useEffect, useRef, useState } from 'react';
import { HiOutlineArrowPath } from 'react-icons/hi2';
import { Avatar } from './components/Avatar';
import { Chat } from './components/Chat';
import { Sidebar } from './components/Sidebar';
import { DefaultLayout } from './layouts/DefaultLayout';
import { ConversationsService, type ConversationResponse } from './api/generated';
import { notifyError } from './lib/notify';

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
  const [sidebarRefresh, setSidebarRefresh] = useState(0);
  const [thinking, setThinking] = useState(false);
  const bootRef = useRef(false);

  // Single setter that also persists. Always use this — never setConversationIdState directly.
  const setConversationId = useCallback((id: string | null) => {
    setConversationIdState(id);
    persistActiveConversation(id);
  }, []);

  const bumpSidebar = useCallback(() => setSidebarRefresh(n => n + 1), []);

  const toggleSidebar = () => {
    setSidebarCollapsed(prev => {
      const next = !prev;
      try { localStorage.setItem(SIDEBAR_STORAGE_KEY, next ? '1' : '0'); } catch { /* ignore */ }
      return next;
    });
  };

  // Chat fires this once it loads the conversation's metadata — lets us pick up
  // the avatar seed without doing a duplicate fetch.
  const handleConversationLoaded = useCallback((conv: ConversationResponse) => {
    setAvatarSeed(conv.avatarSeed);
  }, []);

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
      bumpSidebar();
    } catch (e: unknown) {
      notifyError(e);
    }
  }, [conversationId, bumpSidebar]);

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
      onSelect={id => setConversationId(id)}
      onNewChat={startNewConversation}
      onRename={renameConversation}
      onDelete={deleteConversation}
      refreshKey={sidebarRefresh}
    />
  );

  return (
    <DefaultLayout sidebar={sidebar}>
      <div className={`avatar-wrap${thinking ? ' thinking' : ''}`}>
        <Avatar seed={avatarSeed} />
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
          onMessageSent={bumpSidebar}
          onBusyChange={setThinking}
          onConversationLoaded={handleConversationLoaded}
        />
      )}
    </DefaultLayout>
  );
}
