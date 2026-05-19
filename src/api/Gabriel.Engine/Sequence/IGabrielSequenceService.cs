namespace Gabriel.Engine.Sequence;

// Loads the conversation (user-scoped), pulls AvatarSeed + ConversationState,
// and hands them to the generator. Single integration point so controllers
// don't need to know about repositories or generators directly.
public interface IGabrielSequenceService
{
    Task<GabrielSequence> GetForConversationAsync(Guid conversationId, CancellationToken ct = default);
}
