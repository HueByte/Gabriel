import { useEffect, useRef, useState, type KeyboardEvent, type MouseEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  HiOutlineArrowRightOnRectangle,
  HiOutlineBars3,
  HiOutlineEllipsisVertical,
  HiOutlinePencilSquare,
  HiOutlinePlus,
  HiOutlineTrash,
  HiOutlineWrenchScrewdriver,
  HiOutlineXMark,
} from 'react-icons/hi2';
import { ConversationsService, type ConversationResponse } from '../api/generated';
import { useAuth } from '../auth/AuthContext';
import { notifyError } from '../lib/notify';
import { ProjectPicker, loadActiveProjectId } from './ProjectPicker';

const SIDEBAR_STORAGE_KEY = 'gabriel.sidebar.collapsed';

function loadSidebarCollapsed(): boolean {
  // Default to collapsed (closed overlay) — matches the EchoHub-style burger pattern.
  try {
    const v = localStorage.getItem(SIDEBAR_STORAGE_KEY);
    return v == null ? true : v === '1';
  } catch {
    return true;
  }
}

interface SidebarProps {
  /** Bumped by the layout when an external action (e.g. a chat turn
   *  completing) changes the conversation list's sort order. */
  refreshKey: number;
}

// Per-row menu state. We store viewport coordinates so the menu can render with
// position: fixed and bypass the sidebar-body's overflow clipping (otherwise
// menus on the last rows would get cut off by the scroll container).
interface MenuState {
  id: string;
  top: number;
  right: number;
}

