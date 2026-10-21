import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { HiOutlineSparkles } from 'react-icons/hi2';
import { TasksService, type TaskResponse } from '../api/generated';
import { loadActiveProjectId } from '../components/ProjectPicker';
import { notifyError } from '../lib/notify';
import { TaskStatusChip, relativeTime } from './taskUi';

// Delegate a goal to Gabriel. Creating drafts a contract (one model call, a
// few seconds); the detail page is where it gets reviewed and approved.
// Scoped to the sidebar's active project, like the conversation list.
export function TasksPage() {
  const navigate = useNavigate();
  const [tasks, setTasks] = useState<TaskResponse[] | null>(null);
  const [goal, setGoal] = useState('');
  const [maxSteps, setMaxSteps] = useState('');
  const [maxMinutes, setMaxMinutes] = useState('');
  const [creating, setCreating] = useState(false);
  const projectId = loadActiveProjectId();

  useEffect(() => {
    let cancelled = false;
    const load = () =>
      TasksService.getApiTasks({ projectId: projectId ?? undefined })
        .then(list => { if (!cancelled) setTasks(list); })
        .catch((e: unknown) => { if (!cancelled) notifyError(e, 'Failed to load tasks.'); });
    void load();
    // Running tasks advance in the background; keep the list's step counts live.
    const timer = window.setInterval(() => void load(), 5000);
    return () => { cancelled = true; window.clearInterval(timer); };
  }, [projectId]);

  const create = async (e: FormEvent) => {
    e.preventDefault();
    if (!goal.trim() || creating) return;
    setCreating(true);
    try {
      const task = await TasksService.postApiTasks({
        requestBody: {
          goal: goal.trim(),
          projectId,
          maxSteps: maxSteps ? Number(maxSteps) : null,
          maxDurationMinutes: maxMinutes ? Number(maxMinutes) : null,
        },
      });
      navigate(`/tasks/${encodeURIComponent(task.id)}`);
    } catch (err: unknown) {
      notifyError(err, 'Failed to create the task.');
      setCreating(false);
    }
  };

  return (
    <div className="settings tasks">
      <div className="settings-head">
        <h1 className="settings-title">Tasks</h1>
      </div>

      <form className="settings-section task-new" onSubmit={create}>
        <h2 className="settings-section-title">Delegate a task</h2>
        <p className="settings-hint">
          Describe the outcome you want. Gabriel drafts a contract — objective, done-criteria,
          constraints — for you to review before it starts working unattended. Results land in this
          project's files.
        </p>
        <textarea
          className="task-input"
          rows={4}
          value={goal}
          onChange={e => setGoal(e.target.value)}
          placeholder="e.g. Research the three best sourdough starter methods and write a one-page comparison guide."
          disabled={creating}
        />
        <div className="task-new-row">
          <label className="task-budget">
            <span>Max steps</span>
            <input type="number" min={1} max={500} placeholder="40" value={maxSteps}
              onChange={e => setMaxSteps(e.target.value)} disabled={creating} />
          </label>
          <label className="task-budget">
            <span>Max minutes</span>
            <input type="number" min={1} max={1440} placeholder="180" value={maxMinutes}
              onChange={e => setMaxMinutes(e.target.value)} disabled={creating} />
          </label>
          <button type="submit" className="task-button task-button-primary" disabled={!goal.trim() || creating}>
            <HiOutlineSparkles aria-hidden="true" />
            <span>{creating ? 'Drafting contract…' : 'Draft contract'}</span>
          </button>
        </div>
      </form>

      <section className="settings-section">
        <h2 className="settings-section-title">This project's tasks</h2>
        {tasks === null && <p className="settings-hint">Loading…</p>}
        {tasks?.length === 0 && <p className="settings-hint">No tasks yet.</p>}
        {tasks && tasks.length > 0 && (
          <ul className="task-list">
            {tasks.map(t => (
              <li key={t.id}>
                <button type="button" className="task-row" onClick={() => navigate(`/tasks/${encodeURIComponent(t.id)}`)}>
                  <TaskStatusChip status={t.status} />
                  <span className="task-row-title">{t.objective || t.goal}</span>
                  <span className="task-row-meta">
                    {t.status !== 'draft' && `${t.stepsCompleted}/${t.maxSteps} steps · `}
                    {relativeTime(t.updatedAt)}
                  </span>
                </button>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
