import { useCallback, useEffect, useRef, useState, type FormEvent, type ChangeEvent } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  HiOutlineArrowLeft,
  HiOutlineArrowUpTray,
  HiOutlineTrash,
  HiOutlineDocument,
  HiOutlineArrowsRightLeft,
} from 'react-icons/hi2';
import {
  ApiError,
  ProjectsService,
  ProjectFilesService,
  type ProjectResponse,
  type ProjectFileResponse,
} from '../api/generated';
import { notifyError } from '../lib/notify';
import { toast } from 'react-toastify';
import { SkinPicker } from '../components/SkinPicker';

// Project-scoped settings: name + description, the appended system prompt,
// the file library, and a deep-link to the project's shared diagnostics. The
// Default project still has a system prompt (it acts as the catch-all
// personality bucket) but no file UI — file storage is a "real project"
// affordance per product framing.

export function ProjectSettingsPage() {
  const { projectId = '' } = useParams<{ projectId: string }>();
  const navigate = useNavigate();

  const [project, setProject] = useState<ProjectResponse | null>(null);
  const [files, setFiles] = useState<ProjectFileResponse[]>([]);
  const [error, setError] = useState<string | null>(null);

  // Form state — lifted out of `project` so dirty-checking + cancel-vs-save
  // are trivial.
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [systemPrompt, setSystemPrompt] = useState('');
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [skinSaving, setSkinSaving] = useState(false);
  const [rerolling, setRerolling] = useState(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const loadProject = useCallback(async (signal?: AbortSignal) => {
    try {
      const p = await ProjectsService.getApiProjects1({ id: projectId });
      if (signal?.aborted) return;
      setProject(p);
      setName(p.name);
      setDescription(p.description ?? '');
      setSystemPrompt(p.systemPrompt ?? '');
      setFiles(p.files ?? []);
    } catch (e) {
      if (signal?.aborted) return;
      if (e instanceof ApiError && e.status === 404) {
        setError('Project not found.');
        return;
      }
      notifyError(e);
    }
  }, [projectId]);

  useEffect(() => {
    if (!projectId) return;
    const ctrl = new AbortController();
    void loadProject(ctrl.signal);
    return () => ctrl.abort();
  }, [projectId, loadProject]);

  const dirty = !!project && (
    name !== project.name
    || description !== (project.description ?? '')
    || systemPrompt !== (project.systemPrompt ?? '')
  );

  const onSave = async (e: FormEvent) => {
    e.preventDefault();
    if (!project || !dirty || saving) return;
    setSaving(true);
    try {
      // PATCH semantics: send only what changed so explicit empty-string
      // edits (clear description / clear prompt) don't get bulldozed by
      // unrelated dirty fields.
      const updated = await ProjectsService.patchApiProjects({
        id: projectId,
        requestBody: {
          name: name !== project.name ? name : undefined,
          description: description !== (project.description ?? '') ? description : undefined,
          systemPrompt: systemPrompt !== (project.systemPrompt ?? '') ? systemPrompt : undefined,
        },
      });
      setProject(prev => prev ? { ...prev, ...updated } : updated);
      toast.success('Project saved.');
    } catch (err) {
      notifyError(err);
    } finally {
      setSaving(false);
    }
  };

  const onUpload = async (e: ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0];
    // Reset input value so re-selecting the same file fires onChange again.
    e.target.value = '';
    if (!f) return;
    setUploading(true);
    try {
      const created = await ProjectFilesService.postApiProjectsFiles({
        projectId,
        formData: { file: f },
      });
      setFiles(prev => [...prev, created]);
      toast.success(`Uploaded ${created.name}.`);
    } catch (err) {
      notifyError(err);
    } finally {
      setUploading(false);
    }
  };

  const onDeleteFile = async (file: ProjectFileResponse) => {
    if (!confirm(`Delete "${file.name}"?`)) return;
    try {
      await ProjectFilesService.deleteApiProjectsFiles({ projectId, fileId: file.id });
      setFiles(prev => prev.filter(f => f.id !== file.id));
    } catch (err) {
      notifyError(err);
    }
  };

  // Skin save — fired immediately on picker change so the user sees the
  // sequence update without an explicit "save skin" button. Concurrent saves
  // are gated by `skinSaving` to avoid a race against a fast user.
  const saveSkin = async (pattern: string | null, palette: string | null) => {
    if (skinSaving) return;
    setSkinSaving(true);
    try {
      const updated = await ProjectsService.putApiProjectsSkin({
        id: projectId,
        requestBody: { pattern, palette },
      });
      setProject(prev => prev ? { ...prev, ...updated } : updated);
    } catch (err) {
      notifyError(err);
    } finally {
      setSkinSaving(false);
    }
  };

  // Reroll the project's AvatarSeed. Pinned pattern / palette overrides survive
  // — reroll only changes the seed-derived dimensions (pattern parameters,
  // palette when no override pinned).
  const rerollSeed = async () => {
    if (rerolling) return;
    setRerolling(true);
    try {
      const updated = await ProjectsService.postApiProjectsAvatarReroll({ id: projectId });
      setProject(prev => prev ? { ...prev, ...updated } : updated);
      toast.success('Seed rerolled.');
    } catch (err) {
      notifyError(err);
    } finally {
      setRerolling(false);
    }
  };

  if (error) {
    return (
      <div className="settings">
        <div className="settings-head">
          <button type="button" className="diagnostics-back" onClick={() => navigate('/')}>
            <HiOutlineArrowLeft aria-hidden="true" />
            <span>Back</span>
          </button>
        </div>
        <div className="error">{error}</div>
      </div>
    );
  }

  if (!project) {
    return <div className="settings"><div className="settings-loading">Loading…</div></div>;
  }

  return (
    <div className="settings palette-scope">
      <div className="settings-head">
        <button type="button" className="diagnostics-back" onClick={() => navigate(-1)}>
          <HiOutlineArrowLeft aria-hidden="true" />
          <span>Back</span>
        </button>
        <h1 className="settings-title">
          Project settings
          <span className="settings-scope"> · {project.name}</span>
        </h1>
      </div>

      <form className="settings-section" onSubmit={onSave}>
        <h2 className="settings-section-title">Identity</h2>
        <label className="settings-field">
          <span>Name</span>
          <input
            type="text"
            value={name}
            onChange={e => setName(e.target.value)}
            maxLength={128}
            required
          />
        </label>
        <label className="settings-field">
          <span>Description</span>
          <textarea
            value={description}
            onChange={e => setDescription(e.target.value)}
            maxLength={2048}
            rows={2}
            placeholder="Optional — what's this project for?"
          />
        </label>

        <h2 className="settings-section-title">System prompt</h2>
        <p className="settings-hint">
          Appended to Gabriel's base persona for every chat in this project. Use it to
          steer tone, expertise, or constraints — leave blank to use the base persona only.
        </p>
        <label className="settings-field">
          <textarea
            value={systemPrompt}
            onChange={e => setSystemPrompt(e.target.value)}
            rows={10}
            placeholder="e.g. You're embedded in a TypeScript codebase. Prefer functional patterns; cite file paths in answers."
          />
        </label>

        <div className="settings-actions">
          <button
            type="submit"
            className="settings-primary"
            disabled={!dirty || saving}
          >
            {saving ? 'Saving…' : 'Save changes'}
          </button>
          {dirty && (
            <button
              type="button"
              className="settings-secondary"
              onClick={() => {
                setName(project.name);
                setDescription(project.description ?? '');
                setSystemPrompt(project.systemPrompt ?? '');
              }}
              disabled={saving}
            >
              Discard
            </button>
          )}
        </div>
      </form>

      <section className="settings-section">
        <h2 className="settings-section-title">Skin</h2>
        <p className="settings-hint">
          Pin the avatar's pattern + palette to a specific pick, or leave on <code>Auto</code>
          to let the seed choose. Reroll picks a fresh seed without changing pinned dimensions.
        </p>
        <SkinPicker
          pattern={project.patternOverride ?? null}
          palette={project.paletteOverride ?? null}
          disabled={skinSaving || rerolling}
          onChange={({ pattern, palette }) => void saveSkin(pattern, palette)}
          onReroll={() => void rerollSeed()}
        />
        <div className="settings-meta-row">
          <span className="settings-mono">seed {project.avatarSeed}</span>
          {skinSaving && <span className="settings-faint">Saving…</span>}
          {rerolling && <span className="settings-faint">Rerolling…</span>}
        </div>
      </section>

      {!project.isDefault && (
        <section className="settings-section">
          <h2 className="settings-section-title">Files</h2>
          <p className="settings-hint">
            Project files are visible to Gabriel through the project-scoped tools
            (<code>list_project_files</code>, <code>read_project_file</code>) so the
            agent can reference them in answers without copy-paste.
          </p>

          <div className="settings-actions">
            <button
              type="button"
              className="settings-primary"
              onClick={() => fileInputRef.current?.click()}
              disabled={uploading}
            >
              <HiOutlineArrowUpTray aria-hidden="true" />
              <span>{uploading ? 'Uploading…' : 'Upload file'}</span>
            </button>
            <input
              ref={fileInputRef}
              type="file"
              hidden
              onChange={onUpload}
            />
          </div>

          {files.length === 0 ? (
            <div className="settings-empty">No files yet. Upload one to get started.</div>
          ) : (
            <ul className="settings-files">
              {files.map(f => (
                <li key={f.id} className="settings-file">
                  <HiOutlineDocument aria-hidden="true" className="settings-file-icon" />
                  <div className="settings-file-body">
                    <span className="settings-file-name">{f.name}</span>
                    <span className="settings-file-meta">
                      {formatBytes(f.sizeBytes)} · {new Date(f.uploadedAt).toLocaleString()}
                    </span>
                  </div>
                  <button
                    type="button"
                    className="settings-file-delete"
                    onClick={() => void onDeleteFile(f)}
                    aria-label={`Delete ${f.name}`}
                    title="Delete"
                  >
                    <HiOutlineTrash aria-hidden="true" />
                  </button>
                </li>
              ))}
            </ul>
          )}
        </section>
      )}

      {!project.isDefault && (
        <section className="settings-section">
          <h2 className="settings-section-title">Diagnostics</h2>
          <p className="settings-hint">
            Inspect this project's shared Gabriel Sequence — the 64-frame avatar
            every chat in the project renders.
          </p>
          <Link to={`/p/${encodeURIComponent(projectId)}/diagnostics`} className="settings-secondary">
            <HiOutlineArrowsRightLeft aria-hidden="true" />
            <span>Open diagnostics</span>
          </Link>
        </section>
      )}
    </div>
  );
}

// File size formatting — picks the largest unit that keeps the value <1024.
function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
  return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`;
}
