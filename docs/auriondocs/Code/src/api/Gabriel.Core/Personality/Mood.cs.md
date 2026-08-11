# Mood

> **File:** `src/api/Gabriel.Core/Personality/Mood.cs`  
> **Kind:** enum

```csharp
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


Mood is an enumeration of discrete engagement styles that the heuristic state updater can assign and that the ISystemPromptBuilder uses to steer the tone of each system prompt. Use Mood to select a conversational mood (Neutral, Playful, Venting, Serious, Curious, LowEnergy) instead of embedding tone logic directly in prompts; the builder will normalize the names to lowercase during injection.

## Remarks
Mood serves as a lightweight, typed vocabulary for the system's conversational tone. It decouples mood selection from prompt construction, letting the prompt pipeline apply a consistent style by mapping each enum value to a lowercased token. Since the injection step lowercases the names, rely on the Mood value rather than a hard-coded string if you interact with the prompt directly.

## Notes
- Do not rely on the enum's numeric discriminants; use the named constants.
- The prompt engine lowercases mood names when injecting into prompts, so casing here doesn't dictate the token seen in the prompt.
- Adding new moods should be reflected across all consumers that enumerate or switch on Mood.