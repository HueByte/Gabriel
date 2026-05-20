using System.Text.Json;
using Gabriel.API.Contracts.Conversations;
using Gabriel.API.Contracts.Messages;
using Gabriel.API.Contracts.Projects;
using Gabriel.API.Contracts.Sequence;
using Gabriel.Core.Entities;
using Gabriel.Engine.Sequence;
using Gabriel.Engine.Services;

namespace Gabriel.API.Mapping;

internal static class ContractMappings
{
    public static ProjectResponse ToResponse(this Project p, bool includeFiles)
    {
        var files = includeFiles
            ? p.Files.Select(f => f.ToResponse()).ToList()
            : null;
        return new ProjectResponse(
            p.Id, p.Name, p.Description, p.SystemPrompt,
            p.AvatarSeed, p.IsDefault,
            p.PatternOverride, p.PaletteOverride,
            p.CreatedAt, p.UpdatedAt, files);
    }

    public static ProjectFileResponse ToResponse(this ProjectFile f)
        => new(f.Id, f.Name, f.SizeBytes, f.ContentType, f.UploadedAt);

    public static ConversationResponse ToResponse(this Conversation c, bool includeMessages, Project? project = null)
    {
        // Project metadata is optional - list endpoints don't bother loading it
        // (sidebar rows don't render avatars), so the new fields stay null.
        // Single-conversation endpoints pass the project so the client can
        // render the correct sequence (project's shared one vs the conversation's
        // own standalone one).
        bool? projectIsDefault = project?.IsDefault;
        long? effectiveSeed = project is null
            ? null
            : (project.IsDefault ? c.AvatarSeed : project.AvatarSeed);

        var modeName = c.Mode?.ToString().ToLowerInvariant();

        if (!includeMessages)
        {
            return new ConversationResponse(c.Id, c.ProjectId, c.Title, c.AvatarSeed, c.CreatedAt, c.UpdatedAt, null, projectIsDefault, effectiveSeed, c.PatternOverride, c.PaletteOverride, modeName);
        }

        var allMessages = c.Messages;

        // Tool messages: keep only those whose tool_call.id is referenced by an
        // active assistant. Matches the agent's history-assembly filter so the
        // UI sees the same conversation the model does.
        var activeToolCallIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var m in allMessages)
        {
            if (m.Role != MessageRole.Assistant || !m.IsActiveVariant) continue;
            if (string.IsNullOrEmpty(m.ToolCallsJson)) continue;

            using var doc = JsonDocument.Parse(m.ToolCallsJson);
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var id = el.GetProperty("id").GetString();
                if (id is not null) activeToolCallIds.Add(id);
            }
        }

        // Precompute sibling lists per variant group so the variant picker has
        // everything it needs without hitting the API again. Sort by CreatedAt
        // so the index is stable across reloads.
        var groupSiblings = allMessages
            .GroupBy(m => m.VariantGroupId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(m => m.CreatedAt).Select(m => m.Id).ToList());

        var messages = new List<MessageResponse>();
        foreach (var m in allMessages)
        {
            if (m.Role == MessageRole.Tool)
            {
                if (m.ToolCallId is null || !activeToolCallIds.Contains(m.ToolCallId)) continue;
            }
            else if (!m.IsActiveVariant)
            {
                continue;
            }

            var siblings = groupSiblings[m.VariantGroupId];
            messages.Add(new MessageResponse(
                m.Id,
                m.Role.ToString().ToLowerInvariant(),
                m.Content,
                m.CreatedAt,
                m.VariantGroupId,
                siblings.IndexOf(m.Id),
                siblings.Count,
                siblings,
                m.ToolCallId,
                ParseToolCalls(m.ToolCallsJson),
                m.ReasoningContent));
        }

        return new ConversationResponse(c.Id, c.ProjectId, c.Title, c.AvatarSeed, c.CreatedAt, c.UpdatedAt, messages, projectIsDefault, effectiveSeed, c.PatternOverride, c.PaletteOverride, modeName);
    }

    public static ContextMetricsResponse ToResponse(this ContextMetrics m)
        => new(
            m.CurrentTokens,
            m.ContextWindowTokens,
            m.CompactThresholdTokens,
            m.CompactThresholdRatio,
            m.MessagesAfterCut,
            m.IsSummarized,
            m.SystemPromptTokens,
            m.ProjectPromptTokens,
            m.MemoryTokens,
            m.SummaryTokens,
            m.ToolsTokens,
            m.ConversationTokens);

    public static GabrielSequenceResponse ToResponse(this GabrielSequence sequence)
    {
        var palette = sequence.Palette.Colors
            .Select(c => new[] { (int)c.R, (int)c.G, (int)c.B })
            .ToList();

        var frames = sequence.Frames
            .Select(f => f.Pixels.Select(b => (int)b).ToArray())
            .ToList();

        return new GabrielSequenceResponse(
            sequence.Version,
            palette,
            frames,
            new SequenceMetadataResponse(
                sequence.Metadata.Seed,
                sequence.Metadata.GeneratedAt,
                sequence.Metadata.StateSummary));
    }

    // Re-parses the stored wire-format tool_calls JSON into the API DTO shape.
    private static IReadOnlyList<MessageToolCall>? ParseToolCalls(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(el => new MessageToolCall(
                Id: el.GetProperty("id").GetString()!,
                Name: el.GetProperty("function").GetProperty("name").GetString()!,
                ArgumentsJson: el.GetProperty("function").GetProperty("arguments").GetString()!))
            .ToList();
    }
}
