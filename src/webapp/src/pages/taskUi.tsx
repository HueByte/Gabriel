// Small pieces shared by the task list and task detail pages.

const STATUS_LABELS: Record<string, string> = {
  draft: 'Draft',
  running: 'Running',
  paused: 'Paused',
  blocked: 'Needs you',
  completed: 'Completed',
  failed: 'Failed',
  cancelled: 'Cancelled',
};

export function TaskStatusChip({ status }: { status: string }) {
  return <span className={`task-status task-status-${status}`}>{STATUS_LABELS[status] ?? status}</span>;
}

export function isTerminal(status: string): boolean {
  return status === 'completed' || status === 'failed' || status === 'cancelled';
}

// One item per non-empty line - how the contract lists are edited.
export function parseLines(text: string): string[] {
  return text.split('\n').map(l => l.trim()).filter(l => l.length > 0);
}

export function relativeTime(iso: string): string {
  const seconds = Math.round((Date.now() - new Date(iso).getTime()) / 1000);
  if (seconds < 45) return 'just now';
  const minutes = Math.round(seconds / 60);
  if (minutes < 60) return `${minutes} min ago`;
  const hours = Math.round(minutes / 60);
  if (hours < 24) return `${hours} h ago`;
  return new Date(iso).toLocaleDateString();
}

// "1 h 20 min left" / "overdue" for the time budget.
export function timeLeft(deadlineIso: string): string {
  const minutes = Math.round((new Date(deadlineIso).getTime() - Date.now()) / 60000);
  if (minutes <= 0) return 'time budget used up';
  if (minutes < 60) return `${minutes} min left`;
  return `${Math.floor(minutes / 60)} h ${minutes % 60} min left`;
}
