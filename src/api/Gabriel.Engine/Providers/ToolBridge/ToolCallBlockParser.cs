using System.Text.Json;

namespace Gabriel.Engine.Providers.ToolBridge;

// Extracts <tool_call>{"name":"X","arguments":{...}}</tool_call> blocks from
// the buffered tail produced by ToolCallStreamSplitter. All-or-nothing per
// turn: a single malformed block causes the whole extraction to throw
// ToolCallParseException, which GabrielToolBridge catches to drive the retry
// loop.
//
// The "id" field on each parsed call is synthesised here (the model isn't
// asked to invent one) so AgentContext's tool_call_id matching keeps working
// when the tool result message goes back in the next iteration.
internal static class ToolCallBlockParser
{
    private const string OpenTag = "<tool_call>";
    private const string CloseTag = "</tool_call>";

    public static IReadOnlyList<ParsedToolCall> Extract(string text)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<ParsedToolCall>();

        var calls = new List<ParsedToolCall>();
        var cursor = 0;
        var index = 0;
        while (true)
        {
            var openIdx = text.IndexOf(OpenTag, cursor, StringComparison.Ordinal);
            if (openIdx < 0) break;
            var bodyStart = openIdx + OpenTag.Length;
            var closeIdx = text.IndexOf(CloseTag, bodyStart, StringComparison.Ordinal);
            if (closeIdx < 0)
            {
                throw new ToolCallParseException(
                    $"Unterminated <tool_call> block starting at char {openIdx}. " +
                    "The model emitted an opening marker without a matching </tool_call>.",
                    text.Substring(openIdx));
            }

            var body = text.Substring(bodyStart, closeIdx - bodyStart).Trim();
            ParsedToolCall call;
            try
            {
                call = ParseBody(body, index);
            }
            catch (JsonException ex)
            {
                throw new ToolCallParseException(
                    $"Failed to parse JSON inside <tool_call> at char {openIdx}: {ex.Message}",
                    text.Substring(openIdx, closeIdx + CloseTag.Length - openIdx),
                    ex);
            }

            calls.Add(call);
            cursor = closeIdx + CloseTag.Length;
            index++;
        }
        return calls;
    }

    private static ParsedToolCall ParseBody(string body, int index)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Tool call body must be a JSON object with 'name' and 'arguments'.");
        }

        if (!root.TryGetProperty("name", out var nameEl) || nameEl.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Tool call is missing a string 'name' field.");
        }
        var name = nameEl.GetString()!;

        // Arguments are an object; serialize back to a JSON string because
        // every downstream consumer (ITool.ExecuteAsync, persistence) wants
        // the raw JSON, not a JsonElement. Missing 'arguments' is permitted
        // and reads as "{}" so tools with zero required params still run.
        string argsJson;
        if (root.TryGetProperty("arguments", out var argsEl))
        {
            if (argsEl.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Tool call 'arguments' must be a JSON object.");
            }
            argsJson = argsEl.GetRawText();
        }
        else
        {
            argsJson = "{}";
        }

        // Synthetic deterministic-ish id. Random enough to avoid collisions
        // inside one turn (the index suffix guarantees that within the turn);
        // the "emu_" prefix makes the origin obvious in logs and DB rows.
        var id = $"emu_{Guid.NewGuid():N}".Substring(0, 12) + $"_{index}";
        return new ParsedToolCall(id, name, argsJson);
    }
}

internal sealed record ParsedToolCall(string Id, string Name, string ArgumentsJson);

// Thrown when at least one <tool_call> block in the buffered turn text was
// malformed (missing close tag, invalid JSON, wrong shape). GabrielToolBridge
// catches this to ask the model to fix its previous response and retries up
// to GabrielToolBridge.MaxParseRetries times before giving up.
internal sealed class ToolCallParseException : Exception
{
    // The raw block (or partial block) that failed, quoted back at the model
    // in the fix-up message so it knows what to repair.
    public string OffendingBlock { get; }

    public ToolCallParseException(string message, string offendingBlock)
        : base(message)
    {
        OffendingBlock = offendingBlock;
    }

    public ToolCallParseException(string message, string offendingBlock, Exception inner)
        : base(message, inner)
    {
        OffendingBlock = offendingBlock;
    }
}
