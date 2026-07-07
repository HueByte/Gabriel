# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Defines a reusable formatting guide that describes what the chat surface renders and when to apply specific formatting features. It documents the webapp's supported tools — GitHub-flavored Markdown augmented with Mermaid diagrams, KaTeX math, and code highlighting — and clarifies when to use each. This content lives in a static string constant within Fragments so the model and UI can access a single source of truth about rendering capabilities, independent from persona or behavior logic.

## Remarks
By isolating this guidance in Fragments.PersonaFormatting, the app decouples the knowledge of renderers from the conversational behavior. It acts as a medium concern that the chat surface ships with, enabling consistent guidance across renderers and reducing duplication in prompts or UI text.

## Notes
- Keep the guidance in sync with the webapp's actual rendering capabilities; mismatch can mislead users and the model.
- As a static constant, changes require a rebuild/deploy to propagate; plan changes accordingly.