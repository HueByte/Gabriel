import { useEffect, useRef, useState, type KeyboardEvent } from 'react';
import {
  HiOutlineArrowRightOnRectangle,
  HiOutlineBars3,
  HiOutlinePencilSquare,
  HiOutlinePlus,
  HiOutlineTrash,
  HiOutlineXMark,
} from 'react-icons/hi2';
import { ConversationsService, type ConversationResponse } from '../api/generated';
import { useAuth } from '../auth/AuthContext';
import { notifyError } from '../lib/notify';

interface SidebarProps {
  activeId: string | null;
  /** True = panel is hidden (burger visible). False = panel is open. */
  collapsed: boolean;
  onToggleCollapsed: () => void;
  onSelect: (id: string) => void;
  onNewChat: () => void;
  onRename: (id: string, title: string) => Promise<void> | void;
  onDelete: (id: string) => Promise<void> | void;
  refreshKey: number;
}

export function Sidebar({
  activeId,
  collapsed,
  onToggleCollapsed,
  onSelect,
  onNewChat,
  onRename,
  onDelete,
  refreshKey,
}: SidebarProps) {
  const [conversations, setConversations] = useState<ConversationResponse[]>([]);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editTitle, setEditTitle] = useState('');
  const editInputRef = useRef<HTMLInputElement | null>(null);
  const { user, logout } = useAuth();

  const open = !collapsed;

  useEffect(() => {
    let cancelled = false;
    ConversationsService.getApiConversations()
      .then(list => {
        if (cancelled) return;
        const sorted = [...list].sort((a, b) => b.updatedAt.localeCompare(a.updatedAt));
        setConversations(sorted);
      })
      .catch((e: unknown) => {
        if (!cancelled) notifyError(e, 'Failed to load conversations.');
      });
    return () => { cancelled = true; };
  }, [refreshKey]);

  useEffect(() => {
    if (editingId) editInputRef.current?.select();
  }, [editingId]);

  // Close on Escape when the panel is open.
  useEffect(() => {
    if (!open) return;
    const onKey = (e: globalThis.KeyboardEvent) => {
      if (e.key === 'Escape') onToggleCollapsed();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [open, onToggleCollapsed]);

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
    await onRename(id, title);
  };

  const onEditKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') { e.preventDefault(); void commitRename(); }
    else if (e.key === 'Escape') { e.preventDefault(); cancelRename(); }
  };

  const confirmDelete = (c: ConversationResponse) => {
    const ok = window.confirm(`Delete "${c.title || 'Untitled'}"? This cannot be undone.`);
    if (ok) void onDelete(c.id);
  };

  const handleSelect = (id: string) => {
    onSelect(id);
    onToggleCollapsed();
  };

  const handleNewChat = () => {
    onNewChat();
    onToggleCollapsed();
  };

  return (
    <>
      <button
        type="button"
        className="sidebar-burger"
        onClick={onToggleCollapsed}
        aria-label="Open menu"
        aria-hidden={open}
        tabIndex={open ? -1 : 0}
      >
        <HiOutlineBars3 size={20} aria-hidden="true" />
      </button>

      <div
        className={`sidebar-backdrop${open ? ' visible' : ''}`}
        onClick={onToggleCollapsed}
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
            onClick={onToggleCollapsed}
            aria-label="Close menu"
          >
            <HiOutlineXMark size={18} aria-hidden="true" />
          </button>
        </div>

        <button type="button" className="new-chat" onClick={handleNewChat}>
          <HiOutlinePlus aria-hidden="true" />
          <span>New chat</span>
        </button>

        <div className="sidebar-body">
          {conversations.length === 0 && (
            <div className="sidebar-empty">No conversations yet.</div>
          )}
          <ul className="conv-list">
            {conversations.map(c => (
              <li key={c.id} className={`conv-row${c.id === activeId ? ' active' : ''}`}>
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
                    <div className="conv-actions">
                      <button
                        type="button"
                        className="conv-action"
                        onClick={() => startRename(c)}
                        title="Rename"
                        aria-label="Rename conversation"
                      >
                        <HiOutlinePencilSquare aria-hidden="true" />
                      </button>
                      <button
                        type="button"
                        className="conv-action"
                        onClick={() => confirmDelete(c)}
                        title="Delete"
                        aria-label="Delete conversation"
                      >
                        <HiOutlineTrash aria-hidden="true" />
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
    </>
  );
}
