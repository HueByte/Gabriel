import { useCallback, useEffect, useRef, useState } from 'react';
import { saveMemory, type MemoryType } from '../api/memories';

const TYPES: MemoryType[] = ['user', 'feedback', 'project', 'reference'];

interface MemoryQuickSaveProps {
  // The message body to pre-fill into the memory's `body` field. The user
  // can edit it before saving — it's a starting point, not a commitment.
  seedBody: string;
  // Set when the conversation is inside a real project. When null, the
  // "project" scope option is hidden (user-scope only).
  projectId: string | null;
  onClose: () => void;
}

// Lightweight modal opened from the chat's "Remember this" button. Lets the
// user fill in a memory entry seeded with the message content. Save goes
// straight to POST /api/memories — no agent extraction step (the trigger is
// already user-driven, the agent's own memory_save tool covers the
// implicit / in-conversation path).
export function MemoryQuickSave({ seedBody, projectId, onClose }: MemoryQuickSaveProps) {
  const [scope, setScope] = useState<'user' | 'project'>(projectId ? 'project' : 'user');
  const [type, setType] = useState<MemoryType>('user');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [body, setBody] = useState(seedBody);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const overlayRef = useRef<HTMLDivElement | null>(null);

  // Close on Escape — standard modal affordance.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  const onBackdropClick = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === overlayRef.current) onClose();
  }, [onClose]);

  const onSave = useCallback(async () => {
    if (!name.trim() || !description.trim() || !body.trim()) {
      setError('Name, description, and body must all be non-empty.');
      return;
    }
    setSaving(true);
    setError(null);
    try {
      await saveMemory({
        projectId: scope === 'project' ? projectId : null,
        type,
        name: name.trim(),
        description: description.trim(),
        body: body.trim(),
      });
      onClose();
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSaving(false);
    }
  }, [scope, projectId, type, name, description, body, onClose]);

  return (
    <div ref={overlayRef} className="memory-modal-overlay" onClick={onBackdropClick}>
      <div className="memory-modal" role="dialog" aria-label="Save memory">
        <div className="memory-modal-head">
          <h3>Remember this</h3>
          <button type="button" className="memory-modal-close" onClick={onClose} aria-label="Close">×</button>
        </div>
        <p className="settings-hint">
          Gabriel will see this entry in every future conversation
          {projectId ? ' (or just this project if you pick project scope).' : '.'}
        </p>

        <div className="memory-editor-grid">
          {projectId && (
            <label className="memory-editor-field">
              <span>Scope</span>
              <select value={scope} onChange={e => setScope(e.target.value as 'user' | 'project')}>
                <option value="user">user (everywhere)</option>
                <option value="project">project (this one only)</option>
              </select>
            </label>
          )}
          <label className="memory-editor-field">
            <span>Type</span>
            <select value={type} onChange={e => setType(e.target.value as MemoryType)}>
              {TYPES.map(t => <option key={t} value={t}>{t}</option>)}
            </select>
          </label>
        </div>

        <label className="memory-editor-field">
          <span>Name <span className="settings-faint">(kebab-case, unique)</span></span>
          <input
            type="text"
            value={name}
            onChange={e => setName(e.target.value)}
            placeholder="e.g. prefers-prose"
            autoFocus
          />
        </label>
        <label className="memory-editor-field">
          <span>Description</span>
          <input
            type="text"
            value={description}
            onChange={e => setDescription(e.target.value)}
            placeholder="One-line summary"
          />
        </label>
        <label className="memory-editor-field">
          <span>Body</span>
          <textarea
            value={body}
            onChange={e => setBody(e.target.value)}
            rows={6}
          />
        </label>

        {error && <div className="error">{error}</div>}

        <div className="memory-editor-actions">
          <button type="button" className="memory-editor-cancel" onClick={onClose} disabled={saving}>
            Cancel
          </button>
          <button type="button" className="memory-editor-save" onClick={() => void onSave()} disabled={saving}>
            {saving ? 'Saving…' : 'Save memory'}
          </button>
        </div>
      </div>
    </div>
  );
}
