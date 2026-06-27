# ITool

> **File:** `src/api/Gabriel.Engine/Tools/ITool.cs`  
> **Kind:** interface

An abstraction representing a runtime "tool" the agent can call. Implement this interface to expose an action the agent (and its orchestrator) can invoke; the implementation declares a JSON Schema for its arguments (passed verbatim to the LLM) and produces a string observation or error that the agent reads back. Use this when adding new agent capabilities that should be discoverable via DI and IToolRegistry rather than invoked directly from the core agent code.

## Remarks
ITool is intentionally minimal: tools declare metadata (Name, Description, ParametersJsonSchema) used for discovery and prompt-building, and implement ExecuteAsync to perform the work. The ParametersJsonSchema is forwarded unchanged to the language model so the model can construct valid arguments; ExecuteAsync receives those arguments as a raw JSON string and must interpret them. Tools are registered with dependency injection and discovered by IToolRegistry so the agent can enumerate and call them dynamically.

## Example
```csharp
// A trivial tool that echoes back a provided message.
public class EchoTool : ITool
{
    public string Name => "echo";
    public string Description => "Returns the provided message unchanged.";

    // A small JSON Schema describing a single string parameter 'message'.
    public string ParametersJsonSchema =>
        "{\"type\":\"object\",\"properties\":{\"message\":{\"type\":\"string\"}},\"required\":[\"message\"]}";

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        // Respect the cancellation token in long-running implementations.
        ct.ThrowIfCancellationRequested();

        // In a real tool parse and validate argumentsJson against the schema.
        // This simple example just returns the raw JSON for demonstration.
        await Task.Yield();
        return argumentsJson;
    }
}
```

## Notes
- ParametersJsonSchema is forwarded verbatim to the LLM: it must be valid JSON Schema and stable across deployments if prompts rely on it.
- ExecuteAsync receives a raw JSON string; implementations should validate and parse that string and avoid assuming well-formed input from the caller.
- Honor the CancellationToken (do not block indefinitely) and prefer returning an error string rather than throwing unhandled exceptions so the agent can surface failures to the user.