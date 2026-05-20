using System.Text.Json;
using Gabriel.Core.Entities;
using Gabriel.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gabriel.Engine.Services;

// Default IMetricRecorder. Singleton lifetime because most callers (decorators,
// tools, the agent loop itself) are singletons; bridges to the scoped
// IMetricRepository via IServiceScopeFactory on each call.
//
// Failure policy: a metric-write failure is logged at warn level and
// swallowed. The whole point of telemetry is to observe the system - if
// observability itself breaks the system, we've made it worse, not better.
// Callers can record-and-forget without try/catch.
public sealed class MetricRecorder : IMetricRecorder
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        // snake_case is the convention across the recorded payloads so the
        // raw SQL view of the Metric column reads naturally. Each subsystem
        // can override on its own DTO with [JsonPropertyName] if needed.
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricRecorder> _logger;

    public MetricRecorder(IServiceScopeFactory scopeFactory, ILogger<MetricRecorder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task RecordAsync<T>(string system, T metric, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(system))
        {
            _logger.LogWarning("MetricRecorder: empty System name; dropping payload.");
            return;
        }

        string json;
        try
        {
            json = JsonSerializer.Serialize(metric, JsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MetricRecorder: payload serialization failed for system {System}; dropping.", system);
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IMetricRepository>();
            await repo.AddAsync(MetricEntry.Create(system, json), ct);
        }
        catch (OperationCanceledException)
        {
            // Caller cancellation - propagate, but don't log. The caller knows.
            throw;
        }
        catch (Exception ex)
        {
            // Anything else is observability-breaking-itself territory. Log,
            // swallow, move on.
            _logger.LogWarning(ex, "MetricRecorder: persistence failed for system {System}; metric dropped.", system);
        }
    }
}
