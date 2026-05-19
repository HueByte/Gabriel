using System.Text.Json;
using Gabriel.Core.Entities;
using Gabriel.Engine.Providers;

namespace Gabriel.Engine.Services;

// Single source of truth for what a turn sends to the chat provider.
//
// AgentService used to assemble this inline twice: once in ToProviderHistory
// (for the actual call) and once partially in GetContextMetricsAsync (for the
// UI bar). The two diverged - metrics ignored the persona prompt, project
// prompt, memory block, and tool descriptors entirely. Pulling assembly into
// a value object means the live call and the metrics see exactly the same
// numbers.
//
// Messages here are already filtered (active variants only, tool messages
// keyed to an active assistant's tool_call.id). Filtering happens once in
// the factory so both readers see a clean list.
public record AgentContext(
    string PersonaPrompt,
    string? ProjectPrompt,
    string? MemoryBlock,
    string? Summary,
    IReadOnlyList<Message> Messages,
    IReadOnlyList<ToolDescriptor> Tools)
{
    // Build from a Conversation + the prompt pieces AgentService has already
    // loaded for this turn. Handles variant filtering and orphaned-tool-message
    // cleanup so callers don't have to.
    public static AgentContext Build(
        Conversation conversation,
        string personaPrompt,
        string? projectPrompt,
        string? memoryBlock,
        IReadOnlyList<ToolDescriptor> tools)
    {
        var allMessages = conversation.Messages.ToList();
        var startIdx = 0;
        if (conversation.SummarizedThroughMessageId is { } cutId)
        {
            var cutIdx = allMessages.FindIndex(m => m.Id == cutId);
            if (cutIdx >= 0) startIdx = cutIdx + 1;
        }

        // Collect tool_call.ids referenced by active assistant messages so we
        // can keep their tool results (and drop everyone else's). Legacy tool
        // messages from before variant grouping covered the aftermath need
        // this filter or they'd resurface after a regen.
        var activeToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        for (var i = startIdx; i < allMessages.Count; i++)
        {
            var m = allMessages[i];
            if (m.Role != MessageRole.Assistant || !m.IsActiveVariant) continue;
            if (string.IsNullOrEmpty(m.ToolCallsJson)) continue;

            using var doc = JsonDocument.Parse(m.ToolCallsJson);
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var id = el.GetProperty("id").GetString();
                if (id is not null) activeToolCallIds.Add(id);
            }
        }

        var filtered = new List<Message>(allMessages.Count - startIdx);
        for (var i = startIdx; i < allMessages.Count; i++)
        {
            var m = allMessages[i];
            if (m.Role == MessageRole.Tool)
            {
                if (m.ToolCallId is null || !activeToolCallIds.Contains(m.ToolCallId)) continue;
            }
            else if (!m.IsActiveVariant)
            {
                continue;
            }
            filtered.Add(m);
        }

        return new AgentContext(
            PersonaPrompt: personaPrompt,
            ProjectPrompt: projectPrompt,
            MemoryBlock: memoryBlock,
            Summary: conversation.Summary,
            Messages: filtered,
            Tools: tools);
    }

    // Produce the ChatProviderMessage list the provider sees. System messages
    // are prepended in the same order the previous inline assembly used so
    // model behaviour doesn't shift: persona, project context, saved memories,
    // rolling summary, then the filtered conversation.
    public IReadOnlyList<ChatProviderMessage> ToProviderHistory()
    {
        var result = new List<ChatProviderMessage>(Messages.Count + 4);

        result.Add(new ChatProviderMessage(MessageRole.System, PersonaPrompt));

        if (!string.IsNullOrWhiteSpace(ProjectPrompt))
        {
            result.Add(new ChatProviderMessage(
                MessageRole.System,
                $"[Project context]\n{ProjectPrompt}"));
        }

        if (!string.IsNullOrWhiteSpace(MemoryBlock))
        {
            result.Add(new ChatProviderMessage(MessageRole.System, MemoryBlock));
        }

        if (!string.IsNullOrWhiteSpace(Summary))
        {
            result.Add(new ChatProviderMessage(
                MessageRole.System,
                $"[Summary of earlier conversation]\n{Summary}"));
        }

        foreach (var m in Messages)
        {
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

    // Per-category token estimate. Each non-empty system block is wrapped in
    // the same per-message overhead the estimator applies to entity messages,
    // so the breakdown sums to the same number as if the whole provider
    // history were estimated as one list - i.e. the bar's "current tokens"
    // and the legend's category totals add up.
    public AgentContextBreakdown ComputeBreakdown(ITokenEstimator tokens)
    {
        return new AgentContextBreakdown(
            SystemPromptTokens: EstimateSystemBlock(tokens, PersonaPrompt),
            ProjectPromptTokens: string.IsNullOrWhiteSpace(ProjectPrompt)
                ? 0
                // Mirror ToProviderHistory's actual wire format - the
                // "[Project context]\n" prefix is part of what gets sent.
                : EstimateSystemBlock(tokens, $"[Project context]\n{ProjectPrompt}"),
            MemoryTokens: string.IsNullOrWhiteSpace(MemoryBlock)
                ? 0
                : EstimateSystemBlock(tokens, MemoryBlock),
            SummaryTokens: string.IsNullOrWhiteSpace(Summary)
                ? 0
                : EstimateSystemBlock(tokens, $"[Summary of earlier conversation]\n{Summary}"),
            ToolsTokens: EstimateTools(tokens, Tools),
            ConversationTokens: tokens.EstimateMessages(Messages));
    }

    private static int EstimateSystemBlock(ITokenEstimator tokens, string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        // The "+8" matches NaiveTokenEstimator.MessageOverhead. Kept inline
        // because the interface doesn't expose the constant, and a wrapper
        // overload "estimate a single text-only system message" felt heavier
        // than this two-line helper.
        return tokens.EstimateText(text) + 8;
    }

    private static int EstimateTools(ITokenEstimator tokens, IReadOnlyList<ToolDescriptor> tools)
    {
        // Tool descriptors aren't part of the messages array - providers
        // accept them in a sibling "tools" field. We still budget them
        // against the same context window because every provider we target
        // counts them that way. Name + Description + JSON schema is the
        // bulk of the payload; the JSON envelope around each is negligible.
        var total = 0;
        foreach (var t in tools)
        {
            total += tokens.EstimateText(t.Name);
            total += tokens.EstimateText(t.Description);
            total += tokens.EstimateText(t.ParametersJsonSchema);
        }
        return total;
    }
}

// Per-category token totals from a single AgentContext snapshot. Sums to the
// same number AgentService uses for the compact decision (and the API surfaces
// as ContextMetrics.CurrentTokens).
public record AgentContextBreakdown(
    int SystemPromptTokens,
    int ProjectPromptTokens,
    int MemoryTokens,
    int SummaryTokens,
    int ToolsTokens,
    int ConversationTokens)
{
    public int Total =>
        SystemPromptTokens + ProjectPromptTokens + MemoryTokens
        + SummaryTokens + ToolsTokens + ConversationTokens;
}
