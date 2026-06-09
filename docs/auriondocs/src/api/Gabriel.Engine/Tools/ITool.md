# ITool

> **File:** `src/api/Gabriel.Engine/Tools/ITool.cs`  
> **Kind:** interface

Represents a tool the agent can invoke. Implementations declare a JSON Schema that describes the tool's argument shape (exposed via ParametersJsonSchema) and produce a single string observation (or an error string) that the agent reads. Implement this interface when creating a DI-registered, discoverable tool that an LLM-driven agent can call instead of embedding the capability directly in the agent code.

## Remarks
This abstraction separates tool behavior from the agent: the agent uses the tool's name, description and JSON schema to construct calls, while the tool itself is responsible for parsing the raw JSON arguments and returning a textual observation. Tools are intended to be registered via dependency injection and discovered through IToolRegistry; the ParametersJsonSchema value is forwarded verbatim to the LLM, so it must be valid JSON Schema. ExecuteAsync receives the raw JSON arguments plus a CancellationToken and returns a `Task<string>` containing the tool's observation or an error message.

## Example
```csharp
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class EchoTool : ITool
{
    public string Name => "echo";
    public string Description => "Echoes back the provided message.";

    // Schema: { "type": "object", "properties": { "message": { "type": "string" } }, "required": ["message"] }
    public string ParametersJsonSchema => "{\"type\":\"object\",\"properties\":{\"message\":{\"type\":\"string\"}},\"required\":[\"message\"]}";

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("message", out var msg))
            {
                return msg.GetString() ?? string.Empty;
            }

            return "error: missing 'message' property";
        }
        catch (JsonException ex)
        {
            return $"error: invalid arguments JSON - {ex.Message}";
        }
    }
}
```

## Notes
- ParametersJsonSchema is forwarded verbatim to the LLM; an invalid or incorrect schema can cause malformed prompts or tool usage errors.
- argumentsJson is raw JSON that the tool must parse and validate; do not assume the agent will always send well-formed or fully validated input.
- Respect CancellationToken promptly; long-running tools should observe cancellation to avoid blocking agent workflows.