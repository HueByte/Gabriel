# Mood

> **File:** `src/api/Gabriel.Core/Personality/Mood.cs`  
> **Kind:** enum

Represents discrete conversation moods used by the heuristic state updater and consumed by ISystemPromptBuilder to inject style-of-engagement guidance into the per-turn system prompt. Use this enum when recording or communicating the desired conversational tone for the assistant's next turn (for example: Neutral, Playful, Venting, Serious, Curious, LowEnergy).

## Remarks
Centralizes the set of style labels that heuristics and prompt-building code share so prompts can be adjusted consistently across the system. The names are chosen to be friendly to lowercasing (the prompt builder lowercases them when injecting into prompts), and each value should be treated as a discrete label rather than a numeric intensity or probability.

## Example
```csharp
Mood mood = Mood.Playful;
// The prompt builder will lowercase the mood when injecting it, but callers
// that build prompts themselves can convert explicitly:
string moodToken = mood.ToString().ToLowerInvariant(); // "playful"
string systemPrompt = $"You are a {moodToken} assistant: keep replies light and friendly.";
```

## Notes
- The enum's default value (0) is Neutral — code that relies on default enum initialization will observe Mood.Neutral.
- Adding new moods requires corresponding updates to any prompt-construction logic and tests that consume these labels.
- Treat these values as semantic labels only; do not interpret the enum as expressing intensity or a ranked scale.