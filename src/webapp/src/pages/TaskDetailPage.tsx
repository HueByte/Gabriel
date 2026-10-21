import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  HiOutlineArrowLeft,
  HiOutlineChatBubbleLeftRight,
  HiOutlinePause,
  HiOutlinePlay,
  HiOutlineXMark,
} from 'react-icons/hi2';
import { ApiError, TasksService, type TaskResponse } from '../api/generated';
import { notifyError } from '../lib/notify';
import { TaskStatusChip, isTerminal, parseLines, relativeTime, timeLeft } from './taskUi';

// One task: review + approve its contract while it's a draft, then watch it
// work. Polls while the worker is advancing it; the full transcript of what
// Gabriel did lives in the task's conversation (linked), this page is the
// progress summary.
export function TaskDetailPage() {
  const { taskId = '' } = useParams<{ taskId: string }>();
  const navigate = useNavigate();
  const [task, setTask] = useState<TaskResponse | null>(null);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    try {
      setTask(await TasksService.getApiTasks1({ id: taskId }));
    } catch (e: unknown) {
      if (e instanceof ApiError && e.status === 404) navigate('/tasks', { replace: true });
      else notifyError(e, 'Failed to load the task.');
    }
  }, [taskId, navigate]);

  useEffect(() => { void load(); }, [load]);

  // Poll only while the worker is moving it - nothing else changes on its own.
  const running = task?.status === 'running';
  useEffect(() => {
    if (!running) return;
    const timer = window.setInterval(() => void load(), 2000);
    return () => window.clearInterval(timer);
  }, [running, load]);

  // Every mutation returns the updated task; one helper keeps busy/error handling uniform.
  const act = async (call: () => Promise<TaskResponse>) => {
    setBusy(true);
    try {
      setTask(await call());
    } catch (e: unknown) {
      notifyError(e);
    } finally {
      setBusy(false);
    }
  };

  if (!task) {
    return <div className="settings tasks"><p className="settings-hint">Loading…</p></div>;
  }

  const id = task.id;
  const cancel = () => {
    if (window.confirm('Cancel this task? It stops for good; files it wrote stay.')) {
      void act(() => TasksService.postApiTasksCancel({ id }));
    }
  };

  return (
    <div className="settings tasks">
      <div className="settings-head task-head">
        <button type="button" className="diagnostics-back" onClick={() => navigate('/tasks')}>
          <HiOutlineArrowLeft aria-hidden="true" />
          <span>Tasks</span>
        </button>
        <TaskStatusChip status={task.status} />
        <div className="task-head-actions">
          {task.status !== 'draft' && (
            <button type="button" className="task-button" onClick={() => navigate(`/c/${encodeURIComponent(task.conversationId)}`)}>
              <HiOutlineChatBubbleLeftRight aria-hidden="true" />
              <span>Conversation</span>
            </button>
          )}
          {task.status === 'running' && (
            <button type="button" className="task-button" disabled={busy}
              onClick={() => void act(() => TasksService.postApiTasksPause({ id }))}>
              <HiOutlinePause aria-hidden="true" />
              <span>Pause</span>
            </button>
          )}
          {task.status === 'paused' && (
            <button type="button" className="task-button" disabled={busy}
              onClick={() => void act(() => TasksService.postApiTasksResume({ id }))}>
              <HiOutlinePlay aria-hidden="true" />
              <span>Resume</span>
            </button>
          )}
          {!isTerminal(task.status) && (
            <button type="button" className="task-button task-button-danger" disabled={busy} onClick={cancel}>
              <HiOutlineXMark aria-hidden="true" />
              <span>Cancel</span>
            </button>
          )}
        </div>
      </div>

      {task.status === 'draft'
        ? <ContractEditor task={task} busy={busy} onAct={act} />
        : <ContractView task={task} />}

      {task.status !== 'draft' && <StatusPanel task={task} busy={busy} onAct={act} />}

      {(task.log?.length ?? 0) > 0 && (
        <section className="settings-section">
          <h2 className="settings-section-title">Progress log</h2>
          <ol className="task-log">
            {task.log!.map((e, i) => (
              <li key={i} className={`task-log-entry task-log-${e.kind}`}>
                <span className="task-log-kind">{e.kind}</span>
                <span className="task-log-text">{e.text}</span>
                <time className="task-log-time" dateTime={e.createdAt}>{relativeTime(e.createdAt)}</time>
              </li>
            ))}
          </ol>
        </section>
      )}
    </div>
  );
}

type Act = (call: () => Promise<TaskResponse>) => Promise<void>;

