# Fragments

> **File:** `src/api/Gabriel.Engine/Personality/Prompts/Fragments.FewShot.cs`  
> **Kind:** class

```csharp
public static partial class Fragments
```


Fragments is a static helper that stores pre-built prompt fragments used to calibrate the model's persona during chat interactions. The PersonaFewShot constant contains a large, multi-line exemplar that demonstrates register-mirroring, length-aware responses, and back-and-forth dynamics, with {name} substituted at runtime to tailor the template for the current context.

## Remarks
Centralizing these prompts in a single static class provides a single source of truth for persona scaffolding, reducing duplication across call sites and clarifying intent: these strings are prompts, not UI copy. The code uses a C# 11 raw string literal (triple-quoted) to host long, multi-line content without escaping, and {name} is a placeholder that your runtime layer replaces when building actual prompts, mirroring how the static block behaves. This organization makes it straightforward to extend or swap persona fragments without touching individual prompt builders.

## Notes
- {name} is a runtime placeholder in the template; avoid treating it as an interpolated string during compilation or pre-processing.
- This uses a C# 11 raw string literal to embed multi-line content; ensure your compiler supports raw string literals.