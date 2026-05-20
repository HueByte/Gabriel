using Gabriel.Core.Entities;
using Gabriel.Core.Personality;

namespace Gabriel.Engine.Personality;

// Assembles the per-turn system prompt: static persona block + per-mode
// bias + dynamic guidance derived from ConversationState. Stateless service.
//
// `mode` selects which Fragments.Mode* snippet gets spliced in. Null is
// treated as GabrielMode.Chatty (the baseline behaviour).
public interface ISystemPromptBuilder
{
    string Build(ConversationState? state, GabrielMode? mode = null);
}
