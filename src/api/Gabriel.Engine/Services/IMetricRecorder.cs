namespace Gabriel.Engine.Services;

// Convenient write surface over the generic metric event log. Subsystems
// throughout Engine call this rather than depending directly on the storage
// repository - it serializes the payload to JSON and absorbs storage errors
// so a metric-record failure never bubbles up into the caller's business
// path.
//
// Wire example (per web-search call):
//   await _metrics.RecordAsync("web_search.tavily", new {
//       outcome = "success",
//       query = "...",
//       result_count = 5,
//       latency_ms = 287
//   }, ct);
//
// The System name is a stable dotted identifier (e.g. "web_search.tavily",
// "agent_loop.iteration") that read-side tooling groups on. The payload
// shape is up to each subsystem; convention: include an `outcome` field
// (success | error | empty) so aggregating endpoints can count without
// parsing every other field.
public interface IMetricRecorder
{
    Task RecordAsync<T>(string system, T metric, CancellationToken ct = default);
}
