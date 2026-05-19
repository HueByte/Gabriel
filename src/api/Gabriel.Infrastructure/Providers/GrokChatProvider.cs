using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Gabriel.Core.Entities;
using Gabriel.Engine.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure.Providers;

// Streaming wrapper over xAI's OpenAI-compatible /v1/chat/completions endpoint.
// HTTP plumbing (base URL, timeout, auth header) is configured on a named
// HttpClient at DI registration time - see DependencyInjection.cs and
// GrokAuthHandler.cs. We resolve the client via IHttpClientFactory per call
// so the underlying pooled handler stays healthy (DNS refresh, lifetime, etc.).
public class GrokChatProvider : IChatProvider
{
    public const string HttpClientName = "Grok";

    private readonly IHttpClientFactory _httpFactory;
    private readonly GrokOptions _options;
    private readonly ILogger<GrokChatProvider> _logger;

    public GrokChatProvider(
        IHttpClientFactory httpFactory,
        IOptions<GrokOptions> options,
        ILogger<GrokChatProvider> logger)
    {
        _httpFactory = httpFactory;
        _options = options.Value;
        _logger = logger;
    }

    public string Name => "grok";

    // Falls back to 0 if no model is marked active — the agent's token-budget
    // accounting will then short-circuit and we let the provider's own context
    // overflow be the ground truth. The options validator below already rejects
    // that case at startup, so this is defensive only.
    public int ContextWindowTokens => _options.GetActiveModel()?.ContextWindowTokens ?? 0;

