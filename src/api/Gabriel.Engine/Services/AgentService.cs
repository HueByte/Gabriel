using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Gabriel.Core.Configuration;
using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Memory;
using Gabriel.Core.Personality;
using Gabriel.Core.Repositories;
using Gabriel.Core.Services;
using Gabriel.Engine.Personality;
using Gabriel.Engine.Providers;
using Gabriel.Engine.Providers.ToolBridge;
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
    private readonly IMemoryService _memories;
    private readonly ISemanticMemoryIndex _memoryIndex;
    private readonly SemanticMemoryOptions _semanticOptions;
    private readonly IToolRegistry _tools;
    private readonly IToolExecutionContext _toolContext;
    private readonly IUnitOfWork _uow;
    private readonly ITokenEstimator _tokens;
    private readonly ICurrentUser _currentUser;
    private readonly IConversationStateUpdater _stateUpdater;
    private readonly ISystemPromptBuilder _promptBuilder;
    private readonly IResponsePostProcessor _postProcessor;
    private readonly IConversationTitleGenerator _titleGenerator;
    private readonly IActiveTurnRegistry _turns;
    private readonly AgentOptions _options;
    private readonly ILogger<AgentService> _logger;
    // Held for constructing GabrielToolBridge on demand. The bridge is
    // instantiated per-call (it's stateless across calls) so AgentService
    // owns the logger; alternative would be a factory service but that's
    // extra plumbing for a one-liner.
    private readonly ILogger<GabrielToolBridge> _toolBridgeLogger;

    public AgentService(
        IConversationRepository conversations,
        IProjectRepository projects,
        IChatProviderRegistry providerRegistry,
        IModelCatalog modelCatalog,
        IUserPreferences userPrefs,
        IMemoryService memories,
        ISemanticMemoryIndex memoryIndex,
        IOptions<SemanticMemoryOptions> semanticOptions,
        IToolRegistry tools,
        IToolExecutionContext toolContext,
        IUnitOfWork uow,
        ITokenEstimator tokens,
        ICurrentUser currentUser,
        IConversationStateUpdater stateUpdater,
        ISystemPromptBuilder promptBuilder,
        IResponsePostProcessor postProcessor,
        IConversationTitleGenerator titleGenerator,
        IActiveTurnRegistry turns,
        IOptions<AgentOptions> options,
        ILogger<AgentService> logger,
        ILogger<GabrielToolBridge> toolBridgeLogger)
    {
        _conversations = conversations;
        _projects = projects;
        _providerRegistry = providerRegistry;
        _modelCatalog = modelCatalog;
        _userPrefs = userPrefs;
        _memories = memories;
        _memoryIndex = memoryIndex;
        _semanticOptions = semanticOptions.Value;
        _tools = tools;
        _toolContext = toolContext;
        _uow = uow;
        _tokens = tokens;
        _currentUser = currentUser;
        _stateUpdater = stateUpdater;
        _promptBuilder = promptBuilder;
        _postProcessor = postProcessor;
        _titleGenerator = titleGenerator;
        _turns = turns;
        _options = options.Value;
        _logger = logger;
        _toolBridgeLogger = toolBridgeLogger;
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

        var (userId, conversation) = await LoadConversationAsync(conversationId, ct);
        return await StartUserTurnAsync(userId, conversation, userInput, ct);
    }

    public async Task<IAsyncEnumerable<AgentEvent>> EditAndRunAsync(
        Guid conversationId,
        Guid userMessageId,
        string newContent,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new DomainException("Message content cannot be empty.");

        var (userId, conversation) = await LoadConversationAsync(conversationId, ct);

        var target = conversation.Messages.FirstOrDefault(m => m.Id == userMessageId)
            ?? throw new NotFoundException(nameof(Message), userMessageId);
        if (target.Role != MessageRole.User)
            throw new DomainException("Only user messages can be edited.");

        _logger.LogInformation(
            "ReAct edit-and-resend | conv={ConversationId} targetMsg={TargetMessageId}",
            conversationId, userMessageId);

        // Truncate first, in its own save, so the replacement message below is
        // appended to a history that no longer contains the old branch.
        var removed = conversation.TruncateFrom(userMessageId);
        _conversations.RemoveMessages(removed);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);

        return await StartUserTurnAsync(userId, conversation, newContent, ct);
    }

    public bool StopTurn(Guid conversationId)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");
        return _turns.TryStop(conversationId, userId);
    }

    // Shared tail of RunAsync and EditAndRunAsync: persist the user message,
    // resolve model + prompts, and hand back the stream.
    private async Task<IAsyncEnumerable<AgentEvent>> StartUserTurnAsync(
        Guid userId,
        Conversation conversation,
        string userInput,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "ReAct turn start | conv={ConversationId} project={ProjectId} userId={UserId} inputChars={InputLen} messageCount={MsgCount}",
            conversation.Id, conversation.ProjectId, userId, userInput.Length, conversation.Messages.Count);

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

        // Load the stable-through-turn prompt pieces (persona / project /
        // memory / tools). These don't change between iterations and feed
        // both the compact decision and the stream itself. Selection is
        // passed in so ToolMode.None drops the tool descriptors at the
        // source rather than threading the flag through every consumer.
        var prompts = await LoadTurnPromptsAsync(conversation, userId, selection, ct);

        // Populate the scoped tool-execution context so project-aware tools
        // (list_project_files / read_project_file) know which project to scope
        // to without having the model fill in the projectId itself.
        _toolContext.Set(conversation.Id, userId, conversation.ProjectId);

        var pendingTitle = StartAutoTitle(conversation, selection, ct);

        // Compaction runs inside RunStreamAsync so it can yield CompactStart /
        // CompactDone events to the SSE wire - the user sees "Compacting…"
        // instead of staring at a silent HTTP request while a summary LLM
        // call burns 5-30s before the real turn begins.
        //
        // The user message was persisted above with `userMessage.Id`. The
        // wrapper yields a AgentUserMessagePersisted as the first event so the
        // client can swap its tmp-xxxxx user-entry id for the real DB id
        // without doing a "GET conversation" round-trip after the stream
        // ends. RegenerateAsync skips this preamble (no new user message).
        return RunStreamWithUserPreambleAsync(
            userMessage.Id, conversation, variantGroupIdOverride: null, prompts, selection, pendingTitle, ct);
    }

    private async IAsyncEnumerable<AgentEvent> RunStreamWithUserPreambleAsync(
        Guid userMessageId,
        Conversation conversation,
        Guid? variantGroupIdOverride,
        TurnPrompts prompts,
        ModelSelection selection,
        Task<string?>? pendingTitle,
        [EnumeratorCancellation] CancellationToken ct)
    {
        yield return new AgentUserMessagePersisted(userMessageId);
        await foreach (var evt in RunStreamAsync(conversation, variantGroupIdOverride, prompts, selection, pendingTitle, ct))
        {
            yield return evt;
        }
    }

    public async Task<IAsyncEnumerable<AgentEvent>> RegenerateAsync(
        Guid conversationId,
        Guid assistantMessageId,
        CancellationToken ct = default)
    {
        var (userId, conversation) = await LoadConversationAsync(conversationId, ct);

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
        // state. Compaction happens inside RunStreamAsync so it can yield
        // events (see RunAsync for the rationale).
        var prompts = await LoadTurnPromptsAsync(conversation, userId, selection, ct);

        _toolContext.Set(conversation.Id, userId, conversation.ProjectId);

        return RunStreamAsync(conversation, variantGroupIdOverride: variantGroupId, prompts, selection, pendingTitle: null, ct);
    }

    private async Task<(Guid UserId, Conversation Conversation)> LoadConversationAsync(
        Guid conversationId, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        return (userId, conversation);
    }

    // Centralised resolution: every entry point goes through this so a user
    // who flips their preferred model on the settings page sees the change
    // take effect on the very next turn.
    private async Task<ModelSelection> ResolveModelSelectionAsync(CancellationToken ct)
    {
        var prefs = await _userPrefs.GetAsync(ct);
        return _modelCatalog.Resolve(prefs.PreferredProvider, prefs.PreferredModel);
    }

    // Build the [Project context] block for this turn. Two responsibilities:
    //   1. Tell the agent which project the conversation lives in - without
    //      this, memory_save defaults to scope='user' because nothing in the
    //      system prompt signals that a project scope is even applicable.
    //   2. Inject the user-authored project SystemPrompt (if any) under that
    //      header.
    // Returns null for the Default project (standalone conversations) and for
    // legacy pre-Phase-8 data with no ProjectId.
    private async Task<string?> LoadProjectSystemPromptAsync(Conversation conversation, Guid userId, CancellationToken ct)
    {
        if (conversation.ProjectId is not { } pid) return null;
        var project = await _projects.GetByIdAsync(pid, userId, ct);
        if (project is null || project.IsDefault) return null;

        var sb = new StringBuilder();
        sb.Append("This conversation belongs to project '").Append(project.Name).AppendLine("'.");
        sb.AppendLine("When calling memory_save, default to scope='project' for facts that only matter to this project's work. Use scope='user' only when the user clearly intends the memory to follow them across every project.");
        if (!string.IsNullOrWhiteSpace(project.SystemPrompt))
        {
            sb.AppendLine();
            sb.Append(project.SystemPrompt);
        }
        return sb.ToString();
    }

    // Formats the user's saved memories as a single system message. Two modes:
    //
    //   Semantic memory OFF (default): every visible memory is injected
    //   verbatim - the original behavior, fine at small counts.
    //
    //   Semantic memory ON (Qdrant): every memory appears in a lightweight
    //   name+description index, but only the entries semantically closest to
    //   the latest user message get their full bodies expanded. This keeps
    //   the block's token cost roughly constant as the memory set grows; the
    //   memory_search tool covers mid-task recall of anything not expanded.
    //
    // Returns null when there's nothing to inject.
    private async Task<string?> LoadMemoryBlockAsync(Conversation conversation, Guid userId, CancellationToken ct)
    {
        var entries = await _memories.ListForConversationAsync(conversation.ProjectId, ct);
        if (entries.Count == 0) return null;

        if (!_memoryIndex.IsEnabled)
        {
            return BuildFullMemoryBlock(entries);
        }

        // Recall query: the latest active user message (already persisted by
        // the time turn prompts load, for both RunAsync and Regenerate).
        var query = conversation.Messages
            .LastOrDefault(m => m.Role == MessageRole.User && m.IsActiveVariant)?.Content;

        var recalled = new List<(MemoryEntry Entry, float Score)>();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var hits = await _memoryIndex.SearchAsync(
                query, userId, conversation.ProjectId,
                _semanticOptions.RecallTopK, _semanticOptions.RecallMinScore, ct);
            var byId = entries.ToDictionary(e => e.Id);
            foreach (var hit in hits)
            {
                if (byId.TryGetValue(hit.MemoryId, out var entry))
                {
                    recalled.Add((entry, hit.Score));
                }
            }
        }

        return BuildSemanticMemoryBlock(entries, recalled);
    }

    private static string BuildFullMemoryBlock(IReadOnlyList<MemoryEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Saved memories]");
        sb.AppendLine("Durable facts the user has asked Gabriel to remember. Apply these unless the user contradicts them in the current conversation. Each entry has a scope (user = applies everywhere; project = only in this project), a type (user/feedback/project/reference), and a body.");
        sb.AppendLine();

        var userScope = entries.Where(m => m.ProjectId is null).ToList();
        var projectScope = entries.Where(m => m.ProjectId is not null).ToList();

        if (userScope.Count > 0)
        {
            sb.AppendLine("## User-scope memories");
            foreach (var m in userScope) AppendMemory(sb, m);
            sb.AppendLine();
        }
        if (projectScope.Count > 0)
        {
            sb.AppendLine("## Project-scope memories");
            foreach (var m in projectScope) AppendMemory(sb, m);
        }

        return sb.ToString().TrimEnd();
    }

    private static string BuildSemanticMemoryBlock(
        IReadOnlyList<MemoryEntry> entries,
        IReadOnlyList<(MemoryEntry Entry, float Score)> recalled)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Saved memories]");
        sb.AppendLine("Durable facts the user has asked Gabriel to remember. The index lists every saved entry; entries under 'Relevant now' were selected by semantic similarity to the latest message and include their full content. Call memory_search to read any other entry when its description looks relevant.");
        sb.AppendLine();

        sb.AppendLine("## Memory index");
        foreach (var m in entries)
        {
            var scope = m.ProjectId is null ? "user" : "project";
            sb.Append("- [").Append(m.Type.ToString().ToLowerInvariant()).Append(", ").Append(scope).Append("] ")
              .Append(m.Name).Append(" — ").AppendLine(m.Description);
        }

        if (recalled.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Relevant now");
            foreach (var (entry, _) in recalled) AppendMemory(sb, entry);
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendMemory(StringBuilder sb, MemoryEntry m)
    {
        sb.Append("### [").Append(m.Type.ToString().ToLowerInvariant()).Append("] ").AppendLine(m.Name);
        sb.AppendLine(m.Description);
        sb.AppendLine();
        sb.AppendLine(m.Body);
        sb.AppendLine();
    }

    // The four prompt pieces that are stable through a single turn: persona
    // (derived from ConversationState, which only updates on user input),
    // project SystemPrompt override, saved memories block, and tool
    // descriptors. Loaded once per turn and threaded through MaybeCompactAsync
    // + RunStreamAsync so the compact decision, the stream call, and the UI
    // metrics all see the same numbers.
    //
    // Selection feeds the tool-descriptor decision: ToolMode.None hands back
    // an empty list so the metrics breakdown (Tools bucket) reports zero and
    // the provider call doesn't advertise capabilities the model can't use.
    // Native and Emulated both get the full descriptor list - the actual
    // transport differs at the provider boundary, not here.
    private async Task<TurnPrompts> LoadTurnPromptsAsync(
        Conversation conversation,
        Guid userId,
        ModelSelection selection,
        CancellationToken ct)
    {
        var projectPrompt = await LoadProjectSystemPromptAsync(conversation, userId, ct);
        var memoryBlock = await LoadMemoryBlockAsync(conversation, userId, ct);
        var personaPrompt = _promptBuilder.Build(conversation.GetState(), conversation.Mode);
        IReadOnlyList<ToolDescriptor> tools = selection.ToolMode == ToolMode.None
            ? Array.Empty<ToolDescriptor>()
            : _tools.AsDescriptors();
        return new TurnPrompts(personaPrompt, projectPrompt, memoryBlock, tools);
    }

    // The compact trigger, shared by MaybeCompactAsync and GetContextMetricsAsync
    // so the UI's "trigger line" lands on exactly the number the backend trips on.
    // Ratio is the per-model CompactThreshold when set, else the global default.
    private (double Ratio, int ThresholdTokens) ComputeCompactThreshold(ModelSelection selection)
    {
        var ratio = selection.CompactThreshold ?? _options.CompactThreshold;
        var window = selection.ContextWindowTokens;
        return (ratio, window > 0 ? (int)(window * ratio) : 0);
    }

    // Bundle of per-turn prompt inputs used to construct AgentContext. Private
    // because callers shouldn't reach in and reorder the pieces - assembly
    // happens in AgentContext.Build.
    private sealed record TurnPrompts(
        string PersonaPrompt,
        string? ProjectPrompt,
        string? MemoryBlock,
        IReadOnlyList<ToolDescriptor> Tools)
    {
        public AgentContext BuildContext(Conversation conversation)
            => AgentContext.Build(conversation, PersonaPrompt, ProjectPrompt, MemoryBlock, Tools);
    }

    // Observation recorded for a tool call that a stop (or disconnect) cut off,
    // so every persisted tool_call keeps a matching result - OpenAI-style
    // providers reject a history with unanswered calls.
    private const string StoppedObservation = "Stopped by the user before this tool finished.";

    // What the current iteration has produced but not yet persisted, so a turn
    // cut short still leaves a consistent history behind.
    private sealed class TurnDraft
    {
        public StringBuilder Text { get; } = new();
        public StringBuilder Reasoning { get; } = new();
        public List<ChatProviderToolCall> UnansweredCalls { get; } = [];
        public bool TextCommitted { get; set; }

        public void Reset()
        {
            Text.Clear();
            Reasoning.Clear();
            TextCommitted = false;
        }
    }

    // Wraps the loop with stop support. A stop (IActiveTurnRegistry) winds the
    // loop down gracefully inside RunLoopAsync. Anything else that ends it
    // early - the client disconnecting, an exception - lands in the finally,
    // which persists the partial reply and closes unanswered tool calls.
    // pendingTitle: an in-flight auto-title (RunAsync only).
    private async IAsyncEnumerable<AgentEvent> RunStreamAsync(
        Conversation conversation,
        Guid? variantGroupIdOverride,
        TurnPrompts prompts,
        ModelSelection selection,
        Task<string?>? pendingTitle,
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var turn = _turns.Begin(conversation.Id, conversation.UserId, ct);
        var draft = new TurnDraft();
        var completed = false;
        try
        {
            await foreach (var evt in RunLoopAsync(conversation, variantGroupIdOverride, prompts, selection, pendingTitle, turn, draft, ct))
            {
                if (evt is AgentDone) completed = true;
                yield return evt;
            }
        }
        finally
        {
            if (!completed)
            {
                try
                {
                    // The request token may already be cancelled - this write has to land.
                    await PersistInterruptedAsync(conversation, draft, variantGroupIdOverride, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not persist interrupted turn | conv={ConversationId}", conversation.Id);
                }
            }
        }
    }

    private async IAsyncEnumerable<AgentEvent> RunLoopAsync(
        Conversation conversation,
        Guid? variantGroupIdOverride,
        TurnPrompts prompts,
        ModelSelection selection,
        Task<string?>? pendingTitle,
        ActiveTurn turn,
        TurnDraft draft,
        [EnumeratorCancellation] CancellationToken ct)
    {
        // Compact between turns, never mid-iteration - cutting between an
        // assistant's tool_calls and its tool results would orphan them.
        // Runs first so the summarizer's output is in conversation.Summary
        // before AgentContext.Build assembles history for the real turn.
        await foreach (var evt in MaybeCompactAsync(conversation, prompts, selection, turn.Token, ct))
        {
            yield return evt;
        }

        // ToolMode.Emulated wraps the base provider in GabrielToolBridge,
        // which injects tool docs into the system prompt and parses
        // <tool_call> markers out of the text stream. Native and None both
        // use the raw provider - they just differ in whether tools get
        // advertised (LoadTurnPromptsAsync already dropped the descriptors
        // for None, so the call below sees an empty list either way).
        var rawProvider = _providerRegistry.Resolve(selection.Provider);
        var provider = selection.ToolMode == ToolMode.Emulated
            ? new GabrielToolBridge(rawProvider, _toolBridgeLogger)
            : rawProvider;

        for (var iter = 0; iter < _options.MaxIterations; iter++)
        {
            draft.Reset();
            var textBuffer = draft.Text;
            var reasoningBuffer = draft.Reasoning;
            var pendingCalls = new List<ChatProviderToolCall>();
            FinishReason? finish = null;
            // A stop that landed while the previous iteration's tools ran.
            var stopped = turn.StopRequested;

            // Provider hiccups land here: finish=Stop with no text, no tool
            // calls, and (usually) no reasoning either. Retry the same history
            // a small bounded number of times before surfacing the error - the
            // HTTP resilience handler can't catch this because the response was
            // a successful 200 stream that just happened to be empty.
            for (var emptyRetry = 0; !stopped && emptyRetry <= EmptyStopMaxRetries; emptyRetry++)
            {
                if (emptyRetry > 0)
                {
                    draft.Reset();
                    pendingCalls.Clear();
                    finish = null;
                    try
                    {
                        await Task.Delay(EmptyStopRetryDelayMs * emptyRetry, turn.Token);
                    }
                    catch (OperationCanceledException) when (turn.StopRequested)
                    {
                        stopped = true;
                        break;
                    }
                }

                // Rebuild context per iteration - assistant tool_calls and
                // tool results from the previous iter are now part of the
                // conversation and need to be in the next provider call.
                var history = prompts.BuildContext(conversation).ToProviderHistory();

                // Enumerated by hand so a stop can be caught around MoveNext -
                // C# allows no yield inside a try that has a catch.
                await using (var events = provider.StreamAsync(history, prompts.Tools, selection.Name, turn.Token)
                    .GetAsyncEnumerator(turn.Token))
                {
                    while (true)
                    {
                        try
                        {
                            if (!await events.MoveNextAsync()) break;
                        }
                        catch (OperationCanceledException) when (turn.StopRequested)
                        {
                            stopped = true;
                            break;
                        }

                        switch (events.Current)
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
                }
                if (stopped) break;

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

            if (stopped)
            {
                // Tool calls the model was still emitting are dropped (never
                // persisted, so nothing to answer); streamed text is kept.
                var partial = await PersistInterruptedAsync(conversation, draft, variantGroupIdOverride, ct);
                _logger.LogInformation(
                    "ReAct turn stopped by user | conv={ConversationId} iters={Iters} partialChars={PartialLen}",
                    conversation.Id, iter + 1, partial?.Content?.Length ?? 0);

                if (partial is not null)
                    yield return new AgentAssistantMessage(partial.Id, partial.Content!, partial.ReasoningContent);
                await foreach (var evt in ApplyAutoTitleAsync(conversation, pendingTitle, ct))
                {
                    yield return evt;
                }
                yield return new AgentDone();
                yield break;
            }

            // Keyed off the calls themselves, not just the finish label: some
            // OpenAI-compatible servers report "stop" alongside tool_calls, and
            // GrokChatProvider maps unknown finish strings to Stop. Length is
            // excluded - a truncated stream may carry truncated arguments.
            if (pendingCalls.Count > 0 && finish is FinishReason.ToolCalls or FinishReason.Stop)
            {
                var toolCallsJson = SerializeToolCalls(pendingCalls);
                // OpenAI/xAI allow assistant messages to carry both text and tool_calls;
                // preserve any leading reasoning text we accumulated.
                var leadingText = textBuffer.Length > 0 ? textBuffer.ToString() : null;
                var reasoningForCall = reasoningBuffer.Length > 0 ? reasoningBuffer.ToString() : null;
                var assistantMessage = conversation.AppendAssistantToolCalls(toolCallsJson, leadingText, variantGroupIdOverride, reasoningForCall);
                _conversations.AddMessage(assistantMessage);
                await _uow.SaveChangesAsync(ct);
                draft.TextCommitted = true;
                draft.UnansweredCalls.AddRange(pendingCalls);

                _logger.LogInformation(
                    "Iter {Iter}: model requested {ToolCount} tool call(s) | conv={ConversationId} reasoningChars={ReasoningLen}",
                    iter, pendingCalls.Count, conversation.Id, reasoningForCall?.Length ?? 0);

                // Announce every call up front - the model issued this batch
                // together, so the client sees the full action set before
                // observations start arriving (mirrors how batched tool calls
                // read in Claude Code).
                foreach (var call in pendingCalls)
                {
                    yield return new AgentToolCall(assistantMessage.Id, call.Id, call.Name, call.ArgumentsJson);
                }

                // Concurrency model: tools flagged IsParallelSafe start now and
                // fan out on the thread pool, bounded by MaxParallelToolCalls.
                // Everything else (all DbContext-backed tools) executes serially
                // on this thread in the second pass below - as does persistence,
                // so the scoped DbContext is only ever touched sequentially.
                // Observations are persisted + emitted in the model's original
                // call order regardless of completion order, keeping the
                // timeline deterministic. Tools get the turn token, so a stop
                // turns in-flight calls into StoppedObservation results.
                var parallelGate = new SemaphoreSlim(Math.Max(1, _options.MaxParallelToolCalls));
                var started = new Task<string>?[pendingCalls.Count];
                for (var i = 0; i < pendingCalls.Count; i++)
                {
                    if (_tools.Find(pendingCalls[i].Name) is { IsParallelSafe: true })
                    {
                        started[i] = ExecuteGatedAsync(parallelGate, pendingCalls[i], conversation.Id, turn.Token);
                    }
                }

                for (var i = 0; i < pendingCalls.Count; i++)
                {
                    var call = pendingCalls[i];
                    var observation = started[i] is { } task
                        ? await task
                        : await ExecuteToolSafelyAsync(call, conversation.Id, turn.Token);

                    var toolMessage = conversation.AppendToolResult(call.Id, observation, variantGroupIdOverride);
                    _conversations.AddMessage(toolMessage);
                    // Answered as soon as it's tracked: if the save below fails,
                    // the interrupted-turn cleanup flushes this tracked result
                    // rather than adding a second one.
                    draft.UnansweredCalls.Remove(call);
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
                draft.TextCommitted = true;

                _logger.LogInformation(
                    "ReAct turn complete | conv={ConversationId} iters={Iters} rawChars={RawLen} cleanChars={CleanLen} reasoningChars={ReasoningLen}",
                    conversation.Id, iter + 1, rawText.Length, toPersist.Length, reasoningForFinal?.Length ?? 0);

                yield return new AgentAssistantMessage(assistantMessage.Id, rawText, reasoningForFinal);
                await foreach (var evt in ApplyAutoTitleAsync(conversation, pendingTitle, ct))
                {
                    yield return evt;
                }
                yield return new AgentDone();
                yield break;
            }

            // Length / Error / unexpected - bail out. Whatever streamed is
            // discarded, matching the "only finished messages" rule.
            draft.Reset();
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
        await foreach (var evt in ApplyAutoTitleAsync(conversation, pendingTitle, ct))
        {
            yield return evt;
        }
        yield return new AgentDone();
    }

    // Leaves history consistent after a turn that ended early: every persisted
    // tool call gets a result, and streamed-but-unsaved text becomes the
    // assistant reply (raw - the post-processor is for finished answers).
    // Returns that partial reply, or null when there was no text.
    private async Task<Message?> PersistInterruptedAsync(
        Conversation conversation,
        TurnDraft draft,
        Guid? variantGroupIdOverride,
        CancellationToken ct)
    {
        foreach (var call in draft.UnansweredCalls)
        {
            _conversations.AddMessage(conversation.AppendToolResult(call.Id, StoppedObservation, variantGroupIdOverride));
        }
        draft.UnansweredCalls.Clear();

        Message? partial = null;
        if (!draft.TextCommitted && !string.IsNullOrWhiteSpace(draft.Text.ToString()))
        {
            var reasoning = draft.Reasoning.Length > 0 ? draft.Reasoning.ToString() : null;
            partial = conversation.AppendAssistantText(draft.Text.ToString().TrimEnd(), variantGroupIdOverride, reasoning);
            _conversations.AddMessage(partial);
            draft.TextCommitted = true;
        }

        await _uow.SaveChangesAsync(ct);
        return partial;
    }

    // --- Auto-title --------------------------------------------------------------

    // Starts titling a still-placeholder-titled conversation from its first
    // user message, concurrently with the turn. Provider-only (no DbContext),
    // so it is safe off the loop thread; the generator never throws. Failed
    // turns simply leave the placeholder, and the next turn tries again.
    private Task<string?>? StartAutoTitle(Conversation conversation, ModelSelection selection, CancellationToken ct)
    {
        if (!_options.AutoTitle || !conversation.HasPlaceholderTitle) return null;

        var firstUserMessage = conversation.Messages
            .FirstOrDefault(m => m.Role == MessageRole.User && !string.IsNullOrWhiteSpace(m.Content))?.Content;
        if (firstUserMessage is null) return null;

        var provider = _providerRegistry.Resolve(selection.Provider);
        return _titleGenerator.GenerateAsync(provider, selection.Name, firstUserMessage, ct);
    }

    private async IAsyncEnumerable<AgentEvent> ApplyAutoTitleAsync(
        Conversation conversation,
        Task<string?>? pendingTitle,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (pendingTitle is null) yield break;
        var title = await pendingTitle;
        if (title is null) yield break;

        conversation.Rename(title);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Auto-titled conversation {ConversationId}: {Title}", conversation.Id, title);
        yield return new AgentTitleUpdated(title);
    }

    // --- Compact / summarization -------------------------------------------------

    // Decides whether to trigger a rolling-summary compact this turn.
    // Compares the full AgentContext breakdown (persona + project + memory +
    // summary + tools + conversation) against the configured fraction of the
    // provider's window. The fraction is the per-model CompactThreshold when
    // set (e.g. grok-4.3 trims early so the conversation stays inside its
    // cheaper <200k tier), otherwise the global AgentOptions.CompactThreshold.
    //
    // Using the full breakdown rather than the old "summary + post-cut
    // messages" estimate means a heavy project prompt or memory block also
    // counts toward the trigger - matching what the provider actually sees.
    // callToken (the turn token) governs the summary LLM call so a stop cancels
    // it; ct stays on persistence.
    private async IAsyncEnumerable<AgentEvent> MaybeCompactAsync(
        Conversation conv,
        TurnPrompts prompts,
        ModelSelection selection,
        CancellationToken callToken,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (selection.ContextWindowTokens <= 0) yield break;
        var (_, threshold) = ComputeCompactThreshold(selection);

        var context = prompts.BuildContext(conv);
        var currentTokens = context.ComputeBreakdown(_tokens).Total;
        if (currentTokens < threshold) yield break;

        var messages = conv.Messages.ToList();
        var cutIdx = SelectCompactCutIndex(messages, _options.CompactKeepLast);
        if (cutIdx <= 0) yield break;

        // Don't re-summarize ground we already covered.
        if (conv.SummarizedThroughMessageId is { } prevCutId)
        {
            var prevIdx = messages.FindIndex(m => m.Id == prevCutId);
            if (prevIdx >= 0 && cutIdx <= prevIdx + 1) yield break;
        }

        var toSummarize = messages.Take(cutIdx).ToList();
        if (toSummarize.Count == 0) yield break;

        // Past every short-circuit - we're definitely making the summary call.
        // Yield CompactStart now so the UI can swap to the compacting overlay
        // while the (potentially slow) LLM summary call runs below.
        yield return new AgentCompactStart(toSummarize.Count, currentTokens, threshold);

        string? newSummary = null;
        Exception? failure = null;
        try
        {
            newSummary = await GenerateSummaryAsync(conv.Summary, toSummarize, selection, callToken);
        }
        catch (Exception ex)
        {
            failure = ex;
        }

        if (failure is not null)
        {
            _logger.LogWarning(failure, "Compact summary call failed; skipping compact");
            // Still emit Done so the UI clears its overlay - the turn proceeds
            // un-compacted but the user shouldn't be stuck staring at a swirl.
            yield return new AgentCompactDone(toSummarize.Count, 0);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(newSummary))
        {
            _logger.LogWarning("Compact summary returned empty; skipping compact");
            yield return new AgentCompactDone(toSummarize.Count, 0);
            yield break;
        }

        conv.UpdateSummary(newSummary, toSummarize[^1].Id);
        _conversations.Update(conv);
        await _uow.SaveChangesAsync(ct);

        var summaryTokens = _tokens.EstimateText(newSummary);
        _logger.LogInformation(
            "Compacted conversation {Id}: summarized {Cut} messages at ~{Tokens} tokens (threshold {Threshold}) into ~{SummaryTokens} tokens",
            conv.Id, toSummarize.Count, currentTokens, threshold, summaryTokens);

        yield return new AgentCompactDone(toSummarize.Count, summaryTokens);
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

    public async Task<ContextMetrics> GetContextMetricsAsync(
        Guid conversationId,
        CancellationToken ct = default)
    {
        var (userId, conversation) = await LoadConversationAsync(conversationId, ct);

        var selection = await ResolveModelSelectionAsync(ct);
        var (ratio, thresholdTokens) = ComputeCompactThreshold(selection);

        var prompts = await LoadTurnPromptsAsync(conversation, userId, selection, ct);
        var context = prompts.BuildContext(conversation);
        var breakdown = context.ComputeBreakdown(_tokens);

        return new ContextMetrics(
            CurrentTokens: breakdown.Total,
            ContextWindowTokens: selection.ContextWindowTokens,
            CompactThresholdTokens: thresholdTokens,
            CompactThresholdRatio: ratio,
            MessagesAfterCut: context.Messages.Count,
            IsSummarized: conversation.SummarizedThroughMessageId is not null,
            SummaryTokens: breakdown.SummaryTokens,
            SystemPromptTokens: breakdown.SystemPromptTokens,
            ProjectPromptTokens: breakdown.ProjectPromptTokens,
            MemoryTokens: breakdown.MemoryTokens,
            ToolsTokens: breakdown.ToolsTokens,
            ConversationTokens: breakdown.ConversationTokens);
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
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Tool call STOPPED | conv={ConversationId} tool={Tool} callId={CallId} elapsedMs={ElapsedMs}",
                conversationId, call.Name, call.Id, sw.ElapsedMilliseconds);
            return StoppedObservation;
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

    // Wraps ExecuteToolSafelyAsync in the parallel gate. Never throws for tool
    // failures (the inner method converts them to "Error ..." observations);
    // the gate is intentionally not disposed by callers - SemaphoreSlim holds
    // no unmanaged state unless AvailableWaitHandle is touched, and disposal
    // while a late task still holds it would turn Release() into a crash.
    private async Task<string> ExecuteGatedAsync(
        SemaphoreSlim gate,
        ChatProviderToolCall call,
        Guid conversationId,
        CancellationToken ct)
    {
        try
        {
            await gate.WaitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return StoppedObservation;
        }
        try
        {
            return await ExecuteToolSafelyAsync(call, conversationId, ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private static string Preview(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "(empty)";
        // Collapse newlines so multi-line tool output stays on one log line.
        var flat = text.Replace('\n', ' ').Replace('\r', ' ');
        return flat.Length <= LogPreviewLimit ? flat : flat[..LogPreviewLimit] + "…";
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
