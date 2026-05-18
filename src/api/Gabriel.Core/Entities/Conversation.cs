namespace Gabriel.Core.Entities;

public class Conversation
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Owner — every conversation is scoped to a user. Repository queries always
    // filter by this so users only see their own threads. Required since auth
    // landed; pre-auth dev data was wiped on migration.
    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Drives the avatar's pixel pattern + palette on the client. Each conversation
    // gets its own seed at creation so the visual identity stays stable across loads.
    // Stored as long (DB) but always within JS-safe uint32 range so it round-trips
    // cleanly through JSON.
    public long AvatarSeed { get; private set; }

    // Rolling summary of everything up to and including SummarizedThroughMessageId.
    // History assembly prepends this as a system message and drops the messages it covers
    // so the provider context stays bounded.
    public string? Summary { get; private set; }
    public Guid? SummarizedThroughMessageId { get; private set; }

    private readonly List<Message> _messages = new();
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();

    // EF Core requires a parameterless constructor.
    private Conversation() { }

    public static Conversation Create(Guid userId, string? title = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        return new Conversation
        {
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(title) ? "New conversation" : title.Trim(),
            AvatarSeed = GenerateAvatarSeed(),
        };
    }

    public void RerollAvatar()
    {
        AvatarSeed = GenerateAvatarSeed();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // 1..2^32-1 — positive, fits JS Number safely, matches the client RNG's expected range.
    private static long GenerateAvatarSeed() => Random.Shared.NextInt64(1L, 1L << 32);

    public Message AppendMessage(MessageRole role, string? content, string? toolCallId = null, string? toolCallsJson = null)
    {
        var message = Message.Create(Id, role, content, toolCallId, toolCallsJson);
        _messages.Add(message);
        UpdatedAt = DateTimeOffset.UtcNow;
        return message;
    }

    public Message AppendUserMessage(string content) => AppendMessage(MessageRole.User, content);
    public Message AppendAssistantText(string content) => AppendMessage(MessageRole.Assistant, content);
    public Message AppendAssistantToolCalls(string toolCallsJson, string? content = null)
        => AppendMessage(MessageRole.Assistant, content, toolCallsJson: toolCallsJson);
    public Message AppendToolResult(string toolCallId, string content)
        => AppendMessage(MessageRole.Tool, content, toolCallId: toolCallId);

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        Title = title.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateSummary(string summary, Guid throughMessageId)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary cannot be empty.", nameof(summary));
        Summary = summary;
        SummarizedThroughMessageId = throughMessageId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
