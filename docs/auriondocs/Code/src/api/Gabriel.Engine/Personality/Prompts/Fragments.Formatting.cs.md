# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Formatting.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


The Fragments class serves as a centralized repository for rendering guidance used by the chat UI. It exposes PersonaFormatting, a compile-time constant string that documents the formats the web app supports (GitHub-flavored Markdown with Mermaid diagrams, LaTeX math, and code highlighting) and the rules for when to apply each. Keeping this medium concern separate from persona or behavior logic ensures consistent rendering guidance across prompts.

## Remarks

- It isolates medium concerns (rendering capabilities) from persona/behavior logic, allowing the UI to evolve formatting support independently.
- The static partial Fragments class pattern implies there are additional related fragments across the codebase, enabling a single source of truth for formatting documentation that prompts can reference.
- It acts as a contract between prompt generation and rendering components, guiding when to emit diagrams, math, or code blocks.

## Notes

- The content is a compile-time constant; editing it requires code changes and redeployment.
- The documented features are Mermaid, LaTeX, and code highlighting, with guidance on when to use each (e.g., diagrams for architecture, real equations for math; avoid overusing tables or graphs where prose suffices).
- This documentation does not implement rendering behavior; it merely describes what the UI supports.