export function Sidebar({ refreshKey }: SidebarProps) {
  const navigate = useNavigate();
  // Active id comes from the URL — single source of truth, no prop drilling.
  const { conversationId: activeId } = useParams<{ conversationId: string }>();

  const [conversations, setConversations] = useState<ConversationResponse[]>([]);
  const [collapsed, setCollapsed] = useState<boolean>(loadSidebarCollapsed);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editTitle, setEditTitle] = useState('');
  const [menu, setMenu] = useState<MenuState | null>(null);
  // Local refresh tick — bumped by sidebar-initiated mutations so the list
  // refetches without going through the layout's external refresh signal.
  const [localRefresh, setLocalRefresh] = useState(0);
  // Project filter. localStorage is the canonical source; ProjectPicker keeps
  // it in sync. null = "all projects" but in practice the picker auto-selects
  // one as soon as the user has any.
  const [activeProjectId, setActiveProjectId] = useState<string | null>(loadActiveProjectId);
  const editInputRef = useRef<HTMLInputElement | null>(null);
  const menuRef = useRef<HTMLDivElement | null>(null);
  const { user, logout } = useAuth();

  const open = !collapsed;

  const toggleCollapsed = () => {
    setCollapsed(prev => {
      const next = !prev;
      try { localStorage.setItem(SIDEBAR_STORAGE_KEY, next ? '1' : '0'); } catch { /* ignore */ }
      return next;
    });
  };

  useEffect(() => {
    let cancelled = false;
    // Project-scoped fetch — passing undefined for "all" would return
    // cross-project results, which the picker UX implies isn't what we want.
    // When activeProjectId is null (briefly during first boot before the
    // picker resolves), pass undefined and show whatever the server returns.
    ConversationsService.getApiConversations({ projectId: activeProjectId ?? undefined })
      .then(list => {
        if (cancelled) return;
        const sorted = [...list].sort((a, b) => b.updatedAt.localeCompare(a.updatedAt));
        setConversations(sorted);
      })
      .catch((e: unknown) => {
        if (!cancelled) notifyError(e, 'Failed to load conversations.');
      });
    return () => { cancelled = true; };
  }, [refreshKey, localRefresh, activeProjectId]);

  useEffect(() => {
    if (editingId) editInputRef.current?.select();
  }, [editingId]);

  // Close on Escape when the panel is open.
  useEffect(() => {
    if (!open) return;
    const onKey = (e: globalThis.KeyboardEvent) => {
      if (e.key === 'Escape') toggleCollapsed();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  // Close menu on outside click, Escape, sidebar scroll, and window resize.
  // Scroll/resize matter because the menu is position: fixed — once the anchor
  // button moves, the menu would otherwise float orphaned in the viewport.
  useEffect(() => {
    if (!menu) return;
    const close = () => setMenu(null);
    const onDown = (e: globalThis.MouseEvent) => {
      const target = e.target as Element | null;
      if (!target) return;
      // Clicks inside the menu itself — let the menu item handler run.
      if (menuRef.current?.contains(target)) return;
      // Clicks on any 3-dot trigger — let the button's onClick toggle it,
      // otherwise we'd close-then-immediately-reopen on a same-row re-click.
      if (target.closest('.conv-action')) return;
      close();
    };
    const onKey = (e: globalThis.KeyboardEvent) => {
      if (e.key === 'Escape') close();
    };
    const body = document.querySelector('.sidebar-body');
    document.addEventListener('mousedown', onDown);
    document.addEventListener('keydown', onKey);
    body?.addEventListener('scroll', close);
    window.addEventListener('resize', close);
    return () => {
      document.removeEventListener('mousedown', onDown);
      document.removeEventListener('keydown', onKey);
      body?.removeEventListener('scroll', close);
      window.removeEventListener('resize', close);
    };
  }, [menu]);

  // Close menu whenever the sidebar collapses or the conversation list refreshes.
  useEffect(() => {
    if (!open) setMenu(null);
  }, [open]);
  useEffect(() => {
    setMenu(null);
  }, [refreshKey, localRefresh]);

  const bumpLocal = () => setLocalRefresh(n => n + 1);

  const startRename = (c: ConversationResponse) => {
    setEditingId(c.id);
    setEditTitle(c.title);
  };

  const cancelRename = () => {
    setEditingId(null);
    setEditTitle('');
  };

  const commitRename = async () => {
    const id = editingId;
    const title = editTitle.trim();
    if (!id) return;
    setEditingId(null);
    setEditTitle('');
    const original = conversations.find(c => c.id === id);
    if (!title || title === original?.title) return;
    try {
      await ConversationsService.patchApiConversations({ id, requestBody: { title } });
      bumpLocal();
    } catch (e: unknown) {
      notifyError(e);
    }
  };

  const onEditKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') { e.preventDefault(); void commitRename(); }
    else if (e.key === 'Escape') { e.preventDefault(); cancelRename(); }
  };

  const confirmDelete = async (c: ConversationResponse) => {
    const ok = window.confirm(`Delete "${c.title || 'Untitled'}"? This cannot be undone.`);
    if (!ok) return;
    try {
      await ConversationsService.deleteApiConversations({ id: c.id });
      // If we just deleted the active conversation, hand off to "/" — IndexPage
      // will either pick another stored conv or create a fresh one.
      if (activeId === c.id) {
        navigate('/', { replace: true });
      }
      bumpLocal();
    } catch (e: unknown) {
      notifyError(e);
    }
  };

  const handleSelect = (id: string) => {
    navigate(`/c/${encodeURIComponent(id)}`);
    toggleCollapsed();
  };

  const handleNewChat = async () => {
    toggleCollapsed();
    try {
      // Pass the active projectId so the new chat lands in the right folder.
      // If null, the server falls back to the user's Default project.
      const conv = await ConversationsService.postApiConversations({
        requestBody: { title: 'New chat', projectId: activeProjectId ?? undefined },
      });
      bumpLocal();
      navigate(`/c/${encodeURIComponent(conv.id)}`);
    } catch (e: unknown) {
      notifyError(e);
    }
  };

  const openDiagnostics = (id: string) => {
    navigate(`/c/${encodeURIComponent(id)}/diagnostics`);
    toggleCollapsed();
  };

  const toggleMenu = (e: MouseEvent<HTMLButtonElement>, id: string) => {
    e.stopPropagation();
    if (menu?.id === id) {
      setMenu(null);
      return;
    }
    const rect = e.currentTarget.getBoundingClientRect();
    setMenu({
      id,
      top: rect.bottom + 4,
      right: window.innerWidth - rect.right,
    });
  };

  return (
    <>
      <button
        type="button"
        className="sidebar-burger"
        onClick={toggleCollapsed}
        aria-label="Open menu"
        aria-hidden={open}
        tabIndex={open ? -1 : 0}
      >
        <HiOutlineBars3 size={20} aria-hidden="true" />
      </button>

      <div
        className={`sidebar-backdrop${open ? ' visible' : ''}`}
        onClick={toggleCollapsed}
        aria-hidden="true"
      />

      <nav
        className={`sidebar${open ? ' open' : ''}`}
        aria-hidden={!open}
      >
        <div className="sidebar-head">
          <span className="sidebar-brand">Gabriel</span>
          <button
            type="button"
            className="sidebar-close"
            onClick={toggleCollapsed}
            aria-label="Close menu"
          >
            <HiOutlineXMark size={18} aria-hidden="true" />
          </button>
        </div>

        <ProjectPicker
          activeProjectId={activeProjectId}
          onActiveProjectChange={setActiveProjectId}
          refreshKey={refreshKey}
        />

        <button type="button" className="new-chat" onClick={() => void handleNewChat()}>
          <HiOutlinePlus aria-hidden="true" />
          <span>New chat</span>
        </button>

        <div className="sidebar-body">
          {conversations.length === 0 && (
            <div className="sidebar-empty">No conversations yet.</div>
          )}
          <ul className="conv-list">
            {conversations.map(c => (
              <li
                key={c.id}
                className={`conv-row${c.id === activeId ? ' active' : ''}${menu?.id === c.id ? ' menu-open' : ''}`}
              >
                {editingId === c.id ? (
                  <input
                    ref={editInputRef}
                    className="conv-edit"
                    value={editTitle}
                    onChange={e => setEditTitle(e.target.value)}
                    onBlur={() => void commitRename()}
                    onKeyDown={onEditKeyDown}
                  />
                ) : (
                  <>
                    <button
                      type="button"
                      className="conv-item"
                      onClick={() => handleSelect(c.id)}
                      onDoubleClick={() => startRename(c)}
                      title={c.title}
                    >
                      {c.title || 'Untitled'}
                    </button>
                    <div className="conv-menu-wrap">
                      <button
                        type="button"
                        className="conv-action"
                        onClick={e => toggleMenu(e, c.id)}
                        title="More"
                        aria-label="More options"
                        aria-haspopup="menu"
                        aria-expanded={menu?.id === c.id}
                      >
                        <HiOutlineEllipsisVertical aria-hidden="true" />
                      </button>
                    </div>
                  </>
                )}
              </li>
            ))}
          </ul>
        </div>

        {user && (
          <div className="sidebar-foot">
            <div className="sidebar-user" title={user.email}>{user.email}</div>
            <button
              type="button"
              className="sidebar-logout"
              onClick={() => void logout()}
              title="Sign out"
              aria-label="Sign out"
            >
              <HiOutlineArrowRightOnRectangle aria-hidden="true" />
              <span>Sign out</span>
            </button>
          </div>
        )}
      </nav>

      {menu && (() => {
        const c = conversations.find(x => x.id === menu.id);
        if (!c) return null;
        return (
          <div
            ref={menuRef}
            className="conv-menu"
            role="menu"
            style={{ top: menu.top, right: menu.right }}
          >
            <button
              type="button"
              className="conv-menu-item"
              role="menuitem"
              onClick={() => { setMenu(null); startRename(c); }}
            >
              <HiOutlinePencilSquare aria-hidden="true" />
              <span>Rename</span>
            </button>
            <button
              type="button"
              className="conv-menu-item"
              role="menuitem"
              onClick={() => { setMenu(null); openDiagnostics(c.id); }}
            >
              <HiOutlineWrenchScrewdriver aria-hidden="true" />
              <span>Diagnostics</span>
            </button>
            <button
              type="button"
              className="conv-menu-item danger"
              role="menuitem"
              onClick={() => { setMenu(null); void confirmDelete(c); }}
            >
              <HiOutlineTrash aria-hidden="true" />
              <span>Delete</span>
            </button>
          </div>
        );
      })()}
    </>
  );
}
