import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  HiOutlineCheck,
  HiOutlineChevronDown,
  HiOutlineCog6Tooth,
  HiOutlineFolder,
  HiOutlineFolderPlus,
} from 'react-icons/hi2';
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

// Anchor coords for the floating dropdown menu. Fixed-position coordinates
// computed from the trigger's bounding rect so the menu escapes the sidebar's
// overflow clipping and stays glued to the button on scroll/resize close.
interface MenuAnchor {
  top: number;
  left: number;
  width: number;
}

// Compact project picker for the sidebar header. Two affordances:
//   1. A custom dropdown that lists the user's projects. Selecting one stores
//      it as the active project and tells the parent so conversations can
//      re-scope. Uses a fixed-position floating menu (same pattern as the
//      conversation-row 3-dot menu) so its styling is fully under our control
//      instead of inheriting the OS-native <select> look.
//   2. A "+ New project" button that prompts for a name via window.prompt
//      (intentionally minimal - a full settings drawer is a future enhancement).
export function ProjectPicker({ activeProjectId, onActiveProjectChange, onActiveProjectMetaChange, refreshKey }: ProjectPickerProps) {
  const navigate = useNavigate();
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [localRefresh, setLocalRefresh] = useState(0);
  const [menu, setMenu] = useState<MenuAnchor | null>(null);
  const triggerRef = useRef<HTMLButtonElement | null>(null);
  const menuRef = useRef<HTMLDivElement | null>(null);

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

  // Close on outside click, Escape, scroll, and resize - same lifecycle as the
  // conversation-row menu in Sidebar. Without the scroll/resize listeners the
  // fixed-position menu would drift away from its anchor.
  useEffect(() => {
    if (!menu) return;
    const close = () => setMenu(null);
    const onDown = (e: globalThis.MouseEvent) => {
      const target = e.target as Element | null;
      if (!target) return;
      if (menuRef.current?.contains(target)) return;
      if (triggerRef.current?.contains(target)) return;
      close();
    };
    const onKey = (e: globalThis.KeyboardEvent) => {
      if (e.key === 'Escape') close();
    };
    document.addEventListener('mousedown', onDown);
    document.addEventListener('keydown', onKey);
    window.addEventListener('resize', close);
    // Capture phase so any scrolling ancestor closes the menu.
    window.addEventListener('scroll', close, true);
    return () => {
      document.removeEventListener('mousedown', onDown);
      document.removeEventListener('keydown', onKey);
      window.removeEventListener('resize', close);
      window.removeEventListener('scroll', close, true);
    };
  }, [menu]);

  const toggleMenu = () => {
    if (menu) { setMenu(null); return; }
    const btn = triggerRef.current;
    if (!btn) return;
    const rect = btn.getBoundingClientRect();
    setMenu({ top: rect.bottom + 4, left: rect.left, width: rect.width });
  };

  const selectProject = (id: string) => {
    persistActiveProjectId(id);
    onActiveProjectChange(id);
    onActiveProjectMetaChange?.(projects.find(p => p.id === id) ?? null);
    setMenu(null);
  };

  const activeProject = projects.find(p => p.id === activeProjectId) ?? null;
  const triggerLabel = activeProject?.name ?? (loading ? 'Loading…' : 'No projects');

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
      <button
        ref={triggerRef}
        type="button"
        className={`project-picker-select${menu ? ' open' : ''}`}
        disabled={loading || projects.length === 0}
        onClick={toggleMenu}
        aria-haspopup="listbox"
        aria-expanded={!!menu}
        aria-label="Active project"
        title={triggerLabel}
      >
        <span className="project-picker-select-text">{triggerLabel}</span>
        <HiOutlineChevronDown className="project-picker-caret" aria-hidden="true" />
      </button>
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

      {menu && (
        <div
          ref={menuRef}
          className="project-picker-menu"
          role="listbox"
          style={{ top: menu.top, left: menu.left, minWidth: menu.width }}
        >
          {projects.map(p => {
            const isActive = p.id === activeProjectId;
            return (
              <button
                key={p.id}
                type="button"
                role="option"
                aria-selected={isActive}
                className={`project-picker-menu-item${isActive ? ' active' : ''}`}
                onClick={() => selectProject(p.id)}
                title={p.name}
              >
                <HiOutlineFolder aria-hidden="true" />
                <span className="project-picker-menu-item-name">{p.name}</span>
                {isActive && <HiOutlineCheck className="project-picker-menu-item-check" aria-hidden="true" />}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
