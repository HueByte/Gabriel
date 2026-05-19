using Gabriel.Core.Personality;

namespace Gabriel.Engine.Sequence;

// Deterministically generates a full 64-frame Gabriel Sequence from a seed.
// The seed is the personality's stable identity (Project.AvatarSeed for
// project-shared sequences; Conversation.AvatarSeed for standalone chats).
// ConversationState (optional) drives the Live State layer - frames 48..63
// reflect current mood / tempo / engagement signals; the first three layers
// are pure-seed.
//
// Pattern / palette overrides let the user pin the "skin" of the avatar
// instead of accepting whatever the seed picks. When supplied, they take
// precedence over the seed-derived pick. Unknown / unrecognized identifiers
// fall back to the seed-derived behavior (no error - see SequenceCatalog).
//
// Stateless and safe to register as a singleton.
public interface IGabrielSequenceGenerator
{
    GabrielSequence Generate(
        long seed,
        ConversationState? state,
        string? patternOverride = null,
        string? paletteOverride = null);
}
