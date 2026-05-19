using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;

namespace Gabriel.Engine.Sequence;

public sealed class GabrielSequenceService : IGabrielSequenceService
{
    private readonly IConversationRepository _conversations;
    private readonly IGabrielSequenceGenerator _generator;
    private readonly ICurrentUser _currentUser;

    public GabrielSequenceService(
        IConversationRepository conversations,
        IGabrielSequenceGenerator generator,
        ICurrentUser currentUser)
    {
        _conversations = conversations;
        _generator = generator;
        _currentUser = currentUser;
    }

    public async Task<GabrielSequence> GetForConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        // GetByIdWithMessagesAsync rather than GetByIdAsync — the state lives
        // on Conversation.StateJson which is on the aggregate itself, BUT we
        // also want access to message history for future Context-layer drift.
        // Cheap enough for the first cut; cache-aware versions can come later.
        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        return _generator.Generate(conversation.AvatarSeed, conversation.GetState());
    }
}
