# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments acts as a central, static container for the chat surface’s formatting guidance. Its PersonaFormatting constant codifies when and how the UI may render specialized formatting — Mermaid diagrams, LaTeX math, and code blocks — and tells consumers, including the model, when to reach for these features or to omit them.

## Remarks
This symbol isolates medium-level formatting policy from behavioural logic, ensuring consistent prompts and rendering behavior across prompts. It anchors the model’s expectations about what the UI can render, and clarifies when such rendering should be avoided to preserve readability.

## Notes
- The content is documentation for the UI renderers; changes to supported formats should be synchronized with the web app.
- The class is declared partial and static; expect extensions elsewhere and treat PersonaFormatting as the canonical source of guidance for formatting in prompts.