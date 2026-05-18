using System.Text.Json;
using Gabriel.API.Contracts.Conversations;
using Gabriel.API.Contracts.Messages;
using Gabriel.Core.Entities;

namespace Gabriel.API.Mapping;

internal static class ContractMappings
{
    public static MessageResponse ToResponse(this Message m)
        => new(
            m.Id,
            m.Role.ToString().ToLowerInvariant(),
            m.Content,
            m.CreatedAt,
            m.ToolCallId,
            ParseToolCalls(m.ToolCallsJson));

    public static ConversationResponse ToResponse(this Conversation c, bool includeMessages)
        => new(
            c.Id,
            c.Title,
            c.AvatarSeed,
            c.CreatedAt,
            c.UpdatedAt,
            includeMessages ? c.Messages.Select(ToResponse).ToList() : null);

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
