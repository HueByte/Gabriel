# PromptKey

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/PromptKey.cs`  
> **Kind:** class

```csharp
public static class PromptKey
```


PromptKey is a static registry of string identifiers for every named prompt fragment the system can reference. It exposes a set of const string fields grouped by topic (persona, memory, formatting) and per-mode variants, enabling callers to use a single, compile-time-checked token instead of arbitrary literals when assembling prompts. The keys are defined as compile-time constants so they can be embedded directly into switch arms or dictionaries, catching typos at build time rather than at runtime. When new modes or sections land, add its key here and the matching `Fragments.*` constant it points at. This keeps the surface stable while the actual fragment content can evolve independently.

## Remarks
By declaring keys as const string, they can be embedded into switch arms and dictionary lookups at compile time, surfacing typos as build errors rather than runtime bugs. The design decouples identity (PromptKey) from content (Fragments), allowing content updates without touching call-sites. Per-mode behaviour snippets are selected by Conversation.Mode and appended to the static block per-turn, enabling consistent persona behavior without rewriting the whole prompt.

## Example
```csharp
string key = PromptKey.PersonaFewShot;
string fragment = Fragments.PersonaFewShot;
```

## Notes
- Names must be consistent with Fragments.*; renaming a key without updating its Fragment counterpart will break behavior.
- Keys are static and tightly coupled to the build; avoid attempting to mutate them at runtime.
- Do not rely on these identifiers for user-facing UI text; they are internal tokens.