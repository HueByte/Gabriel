namespace Gabriel.Core.Configuration;

// Autonomous tasks: long-running delegated work driven by a background worker
// (see AgentTask). Defaults aim at "a few hours, unattended" without letting a
// confused run burn steps indefinitely.
public class AgentTaskOptions : IConfigSection<AgentTaskOptions>
{
    public static string SectionName => "AgentTasks";

    // Master switch for the background worker. Tasks can still be created and
    // approved while it's off; they just don't advance.
    public bool Enabled { get; set; } = true;

    // Per-task budgets, overridable at creation. Each step is one agent turn
    // (itself capped by Agent:MaxIterations tool rounds).
    public int DefaultMaxSteps { get; set; } = 40;
    public int DefaultMaxDurationMinutes { get; set; } = 180;

    // Steps granted when the user resumes a task that ran out of step budget.
    public int ResumeStepIncrement { get; set; } = 20;

    // Guardrails: block for the user after this many steps the reviewer judged
    // as no progress; fail after this many consecutive step errors.
    public int MaxStepsWithoutProgress { get; set; } = 3;
    public int MaxConsecutiveErrors { get; set; } = 3;

    // How often the worker looks for runnable tasks when idle.
    public int PollIntervalSeconds { get; set; } = 3;
}
