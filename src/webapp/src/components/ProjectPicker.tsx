import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { HiOutlineFolder, HiOutlineFolderPlus, HiOutlineCog6Tooth } from 'react-icons/hi2';
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
  /** Fires with the full active-project metadata each time the picker
   *  resolves which project is active (initial load, user selects another,
   *  newly created project auto-activates). Lets the parent route correctly
   *  between project-shared and conversation-keyed behaviors (e.g. which
   *  diagnostics URL to open from a chat's 3-dot menu). */
  onActiveProjectMetaChange?: (project: ProjectResponse | null) => void;
  /** Bumped by the parent when a fresh fetch is desired (e.g. after creating
   *  a project elsewhere). The picker also refetches on its own internal
   *  mutations. */
  refreshKey?: number;
}

// Compact project picker for the sidebar header. Two affordances:
//   1. A dropdown that lists the user's projects. Selecting one stores it as
//      the active project and tells the parent so conversations can re-scope.
//   2. A "+ New project" button that prompts for a name via window.prompt
//      (intentionally minimal - a full settings drawer is a future enhancement).
export function ProjectPicker({ activeProjectId, onActiveProjectChange, onActiveProjectMetaChange, refreshKey }: ProjectPickerProps) {
  const navigate = useNavigate();
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
        // First boot - if nothing is selected yet, default to the first
        // project (which will be Default for a fresh user).
        if (!activeProjectId && list.length > 0) {
          const next = list[0];
          persistActiveProjectId(next.id);
          onActiveProjectChange(next.id);
          onActiveProjectMetaChange?.(next);
        } else if (activeProjectId && !list.find(p => p.id === activeProjectId)) {
          // Active project was deleted? Fall back to the first available.
          const next = list[0] ?? null;
          persistActiveProjectId(next?.id ?? null);
          onActiveProjectChange(next?.id ?? null);
          onActiveProjectMetaChange?.(next);
        } else if (activeProjectId) {
          // Already-selected project - re-emit metadata so parent stays in sync
          // when the list refreshes (name/isDefault could have changed).
          const current = list.find(p => p.id === activeProjectId) ?? null;
          onActiveProjectMetaChange?.(current);
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
    onActiveProjectMetaChange?.(projects.find(p => p.id === id) ?? null);
  };

  const activeProject = projects.find(p => p.id === activeProjectId) ?? null;

  const handleNew = async () => {
    const name = window.prompt('New project name:')?.trim();
    if (!name) return;
    try {
      const project = await ProjectsService.postApiProjects({ requestBody: { name } });
      setLocalRefresh(n => n + 1);
      // Auto-activate the freshly created project.
      persistActiveProjectId(project.id);
      onActiveProjectChange(project.id);
      onActiveProjectMetaChange?.(project);
    } catch (e: unknown) {
      notifyError(e, 'Failed to create project.');
    }
  };

  const openSettings = () => {
    if (!activeProject) return;
    navigate(`/p/${encodeURIComponent(activeProject.id)}/settings`);
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
      {activeProject && !activeProject.isDefault && (
        <button
          type="button"
          className="project-picker-settings"
          onClick={openSettings}
          title="Project settings"
          aria-label="Project settings"
        >
          <HiOutlineCog6Tooth aria-hidden="true" />
        </button>
      )}
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
