# Mood

> **File:** `src/api/Gabriel.Core/Personality/Mood.cs`  
> **Kind:** enum

```csharp
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
```


Represents a discrete conversation mood used by the heuristic state updater and consumed by the system-prompt builder to inject a style-of-engagement token into each turn's system prompt. Use this enum when selecting or persisting the conversational style instead of scattering raw strings across code; the prompt builder lowercases enum names when injecting them into prompts.

## Remarks
This enum centralizes the supported high-level engagement styles (e.g., Playful, Serious, Curious) so heuristics and prompt construction remain consistent. Names are defined to be "lowercase-friendly" because the prompt builder performs its own lowercasing; adding or renaming moods typically requires coordinating changes in any prompt templates or heuristic logic that reference them.

## Example
```csharp
// Select a mood for the current conversation turn
Mood current = Mood.Playful;

// If building a prompt manually you can convert to the token the prompt builder uses
string moodToken = current.ToString().ToLowerInvariant(); // "playful"

// Use the token when composing a system prompt (the real prompt builder does this for you)
string systemPrompt = $"You are an assistant with a {moodToken} tone. Follow user instructions concisely.";
```

## Notes
- The prompt builder lowercases enum names before injection — code should not rely on enum member casing when matching prompt tokens.
- Adding, removing, or renaming moods can affect heuristics and prompt text; update all consumers (prompt templates, state updater logic) together.
- This is a plain enum (not a [Flags] enum); members are distinct, mutually exclusive moods.