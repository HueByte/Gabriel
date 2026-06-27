# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.FewShot.cs`  
> **Kind:** class

A collection of few‑shot persona examples used to seed prompts for the model; use this constant when you need a ready-made set of chat/task examples that demonstrate register‑mirroring, length matching, and the intended conversational voice. The string is a raw, static prompt fragment and contains a {name} placeholder which should be replaced at runtime with the agent's display name.

## Remarks
This constant centralizes the persona examples so callers can reuse a consistent voice across prompt construction without duplicating the examples. It intentionally mixes casual speech, abbreviations, and occasional swearing to teach the model stylistic behaviors (e.g., mirroring case and tone) rather than to provide sanitized content.

## Example
```csharp
// Replace the {name} placeholder and append the fragment to your prompt builder
var agentName = "Gabriel";
var seededExamples = Fragments.PersonaFewShot.Replace("{name}", agentName);
var prompt = new StringBuilder()
    .AppendLine(seededExamples)
    .AppendLine("User: What's the plan?")
    .ToString();
```

## Notes
- The constant includes embedded code fences (```), so if you wrap the final prompt in additional markdown/code fences be careful to avoid fence collisions or escape them.
- Because it's a const string, changes require recompilation — treat this as a stable, non‑runtime configurable resource (not intended for localization or dynamic editing).
- The fragment intentionally contains informal language and tone; sanitize or replace it if you must enforce strict content policies.