using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;

namespace Gabriel.Core.Services;

public class ChatService : IChatService
{
    private readonly IConversationRepository _conversations;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public ChatService(
        IConversationRepository conversations,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _conversations = conversations;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Conversation> CreateConversationAsync(string? title, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var conversation = Conversation.Create(userId, title);
        await _conversations.AddAsync(conversation, ct);
        await _uow.SaveChangesAsync(ct);
        return conversation;
    }

    public Task<IReadOnlyList<Conversation>> ListConversationsAsync(CancellationToken ct = default)
        => _conversations.ListAsync(RequireUserId(), ct);

    public async Task<Conversation> GetConversationAsync(Guid id, CancellationToken ct = default)
    {
        return await _conversations.GetByIdWithMessagesAsync(id, RequireUserId(), ct)
            ?? throw new NotFoundException(nameof(Conversation), id);
    }

    public async Task<Conversation> RenameConversationAsync(Guid id, string title, CancellationToken ct = default)
    {
        var conversation = await _conversations.GetByIdAsync(id, RequireUserId(), ct)
            ?? throw new NotFoundException(nameof(Conversation), id);

        // Conversation.Rename throws ArgumentException on empty/whitespace; the global
        // exception handler maps that to 400 Bad Request.
        conversation.Rename(title);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task<Conversation> RerollAvatarAsync(Guid id, CancellationToken ct = default)
    {
        var conversation = await _conversations.GetByIdAsync(id, RequireUserId(), ct)
            ?? throw new NotFoundException(nameof(Conversation), id);

        conversation.RerollAvatar();
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task DeleteConversationAsync(Guid id, CancellationToken ct = default)
    {
        var conversation = await _conversations.GetByIdAsync(id, RequireUserId(), ct)
            ?? throw new NotFoundException(nameof(Conversation), id);

        _conversations.Remove(conversation);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<Conversation> DeleteMessageAsync(Guid conversationId, Guid messageId, CancellationToken ct = default)
    {
        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, RequireUserId(), ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        if (conversation.Messages.All(m => m.Id != messageId))
            throw new NotFoundException(nameof(Message), messageId);

        // Conversation.TruncateFrom returns the removed messages so we can hand
        // them to the repository for explicit EF removal — orphan-removal alone
        // is fragile, this is the safe path.
        var removed = conversation.TruncateFrom(messageId);
        _conversations.RemoveMessages(removed);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task<Conversation> SetActiveVariantAsync(Guid conversationId, Guid messageId, CancellationToken ct = default)
    {
        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, RequireUserId(), ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        if (conversation.Messages.All(m => m.Id != messageId))
            throw new NotFoundException(nameof(Message), messageId);

        conversation.SetActiveVariant(messageId);
        _conversations.Update(conversation);
        await _uow.SaveChangesAsync(ct);
        return conversation;
    }

    // Belt-and-suspenders: controllers already carry [Authorize], so this should
    // never throw in practice. The check guarantees we never accidentally execute
    // a service call as "no user" if someone forgets the attribute.
    private Guid RequireUserId()
        => _currentUser.UserId ?? throw new UnauthorizedAccessException("Authenticated user required.");
}
