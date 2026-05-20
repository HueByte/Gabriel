using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Microsoft.Extensions.Logging;

namespace Gabriel.Engine.Providers.ToolBridge;

// "Bridges" a tool-incapable provider (a local Ollama model, a small
// commercial model without function calling) to the same IChatProvider
// surface as a native tool-calling provider. Strategy:
//
//   1. Pre-call: rewrite the message history so prior structured tool calls
//      become inline <tool_call> markers in the assistant's text, and prior
//      Tool-role results become labelled User messages (since non-native
//      protocols don't have a Tool role).
//   2. Pre-call: inject a system message at the end of the system block
//      describing the tools and the wire format.
//   3. Stream the inner provider's text deltas through ToolCallStreamSplitter:
//      live-emit deltas until "<tool_call>" is seen, then buffer the tail.
//   4. After stream completes, run ToolCallBlockParser over the buffered
//      tail. On success, synthesise ToolCallReadyEvents and finish with
//      ToolCalls. On parse failure, append a fix-up user message and retry
//      up to MaxParseRetries times.
//
// The agent loop sees the same event shape it gets from native providers,
// so RunStreamAsync / AgentContext / the UI stay unchanged. The bridge does
// NOT execute tools - that still happens server-side in
// AgentService.ExecuteToolSafelyAsync via ITool.ExecuteAsync. This class
// only bridges the wire protocol.
//
// One UX tradeoff worth flagging: live streaming happens on attempt 1 only.
// If the parse fails and we retry, the retry's text is NOT streamed (we
// can't unsend what we already yielded) and we only emit its tool calls. In
// practice the typewriter effect is preserved for the common case (single
// attempt, success) and gracefully degrades on the rare retry path.
public sealed class GabrielToolBridge : IChatProvider
{
    // Three attempts total = original + 2 retries. Matches the
    // EmptyStopMaxRetries pattern already used elsewhere in the agent loop.
    private const int MaxParseRetries = 2;

    private readonly IChatProvider _inner;
    private readonly ILogger<GabrielToolBridge> _logger;

