namespace Gabriel.Engine.Sequence;

// Loads the conversation (user-scoped), pulls AvatarSeed + ConversationState,
// and hands them to the generator. Single integration point so controllers
// don't need to know about repositories or generators directly.
public interface IGabrielSequenceService
{
    Task<GabrielSequence> GetForConversationAsync(Guid conversationId, CancellationToken ct = default);

    // Project-scoped variant. Uses the *project's* AvatarSeed so every
    // conversation inside the project renders the same shared sequence (the
    // project has its own visual identity). Live State frames come from the
    // project's most-recently-active conversation - projects don't have their
    // own ConversationState, so we aggregate from the latest pulse the user
    // gave the project. If the project has no conversations yet, Live State
    // is rendered against a default neutral state.
    Task<GabrielSequence> GetForProjectAsync(Guid projectId, CancellationToken ct = default);
}