// Draft stage: the contract is what every step and every review is judged
// against, so it is editable here and frozen once approved.
function ContractEditor({ task, busy, onAct }: { task: TaskResponse; busy: boolean; onAct: Act }) {
  const [objective, setObjective] = useState(task.objective);
  const [criteria, setCriteria] = useState(task.doneCriteria.join('\n'));
  const [constraints, setConstraints] = useState(task.constraints.join('\n'));
  const id = task.id;

  const contract = () => ({
    objective: objective.trim(),
    doneCriteria: parseLines(criteria),
    constraints: parseLines(constraints),
  });
  const canApprove = objective.trim().length > 0 && parseLines(criteria).length > 0;

  // Save then approve in one click - approving must never run on a stale contract.
  const approve = () => onAct(async () => {
    await TasksService.putApiTasksContract({ id, requestBody: contract() });
    return TasksService.postApiTasksApprove({ id });
  });

  return (
    <section className="settings-section">
      <h2 className="settings-section-title">Review the contract</h2>
      <p className="settings-hint">
        Gabriel works until every done-criterion is met — an independent reviewer checks each one —
        or the budget of {task.maxSteps} steps / {task.maxDurationMinutes} min runs out. Make the
        criteria concrete and checkable.
      </p>
      <label className="task-field">
        <span>Objective</span>
        <textarea className="task-input" rows={2} value={objective} onChange={e => setObjective(e.target.value)} disabled={busy} />
      </label>
      <label className="task-field">
        <span>Done when <em>(one per line)</em></span>
        <textarea className="task-input" rows={4} value={criteria} onChange={e => setCriteria(e.target.value)} disabled={busy} />
      </label>
      <label className="task-field">
        <span>Constraints <em>(one per line, optional)</em></span>
        <textarea className="task-input" rows={3} value={constraints} onChange={e => setConstraints(e.target.value)} disabled={busy} />
      </label>
      <div className="task-actions">
        <button type="button" className="task-button" disabled={busy}
          onClick={() => void onAct(() => TasksService.putApiTasksContract({ id, requestBody: contract() }))}>
          Save draft
        </button>
        <button type="button" className="task-button task-button-primary" disabled={busy || !canApprove} onClick={() => void approve()}>
          <HiOutlinePlay aria-hidden="true" />
          <span>Approve &amp; start</span>
        </button>
      </div>
      {!canApprove && <p className="settings-hint">Add at least one done-criterion to start.</p>}
    </section>
  );
}

function ContractView({ task }: { task: TaskResponse }) {
  return (
    <section className="settings-section">
      <h2 className="settings-section-title">Contract</h2>
      <p className="task-objective">{task.objective}</p>
      <div className="task-contract-lists">
        <div>
          <h3 className="task-subtitle">Done when</h3>
          <ul className="task-bullets">{task.doneCriteria.map((c, i) => <li key={i}>{c}</li>)}</ul>
        </div>
        {task.constraints.length > 0 && (
          <div>
            <h3 className="task-subtitle">Constraints</h3>
            <ul className="task-bullets">{task.constraints.map((c, i) => <li key={i}>{c}</li>)}</ul>
          </div>
        )}
      </div>
    </section>
  );
}

function StatusPanel({ task, busy, onAct }: { task: TaskResponse; busy: boolean; onAct: Act }) {
  const [answer, setAnswer] = useState('');
  const id = task.id;
  const pct = Math.min(100, Math.round((task.stepsCompleted / Math.max(1, task.maxSteps)) * 100));

  return (
    <section className="settings-section">
      <h2 className="settings-section-title">Status</h2>
      <div className="task-budget-bar" role="progressbar" aria-valuenow={task.stepsCompleted} aria-valuemin={0} aria-valuemax={task.maxSteps}>
        <div className="task-budget-fill" style={{ width: `${pct}%` }} />
      </div>
      <p className="settings-hint">
        {task.stepsCompleted} of {task.maxSteps} steps
        {task.status === 'running' && task.deadline && ` · ${timeLeft(task.deadline)}`}
        {task.status === 'running' && ' · working…'}
      </p>

      {task.status === 'blocked' && (
        <div className="task-question">
          <p className="task-question-text">{task.statusReason}</p>
          <textarea className="task-input" rows={3} value={answer} placeholder="Your answer…"
            onChange={e => setAnswer(e.target.value)} disabled={busy} />
          <div className="task-actions">
            <button type="button" className="task-button task-button-primary" disabled={busy || !answer.trim()}
              onClick={() => void onAct(() => TasksService.postApiTasksAnswer({ id, requestBody: { answer: answer.trim() } }))}>
              Answer &amp; continue
            </button>
          </div>
        </div>
      )}

      {(task.status === 'paused' || task.status === 'failed') && task.statusReason && (
        <p className="task-reason">{task.statusReason}</p>
      )}

      {task.status === 'completed' && task.finalSummary && (
        <div className="task-summary">
          <h3 className="task-subtitle">Result</h3>
          <p>{task.finalSummary}</p>
        </div>
      )}
    </section>
  );
}
