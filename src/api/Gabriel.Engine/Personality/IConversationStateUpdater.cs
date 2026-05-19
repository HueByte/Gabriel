using Gabriel.Core.Personality;

namespace Gabriel.Engine.Personality;

// Reads the latest user message + the prior state, returns the new state.
// Stateless service - implementations should be safe to register as a singleton.
public interface IConversationStateUpdater
{
    ConversationState Update(ConversationState? current, string userMessage);
}