    public async IAsyncEnumerable<ChatProviderEvent> StreamAsync(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var http = _httpFactory.CreateClient(HttpClientName);
        var body = BuildRequestBody(history, tools);
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(body),
        };

        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("xAI streaming request failed: {Status} {Body}", (int)response.StatusCode, errBody);
            yield return new FinishEvent(FinishReason.Error);
            yield break;
        }

        // Tool calls arrive piecewise - accumulate by index until the chunk
        // with finish_reason="tool_calls" arrives, then emit them all.
        var toolCalls = new Dictionary<int, ToolCallBuilder>();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            if (line.Length == 0) continue;
            if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;

            var data = line[5..].TrimStart();
            if (data == "[DONE]") yield break;

            StreamChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<StreamChunk>(data);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Skipping malformed xAI chunk: {Data}", data);
                continue;
            }

            var choice = chunk?.Choices?.FirstOrDefault();
            if (choice is null) continue;

            if (!string.IsNullOrEmpty(choice.Delta?.ReasoningContent))
            {
                yield return new ReasoningDeltaEvent(choice.Delta.ReasoningContent);
            }

            if (!string.IsNullOrEmpty(choice.Delta?.Content))
            {
                yield return new TextDeltaEvent(choice.Delta.Content);
            }

            if (choice.Delta?.ToolCalls is { Count: > 0 } deltas)
            {
                foreach (var d in deltas)
                {
                    if (!toolCalls.TryGetValue(d.Index, out var builder))
                    {
                        builder = new ToolCallBuilder();
                        toolCalls[d.Index] = builder;
                    }
                    if (d.Id is not null) builder.Id = d.Id;
                    if (d.Function?.Name is not null) builder.Name = d.Function.Name;
                    if (d.Function?.Arguments is not null) builder.Arguments.Append(d.Function.Arguments);
                }
            }

            if (choice.FinishReason is not null)
            {
                foreach (var b in toolCalls.OrderBy(kv => kv.Key).Select(kv => kv.Value))
                {
                    yield return new ToolCallReadyEvent(
                        Id: b.Id ?? Guid.NewGuid().ToString("N")[..16],
                        Name: b.Name ?? "unknown",
                        ArgumentsJson: b.Arguments.Length > 0 ? b.Arguments.ToString() : "{}");
                }
                yield return new FinishEvent(MapFinishReason(choice.FinishReason));
                yield break;
            }
        }
    }

    private JsonObject BuildRequestBody(IReadOnlyList<ChatProviderMessage> history, IReadOnlyList<ToolDescriptor> tools)
    {
        // Same fallback story as ContextWindowTokens above: the options validator
        // rejects "no active model" at startup, so by the time we reach this
        // hot path the null branch can never run.
        var activeModelName = _options.GetActiveModel()?.Name ?? string.Empty;

        var body = new JsonObject
        {
            ["model"] = activeModelName,
            ["stream"] = true,
            ["messages"] = BuildMessages(history),
        };

        if (_options.Temperature.HasValue) body["temperature"] = _options.Temperature.Value;
        if (_options.TopP.HasValue) body["top_p"] = _options.TopP.Value;

        if (tools.Count > 0)
        {
            var arr = new JsonArray();
            foreach (var t in tools)
            {
                arr.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = t.Name,
                        ["description"] = t.Description,
                        // Schema is a raw JSON object string - parse so it serializes as nested JSON, not a string.
                        ["parameters"] = JsonNode.Parse(t.ParametersJsonSchema),
                    },
                });
            }
            body["tools"] = arr;
        }

        return body;
    }

    private static JsonArray BuildMessages(IReadOnlyList<ChatProviderMessage> history)
    {
        var arr = new JsonArray();
        foreach (var m in history)
        {
            var msg = new JsonObject { ["role"] = ToWireRole(m.Role) };

            if (m.Content is not null) msg["content"] = m.Content;
            if (m.ToolCallId is not null) msg["tool_call_id"] = m.ToolCallId;

            if (m.ToolCalls is { Count: > 0 } calls)
            {
                var tcArr = new JsonArray();
                foreach (var tc in calls)
                {
                    tcArr.Add(new JsonObject
                    {
                        ["id"] = tc.Id,
                        ["type"] = "function",
                        ["function"] = new JsonObject
                        {
                            ["name"] = tc.Name,
                            ["arguments"] = tc.ArgumentsJson,
                        },
                    });
                }
                msg["tool_calls"] = tcArr;
            }

            arr.Add(msg);
        }
        return arr;
    }

    private static string ToWireRole(MessageRole role) => role switch
    {
        MessageRole.User => "user",
        MessageRole.Assistant => "assistant",
        MessageRole.System => "system",
        MessageRole.Tool => "tool",
        _ => "user",
    };

    private static FinishReason MapFinishReason(string reason) => reason switch
    {
        "stop" => FinishReason.Stop,
        "tool_calls" => FinishReason.ToolCalls,
        "length" => FinishReason.Length,
        _ => FinishReason.Stop,
    };

    // Mutable builder for accumulating streamed tool-call pieces.
    private sealed class ToolCallBuilder
    {
        public string? Id;
        public string? Name;
        public StringBuilder Arguments { get; } = new();
    }

    // Wire DTOs for response parsing - kept private since they only describe the boundary with xAI.

    private sealed record StreamChunk(
        [property: JsonPropertyName("choices")] List<StreamChoice>? Choices);

    private sealed record StreamChoice(
        [property: JsonPropertyName("delta")] StreamDelta? Delta,
        [property: JsonPropertyName("finish_reason")] string? FinishReason);

    // `reasoning_content` is xAI/Grok 4's parallel stream channel for the
    // model's chain-of-thought (same shape used by DeepSeek-R1 and others).
    // Treated separately from `content` so the UI can render a "thinking"
    // panel without polluting the final assistant message body.
    private sealed record StreamDelta(
        [property: JsonPropertyName("content")] string? Content,
        [property: JsonPropertyName("reasoning_content")] string? ReasoningContent,
        [property: JsonPropertyName("tool_calls")] List<StreamToolCallDelta>? ToolCalls);

    private sealed record StreamToolCallDelta(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("function")] StreamFunctionDelta? Function);

    private sealed record StreamFunctionDelta(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("arguments")] string? Arguments);
}