    public GabrielToolBridge(IChatProvider inner, ILogger<GabrielToolBridge> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    // Identity passes through - the agent registry still resolves by the
    // base provider's name. Same for Models: the decorator is wrapping a
    // specific provider that owns those entries.
    public string Name => _inner.Name;
    public IReadOnlyList<LLMModel> Models => _inner.Models;

    public async IAsyncEnumerable<ChatProviderEvent> StreamAsync(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools,
        string modelName,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var translatedHistory = TranslateHistory(history);
        var workingHistory = InjectToolDocs(translatedHistory, tools);

        // Attempt 1 streams live; attempts >= 2 buffer everything (we can't
        // retract what we already streamed, so we just suppress the retry's
        // textual half and emit only the corrected tool calls).
        for (var attempt = 0; attempt <= MaxParseRetries; attempt++)
        {
            var liveMode = attempt == 0;
            var splitter = new ToolCallStreamSplitter();
            var retryBuffer = new StringBuilder();
            var nativeFinish = FinishReason.Stop;

            // Inner is called with an empty tools list - the descriptors
            // are already in the system prompt via InjectToolDocs, and the
            // inner provider wouldn't know what to do with them in the
            // sibling field anyway.
            await foreach (var evt in _inner.StreamAsync(workingHistory, Array.Empty<ToolDescriptor>(), modelName, ct))
            {
                switch (evt)
                {
                    case TextDeltaEvent td:
                        if (liveMode)
                        {
                            var safe = splitter.Process(td.Delta);
                            if (safe.Length > 0)
                                yield return new TextDeltaEvent(safe);
                        }
                        else
                        {
                            retryBuffer.Append(td.Delta);
                        }
                        break;

                    case ReasoningDeltaEvent rd:
                        // Reasoning channel passes through unchanged - some
                        // emulated-tools providers (DeepSeek-R1 distills,
                        // open-weight thinking models) do still expose this.
                        yield return rd;
                        break;

                    case FinishEvent fe:
                        nativeFinish = fe.Reason;
                        break;

                    // The inner provider shouldn't emit native tool calls
                    // (we passed it an empty tools list) but if it does we
                    // pass them straight through - means the underlying model
                    // had hidden tool support and the decorator was wired up
                    // unnecessarily, but the safest thing is honour it.
                    case ToolCallReadyEvent tc:
                        yield return tc;
                        break;
                }
            }

            // Flush any held candidate that never grew into a marker. Only
            // matters in live mode; in retry mode everything's already in
            // retryBuffer.
            if (liveMode)
            {
                var trailing = splitter.Flush();
                if (trailing.Length > 0)
                    yield return new TextDeltaEvent(trailing);
            }

            // Figure out where the tool-call-candidate text lives.
            var bufferedTail = liveMode ? splitter.BufferedText : retryBuffer.ToString();

            // If no <tool_call> marker showed up at all, this is a pure-text
            // turn. Pass through whatever the inner finish reason was. In
            // retry mode we also need to emit the retry's text as one bulk
            // delta so the user gets to see it.
            if (liveMode && !splitter.InBufferMode)
            {
                yield return new FinishEvent(nativeFinish);
                yield break;
            }
            if (!liveMode && !bufferedTail.Contains("<tool_call>", StringComparison.Ordinal))
            {
                // Retry produced pure text. Emit it in one shot so the user
                // sees the corrected answer even though it didn't stream.
                if (bufferedTail.Length > 0)
                    yield return new TextDeltaEvent(bufferedTail);
                yield return new FinishEvent(nativeFinish);
                yield break;
            }

            // Yield rules forbid yield-from-catch, so capture the parse
            // outcome and branch after the try/catch settles.
            IReadOnlyList<ParsedToolCall>? calls = null;
            ToolCallParseException? parseError = null;
            try
            {
                calls = ToolCallBlockParser.Extract(bufferedTail);
            }
            catch (ToolCallParseException ex)
            {
                parseError = ex;
            }

            if (parseError is not null)
            {
                if (attempt >= MaxParseRetries)
                {
                    _logger.LogWarning(parseError,
                        "Emulated tool-call parse failed after {Attempts} attempts; surfacing error to agent | model={Model}",
                        attempt + 1, modelName);
                    yield return new FinishEvent(FinishReason.Error);
                    yield break;
                }

                _logger.LogInformation(
                    "Emulated tool-call parse failed on attempt {Attempt}/{Max}; appending fix-up message and retrying | model={Model}",
                    attempt + 1, MaxParseRetries + 1, modelName);
                workingHistory = AppendFixupMessage(workingHistory, bufferedTail, parseError);
                continue;
            }

            if (calls!.Count == 0)
            {
                // Open marker present but no closed pairs extracted -
                // parser would normally throw, so reaching here means a
                // pathological edge case. Bail to the native finish reason.
                yield return new FinishEvent(nativeFinish);
                yield break;
            }

            foreach (var call in calls)
            {
                yield return new ToolCallReadyEvent(call.Id, call.Name, call.ArgumentsJson);
            }
            yield return new FinishEvent(FinishReason.ToolCalls);
            yield break;
        }

        // Unreachable; the retry loop always yield-breaks.
        yield return new FinishEvent(FinishReason.Error);
    }

    // --- History translation -------------------------------------------------

    // Rewrites the assembled history so the inner (non-tool-capable) model
    // sees its previous tool interactions in the wire format it understands:
    //   - Assistant messages with native ToolCalls → Content gains inline
    //     <tool_call>...</tool_call> blocks; ToolCalls field cleared so the
    //     ChatProviderMessage looks like a plain assistant text message.
    //   - Tool messages → User messages tagged "[Tool result: <name>] ...".
    //     The original tool name is recovered by matching ToolCallId against
    //     the preceding assistant's tool_calls list.
    private static IReadOnlyList<ChatProviderMessage> TranslateHistory(IReadOnlyList<ChatProviderMessage> history)
    {
        // Build an id → tool-name map by walking forward. Same iteration we
        // use to produce the translated list, just collected first so Tool
        // messages can always look back at known names.
        var idToName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var m in history)
        {
            if (m.Role == MessageRole.Assistant && m.ToolCalls is { Count: > 0 } calls)
            {
                foreach (var c in calls)
                {
                    idToName[c.Id] = c.Name;
                }
            }
        }

        var translated = new List<ChatProviderMessage>(history.Count);
        foreach (var m in history)
        {
            switch (m.Role)
            {
                case MessageRole.Assistant when m.ToolCalls is { Count: > 0 } calls:
                    {
                        var sb = new StringBuilder();
                        if (!string.IsNullOrEmpty(m.Content))
                        {
                            sb.Append(m.Content);
                            if (!m.Content.EndsWith('\n')) sb.Append('\n');
                        }
                        foreach (var c in calls)
                        {
                            sb.Append("<tool_call>");
                            sb.Append(SerializeCallInline(c.Name, c.ArgumentsJson));
                            sb.Append("</tool_call>");
                            sb.Append('\n');
                        }
                        translated.Add(new ChatProviderMessage(
                            Role: MessageRole.Assistant,
                            Content: sb.ToString().TrimEnd(),
                            ToolCallId: null,
                            ToolCalls: null));
                        break;
                    }

                case MessageRole.Tool:
                    {
                        var name = m.ToolCallId is not null && idToName.TryGetValue(m.ToolCallId, out var n)
                            ? n
                            : "unknown";
                        var content = $"[Tool result: {name}] {m.Content ?? string.Empty}";
                        translated.Add(new ChatProviderMessage(
                            Role: MessageRole.User,
                            Content: content,
                            ToolCallId: null,
                            ToolCalls: null));
                        break;
                    }

                default:
                    translated.Add(m);
                    break;
            }
        }
        return translated;
    }

