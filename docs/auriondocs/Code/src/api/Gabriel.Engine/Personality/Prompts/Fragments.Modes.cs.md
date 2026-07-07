# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Modes.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments defines mode-specific bias blocks that are appended to the static persona before each turn, selected by Conversation.Mode. The base persona drives behavior, while the mode block re-weights style (length, depth, stance) to shift tone without rewriting core rules. This lets developers tailor responses for TASK vs CHAT contexts or experiment with different personas without changing the underlying baseline.

## Remarks

By isolating mode bias from the base persona, this abstraction keeps the baseline stable while enabling targeted experimentation with tone and verbosity. New modes can be added by introducing additional constant strings and extending the mode chooser, without touching the core persona. The mode blocks describe how to behave in both TASK and CHAT halves, providing a cohesive policy across contexts.

## Notes

- The ModeTutor constant content in the snippet appears truncated; consult the full source to document its intended guidance.
- Because these are const string literals, updates require recompilation; version alignment between code and docs matters.
- Be mindful of prompt length; long mode blocks can inflate the prompt and affect token budgets.