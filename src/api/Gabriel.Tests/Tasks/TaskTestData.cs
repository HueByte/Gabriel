using Gabriel.Core.Entities;

namespace Gabriel.Tests.Tasks;

internal static class TaskTestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    public static AgentTask Draft(int maxSteps = 10, int maxMinutes = 60)
    {
        var task = AgentTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "write a report on sourdough", maxSteps, maxMinutes);
        task.SetContract("Produce report.md on sourdough starters", ["report.md exists", "covers feeding"], ["no paid sources"]);
        return task;
    }

    public static AgentTask Running(int maxSteps = 10, int maxMinutes = 60)
    {
        var task = Draft(maxSteps, maxMinutes);
        task.Approve(Now);
        return task;
    }
}
