# ResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/ResponsePostProcessor.cs`  
> **Kind:** class

```csharp
public sealed class ResponsePostProcessor : IResponsePostProcessor
```


ResponsePostProcessor is a compact post-processing utility that strips common AI persona boilerplate from responses. It detects predefined opener phrases at the start and optional closing phrases at the end using precompiled, case-insensitive regular expressions, and returns the cleaned content trimmed of surrounding whitespace. Use Clean when you want to present user-facing text that should omit the typical introductory or concluding AI-style lines.

## Remarks
Centralizes boilerplate-stripping logic behind a small, focused API so downstream rendering code can rely on a single normalization step. By keeping the opener and closer definitions inside the symbol, updates to what counts as AI-ism text are localized and do not spread through the response pipeline. The approach supports reuse across different parts of the response-generation flow and keeps testing focused on the normalization behavior.

## Notes
- The state parameter is currently unused; it exists to satisfy the interface and may be leveraged in a future extension.
- If the input consists solely of boilerplate phrases, the result may be an empty string after cleaning.