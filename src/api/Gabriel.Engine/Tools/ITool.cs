namespace Gabriel.Engine.Tools;

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

    // Whether this tool may run concurrently with other tools in the same
    // batch of tool calls. Default false: every ITool is resolved from the
    // request scope, so tools that (transitively) touch the scoped
    // AppDbContext - memory_*, project files, the path-resolver-backed
    // filesystem tools - would race on a single DbContext instance if run in
    // parallel. Only opt in tools that are pure or depend exclusively on
    // singleton/thread-safe services (HTTP clients, in-process compute).
    bool IsParallelSafe => false;

    Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct);
}
