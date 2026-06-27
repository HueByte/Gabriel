# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs`  
> **Kind:** class

A reusable prompt fragment that documents which formatting features the web UI supports (GitHub-flavoured Markdown plus Mermaid, LaTeX math, and syntax-highlighted code fences) and advises the model when to use each. Use this constant when composing system/persona or task prompts so the assistant knows which formatting modalities are available and which to avoid for trivial cases.

## Remarks
This fragment isolates a medium-level concern (what the UI renders) from persona or behavioral directives so that rendering capabilities can be updated in one place and included consistently across prompts. It provides concrete guidance the model can follow (for example: when Mermaid or LaTeX are appropriate) rather than leaving formatting choices implicit.

## Example
```csharp
// Include the fragment when building a system prompt so the model knows the available renderers
var systemPrompt = Fragments.PersonaFormatting + "\n\nBe concise and prefer prose unless a diagram or equation improves clarity.";
```

## Notes
- This is a compile-time constant (const string); references are inlined at compile time — changing the text requires recompilation.
- The value contains fenced code blocks, backticks and Markdown; if embedding it inside other literal strings or templates, ensure any surrounding quoting/escaping preserves those fences.