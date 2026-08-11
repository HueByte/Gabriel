# PromptKey

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs`  
> **Kind:** class

```csharp
public static class PromptKey
```


PromptKey is a static class that stores the canonical, compile-time keys for named prompt fragments used by the prompt registry. The keys are grouped by topic (persona, memory, formatting, and per-mode blocks) and exposed as const strings so callers rely on a single source of truth instead of ad hoc literals when assembling prompts or selecting fragments. When building a prompt, developers reach for PromptKey.* constants to refer to the appropriate fragment group or mode rather than hard-coding strings.

## Remarks

Centralizing these keys decouples the fragment registry from its consumers, enabling safer refactoring and consistent vocabulary across the system without inspecting raw strings at each call site. The constants point to corresponding entries in the Fragments.* definitions, which keeps the wording of fragment keys aligned across registration and retrieval paths. This design minimizes human error (typos) and makes evolution (adding new modes or sections) a matter of updating a single canonical set.

## Notes

- Keep PromptKey and Fragments.* in sync; renaming or moving a key requires updating both sides to avoid mismatches. 
- Because these are compile-time constants, references to non-existent keys are caught at build time, but you must ensure the corresponding Fragments.* entry exists and mirrors PromptKey's value for runtime lookups.