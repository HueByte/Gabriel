using Gabriel.Core.Entities;

namespace Gabriel.Engine.Tasks;

// What a finished step means for the task, as a pure function of the task's
// state, the model's checkpoint, and the reviewer's verdict. Kept apart from
// AgentTaskRunner (which owns the I/O) so the rules that decide whether an
// unattended run stops, continues, or asks for help are directly testable.
public static class TaskStepPolicy
{
    // The agent asked for help: the user, not the reviewer, decides next, so
    // no review is needed for this step.
    public static bool NeedsReview(AgentTask task) => task.CurrentStepCheckpoint != TaskCheckpointStatus.Blocked;

    // Applies the outcome of step `step`. `review` may only be null when
    // NeedsReview is false. `lastAssistantText` backs the log line when the
    // model didn't report.
    public static void Apply(
        AgentTask task,
        int step,
        StepReview? review,
        string? lastAssistantText,
        int maxStepsWithoutProgress,
        DateTimeOffset now)
    {
        var checkpoint = task.CurrentStepCheckpoint;
        var report = checkpoint is null ? null : task.CheckpointProgress;
        var stepLog = $"Step {step}: {report ?? Shorten(lastAssistantText) ?? "(no report)"}";

        if (checkpoint == TaskCheckpointStatus.Blocked)
        {
            task.CompleteStep(progressMade: true, guidance: null, stepLog);
            task.Block(task.CheckpointQuestion ?? "The agent needs your input to continue.");
            return;
        }

        ArgumentNullException.ThrowIfNull(review);

        if (checkpoint == TaskCheckpointStatus.Done)
        {
            if (review.Done)
            {
                task.CompleteStep(progressMade: true, guidance: null, stepLog);
                LogReview(task, step, review);
                task.Complete(task.CheckpointSummary ?? report ?? "Done.", now);
                return;
            }

            // Claimed done, not confirmed (or the review was unreadable): keep
            // going with the objection as the next step's brief. Counts as
            // progress - the agent did finish something - so a persistent
            // disagreement surfaces through the step budget, not the stuck guard.
            var objection = review.Parsed
                ? $"You reported the task done, but the reviewer disagrees: {review.Feedback ?? "not every done-criterion is met."} Address that, then report again."
                : "You reported the task done, but completion could not be verified. Check every done-criterion explicitly against what you produced, then report again.";
            task.CompleteStep(progressMade: true, guidance: objection, stepLog);
            LogReview(task, step, review);
            return;
        }

        task.CompleteStep(review.Progress, review.Feedback, stepLog);
        LogReview(task, step, review);
        if (checkpoint is null)
            task.AddLog(AgentTaskLogKind.System, $"Step {step} ended without task_checkpoint.", step);

        if (task.ConsecutiveNoProgress >= maxStepsWithoutProgress)
        {
            task.Block(
                $"I've gone {task.ConsecutiveNoProgress} steps without clear progress" +
                (review.Feedback is null ? "." : $" (reviewer: {review.Feedback})") +
                " How should I proceed?");
        }
    }

    private static void LogReview(AgentTask task, int step, StepReview review)
    {
        if (review.Feedback is not null)
            task.AddLog(AgentTaskLogKind.Review, review.Feedback, step);
    }

    private static string? Shorten(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var flat = text.Trim().ReplaceLineEndings(" ");
        return flat.Length <= 240 ? flat : flat[..240] + "…";
    }
}
