namespace Gabriel.Core.Tools;

// A tool the agent can call. Implementations declare a JSON schema for their
// arguments and return a string observation (or error) for the agent to read.
// Tools are registered via DI and discovered automatically by IToolRegistry.
public interface ITool
{
    string Name { get; }
    string Description { get; }

    // Raw JSON schema object describing the argument shape. Passed through to
    // the LLM verbatim, so it must be valid JSON schema.
    string ParametersJsonSchema { get; }

    Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct);
}
