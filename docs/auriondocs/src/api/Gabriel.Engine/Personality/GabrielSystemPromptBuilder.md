# GabrielSystemPromptBuilder

> **File:** `src/api/Gabriel.Engine/Personality/GabrielSystemPromptBuilder.cs`  
> **Kind:** class

Composes the system prompt used to drive the "Gabriel" persona and returns a ready-to-send system message. Use this builder when you need a stable, per-turn system prompt that combines persona identity, formatting rules, mode-specific instructions, per-conversation metadata, guidance, and few-shot examples — without re-running expensive substitutions each turn.

## Remarks
This class centralizes how the persona is presented to the model: it pulls fragment blocks from an IPromptRegistry, substitutes the configured persona name once at construction time, and caches those results so Build can be called every turn without repeatedly re-running substitutions. The Build method emits a consistent prompt structure (static persona, formatting block, mode snippet, metadata, guidance, few-shot) so changes in behavior come from content (mode/state) rather than structural differences.

## Example
```csharp
// Typical usage (DI-provided options and promptRegistry omitted)
var builder = new GabrielSystemPromptBuilder(options, promptRegistry);

// produce the system prompt for turn 5 in concise mode
var prompt = builder.Build(new ConversationState { TurnCount = 5, Mood = Mood.Happy }, GabrielMode.Concise);

// prompt now contains the static persona, formatting guidance, mode snippet,
// conversation metadata (turn count, mood, flags), overall guidance, and few-shot.
```

## Notes
- Build accepts a nullable ConversationState; when state is null the prompt emits sensible defaults (Turn: 0, Mood: neutral, etc.).
- Persona name substitution and the static/formatting/few-shot blocks are performed at construction and stored in readonly fields to avoid repeated string.Replace work per turn.
- A "STALL WARNING" line is appended when UserAskedForDetail is true and ConsecutiveShortMessages >= 1; this signals the model to return the full requested artifact rather than asking for confirmations.
- If mode is null, ModeKey defaults to GabrielMode.Chatty; different GabrielMode values map to different prompt keys so changing mode alters only the mode snippet section.
