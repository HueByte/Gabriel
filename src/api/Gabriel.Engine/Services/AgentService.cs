using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Personality;
using Gabriel.Core.Repositories;
using Gabriel.Engine.Personality;
using Gabriel.Engine.Providers;
using Gabriel.Engine.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gabriel.Engine.Services;

public class AgentService : IAgentService
{
    private readonly IConversationRepository _conversations;
    private readonly IChatProvider _provider;
    private readonly IToolRegistry _tools;
    private readonly IUnitOfWork _uow;
    private readonly ITokenEstimator _tokens;
    private readonly ICurrentUser _currentUser;
    private readonly IConversationStateUpdater _stateUpdater;
    private readonly ISystemPromptBuilder _promptBuilder;
    private readonly IResponsePostProcessor _postProcessor;
    private readonly AgentOptions _options;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        IConversationRepository conversations,
        IChatProvider provider,
        IToolRegistry tools,
        IUnitOfWork uow,
        ITokenEstimator tokens,
        ICurrentUser currentUser,
        IConversationStateUpdater stateUpdater,
        ISystemPromptBuilder promptBuilder,
        IResponsePostProcessor postProcessor,
        IOptions<AgentOptions> options,
        ILogger<AgentService> logger)
    {
        _conversations = conversations;
        _provider = provider;
        _tools = tools;
        _uow = uow;
        _tokens = tokens;
        _currentUser = currentUser;
        _stateUpdater = stateUpdater;
        _promptBuilder = promptBuilder;
        _postProcessor = postProcessor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IAsyncEnumerable<AgentEvent>> RunAsync(
        Guid conversationId,
        string userInput,
        CancellationToken ct = default)
    {
        // Validate up-front so we throw cleanly BEFORE the SSE headers are sent —
        // this lets the global exception handler return 4xx with ProblemDetails.
        if (string.IsNullOrWhiteSpace(userInput))
            throw new DomainException("Message content cannot be empty.");

        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        // Persist the user message before starting the stream so the timeline is
        // consistent even if the client disconnects mid-loop. State is updated in
        // the same save — the new state feeds the per-turn system prompt and the
        // post-processor when the reply lands.
        var userMessage = conversation.AppendUserMessage(userInput);
        _conversations.AddMessage(userMessage);
        var newState = _stateUpdater.Update(conversation.GetState(), userInput);
        conversation.SetState(newState);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);

        // Compact here (between turns), never mid-iteration — cutting between an
        // assistant's tool_calls and its tool results would orphan them.
        await MaybeCompactAsync(conversation, ct);

        return RunStreamAsync(conversation, variantGroupIdOverride: null, ct);
    }

    public async Task<IAsyncEnumerable<AgentEvent>> RegenerateAsync(
        Guid conversationId,
        Guid assistantMessageId,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        var target = conversation.Messages.FirstOrDefault(m => m.Id == assistantMessageId)
            ?? throw new NotFoundException(nameof(Message), assistantMessageId);

        if (target.Role != MessageRole.Assistant)
            throw new DomainException("Can only regenerate assistant messages.");

        if (!target.IsActiveVariant)
            throw new DomainException("Cannot regenerate an inactive variant — switch to it first.");

        // Deactivate the chosen variant group so the next history assembly sees
        // the old reply (and any sibling tool messages tagged with this group)
        // as inactive. The new turn re-uses the same group id so the picker UI
        // can navigate between the alternatives.
        var variantGroupId = target.VariantGroupId;
        conversation.DeactivateVariantGroup(variantGroupId);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);

        // No new user message + no state update — state was set when the user
        // originally sent this turn, and we're regenerating against that same
        // state. Compact between turns as usual.
        await MaybeCompactAsync(conversation, ct);

        return RunStreamAsync(conversation, variantGroupIdOverride: variantGroupId, ct);
    }

    private async IAsyncEnumerable<AgentEvent> RunStreamAsync(
        Conversation conversation,
        Guid? variantGroupIdOverride,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var toolDescriptors = _tools.AsDescriptors();

        for (var iter = 0; iter < _options.MaxIterations; iter++)
        {
            var history = ToProviderHistory(conversation);
            var textBuffer = new StringBuilder();
            var pendingCalls = new List<ChatProviderToolCall>();
            FinishReason? finish = null;

            await foreach (var evt in _provider.StreamAsync(history, toolDescriptors, ct))
            {
                switch (evt)
                {
                    case TextDeltaEvent td:
                        textBuffer.Append(td.Delta);
                        yield return new AgentTextDelta(td.Delta);
                        break;

                    case ToolCallReadyEvent tc:
                        pendingCalls.Add(new ChatProviderToolCall(tc.Id, tc.Name, tc.ArgumentsJson));
                        break;

                    case FinishEvent fe:
                        finish = fe.Reason;
                        break;
                }
            }

            if (finish == FinishReason.ToolCalls && pendingCalls.Count > 0)
            {
                var toolCallsJson = SerializeToolCalls(pendingCalls);
                // OpenAI/xAI allow assistant messages to carry both text and tool_calls;
                // preserve any leading reasoning text we accumulated.
                var leadingText = textBuffer.Length > 0 ? textBuffer.ToString() : null;
                var assistantMessage = conversation.AppendAssistantToolCalls(toolCallsJson, leadingText, variantGroupIdOverride);
                _conversations.AddMessage(assistantMessage);
                await _uow.SaveChangesAsync(ct);

                foreach (var call in pendingCalls)
                {
                    yield return new AgentToolCall(assistantMessage.Id, call.Id, call.Name, call.ArgumentsJson);

                    var observation = await ExecuteToolSafelyAsync(call, ct);

                    var toolMessage = conversation.AppendToolResult(call.Id, observation, variantGroupIdOverride);
                    _conversations.AddMessage(toolMessage);
                    await _uow.SaveChangesAsync(ct);

                    yield return new AgentToolResult(toolMessage.Id, call.Id, observation);
                }

                // Next iteration — provider will see the tool results and (usually) reply with text.
                continue;
            }

            if (finish == FinishReason.Stop)
            {
                var rawText = textBuffer.ToString();
                if (string.IsNullOrWhiteSpace(rawText))
                {
                    yield return new AgentError("Provider returned an empty response.");
                    yield return new AgentDone();
                    yield break;
                }
                // Save the cleaned version to the DB so reloads show the persona-
                // safe form. Emit the RAW text in AgentAssistantMessage so the
                // live client view (which reconciles against this) doesn't visibly
                // swap mid-stream. Fall back to raw if the cleaner stripped it to
                // empty — Message.Create rejects empty assistant content.
                var cleaned = _postProcessor.Clean(rawText, conversation.GetState());
                var toPersist = string.IsNullOrWhiteSpace(cleaned) ? rawText : cleaned;
                var assistantMessage = conversation.AppendAssistantText(toPersist, variantGroupIdOverride);
                _conversations.AddMessage(assistantMessage);
                await _uow.SaveChangesAsync(ct);
                yield return new AgentAssistantMessage(assistantMessage.Id, rawText);
                yield return new AgentDone();
                yield break;
            }

            // Length / Error / unexpected — bail out.
            yield return new AgentError($"Provider finished unexpectedly: {finish?.ToString() ?? "no finish event"}.");
            yield return new AgentDone();
            yield break;
        }

        // Hit the iteration cap without the model giving a final answer.
        var giveup = conversation.AppendAssistantText(
            $"(stopped after {_options.MaxIterations} tool iterations without a final answer)",
            variantGroupIdOverride);
        _conversations.AddMessage(giveup);
        await _uow.SaveChangesAsync(ct);
        yield return new AgentAssistantMessage(giveup.Id, giveup.Content!);
        yield return new AgentDone();
    }

    // --- Compact / summarization -------------------------------------------------

    // Estimates the tokens we'd send right now and triggers a rolling-summary
    // compact if we're at/above the configured fraction of the provider's window.
    private async Task MaybeCompactAsync(Conversation conv, CancellationToken ct)
    {
        var window = _provider.ContextWindowTokens;
        if (window <= 0) return;
        var threshold = (int)(window * _options.CompactThreshold);

        var currentTokens = EstimateCurrentTokens(conv);
        if (currentTokens < threshold) return;

        var messages = conv.Messages.ToList();
        var cutIdx = SelectCompactCutIndex(messages, _options.CompactKeepLast);
        if (cutIdx <= 0) return;

        // Don't re-summarize ground we already covered.
        if (conv.SummarizedThroughMessageId is { } prevCutId)
        {
            var prevIdx = messages.FindIndex(m => m.Id == prevCutId);
            if (prevIdx >= 0 && cutIdx <= prevIdx + 1) return;
        }

        var toSummarize = messages.Take(cutIdx).ToList();
        if (toSummarize.Count == 0) return;

        string newSummary;
        try
        {
            newSummary = await GenerateSummaryAsync(conv.Summary, toSummarize, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Compact summary call failed; skipping compact");
            return;
        }

        if (string.IsNullOrWhiteSpace(newSummary))
        {
            _logger.LogWarning("Compact summary returned empty; skipping compact");
            return;
        }

        conv.UpdateSummary(newSummary, toSummarize[^1].Id);
        _conversations.Update(conv);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Compacted conversation {Id}: summarized {Cut} messages at ~{Tokens} tokens (threshold {Threshold})",
            conv.Id, toSummarize.Count, currentTokens, threshold);
    }

    // Walks back from the end keeping at least `keepLast` messages, then keeps walking
    // until we land on a User message — that's our cut boundary. Doing this avoids
    // ever cutting between an assistant's tool_calls and the matching tool results,
    // which the model needs to see together.
    private static int SelectCompactCutIndex(IReadOnlyList<Message> messages, int keepLast)
    {
        var idx = messages.Count - keepLast;
        if (idx <= 0) return 0;
        while (idx > 0 && messages[idx].Role != MessageRole.User)
        {
            idx--;
        }
        return idx;
    }

    private async Task<string> GenerateSummaryAsync(
        string? previousSummary,
        IReadOnlyList<Message> toSummarize,
        CancellationToken ct)
    {
        const string system =
            "You are a conversation summarizer. Produce a concise, factual summary that preserves: " +
            "key facts established, decisions made, ongoing threads, and user preferences. " +
            "Skip greetings and chit-chat. Do not preface with phrases like 'Here is the summary'.";

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(previousSummary))
        {
            sb.AppendLine("Existing summary of even earlier turns:");
            sb.AppendLine(previousSummary);
            sb.AppendLine();
            sb.AppendLine("New turns to fold into the summary:");
        }
        else
        {
            sb.AppendLine("Conversation to summarize:");
        }

        foreach (var m in toSummarize)
        {
            var content = m.Content ?? (m.ToolCallsJson is not null ? "(requested tools)" : "(no content)");
            sb.Append('[').Append(m.Role).Append("] ").AppendLine(content);
        }

        var history = new List<ChatProviderMessage>
        {
            new(MessageRole.System, system),
            new(MessageRole.User, sb.ToString()),
        };

        var text = new StringBuilder();
        await foreach (var evt in _provider.StreamAsync(history, Array.Empty<ToolDescriptor>(), ct))
        {
            if (evt is TextDeltaEvent td) text.Append(td.Delta);
            else if (evt is FinishEvent) break;
        }
        return text.ToString().Trim();
    }

    private int EstimateCurrentTokens(Conversation conv)
    {
        var summaryTokens = _tokens.EstimateText(conv.Summary);
        var postCut = MessagesAfterCut(conv);
        return summaryTokens + _tokens.EstimateMessages(postCut);
    }

    private static IEnumerable<Message> MessagesAfterCut(Conversation conv)
    {
        if (conv.SummarizedThroughMessageId is null)
            return conv.Messages;

        var cutId = conv.SummarizedThroughMessageId.Value;
        var idx = conv.Messages.ToList().FindIndex(m => m.Id == cutId);
        return idx < 0 ? conv.Messages : conv.Messages.Skip(idx + 1);
    }

    // --- Tool execution ----------------------------------------------------------

    private async Task<string> ExecuteToolSafelyAsync(ChatProviderToolCall call, CancellationToken ct)
    {
        var tool = _tools.Find(call.Name);
        if (tool is null)
        {
            _logger.LogWarning("Model called unknown tool {Tool}", call.Name);
            return $"Error: tool '{call.Name}' is not registered.";
        }

        try
        {
            return await tool.ExecuteAsync(call.ArgumentsJson, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool {Tool} threw", call.Name);
            return $"Error executing {call.Name}: {ex.Message}";
        }
    }

    // --- History assembly --------------------------------------------------------

    // Builds the message list sent to the provider:
    //   1. Persona system message (static persona + per-turn dynamic guidance)
    //   2. (Optional) system message containing the rolling summary
    //   3. All messages strictly after SummarizedThroughMessageId — filtered to
    //      active variants. Tool messages are additionally required to point at
    //      an active assistant's tool_call.id, so orphaned tool aftermath from
    //      a deactivated regen turn never reaches the provider.
    private IReadOnlyList<ChatProviderMessage> ToProviderHistory(Conversation conv)
    {
        var messages = conv.Messages.ToList();
        var startIdx = 0;
        if (conv.SummarizedThroughMessageId is { } cutId)
        {
            var cutIdx = messages.FindIndex(m => m.Id == cutId);
            if (cutIdx >= 0) startIdx = cutIdx + 1;
        }

        // Collect tool_call.ids referenced by active assistant messages so we
        // can keep their tool results (and drop everyone else's).
        var activeToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        for (var i = startIdx; i < messages.Count; i++)
        {
            var m = messages[i];
            if (m.Role != MessageRole.Assistant || !m.IsActiveVariant) continue;
            if (string.IsNullOrEmpty(m.ToolCallsJson)) continue;

            using var doc = JsonDocument.Parse(m.ToolCallsJson);
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var id = el.GetProperty("id").GetString();
                if (id is not null) activeToolCallIds.Add(id);
            }
        }

        var result = new List<ChatProviderMessage>(messages.Count - startIdx + 2);

        // Persona prompt always goes first — it's the "who you are" header. State
        // may be null on the very first call before the updater has run; the
        // builder handles null gracefully.
        result.Add(new ChatProviderMessage(
            MessageRole.System,
            _promptBuilder.Build(conv.GetState())));

        if (!string.IsNullOrEmpty(conv.Summary))
        {
            result.Add(new ChatProviderMessage(
                MessageRole.System,
                $"[Summary of earlier conversation]\n{conv.Summary}"));
        }

        for (var i = startIdx; i < messages.Count; i++)
        {
            var m = messages[i];

            // Filter inactive variants. Tool messages are kept iff their parent
            // assistant's tool_call.id is still in the active set — this catches
            // legacy tool messages that were created before variant grouping
            // covered the tool aftermath.
            if (m.Role == MessageRole.Tool)
            {
                if (m.ToolCallId is null || !activeToolCallIds.Contains(m.ToolCallId)) continue;
            }
            else if (!m.IsActiveVariant)
            {
                continue;
            }

            List<ChatProviderToolCall>? toolCalls = null;
            if (!string.IsNullOrEmpty(m.ToolCallsJson))
            {
                using var doc = JsonDocument.Parse(m.ToolCallsJson);
                toolCalls = doc.RootElement.EnumerateArray().Select(el => new ChatProviderToolCall(
                    Id: el.GetProperty("id").GetString()!,
                    Name: el.GetProperty("function").GetProperty("name").GetString()!,
                    ArgumentsJson: el.GetProperty("function").GetProperty("arguments").GetString()!
                )).ToList();
            }
            result.Add(new ChatProviderMessage(m.Role, m.Content, m.ToolCallId, toolCalls));
        }
        return result;
    }

    private static string SerializeToolCalls(IReadOnlyList<ChatProviderToolCall> calls)
    {
        var arr = calls.Select(c => new
        {
            id = c.Id,
            type = "function",
            function = new { name = c.Name, arguments = c.ArgumentsJson },
        });
        return JsonSerializer.Serialize(arr);
    }
}
