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


Mood enumerates discrete conversation moods that guide engagement per turn. It is consumed by the heuristic state updater and by ISystemPromptBuilder to inject style-of-engagement guidance into per-turn system prompts; the names are intentionally lowercase-friendly and are lowercased by the prompt builder when injecting.

## Remarks
Mood provides a compact, strongly-typed vocabulary for engagement style. This abstraction decouples the concern of choosing a mood from the text inserted into prompts, enabling easier testing and extension as new moods are introduced. It centralizes policy around how moods map to prompt content across the system.

## Notes
- Do not rely on enum member order for semantics; use explicit comparisons.
- Adding new moods requires corresponding handling in any mappings or switch statements that translate moods to prompt templates.