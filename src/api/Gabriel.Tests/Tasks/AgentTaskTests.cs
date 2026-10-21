using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Xunit;
using static Gabriel.Tests.Tasks.TaskTestData;

namespace Gabriel.Tests.Tasks;

public class AgentTaskTests
{
    [Fact]
    public void Approval_requires_a_done_criterion()
    {
        var task = AgentTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "do things", 10, 60);

        Assert.Throws<DomainException>(() => task.Approve(Now));
        Assert.Equal(AgentTaskStatus.Draft, task.Status);
    }

    [Fact]
    public void Approval_starts_the_clock()
    {
        var task = Running(maxMinutes: 90);

        Assert.Equal(AgentTaskStatus.Running, task.Status);
        Assert.Equal(Now.AddMinutes(90), task.Deadline);
    }

    [Fact]
    public void Contract_is_frozen_once_running()
    {
        var task = Running();

        Assert.Throws<ConflictException>(() => task.SetContract("new", ["x"], []));
    }

    [Fact]
    public void Budget_reports_steps_and_time()
    {
        var task = Running(maxSteps: 1, maxMinutes: 30);
        Assert.Null(task.BudgetExhaustedReason(Now));
        Assert.Contains("Time budget", task.BudgetExhaustedReason(Now.AddMinutes(31)));

        task.CompleteStep(progressMade: true, guidance: null, "Step 1: did it");
        Assert.Contains("Step budget", task.BudgetExhaustedReason(Now));
    }

    [Fact]
    public void Checkpoint_belongs_to_the_step_it_was_recorded_in()
    {
        var task = Running();
        task.RecordCheckpoint(TaskCheckpointStatus.Continue, "drafted outline", null, null);
        Assert.Equal(TaskCheckpointStatus.Continue, task.CurrentStepCheckpoint);

        task.CompleteStep(progressMade: true, guidance: null, "Step 1");

        // A new step hasn't reported yet - the old checkpoint must not leak into it.
        Assert.Null(task.CurrentStepCheckpoint);
    }

    [Fact]
    public void Guidance_is_consumed_by_the_next_step()
    {
        var task = Running();
        task.CompleteStep(progressMade: true, guidance: "cite sources", "Step 1");

        Assert.Equal("cite sources", task.TakeGuidance());
        Assert.Null(task.TakeGuidance());
    }

    [Fact]
    public void Resume_after_budget_grants_more_steps_and_time()
    {
        var task = Running(maxSteps: 1, maxMinutes: 30);
        task.CompleteStep(progressMade: true, guidance: null, "Step 1");
        task.Pause("Step budget reached.");

        var later = Now.AddHours(2);
        task.Resume(later, extraSteps: 5);

        Assert.Equal(AgentTaskStatus.Running, task.Status);
        Assert.Equal(6, task.MaxSteps);
        Assert.Equal(later.AddMinutes(30), task.Deadline);
        Assert.Null(task.BudgetExhaustedReason(later));
    }

    [Fact]
    public void Answer_unblocks_with_the_answer_as_guidance()
    {
        var task = Running();
        task.CompleteStep(progressMade: false, guidance: null, "Step 1");
        task.Block("Which format?");

        task.Answer("Markdown please");

        Assert.Equal(AgentTaskStatus.Running, task.Status);
        Assert.Equal(0, task.ConsecutiveNoProgress);
        Assert.Contains("Markdown please", task.TakeGuidance());
    }

    [Fact]
    public void Log_order_survives_same_tick_writes()
    {
        var task = Running();
        for (var i = 0; i < 50; i++) task.AddLog(AgentTaskLogKind.System, $"entry {i}");

        var times = task.Log.Select(e => e.CreatedAt).ToList();
        Assert.Equal(times.Order().ToList(), times);
        Assert.Equal(times.Count, times.Distinct().Count());
    }

    [Fact]
    public void Lifecycle_moves_are_guarded()
    {
        var task = Running();
        Assert.Throws<ConflictException>(() => task.Answer("not blocked"));
        Assert.Throws<ConflictException>(() => task.Resume(Now, 5));

        task.Cancel(Now);
        Assert.True(task.IsTerminal);
        Assert.Throws<ConflictException>(() => task.Cancel(Now));
        Assert.Throws<ConflictException>(() => task.Pause("late"));
    }
}
