using Gabriel.Core.Entities;
using Gabriel.Engine.Tasks;
using Xunit;
using static Gabriel.Tests.Tasks.TaskTestData;

namespace Gabriel.Tests.Tasks;

public class TaskStepPolicyTests
{
    private const int StuckAfter = 3;

    private static StepReview Review(bool progress = true, bool done = false, string? feedback = null, bool parsed = true)
        => new(progress, done, feedback, parsed);

    [Fact]
    public void Done_is_accepted_only_when_the_reviewer_agrees()
    {
        var task = Running();
        task.RecordCheckpoint(TaskCheckpointStatus.Done, "wrote report.md", null, "Report is in report.md.");

        TaskStepPolicy.Apply(task, 1, Review(done: true), null, StuckAfter, Now);

        Assert.Equal(AgentTaskStatus.Completed, task.Status);
        Assert.Equal("Report is in report.md.", task.FinalSummary);
    }

    [Fact]
    public void Disputed_done_continues_with_the_objection_as_guidance()
    {
        var task = Running();
        task.RecordCheckpoint(TaskCheckpointStatus.Done, "wrote report.md", null, "Done.");

        TaskStepPolicy.Apply(task, 1, Review(done: false, feedback: "Feeding is not covered."), null, StuckAfter, Now);

        Assert.Equal(AgentTaskStatus.Running, task.Status);
        Assert.Contains("Feeding is not covered.", task.TakeGuidance());
    }

    [Fact]
    public void Unreadable_review_never_completes_the_task()
    {
        var task = Running();
        task.RecordCheckpoint(TaskCheckpointStatus.Done, "finished", null, "Done.");

        TaskStepPolicy.Apply(task, 1, Review(done: false, parsed: false), null, StuckAfter, Now);

        Assert.Equal(AgentTaskStatus.Running, task.Status);
        Assert.Contains("could not be verified", task.TakeGuidance());
    }

    [Fact]
    public void Blocked_checkpoint_asks_the_user_without_review()
    {
        var task = Running();
        task.RecordCheckpoint(TaskCheckpointStatus.Blocked, "need a decision", "Metric or imperial units?", null);

        Assert.False(TaskStepPolicy.NeedsReview(task));
        TaskStepPolicy.Apply(task, 1, review: null, null, StuckAfter, Now);

        Assert.Equal(AgentTaskStatus.Blocked, task.Status);
        Assert.Equal("Metric or imperial units?", task.StatusReason);
    }

    [Fact]
    public void Repeated_no_progress_blocks_for_the_user()
    {
        var task = Running();
        for (var step = 1; step <= StuckAfter; step++)
        {
            Assert.Equal(AgentTaskStatus.Running, task.Status);
            task.RecordCheckpoint(TaskCheckpointStatus.Continue, "looked around", null, null);
            TaskStepPolicy.Apply(task, step, Review(progress: false, feedback: "Start writing."), null, StuckAfter, Now);
        }

        Assert.Equal(AgentTaskStatus.Blocked, task.Status);
        Assert.Contains("without clear progress", task.StatusReason);
        Assert.Contains("Start writing.", task.StatusReason);
    }

    [Fact]
    public void Progress_resets_the_stuck_counter()
    {
        var task = Running();
        TaskStepPolicy.Apply(task, 1, Review(progress: false), null, StuckAfter, Now);
        TaskStepPolicy.Apply(task, 2, Review(progress: false), null, StuckAfter, Now);
        TaskStepPolicy.Apply(task, 3, Review(progress: true), null, StuckAfter, Now);

        Assert.Equal(0, task.ConsecutiveNoProgress);
        Assert.Equal(AgentTaskStatus.Running, task.Status);
    }

    [Fact]
    public void Missing_checkpoint_is_logged_and_falls_back_to_the_reply_text()
    {
        var task = Running();

        TaskStepPolicy.Apply(task, 1, Review(), "I searched three sources.", StuckAfter, Now);

        Assert.Contains(task.Log, e => e.Text == "Step 1: I searched three sources.");
        Assert.Contains(task.Log, e => e.Text.Contains("without task_checkpoint"));
    }
}
