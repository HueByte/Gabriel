namespace Gabriel.Engine.Tools;

// Trivial starter tool - proves the ReAct loop works end-to-end without any
// external dependencies.
public class GetCurrentTimeTool : ITool
{
    public string Name => "get_current_time";

    public string Description => "Returns the current UTC time as an ISO 8601 string.";

    public string ParametersJsonSchema => """{"type":"object","properties":{}}""";

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
        => Task.FromResult(DateTimeOffset.UtcNow.ToString("o"));
}
