using Gabriel.Core.Personality;

namespace Gabriel.Engine.Personality;

// Assembles the per-turn system prompt: static persona block + dynamic guidance
// derived from ConversationState. Stateless service.
public interface ISystemPromptBuilder
{
    string Build(ConversationState? state);
}
