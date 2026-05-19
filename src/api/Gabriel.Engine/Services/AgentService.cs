using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
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
    // Bounded retries for the "provider finished Stop with empty text" hiccup.
    // The HTTP resilience pipeline can't catch this - the response itself was a
    // successful 200 stream - so retry has to live in the agent loop. Two extra
    // attempts (= three total) is enough to ride through transient blanks
    // without noticeably slowing down a real failure path. Linear backoff:
    // attempt N waits N * EmptyStopRetryDelayMs.
    private const int EmptyStopMaxRetries = 2;
    private const int EmptyStopRetryDelayMs = 500;

    private readonly IConversationRepository _conversations;
    private readonly IProjectRepository _projects;
    private readonly IChatProviderRegistry _providerRegistry;
    private readonly IModelCatalog _modelCatalog;
    private readonly IUserPreferences _userPrefs;
    private readonly IToolRegistry _tools;
    private readonly IToolExecutionContext _toolContext;
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
        IProjectRepository projects,
        IChatProviderRegistry providerRegistry,
        IModelCatalog modelCatalog,
        IUserPreferences userPrefs,
        IToolRegistry tools,
        IToolExecutionContext toolContext,
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
        _projects = projects;
        _providerRegistry = providerRegistry;
        _modelCatalog = modelCatalog;
        _userPrefs = userPrefs;
        _tools = tools;
        _toolContext = toolContext;
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
        // Validate up-front so we throw cleanly BEFORE the SSE headers are sent -
        // this lets the global exception handler return 4xx with ProblemDetails.
        if (string.IsNullOrWhiteSpace(userInput))
            throw new DomainException("Message content cannot be empty.");

        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        _logger.LogInformation(
            "ReAct turn start | conv={ConversationId} project={ProjectId} userId={UserId} inputChars={InputLen} messageCount={MsgCount}",
            conversationId, conversation.ProjectId, userId, userInput.Length, conversation.Messages.Count);

        // Persist the user message before starting the stream so the timeline is
        // consistent even if the client disconnects mid-loop. State is updated in
        // the same save - the new state feeds the per-turn system prompt and the
        // post-processor when the reply lands.
        var userMessage = conversation.AppendUserMessage(userInput);
        _conversations.AddMessage(userMessage);
        var newState = _stateUpdater.Update(conversation.GetState(), userInput);
        conversation.SetState(newState);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);

        // Resolve the model for this turn (per-user preference → config default).
        // We commit to a single selection up front and pass it through every
        // helper so the compact threshold, the provider call, and the metrics
        // calculation all agree.
        var selection = await ResolveModelSelectionAsync(ct);

        // Compact here (between turns), never mid-iteration - cutting between an
        // assistant's tool_calls and its tool results would orphan them.
        await MaybeCompactAsync(conversation, selection, ct);

        // Load the project's SystemPrompt once per turn (cheap indexed read).
        // Threaded into the iterator so each iteration re-uses the same prompt
        // without re-querying the DB.
        var projectPrompt = await LoadProjectSystemPromptAsync(conversation, userId, ct);

        // Populate the scoped tool-execution context so project-aware tools
        // (list_project_files / read_project_file) know which project to scope
        // to without having the model fill in the projectId itself.
        _toolContext.Set(conversation.Id, userId, conversation.ProjectId);

        return RunStreamAsync(conversation, variantGroupIdOverride: null, projectPrompt, selection, ct);
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

        _logger.LogInformation(
            "ReAct regenerate | conv={ConversationId} project={ProjectId} userId={UserId} targetMsg={TargetMessageId}",
            conversationId, conversation.ProjectId, userId, assistantMessageId);

        var target = conversation.Messages.FirstOrDefault(m => m.Id == assistantMessageId)
            ?? throw new NotFoundException(nameof(Message), assistantMessageId);

        if (target.Role != MessageRole.Assistant)
            throw new DomainException("Can only regenerate assistant messages.");

        if (!target.IsActiveVariant)
            throw new DomainException("Cannot regenerate an inactive variant - switch to it first.");

        // Deactivate the chosen variant group so the next history assembly sees
        // the old reply (and any sibling tool messages tagged with this group)
        // as inactive. The new turn re-uses the same group id so the picker UI
        // can navigate between the alternatives.
        var variantGroupId = target.VariantGroupId;
        conversation.DeactivateVariantGroup(variantGroupId);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);

        var selection = await ResolveModelSelectionAsync(ct);

        // No new user message + no state update - state was set when the user
        // originally sent this turn, and we're regenerating against that same
        // state. Compact between turns as usual.
        await MaybeCompactAsync(conversation, selection, ct);

        var projectPrompt = await LoadProjectSystemPromptAsync(conversation, userId, ct);
        _toolContext.Set(conversation.Id, userId, conversation.ProjectId);

        return RunStreamAsync(conversation, variantGroupIdOverride: variantGroupId, projectPrompt, selection, ct);
    }

    // Centralised resolution: every entry point goes through this so a user
    // who flips their preferred model on the settings page sees the change
    // take effect on the very next turn.
    private async Task<ModelSelection> ResolveModelSelectionAsync(CancellationToken ct)
    {
        var prefs = await _userPrefs.GetAsync(ct);
        return _modelCatalog.Resolve(prefs.PreferredProvider, prefs.PreferredModel);
    }

    // One-shot lookup of the project's SystemPrompt for this turn. Returns null
    // when the conversation has no ProjectId yet (legacy pre-Phase-8 data that
    // hasn't been backfilled) or the project has no prompt set.
    private async Task<string?> LoadProjectSystemPromptAsync(Conversation conversation, Guid userId, CancellationToken ct)
    {
        if (conversation.ProjectId is not { } pid) return null;
        var project = await _projects.GetByIdAsync(pid, userId, ct);
        return project?.SystemPrompt;
    }

    private async IAsyncEnumerable<AgentEvent> RunStreamAsync(
        Conversation conversation,
        Guid? variantGroupIdOverride,
        string? projectSystemPrompt,
        ModelSelection selection,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var toolDescriptors = _tools.AsDescriptors();
        var provider = _providerRegistry.Resolve(selection.Provider);

        for (var iter = 0; iter < _options.MaxIterations; iter++)
        {
            var textBuffer = new StringBuilder();
            var reasoningBuffer = new StringBuilder();
            var pendingCalls = new List<ChatProviderToolCall>();
            FinishReason? finish = null;

            // Provider hiccups land here: finish=Stop with no text, no tool
            // calls, and (usually) no reasoning either. Retry the same history
            // a small bounded number of times before surfacing the error - the
            // HTTP resilience handler can't catch this because the response was
            // a successful 200 stream that just happened to be empty.
            for (var emptyRetry = 0; emptyRetry <= EmptyStopMaxRetries; emptyRetry++)
            {
                if (emptyRetry > 0)
                {
                    textBuffer.Clear();
                    reasoningBuffer.Clear();
                    pendingCalls.Clear();
                    finish = null;
                    await Task.Delay(EmptyStopRetryDelayMs * emptyRetry, ct);
                }

                var history = ToProviderHistory(conversation, projectSystemPrompt);
                await foreach (var evt in provider.StreamAsync(history, toolDescriptors, selection.Name, ct))
                {
                    switch (evt)
                    {
                        case TextDeltaEvent td:
                            textBuffer.Append(td.Delta);
                            yield return new AgentTextDelta(td.Delta);
                            break;

                        case ReasoningDeltaEvent rd:
                            reasoningBuffer.Append(rd.Delta);
                            yield return new AgentReasoningDelta(rd.Delta);
                            break;

                        case ToolCallReadyEvent tc:
                            pendingCalls.Add(new ChatProviderToolCall(tc.Id, tc.Name, tc.ArgumentsJson));
                            break;

                        case FinishEvent fe:
                            finish = fe.Reason;
                            break;
                    }
                }

                // Only retry when the attempt produced literally nothing the
                // caller can act on. Partial output (any text delta, any tool
                // call) commits this attempt - retrying would either duplicate
                // streamed content or replay tool calls.
                var emptyStop = finish == FinishReason.Stop
                    && textBuffer.Length == 0
                    && pendingCalls.Count == 0;
                if (!emptyStop) break;

                if (emptyRetry < EmptyStopMaxRetries)
                {
                    _logger.LogWarning(
                        "Iter {Iter}: empty Stop from provider, retrying ({Retry}/{Max}) | conv={ConversationId}",
                        iter, emptyRetry + 1, EmptyStopMaxRetries, conversation.Id);
                }
            }

            if (finish == FinishReason.ToolCalls && pendingCalls.Count > 0)
            {
                var toolCallsJson = SerializeToolCalls(pendingCalls);
                // OpenAI/xAI allow assistant messages to carry both text and tool_calls;
                // preserve any leading reasoning text we accumulated.
                var leadingText = textBuffer.Length > 0 ? textBuffer.ToString() : null;
                var reasoningForCall = reasoningBuffer.Length > 0 ? reasoningBuffer.ToString() : null;
                var assistantMessage = conversation.AppendAssistantToolCalls(toolCallsJson, leadingText, variantGroupIdOverride, reasoningForCall);
                _conversations.AddMessage(assistantMessage);
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Iter {Iter}: model requested {ToolCount} tool call(s) | conv={ConversationId} reasoningChars={ReasoningLen}",
                    iter, pendingCalls.Count, conversation.Id, reasoningForCall?.Length ?? 0);

                foreach (var call in pendingCalls)
                {
                    yield return new AgentToolCall(assistantMessage.Id, call.Id, call.Name, call.ArgumentsJson);

                    var observation = await ExecuteToolSafelyAsync(call, conversation.Id, ct);

                    var toolMessage = conversation.AppendToolResult(call.Id, observation, variantGroupIdOverride);
                    _conversations.AddMessage(toolMessage);
                    await _uow.SaveChangesAsync(ct);

                    yield return new AgentToolResult(toolMessage.Id, call.Id, observation);
                }

                // Next iteration - provider will see the tool results and (usually) reply with text.
                continue;
            }

            if (finish == FinishReason.Stop)
            {
                var rawText = textBuffer.ToString();
                if (string.IsNullOrWhiteSpace(rawText))
                {
                    // Exhausted retries above and still got nothing - surface
                    // the failure to the client. No assistant message is
                    // persisted (rule: only finished messages reach the DB).
                    _logger.LogWarning(
                        "Iter {Iter}: provider finished Stop with empty text after {Max} retries | conv={ConversationId}",
                        iter, EmptyStopMaxRetries, conversation.Id);
                    yield return new AgentError("Provider returned an empty response.");
                    yield return new AgentDone();
                    yield break;
                }
                // Persist the raw streamed text so a reload of the conversation
                // shows exactly what the client rendered live. The post-processor
                // still runs to scrub residual AI-ism openers/closers; its length
                // cap was removed because it could truncate the persisted message
                // to less than what the user actually saw on screen. Fall back to
                // raw if the cleaner stripped it to empty - Message.Create rejects
                // empty assistant content.
                var cleaned = _postProcessor.Clean(rawText, conversation.GetState());
                var toPersist = string.IsNullOrWhiteSpace(cleaned) ? rawText : cleaned;
                var reasoningForFinal = reasoningBuffer.Length > 0 ? reasoningBuffer.ToString() : null;
                var assistantMessage = conversation.AppendAssistantText(toPersist, variantGroupIdOverride, reasoningForFinal);
                _conversations.AddMessage(assistantMessage);
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "ReAct turn complete | conv={ConversationId} iters={Iters} rawChars={RawLen} cleanChars={CleanLen} reasoningChars={ReasoningLen}",
                    conversation.Id, iter + 1, rawText.Length, toPersist.Length, reasoningForFinal?.Length ?? 0);

                yield return new AgentAssistantMessage(assistantMessage.Id, rawText, reasoningForFinal);
                yield return new AgentDone();
                yield break;
            }

            // Length / Error / unexpected - bail out.
            _logger.LogWarning(
                "Iter {Iter}: provider finished unexpectedly with {Finish} | conv={ConversationId}",
                iter, finish?.ToString() ?? "no-finish-event", conversation.Id);
            yield return new AgentError($"Provider finished unexpectedly: {finish?.ToString() ?? "no finish event"}.");
            yield return new AgentDone();
            yield break;
        }

        _logger.LogWarning(
            "Hit MaxIterations={Max} without final answer | conv={ConversationId}",
            _options.MaxIterations, conversation.Id);

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
    private async Task MaybeCompactAsync(Conversation conv, ModelSelection selection, CancellationToken ct)
    {
        var window = selection.ContextWindowTokens;
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
            newSummary = await GenerateSummaryAsync(conv.Summary, toSummarize, selection, ct);
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
    // until we land on a User message - that's our cut boundary. Doing this avoids
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
        ModelSelection selection,
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
        var provider = _providerRegistry.Resolve(selection.Provider);
        await foreach (var evt in provider.StreamAsync(history, Array.Empty<ToolDescriptor>(), selection.Name, ct))
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

    public async Task<ContextMetrics> GetContextMetricsAsync(
        Guid conversationId,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        var selection = await ResolveModelSelectionAsync(ct);
        var window = selection.ContextWindowTokens;
        var ratio = _options.CompactThreshold;
        // Match MaybeCompactAsync's calculation exactly so the UI's "trigger
        // line" lands on the same number the backend would actually trip on.
        var thresholdTokens = window > 0 ? (int)(window * ratio) : 0;

        var summaryTokens = _tokens.EstimateText(conversation.Summary);
        var postCut = MessagesAfterCut(conversation).ToList();
        var currentTokens = summaryTokens + _tokens.EstimateMessages(postCut);

        return new ContextMetrics(
            CurrentTokens: currentTokens,
            ContextWindowTokens: window,
            CompactThresholdTokens: thresholdTokens,
            CompactThresholdRatio: ratio,
            MessagesAfterCut: postCut.Count,
            IsSummarized: conversation.SummarizedThroughMessageId is not null,
            SummaryTokens: summaryTokens);
    }

    // --- Tool execution ----------------------------------------------------------

    // Maximum chars we put into a log message for tool args / results. Big
    // payloads (a fetched web page, a 12k-char file read) would otherwise
    // bloat the log file without adding diagnostic value.
    private const int LogPreviewLimit = 240;

    private async Task<string> ExecuteToolSafelyAsync(ChatProviderToolCall call, Guid conversationId, CancellationToken ct)
    {
        var tool = _tools.Find(call.Name);
        if (tool is null)
        {
            _logger.LogWarning(
                "Tool call REJECTED - unknown tool | conv={ConversationId} tool={Tool} callId={CallId} args={Args}",
                conversationId, call.Name, call.Id, Preview(call.ArgumentsJson));
            return $"Error: tool '{call.Name}' is not registered.";
        }

        _logger.LogInformation(
            "Tool call START | conv={ConversationId} tool={Tool} callId={CallId} args={Args}",
            conversationId, call.Name, call.Id, Preview(call.ArgumentsJson));

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var observation = await tool.ExecuteAsync(call.ArgumentsJson, ct);
            sw.Stop();

            // A tool's "soft error" (it returns a string starting with "Error:"
            // rather than throwing) is a meaningful signal - log at Warning so
            // it stands out from successful executions.
            var isSoftError = observation?.StartsWith("Error", StringComparison.OrdinalIgnoreCase) == true;
            var resultLen = observation?.Length ?? 0;

            if (isSoftError)
            {
                _logger.LogWarning(
                    "Tool call SOFT-ERROR | conv={ConversationId} tool={Tool} callId={CallId} elapsedMs={ElapsedMs} resultLen={ResultLen} result={Result}",
                    conversationId, call.Name, call.Id, sw.ElapsedMilliseconds, resultLen, Preview(observation));
            }
            else
            {
                _logger.LogInformation(
                    "Tool call OK | conv={ConversationId} tool={Tool} callId={CallId} elapsedMs={ElapsedMs} resultLen={ResultLen} resultPreview={Preview}",
                    conversationId, call.Name, call.Id, sw.ElapsedMilliseconds, resultLen, Preview(observation));
            }
            return observation ?? string.Empty;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Tool call THREW | conv={ConversationId} tool={Tool} callId={CallId} elapsedMs={ElapsedMs}",
                conversationId, call.Name, call.Id, sw.ElapsedMilliseconds);
            return $"Error executing {call.Name}: {ex.Message}";
        }
    }

    private static string Preview(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "(empty)";
        // Collapse newlines so multi-line tool output stays on one log line.
        var flat = text.Replace('\n', ' ').Replace('\r', ' ');
        return flat.Length <= LogPreviewLimit ? flat : flat[..LogPreviewLimit] + "…";
    }

    // --- History assembly --------------------------------------------------------

    // Builds the message list sent to the provider:
    //   1. Persona system message (static persona + per-turn dynamic guidance)
    //   2. (Optional) Project SystemPrompt - Phase 8 per-project personality override
    //   3. (Optional) system message containing the rolling summary
    //   4. All messages strictly after SummarizedThroughMessageId - filtered to
    //      active variants. Tool messages are additionally required to point at
    //      an active assistant's tool_call.id, so orphaned tool aftermath from
    //      a deactivated regen turn never reaches the provider.
    private IReadOnlyList<ChatProviderMessage> ToProviderHistory(Conversation conv, string? projectSystemPrompt)
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

        var result = new List<ChatProviderMessage>(messages.Count - startIdx + 3);

        // Persona prompt always goes first - it's the "who you are" header. State
        // may be null on the very first call before the updater has run; the
        // builder handles null gracefully.
        result.Add(new ChatProviderMessage(
            MessageRole.System,
            _promptBuilder.Build(conv.GetState())));

        // Per-project personality override. Sits between the global persona and
        // the rolling summary so the model treats it as additional identity
        // context BEFORE absorbing the conversation's specific history.
        if (!string.IsNullOrWhiteSpace(projectSystemPrompt))
        {
            result.Add(new ChatProviderMessage(
                MessageRole.System,
                $"[Project context]\n{projectSystemPrompt}"));
        }

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
            // assistant's tool_call.id is still in the active set - this catches
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
