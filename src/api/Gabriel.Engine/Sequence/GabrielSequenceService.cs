using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;

namespace Gabriel.Engine.Sequence;

public sealed class GabrielSequenceService : IGabrielSequenceService
{
    private readonly IConversationRepository _conversations;
    private readonly IProjectRepository _projects;
    private readonly IGabrielSequenceGenerator _generator;
    private readonly ICurrentUser _currentUser;

    public GabrielSequenceService(
        IConversationRepository conversations,
        IProjectRepository projects,
        IGabrielSequenceGenerator generator,
        ICurrentUser currentUser)
    {
        _conversations = conversations;
        _projects = projects;
        _generator = generator;
        _currentUser = currentUser;
    }

    public async Task<GabrielSequence> GetForConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        // GetByIdWithMessagesAsync rather than GetByIdAsync - the state lives
        // on Conversation.StateJson which is on the aggregate itself, BUT we
        // also want access to message history for future Context-layer drift.
        // Cheap enough for the first cut; cache-aware versions can come later.
        var conversation = await _conversations.GetByIdWithMessagesAsync(conversationId, userId, ct)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        return _generator.Generate(
            conversation.AvatarSeed,
            conversation.GetState(),
            conversation.PatternOverride,
            conversation.PaletteOverride);
    }

    public async Task<GabrielSequence> GetForProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");

        var project = await _projects.GetByIdAsync(projectId, userId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        // Pick the latest conversation in the project to drive Live State.
        // ListAsync returns ordered by UpdatedAt DESC, so [0] is the freshest.
        // A project with no conversations yet renders against a null state -
        // the generator's neutral defaults take over.
        var conversations = await _conversations.ListAsync(userId, projectId, ct);
        var latest = conversations.Count > 0 ? conversations[0] : null;

        return _generator.Generate(
            project.AvatarSeed,
            latest?.GetState(),
            project.PatternOverride,
            project.PaletteOverride);
    }
}
