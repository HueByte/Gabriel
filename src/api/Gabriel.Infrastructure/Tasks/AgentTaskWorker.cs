using Gabriel.Core.Configuration;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;
using Gabriel.Engine.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Tasks;

// Drives autonomous tasks independently of any browser. Each pass takes every
// Running task, least recently advanced first, and runs ONE step of each -
// round-robin, so a long task can't starve a newer one. One step at a time,
// in a fresh DI scope acting as the task's owner: steps of different tasks
// never share a DbContext, and concurrency stays at one turn (provider rate
// limits and the single SQLite file both prefer it).
//
// Restart-safe by construction: all task state is persisted after every step,
// and a step cut short by shutdown is persisted by the agent's interrupted-
// turn path, so the next process simply runs the next step.
public sealed class AgentTaskWorker : BackgroundService
{
    // After a step blew up past the runner's own handling - a bug, not a task
    // condition - back off so it can't spin.
    private static readonly TimeSpan CrashBackoff = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AgentTaskOptions _options;
    private readonly ILogger<AgentTaskWorker> _logger;

    public AgentTaskWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentTaskOptions> options,
        ILogger<AgentTaskWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Same guard as the migration block: the swagger-codegen host must not
        // touch the database.
        if (Environment.GetEnvironmentVariable("SKIP_DB_INIT") == "true") return;
        if (!_options.Enabled)
        {
            _logger.LogInformation("Autonomous task worker disabled (AgentTasks:Enabled=false)");
            return;
        }

        var idle = TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds));
        try
        {
            // Let migrations finish before the first scan.
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                var runnable = await ListRunnableAsync(stoppingToken);
                if (runnable.Count == 0)
                {
                    await Task.Delay(idle, stoppingToken);
                    continue;
                }

                foreach (var (taskId, userId) in runnable)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    await RunOneStepAsync(taskId, userId, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task<IReadOnlyList<(Guid TaskId, Guid UserId)>> ListRunnableAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IAgentTaskRepository>().ListRunnableAsync(ct);
    }

    private async Task RunOneStepAsync(Guid taskId, Guid userId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentUserOverride>().Set(userId);
        var runner = scope.ServiceProvider.GetRequiredService<IAgentTaskRunner>();
        try
        {
            await runner.RunStepAsync(taskId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Autonomous task step crashed | task={TaskId}", taskId);
            await Task.Delay(CrashBackoff, ct);
        }
    }
}
