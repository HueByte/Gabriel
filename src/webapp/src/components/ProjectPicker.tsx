import { useEffect, useState } from 'react';
import { HiOutlineFolder, HiOutlineFolderPlus } from 'react-icons/hi2';
import { ProjectsService, type ProjectResponse } from '../api/generated';
import { notifyError } from '../lib/notify';

const ACTIVE_PROJECT_KEY = 'gabriel.activeProjectId';

export function loadActiveProjectId(): string | null {
  try { return localStorage.getItem(ACTIVE_PROJECT_KEY); }
  catch { return null; }
}

function persistActiveProjectId(id: string | null) {
  try {
    if (id) localStorage.setItem(ACTIVE_PROJECT_KEY, id);
    else localStorage.removeItem(ACTIVE_PROJECT_KEY);
  } catch { /* ignore */ }
}

interface ProjectPickerProps {
  activeProjectId: string | null;
  onActiveProjectChange: (projectId: string | null) => void;
  /** Bumped by the parent when a fresh fetch is desired (e.g. after creating
   *  a project elsewhere). The picker also refetches on its own internal
   *  mutations. */
  refreshKey?: number;
}

// Compact project picker for the sidebar header. Two affordances:
//   1. A dropdown that lists the user's projects. Selecting one stores it as
//      the active project and tells the parent so conversations can re-scope.
//   2. A "+ New project" button that prompts for a name via window.prompt
//      (intentionally minimal — a full settings drawer is a future enhancement).
export function ProjectPicker({ activeProjectId, onActiveProjectChange, refreshKey }: ProjectPickerProps) {
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [localRefresh, setLocalRefresh] = useState(0);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    ProjectsService.getApiProjects()
      .then(list => {
        if (cancelled) return;
        setProjects(list);
        // First boot — if nothing is selected yet, default to the first
        // project (which will be Default for a fresh user).
        if (!activeProjectId && list.length > 0) {
          const next = list[0].id;
          persistActiveProjectId(next);
          onActiveProjectChange(next);
        }
        // Active project was deleted? Fall back to the first available.
        if (activeProjectId && !list.find(p => p.id === activeProjectId)) {
          const next = list[0]?.id ?? null;
          persistActiveProjectId(next);
          onActiveProjectChange(next);
        }
      })
      .catch(e => { if (!cancelled) notifyError(e, 'Failed to load projects.'); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [refreshKey, localRefresh]);

  const selectProject = (id: string) => {
    persistActiveProjectId(id);
    onActiveProjectChange(id);
  };

  const handleNew = async () => {
    const name = window.prompt('New project name:')?.trim();
    if (!name) return;
    try {
      const project = await ProjectsService.postApiProjects({ requestBody: { name } });
      setLocalRefresh(n => n + 1);
      // Auto-activate the freshly created project.
      persistActiveProjectId(project.id);
      onActiveProjectChange(project.id);
    } catch (e: unknown) {
      notifyError(e, 'Failed to create project.');
    }
  };

  return (
    <div className="project-picker">
      <label className="project-picker-label">
        <HiOutlineFolder aria-hidden="true" />
        <span>Project</span>
      </label>
      <select
        className="project-picker-select"
        value={activeProjectId ?? ''}
        disabled={loading}
        onChange={e => selectProject(e.target.value)}
        aria-label="Active project"
      >
        {projects.length === 0 && (
          <option value="" disabled>
            {loading ? 'Loading…' : 'No projects'}
          </option>
        )}
        {projects.map(p => (
          <option key={p.id} value={p.id}>{p.name}</option>
        ))}
      </select>
      <button
        type="button"
        className="project-picker-new"
        onClick={() => void handleNew()}
        title="New project"
        aria-label="New project"
      >
        <HiOutlineFolderPlus aria-hidden="true" />
      </button>
    </div>
  );
}
