namespace Gabriel.API.Contracts.Tasks;

// Wire shape of an autonomous task. Status is lowercase: draft, running,
// paused, blocked, completed, failed, cancelled. StatusReason carries the
// blocking question / pause reason / failure cause. Log is only populated on
// the single-task endpoints.
public sealed record TaskResponse(
    Guid Id,
    Guid ProjectId,
    Guid ConversationId,
    string Goal,
    string Objective,
    IReadOnlyList<string> DoneCriteria,
    IReadOnlyList<string> Constraints,
    string Status,
    string? StatusReason,
    int StepsCompleted,
    int MaxSteps,
    int MaxDurationMinutes,
    DateTimeOffset? Deadline,
    string? FinalSummary,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    IReadOnlyList<TaskLogEntryResponse>? Log);

// Kind: system, user, step, review, error.
public sealed record TaskLogEntryResponse(int? Step, string Kind, string Text, DateTimeOffset CreatedAt);

// Optional budgets fall back to AgentTasks:DefaultMaxSteps / DefaultMaxDurationMinutes.
public sealed record CreateTaskRequest(string Goal, Guid? ProjectId, int? MaxSteps, int? MaxDurationMinutes);

public sealed record UpdateTaskContractRequest(string Objective, IReadOnlyList<string> DoneCriteria, IReadOnlyList<string> Constraints);

public sealed record AnswerTaskRequest(string Answer);
