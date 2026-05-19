using Gabriel.Core.Personality;

namespace Gabriel.Engine.Sequence;

// Deterministically generates a full 64-frame Gabriel Sequence from a seed.
// The seed is the personality's stable identity (currently Conversation.AvatarSeed;
// post-Phase-8 it will be Project.Id-derived). ConversationState (optional)
// drives the Live State layer — frames 48..63 reflect current mood / tempo /
// engagement signals; the first three layers are pure-seed.
//
// Stateless and safe to register as a singleton.
public interface IGabrielSequenceGenerator
{
    GabrielSequence Generate(long seed, ConversationState? state);
}
