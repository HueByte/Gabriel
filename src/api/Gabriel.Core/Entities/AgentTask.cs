using Gabriel.Core.Exceptions;

namespace Gabriel.Core.Entities;

public enum AgentTaskStatus
{
    Draft = 0,       // contract drafted, waiting for the user's approval
    Running = 1,     // the worker is stepping it
    Paused = 2,      // by the user, or a budget ran out - Resume continues
    Blocked = 3,     // needs the user's answer (question in StatusReason)
    Completed = 4,
    Failed = 5,
    Cancelled = 6,
}

public enum TaskCheckpointStatus
{
    Continue = 0,
    Done = 1,
    Blocked = 2,
}

// A unit of delegated, long-running work. The task owns a conversation: the
// background worker advances it by sending step messages into that
// conversation through the normal agent loop, so every turn-level guarantee
// (compaction, stop, interrupted-turn persistence, scoping) applies per step.
//
// The contract (Objective / DoneCriteria / Constraints) is what every step and
// every review is judged against - drafted by the model, approved by the user,
// and frozen once the task starts.
public class AgentTask
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid ConversationId { get; private set; }

    // What the user asked for, verbatim.
    public string Goal { get; private set; } = string.Empty;

    public string Objective { get; private set; } = string.Empty;
    public IReadOnlyList<string> DoneCriteria { get; private set; } = [];
    public IReadOnlyList<string> Constraints { get; private set; } = [];

    public AgentTaskStatus Status { get; private set; } = AgentTaskStatus.Draft;

    // Why the task is in its current non-running state: the blocking question,
    // the pause reason, the failure cause. Null while running.
    public string? StatusReason { get; private set; }

    public int StepsCompleted { get; private set; }
    public int MaxSteps { get; private set; }
    public int MaxDurationMinutes { get; private set; }
    public DateTimeOffset? Deadline { get; private set; }

    public int ConsecutiveNoProgress { get; private set; }
    public int ConsecutiveErrors { get; private set; }

    // Injected into the next step message, then cleared: reviewer feedback or
    // the user's answer to a blocking question.
    public string? PendingGuidance { get; private set; }

    // The model's end-of-step signal (task_checkpoint tool), stamped with the
    // step it belongs to so the runner can tell this step's from a stale one.
    public int? CheckpointStep { get; private set; }
    public TaskCheckpointStatus? CheckpointStatus { get; private set; }
    public string? CheckpointProgress { get; private set; }
    public string? CheckpointQuestion { get; private set; }
    public string? CheckpointSummary { get; private set; }

    public string? FinalSummary { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }

    private readonly List<AgentTaskLogEntry> _log = new();
    public IReadOnlyList<AgentTaskLogEntry> Log => _log.AsReadOnly();

    private AgentTask() { }

    public static AgentTask Create(
        Guid userId, Guid projectId, Guid conversationId, string goal, int maxSteps, int maxDurationMinutes)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (projectId == Guid.Empty) throw new ArgumentException("ProjectId is required.", nameof(projectId));
        if (conversationId == Guid.Empty) throw new ArgumentException("ConversationId is required.", nameof(conversationId));
        if (string.IsNullOrWhiteSpace(goal)) throw new ArgumentException("Goal is required.", nameof(goal));
        if (maxSteps < 1) throw new ArgumentException("MaxSteps must be at least 1.", nameof(maxSteps));
        if (maxDurationMinutes < 1) throw new ArgumentException("MaxDurationMinutes must be at least 1.", nameof(maxDurationMinutes));

        var task = new AgentTask
        {
            UserId = userId,
            ProjectId = projectId,
            ConversationId = conversationId,
            Goal = goal.Trim(),
            Objective = goal.Trim(),
            MaxSteps = maxSteps,
            MaxDurationMinutes = maxDurationMinutes,
        };
        task.AddLog(AgentTaskLogKind.System, "Task created.");
        return task;
    }

    public bool IsTerminal => Status is AgentTaskStatus.Completed or AgentTaskStatus.Failed or AgentTaskStatus.Cancelled;

    public void SetContract(string objective, IEnumerable<string> doneCriteria, IEnumerable<string> constraints)
    {
        RequireStatus(AgentTaskStatus.Draft, "edit the contract of");
        if (string.IsNullOrWhiteSpace(objective))
            throw new ArgumentException("Objective cannot be empty.", nameof(objective));

        Objective = objective.Trim();
        DoneCriteria = Clean(doneCriteria);
        Constraints = Clean(constraints);
        Touch();
    }

    public void Approve(DateTimeOffset now)
    {
        RequireStatus(AgentTaskStatus.Draft, "approve");
        if (DoneCriteria.Count == 0)
            throw new DomainException("Add at least one done-criterion before approving - it's how the task knows when to stop.");

        Status = AgentTaskStatus.Running;
        StatusReason = null;
        StartedAt = now;
        Deadline = now.AddMinutes(MaxDurationMinutes);
        AddLog(AgentTaskLogKind.User, "Contract approved; task started.");
    }

    // True when the next step would exceed a budget. The runner pauses instead
    // of starting it.
    public string? BudgetExhaustedReason(DateTimeOffset now)
    {
        if (StepsCompleted >= MaxSteps) return $"Step budget reached ({StepsCompleted}/{MaxSteps}).";
        if (Deadline is { } d && now >= d) return $"Time budget reached ({MaxDurationMinutes} min).";
        return null;
    }

    public void RecordCheckpoint(TaskCheckpointStatus status, string progress, string? question, string? summary)
    {
        RequireStatus(AgentTaskStatus.Running, "checkpoint");
        CheckpointStep = StepsCompleted + 1;
        CheckpointStatus = status;
        CheckpointProgress = progress.Trim();
        CheckpointQuestion = string.IsNullOrWhiteSpace(question) ? null : question.Trim();
        CheckpointSummary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        Touch();
    }

    // The checkpoint the model recorded during the step about to be finished,
    // or null if it didn't call task_checkpoint this step.
    public TaskCheckpointStatus? CurrentStepCheckpoint
        => CheckpointStep == StepsCompleted + 1 ? CheckpointStatus : null;

    // Consumes the guidance for the step that is starting.
    public string? TakeGuidance()
    {
        var guidance = PendingGuidance;
        PendingGuidance = null;
        return guidance;
    }

    public void CompleteStep(bool progressMade, string? guidance, string logText)
    {
        StepsCompleted++;
        ConsecutiveErrors = 0;
        ConsecutiveNoProgress = progressMade ? 0 : ConsecutiveNoProgress + 1;
        PendingGuidance = string.IsNullOrWhiteSpace(guidance) ? null : guidance.Trim();
        AddLog(AgentTaskLogKind.Step, logText, StepsCompleted);
    }

    public void RecordStepError(string error)
    {
        StepsCompleted++;
        ConsecutiveErrors++;
        AddLog(AgentTaskLogKind.Error, error, StepsCompleted);
    }

    public void Complete(string summary, DateTimeOffset now)
    {
        Status = AgentTaskStatus.Completed;
        StatusReason = null;
        FinalSummary = summary.Trim();
        FinishedAt = now;
        AddLog(AgentTaskLogKind.System, "Completed: " + FinalSummary, LastStep);
    }

    public void Block(string question)
    {
        Status = AgentTaskStatus.Blocked;
        StatusReason = question.Trim();
        AddLog(AgentTaskLogKind.System, "Blocked: " + StatusReason, LastStep);
    }

    public void Fail(string reason, DateTimeOffset now)
    {
        Status = AgentTaskStatus.Failed;
        StatusReason = reason.Trim();
        FinishedAt = now;
        AddLog(AgentTaskLogKind.System, "Failed: " + StatusReason, LastStep);
    }

    public void Pause(string reason, AgentTaskLogKind by = AgentTaskLogKind.User)
    {
        RequireStatus(AgentTaskStatus.Running, "pause");
        Status = AgentTaskStatus.Paused;
        StatusReason = reason.Trim();
        AddLog(by, "Paused: " + StatusReason, LastStep);
    }

    // Resuming past an exhausted budget grants another increment of the same
    // size, so "keep going" is one click rather than a settings change.
    public void Resume(DateTimeOffset now, int extraSteps)
    {
        RequireStatus(AgentTaskStatus.Paused, "resume");
        if (StepsCompleted >= MaxSteps) MaxSteps = StepsCompleted + Math.Max(1, extraSteps);
        if (Deadline is null || now >= Deadline) Deadline = now.AddMinutes(MaxDurationMinutes);
        Status = AgentTaskStatus.Running;
        StatusReason = null;
        AddLog(AgentTaskLogKind.User, "Resumed.", LastStep);
    }

    public void Answer(string answer)
    {
        RequireStatus(AgentTaskStatus.Blocked, "answer");
        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Answer cannot be empty.", nameof(answer));

        PendingGuidance = $"The user answered your question (\"{StatusReason}\"): {answer.Trim()}";
        ConsecutiveNoProgress = 0;
        Status = AgentTaskStatus.Running;
        StatusReason = null;
        AddLog(AgentTaskLogKind.User, "Answered: " + answer.Trim(), LastStep);
    }

    public void Cancel(DateTimeOffset now)
    {
        if (IsTerminal) throw new ConflictException($"Task is already {Status}.");
        Status = AgentTaskStatus.Cancelled;
        StatusReason = "Cancelled by the user.";
        FinishedAt = now;
        AddLog(AgentTaskLogKind.User, "Cancelled.", LastStep);
    }

    public void AddLog(AgentTaskLogKind kind, string text, int? step = null)
    {
        // The log is read back ordered by CreatedAt, and a step's entries are
        // written within the same clock tick - keep timestamps strictly
        // increasing so "step, then its review" can't come back swapped.
        var at = DateTimeOffset.UtcNow;
        if (_log.Count > 0 && at <= _log[^1].CreatedAt) at = _log[^1].CreatedAt.AddTicks(1);

        _log.Add(AgentTaskLogEntry.Create(Id, kind, text, step, at));
        Touch();
    }

    // Step number for lifecycle log lines: the last completed step, none before step 1.
    private int? LastStep => StepsCompleted > 0 ? StepsCompleted : null;

    private void RequireStatus(AgentTaskStatus expected, string action)
    {
        if (Status != expected)
            throw new ConflictException($"Cannot {action} a task that is {Status}.");
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    private static IReadOnlyList<string> Clean(IEnumerable<string> items)
        => items.Select(i => i.Trim()).Where(i => i.Length > 0).ToList();
}

public enum AgentTaskLogKind
{
    System = 0,
    User = 1,
    Step = 2,
    Review = 3,
    Error = 4,
}

// One line of a task's progress log - what the UI shows as the task's history.
public class AgentTaskLogEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TaskId { get; private set; }
    public int? Step { get; private set; }
    public AgentTaskLogKind Kind { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private AgentTaskLogEntry() { }

    internal static AgentTaskLogEntry Create(Guid taskId, AgentTaskLogKind kind, string text, int? step, DateTimeOffset createdAt)
        => new() { TaskId = taskId, Kind = kind, Text = text.Trim(), Step = step, CreatedAt = createdAt };
}
