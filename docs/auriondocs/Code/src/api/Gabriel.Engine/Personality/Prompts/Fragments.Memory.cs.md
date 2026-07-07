# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.Memory.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a small utility container for prompt fragments used to shape the model's prompts. The PersonaMemory member stores the long-term-memory guidance that instructs the agent on how to interact with memory tools (memory_save, memory_list, memory_remove) and when to persist information. This fragment is designed to be appended after the static persona block so memory capabilities are presented as functional constraints rather than as part of the model's identity.

## Remarks
Fragments centralizes memory-policy text, making it easy to adjust or reuse across prompts without duplicating content. It clarifies the separation between the core persona and memory behavior, supporting consistent behavior across environments and test scenarios. The static const nature ensures the policy remains stable at runtime while still being accessible from code.

## Example
```csharp
// Example: including memory guidance in the system prompt
string systemPrompt = Fragments.PersonaMemory + "\n" + otherPersonaContent;
```

## Notes
- If memory tools aren't registered the builder can skip this fragment cleanly without rewriting anything upstream.
- The fragment is a long, raw string literal; consider impact on prompt length and maintainability when integrating into larger prompts.