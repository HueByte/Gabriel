# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments provides per-mode bias blocks that are appended to the static persona block on each turn, selected according to Conversation.Mode. The base persona remains the core driver; the mode block re-weights tone and depth to tailor outcomes without rewriting the underlying behavior. The default ModeChatty serves as baseline, and other modes (ModeElaborative, ModeConcise, ModeTutor) adjust verbosity and guidance.

## Remarks
Fragments centralizes mode-driven nudges, keeping the prompt assembly uniform and making it easy to introduce new modes by adding a constant rather than changing the builder. Because these blocks are raw strings, keep them short and focused on bias (the 'what' of the mode) rather than detailed policy or explanations. They operate on both Task and Chat halves; the mode-specific text should be mindful of the context to avoid contradicting the base instructions.

## Example
```csharp
// Example usage: apply elaborative bias for the next turn
string modeBlock = Fragments.ModeElaborative;
string basePrompt = "Base persona prompt";
string fullPrompt = modeBlock + " " + basePrompt;
```

## Notes
- The default ModeChatty baseline is intentionally minimal to preserve the existing persona unless a mode is selected.
- Mode blocks are parsed at runtime by the prompt builder and compiled into the final prompt; changes require code recompilation.
- Use concise blocks for modes not to overwhelm the base persona with excessive verbosity.