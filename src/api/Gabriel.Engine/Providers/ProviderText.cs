using System.Text;
using System.Text.Json;

namespace Gabriel.Engine.Providers;

// Helpers for the engine's tool-less side calls (titles, summaries, task
// contracts, step reviews): one request, the text channel collected, nothing
// streamed to a client.
public static class ProviderText
{
    // Text deltas only - reasoning models stream their thinking on a separate
    // channel that must not leak into what the caller parses.
    public static async Task<string> CompleteAsync(
        IChatProvider provider,
        string modelName,
        string systemPrompt,
        string userPrompt,
        CancellationToken ct)
    {
        var history = new List<ChatProviderMessage>
        {
            new(Core.Entities.MessageRole.System, systemPrompt),
            new(Core.Entities.MessageRole.User, userPrompt),
        };

        var text = new StringBuilder();
        await foreach (var evt in provider.StreamAsync(history, [], modelName, ct))
        {
            if (evt is TextDeltaEvent td) text.Append(td.Delta);
            else if (evt is FinishEvent) break;
        }
        return text.ToString().Trim();
    }

    // Models wrap JSON in prose or ``` fences often enough that "the first
    // balanced object in the reply" is the robust read. Null when there is none
    // or it doesn't parse; the caller owns the fallback.
    public static JsonDocument? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        while (start >= 0)
        {
            var depth = 0;
            var inString = false;
            for (var i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (inString)
                {
                    if (c == '\\') i++;
                    else if (c == '"') inString = false;
                    continue;
                }
                if (c == '"') inString = true;
                else if (c == '{') depth++;
                else if (c == '}' && --depth == 0)
                {
                    try { return JsonDocument.Parse(text[start..(i + 1)]); }
                    catch (JsonException) { break; }
                }
            }
            start = text.IndexOf('{', start + 1);
        }
        return null;
    }
}
