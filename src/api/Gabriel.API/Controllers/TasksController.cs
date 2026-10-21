using Gabriel.API.Contracts.Tasks;
using Gabriel.Core.Entities;
using Gabriel.Engine.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

// Autonomous tasks: delegate a goal, review and approve its contract, then
// let the background worker step it. Every state change returns the full task
// (with log) so the client can re-render from one response. Invalid
// transitions (approve a running task, answer one that isn't blocked) are 409.
[ApiController]
[Authorize]
[Route("tasks")]
public class TasksController : ControllerBase
{
    private readonly IAgentTaskService _tasks;

    public TasksController(IAgentTaskService tasks)
    {
        _tasks = tasks;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List([FromQuery] Guid? projectId, CancellationToken ct)
    {
        var tasks = await _tasks.ListAsync(projectId, ct);
        return Ok(tasks.Select(t => ToResponse(t, includeLog: false)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Get(Guid id, CancellationToken ct)
        => Ok(ToResponse(await _tasks.GetAsync(id, ct), includeLog: true));

    // Drafts the contract with one LLM call (a few seconds); the task comes
    // back as a draft and nothing runs until /approve.
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var task = await _tasks.CreateAsync(request.Goal, request.ProjectId, request.MaxSteps, request.MaxDurationMinutes, ct);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, ToResponse(task, includeLog: true));
    }

    [HttpPut("{id:guid}/contract")]
    public async Task<ActionResult<TaskResponse>> UpdateContract(
        Guid id, [FromBody] UpdateTaskContractRequest request, CancellationToken ct)
        => Ok(ToResponse(
            await _tasks.UpdateContractAsync(id, request.Objective, request.DoneCriteria, request.Constraints, ct),
            includeLog: true));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<TaskResponse>> Approve(Guid id, CancellationToken ct)
        => Ok(ToResponse(await _tasks.ApproveAsync(id, ct), includeLog: true));

    [HttpPost("{id:guid}/pause")]
    public async Task<ActionResult<TaskResponse>> Pause(Guid id, CancellationToken ct)
        => Ok(ToResponse(await _tasks.PauseAsync(id, ct), includeLog: true));

    // Also grants a fresh budget increment when the pause was a budget stop.
    [HttpPost("{id:guid}/resume")]
    public async Task<ActionResult<TaskResponse>> Resume(Guid id, CancellationToken ct)
        => Ok(ToResponse(await _tasks.ResumeAsync(id, ct), includeLog: true));

    [HttpPost("{id:guid}/answer")]
    public async Task<ActionResult<TaskResponse>> Answer(Guid id, [FromBody] AnswerTaskRequest request, CancellationToken ct)
        => Ok(ToResponse(await _tasks.AnswerAsync(id, request.Answer, ct), includeLog: true));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<TaskResponse>> Cancel(Guid id, CancellationToken ct)
        => Ok(ToResponse(await _tasks.CancelAsync(id, ct), includeLog: true));

    private static TaskResponse ToResponse(AgentTask t, bool includeLog) => new(
        t.Id, t.ProjectId, t.ConversationId,
        t.Goal, t.Objective, t.DoneCriteria, t.Constraints,
        t.Status.ToString().ToLowerInvariant(), t.StatusReason,
        t.StepsCompleted, t.MaxSteps, t.MaxDurationMinutes, t.Deadline,
        t.FinalSummary,
        t.CreatedAt, t.UpdatedAt, t.StartedAt, t.FinishedAt,
        includeLog
            ? t.Log.OrderBy(e => e.CreatedAt)
                .Select(e => new TaskLogEntryResponse(e.Step, e.Kind.ToString().ToLowerInvariant(), e.Text, e.CreatedAt))
                .ToList()
            : null);
}
