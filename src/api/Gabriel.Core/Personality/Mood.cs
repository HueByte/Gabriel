namespace Gabriel.Core.Personality;

// Discrete conversation moods used by the heuristic state updater and consumed
// by ISystemPromptBuilder to inject style-of-engagement guidance into the
// per-turn system prompt. Names are intentionally lowercase-friendly; the
// prompt builder lowercases them when injecting.
public enum Mood
{
    Neutral,
    Playful,
    Venting,
    Serious,
    Curious,
    LowEnergy,
}
