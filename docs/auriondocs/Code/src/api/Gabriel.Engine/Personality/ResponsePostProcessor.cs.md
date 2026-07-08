# ResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/ResponsePostProcessor.cs`  
> **Kind:** class

```csharp
public sealed class ResponsePostProcessor : IResponsePostProcessor
```


ResponsePostProcessor cleans AI-generated content by stripping predefined opener phrases at the start and closing phrases at the end. This normalization yields concise, substantive content suitable for persistence or user-facing rendering.

## Remarks
Centralizes post-processing of assistant messages to ensure a consistent user experience across UIs and storage layers. It relies on two static, compiled regex collections to detect and strip edge boilerplate, keeping per-call cost low and allocations minimal. The design is easily extensible: you can add more opener/closer patterns or adjust behavior without changing call sites. The class is sealed, providing a single, well-defined normalization point.

## Notes
- Returns the input unchanged if the input is null or whitespace.
- Only matches opener phrases at the very start and closer phrases at the very end; inline content remains untouched.
- Patterns are case-insensitive and compiled for performance; concurrent calls are safe to use the same instance.
