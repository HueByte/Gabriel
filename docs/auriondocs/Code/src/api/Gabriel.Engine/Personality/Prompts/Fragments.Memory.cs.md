# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Memory.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a static partial class that hosts memory-related text fragments used to shape the model’s persona. The PersonaMemory constant contains the memory-system guidance: when to save user information, how to reference memory tools (memory_save, memory_list, memory_remove), formatting conventions, and boundaries about what not to persist. This fragment is appended after the static persona block so the model treats memory capabilities as an operational instruction rather than core identity.

## Remarks
Fragments acts as a modular repository of prompt fragments; the partial class pattern lets other parts of the codebase extend its content without mutating existing code. PersonaMemory centralizes long-term memory policy, ensuring consistent behavior across conversations and projects. It decouples memory policy from other persona fragments, making it easier to evolve memory rules independently.

## Notes
- The string contains verbatim memory guidance intended to be embedded in prompts; changes should be reviewed for potential impact on user privacy and model behavior.
- Because PersonaMemory is a raw string literal, indentation and formatting are preserved; be mindful when editing to keep readability intact.