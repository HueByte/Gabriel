# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs`  
> **Kind:** class

A reusable, human-readable fragment that explains exactly which formatting features the web UI renders (GitHub-flavoured Markdown, Mermaid diagrams, LaTeX math and fenced code blocks) and gives guidance about when the model should use each. Use this constant when assembling system or assistant prompt text to inform the model about the medium-level rendering capabilities and the recommended trade-offs between prose, diagrams, tables and code blocks.

## Remarks
This fragment captures rendering guidance as a medium-level concern distinct from persona instructions: it tells the model what the client can actually render and when it should choose a diagram, LaTeX, table or code block. Centralising this text ensures all prompt templates give consistent advice to the model and makes it straightforward to update the guidance if the front-end renderer adds or removes features.

## Example
```csharp
// Include the formatting guidance in a system prompt so the model knows what the client will render
var systemPrompt = $"You are an assistant that adapts output to the client's renderer.\n{Fragments.PersonaFormatting}";
var message = new PromptMessage("system", systemPrompt);
// then pass `message` into the prompt assembly used by the model
```

## Notes
- PersonaFormatting is a compile-time constant; changing it requires a code change and redeploy.
- The text is intended as model-facing guidance (what the model may emit), not as user-facing UI help — keep phrasing concise and authoritative.
- If the webapp's renderer capabilities change (add/remove diagram support, change math delimiters, etc.), update this fragment to keep model behavior aligned with actual rendering.