    // Inline rendering of one assistant-emitted tool call. ArgumentsJson is
    // already a JSON object string; we just wrap it. Falls back to "{}" if
    // the stored args aren't valid JSON (shouldn't happen but defensive).
    private static string SerializeCallInline(string name, string argumentsJson)
    {
        var args = argumentsJson;
        if (string.IsNullOrWhiteSpace(args)) args = "{}";
        else
        {
            try { using var _ = JsonDocument.Parse(args); }
            catch (JsonException) { args = "{}"; }
        }
        return $"{{\"name\":\"{EscapeJsonString(name)}\",\"arguments\":{args}}}";
    }

    private static string EscapeJsonString(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    // --- Tool-doc injection --------------------------------------------------

    // Builds the system message that tells the model how to call tools. The
    // descriptors are rendered with name + description + the raw JSON schema
    // so the model can fill in arguments correctly. Inserted at the end of
    // the existing system-message block (right before the first
    // user/assistant/tool message) so the persona, project prompt, memory,
    // and summary all stay at the cacheable top of the prefix.
    private static IReadOnlyList<ChatProviderMessage> InjectToolDocs(
        IReadOnlyList<ChatProviderMessage> history,
        IReadOnlyList<ToolDescriptor> tools)
    {
        if (tools.Count == 0) return history;

        var docsMessage = new ChatProviderMessage(
            Role: MessageRole.System,
            Content: BuildToolDocs(tools));

        // Find the boundary between system messages and the conversation.
        var insertAt = history.Count;
        for (var i = 0; i < history.Count; i++)
        {
            if (history[i].Role != MessageRole.System)
            {
                insertAt = i;
                break;
            }
        }

        var result = new List<ChatProviderMessage>(history.Count + 1);
        result.AddRange(history.Take(insertAt));
        result.Add(docsMessage);
        result.AddRange(history.Skip(insertAt));
        return result;
    }

    private static string BuildToolDocs(IReadOnlyList<ToolDescriptor> tools)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Tool calling]");
        sb.AppendLine("You have access to the following tools. To use one, emit a block in this exact format, inline anywhere in your response:");
        sb.AppendLine();
        sb.AppendLine("<tool_call>{\"name\":\"<tool_name>\",\"arguments\":{...}}</tool_call>");
        sb.AppendLine();
        sb.AppendLine("Rules:");
        sb.AppendLine("- The block must contain valid JSON. The \"arguments\" field must be a JSON object - use {} if the tool takes no arguments.");
        sb.AppendLine("- Do not wrap the block in markdown code fences or backticks.");
        sb.AppendLine("- You may emit multiple <tool_call> blocks in one response.");
        sb.AppendLine("- After your tool calls run, their results will appear as user messages prefixed \"[Tool result: <tool_name>] ...\". Use those results to continue reasoning or compose your final answer.");
        sb.AppendLine("- When you have enough information to answer the user, respond with plain text and no <tool_call> blocks.");
        sb.AppendLine();
        sb.AppendLine("Available tools:");
        sb.AppendLine();
        foreach (var t in tools)
        {
            sb.Append("- ").Append(t.Name).Append(": ").AppendLine(t.Description);
            sb.Append("  Parameters: ").AppendLine(t.ParametersJsonSchema);
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    // --- Retry fix-up --------------------------------------------------------

    // After a parse failure, append two messages:
    //   - An assistant message containing what the model just emitted (so it
    //     sees its own broken output in the conversation history).
    //   - A user message quoting the offending block and asking for a fix.
    // The retry call re-streams from this augmented history.
    private static IReadOnlyList<ChatProviderMessage> AppendFixupMessage(
        IReadOnlyList<ChatProviderMessage> history,
        string fullModelOutput,
        ToolCallParseException ex)
    {
        var fixup = new StringBuilder();
        fixup.AppendLine("Your previous response contained an invalid tool call.");
        fixup.AppendLine();
        fixup.AppendLine("Offending block:");
        fixup.AppendLine(ex.OffendingBlock);
        fixup.AppendLine();
        fixup.Append("Parse error: ").AppendLine(ex.Message);
        fixup.AppendLine();
        fixup.Append("Please re-issue your response from scratch with valid <tool_call>{\"name\":\"...\",\"arguments\":{...}}</tool_call> blocks. ");
        fixup.Append("The \"arguments\" field must be a JSON object. Do not wrap the block in code fences.");

        var result = new List<ChatProviderMessage>(history.Count + 2);
        result.AddRange(history);
        result.Add(new ChatProviderMessage(MessageRole.Assistant, fullModelOutput));
        result.Add(new ChatProviderMessage(MessageRole.User, fixup.ToString()));
        return result;
    }
}
