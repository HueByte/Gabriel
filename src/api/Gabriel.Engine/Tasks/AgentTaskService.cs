using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;
using Gabriel.Core.Services;
using Gabriel.Engine.Providers;
using Gabriel.Engine.Services;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Tasks;

// The user-facing side of autonomous tasks (the worker side is
// IAgentTaskRunner). Every operation is scoped to the current user.
public interface IAgentTaskService
{
    Task<IReadOnlyList<AgentTask>> ListAsync(Guid? projectId, CancellationToken ct = default);
    Task<AgentTask> GetAsync(Guid id, CancellationToken ct = default);

    // Creates the task's conversation and drafts the contract. The task starts
    // as a Draft: nothing runs until ApproveAsync.
    Task<AgentTask> CreateAsync(string goal, Guid? projectId, int? maxSteps, int? maxDurationMinutes, CancellationToken ct = default);

    Task<AgentTask> UpdateContractAsync(
        Guid id, string objective, IReadOnlyList<string> doneCriteria, IReadOnlyList<string> constraints, CancellationToken ct = default);

    Task<AgentTask> ApproveAsync(Guid id, CancellationToken ct = default);
    Task<AgentTask> PauseAsync(Guid id, CancellationToken ct = default);
    Task<AgentTask> ResumeAsync(Guid id, CancellationToken ct = default);
    Task<AgentTask> AnswerAsync(Guid id, string answer, CancellationToken ct = default);
    Task<AgentTask> CancelAsync(Guid id, CancellationToken ct = default);

    // The task that owns this conversation, if any (current user's only).
    Task<AgentTask?> FindForConversationAsync(Guid conversationId, CancellationToken ct = default);

    // Throws ConflictException while a running task drives the conversation -
    // a second writer would interleave turns with the worker's steps.
    Task EnsureConversationNotDrivenAsync(Guid conversationId, CancellationToken ct = default);
}

public sealed class AgentTaskService : IAgentTaskService
{
    private const int TitleGoalChars = 60;

    private readonly IAgentTaskRepository _tasks;
    private readonly IChatService _chat;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ITaskContractDrafter _drafter;
    private readonly IUserPreferences _userPrefs;
    private readonly IModelCatalog _modelCatalog;
    private readonly IChatProviderRegistry _providers;
    private readonly IActiveTurnRegistry _turns;
    private readonly AgentTaskOptions _options;

    public AgentTaskService(
        IAgentTaskRepository tasks,
        IChatService chat,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        ITaskContractDrafter drafter,
        IUserPreferences userPrefs,
        IModelCatalog modelCatalog,
        IChatProviderRegistry providers,
        IActiveTurnRegistry turns,
        IOptions<AgentTaskOptions> options)
    {
        _tasks = tasks;
        _chat = chat;
        _uow = uow;
        _currentUser = currentUser;
        _drafter = drafter;
        _userPrefs = userPrefs;
        _modelCatalog = modelCatalog;
        _providers = providers;
        _turns = turns;
        _options = options.Value;
    }

    public Task<IReadOnlyList<AgentTask>> ListAsync(Guid? projectId, CancellationToken ct = default)
        => _tasks.ListAsync(RequireUserId(), projectId, ct);

    public async Task<AgentTask> GetAsync(Guid id, CancellationToken ct = default)
        => await _tasks.GetByIdAsync(id, RequireUserId(), ct) ?? throw new NotFoundException(nameof(AgentTask), id);

    public async Task<AgentTask> CreateAsync(
        string goal, Guid? projectId, int? maxSteps, int? maxDurationMinutes, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(goal))
            throw new DomainException("Describe what the task should achieve.");
        var userId = RequireUserId();
        var steps = Math.Clamp(maxSteps ?? _options.DefaultMaxSteps, 1, 500);
        var minutes = Math.Clamp(maxDurationMinutes ?? _options.DefaultMaxDurationMinutes, 1, 24 * 60);

        var trimmed = goal.Trim();
        var conversation = await _chat.CreateConversationAsync(projectId, "Task · " + Shorten(trimmed, TitleGoalChars), ct);

        var prefs = await _userPrefs.GetAsync(ct);
        var selection = _modelCatalog.Resolve(prefs.PreferredProvider, prefs.PreferredModel);
        var contract = await _drafter.DraftAsync(trimmed, _providers.Resolve(selection.Provider), selection.Name, minutes, ct);

        // CreateConversationAsync always resolves a project (Default when none
        // given); the nullable is for pre-project legacy rows only.
        var projectIdResolved = conversation.ProjectId
            ?? throw new InvalidOperationException("New conversation has no project.");
        var task = AgentTask.Create(userId, projectIdResolved, conversation.Id, trimmed, steps, minutes);
        task.SetContract(contract.Objective, contract.DoneCriteria, contract.Constraints);
        await _tasks.AddAsync(task, ct);
        await _uow.SaveChangesAsync(ct);
        return task;
    }

    public Task<AgentTask> UpdateContractAsync(
        Guid id, string objective, IReadOnlyList<string> doneCriteria, IReadOnlyList<string> constraints, CancellationToken ct = default)
        => MutateAsync(id, t => t.SetContract(objective, doneCriteria, constraints), ct);

    public Task<AgentTask> ApproveAsync(Guid id, CancellationToken ct = default)
        => MutateAsync(id, t => t.Approve(DateTimeOffset.UtcNow), ct);

    public Task<AgentTask> ResumeAsync(Guid id, CancellationToken ct = default)
        => MutateAsync(id, t => t.Resume(DateTimeOffset.UtcNow, _options.ResumeStepIncrement), ct);

    public Task<AgentTask> AnswerAsync(Guid id, string answer, CancellationToken ct = default)
        => MutateAsync(id, t => t.Answer(answer), ct);

    // Pause and cancel also stop the step in flight - the runner notices the
    // status change once the turn winds down and records the interruption.
    public async Task<AgentTask> PauseAsync(Guid id, CancellationToken ct = default)
    {
        var task = await MutateAsync(id, t => t.Pause("Paused by the user."), ct);
        _turns.TryStop(task.ConversationId, task.UserId);
        return task;
    }

    public async Task<AgentTask> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var task = await MutateAsync(id, t => t.Cancel(DateTimeOffset.UtcNow), ct);
        _turns.TryStop(task.ConversationId, task.UserId);
        return task;
    }

    public async Task<AgentTask?> FindForConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        var task = await _tasks.FindByConversationAsync(conversationId, ct);
        return task?.UserId == RequireUserId() ? task : null;
    }

    public async Task EnsureConversationNotDrivenAsync(Guid conversationId, CancellationToken ct = default)
    {
        if (await _tasks.IsConversationDrivenAsync(conversationId, ct))
            throw new ConflictException("An autonomous task is working in this conversation. Pause it from the Tasks page to take over.");
    }

    private async Task<AgentTask> MutateAsync(Guid id, Action<AgentTask> change, CancellationToken ct)
    {
        var task = await GetAsync(id, ct);
        change(task);
        await _uow.SaveChangesAsync(ct);
        return task;
    }

    private Guid RequireUserId()
        => _currentUser.UserId ?? throw new UnauthorizedAccessException("Authenticated user required.");

    private static string Shorten(string text, int max)
    {
        var line = text.ReplaceLineEndings(" ");
        if (line.Length <= max) return line;
        var cut = line.LastIndexOf(' ', max - 1);
        return (cut > max / 2 ? line[..cut] : line[..(max - 1)]).TrimEnd() + "…";
    }
}
