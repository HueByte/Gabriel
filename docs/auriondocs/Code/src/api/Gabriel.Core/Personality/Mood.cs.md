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


Discrete engagement moods used to steer the assistant's conversational style. The heuristic state updater selects a Mood and ISystemPromptBuilder uses it to inject tone guidance into the per-turn system prompt; the values Neutral, Playful, Venting, Serious, Curious, and LowEnergy provide a compact, strongly-typed vocabulary for tone control, with the prompt builder lowering the names when inserting them into prompts.

## Remarks
Centralizes tone decisions in a single enum, decoupling mood selection from prompt construction. This makes it straightforward to extend or adjust available moods without touching how prompts are assembled, and ensures consistent interpretation of mood across components that participate in shaping the assistant's persona.

## Notes
- If you add new moods, update any mappings that translate Mood values to user-visible prompts.
- Rely on the lowercase-friendly contract when constructing prompt segments; do not assume case-sensitive behavior elsewhere.
- Avoid reordering or removing existing members if they are persisted or serialized externally.