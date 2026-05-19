import { useCallback, useEffect, useMemo, useState } from 'react';
import { HiOutlinePlus, HiOutlinePencilSquare, HiOutlineTrash } from 'react-icons/hi2';
import {
  deleteMemory,
  listMemories,
  saveMemory,
  type MemoryDto,
  type MemoryScope,
  type MemoryType,
} from '../api/memories';

const TYPES: MemoryType[] = ['user', 'feedback', 'project', 'reference'];

interface MemoryListProps {
  // What scope of memories this list shows + saves into:
  //   - userScope: { kind: 'user' }
  //   - one project: { kind: 'project', projectId }
  // Determines the scope for newly-added entries.
  scope: MemoryScope;
}

// Inline list of memories with add/edit/delete. Dropped into UserSettingsPage
// and ProjectSettingsPage; the scope prop decides what the controller sees on
// both reads and writes.
//
// Editing happens in an inline expander rather than a modal — the page is
// already a vertical scroll so the expander reads cleanly without context
// loss. New entries open the expander with empty fields.
export function MemoryList({ scope }: MemoryListProps) {
  const [entries, setEntries] = useState<MemoryDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState<MemoryDto | 'new' | null>(null);

  const projectId = useMemo(() => {
    if (scope.kind === 'project') return scope.projectId;
    return null;
  }, [scope]);

  const refresh = useCallback(async (signal?: AbortSignal) => {
    try {
      const data = await listMemories(scope, signal);
      setEntries(data);
    } catch (e) {
      if ((e as Error).name !== 'AbortError') setError((e as Error).message);
    }
  }, [scope]);

  useEffect(() => {
    const ac = new AbortController();
    void refresh(ac.signal);
    return () => ac.abort();
  }, [refresh]);

  const onDelete = useCallback(async (id: string) => {
    if (!confirm('Delete this memory? Gabriel will forget it on the next message.')) return;
    try {
      await deleteMemory(id);
      await refresh();
    } catch (e) {
      setError((e as Error).message);
    }
  }, [refresh]);

  const onSaved = useCallback(async () => {
    setEditing(null);
    await refresh();
  }, [refresh]);

  if (entries === null && error === null) {
    return <div className="settings-loading">Loading memories…</div>;
  }
  if (error && entries === null) {
    return <div className="error">Failed to load memories: {error}</div>;
  }

  return (
    <div className="memory-list">
      <div className="memory-list-head">
        <p className="settings-hint">
          Things Gabriel remembers across conversations. Saved automatically when
          you give feedback or share durable context — or add one here.
        </p>
        <button
          type="button"
          className="memory-list-add"
          onClick={() => setEditing('new')}
          disabled={editing !== null}
        >
          <HiOutlinePlus aria-hidden="true" />
          <span>Add memory</span>
        </button>
      </div>

      {editing === 'new' && (
        <MemoryEditor
          projectId={projectId}
          onSaved={onSaved}
          onCancel={() => setEditing(null)}
        />
      )}

      {entries && entries.length === 0 && editing !== 'new' && (
        <div className="memory-empty">
          No memories saved yet. Gabriel will save things as the conversation
          goes — or you can add one explicitly.
        </div>
      )}

      <ul className="memory-items">
        {entries?.map(m => (
          <li key={m.id} className="memory-item">
            {editing && editing !== 'new' && editing.id === m.id ? (
              <MemoryEditor
                projectId={projectId}
                initial={m}
                onSaved={onSaved}
                onCancel={() => setEditing(null)}
              />
            ) : (
              <MemoryRow
                memory={m}
                onEdit={() => setEditing(m)}
                onDelete={() => void onDelete(m.id)}
              />
            )}
          </li>
        ))}
      </ul>

      {error && <div className="error">{error}</div>}
    </div>
  );
}

function MemoryRow({
  memory,
  onEdit,
  onDelete,
}: {
  memory: MemoryDto;
  onEdit: () => void;
  onDelete: () => void;
}) {
  return (
    <div className="memory-row">
      <div className="memory-row-head">
        <span className={`memory-type memory-type-${memory.type}`}>{memory.type}</span>
        <span className="memory-name">{memory.name}</span>
        <span className="memory-row-actions">
          <button type="button" className="memory-row-action" onClick={onEdit} aria-label="Edit">
            <HiOutlinePencilSquare aria-hidden="true" />
          </button>
          <button type="button" className="memory-row-action memory-row-danger" onClick={onDelete} aria-label="Delete">
            <HiOutlineTrash aria-hidden="true" />
          </button>
        </span>
      </div>
      <p className="memory-description">{memory.description}</p>
      <pre className="memory-body">{memory.body}</pre>
    </div>
  );
}

interface MemoryEditorProps {
  projectId: string | null;
  initial?: MemoryDto;
  onSaved: () => void;
  onCancel: () => void;
}

// Inline form: type / name / description / body. Used for both create and
// update — the API treats them as the same upsert call.
export function MemoryEditor({ projectId, initial, onSaved, onCancel }: MemoryEditorProps) {
  const [type, setType] = useState<MemoryType>(initial?.type ?? 'user');
  const [name, setName] = useState(initial?.name ?? '');
  const [description, setDescription] = useState(initial?.description ?? '');
  const [body, setBody] = useState(initial?.body ?? '');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSave = useCallback(async () => {
    if (!name.trim() || !description.trim() || !body.trim()) {
      setError('Name, description, and body must all be non-empty.');
      return;
    }
    setSaving(true);
    setError(null);
    try {
      await saveMemory({
        projectId,
        type,
        name: name.trim(),
        description: description.trim(),
        body: body.trim(),
      });
      onSaved();
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSaving(false);
    }
  }, [name, description, body, type, projectId, onSaved]);

  return (
    <div className="memory-editor">
      <div className="memory-editor-grid">
        <label className="memory-editor-field">
          <span>Type</span>
          <select value={type} onChange={e => setType(e.target.value as MemoryType)}>
            {TYPES.map(t => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>
        </label>
        <label className="memory-editor-field">
          <span>Name <span className="settings-faint">(kebab-case, unique)</span></span>
          <input
            type="text"
            value={name}
            onChange={e => setName(e.target.value)}
            placeholder="e.g. prefers-prose"
            disabled={!!initial}
          />
        </label>
      </div>
      <label className="memory-editor-field">
        <span>Description <span className="settings-faint">(one-line summary)</span></span>
        <input
          type="text"
          value={description}
          onChange={e => setDescription(e.target.value)}
          placeholder="What this entry is about, in one line"
        />
      </label>
      <label className="memory-editor-field">
        <span>Body</span>
        <textarea
          value={body}
          onChange={e => setBody(e.target.value)}
          rows={6}
          placeholder="The actual content. For feedback/project entries: rule, then **Why:** and **How to apply:** lines."
        />
      </label>

      {error && <div className="error">{error}</div>}

      <div className="memory-editor-actions">
        <button type="button" className="memory-editor-cancel" onClick={onCancel} disabled={saving}>
          Cancel
        </button>
        <button type="button" className="memory-editor-save" onClick={() => void onSave()} disabled={saving}>
          {saving ? 'Saving…' : initial ? 'Update' : 'Save memory'}
        </button>
      </div>
    </div>
  );
}
