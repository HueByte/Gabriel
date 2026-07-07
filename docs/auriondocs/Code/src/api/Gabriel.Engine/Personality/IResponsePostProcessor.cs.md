# IResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/IResponsePostProcessor.cs`  
> **Kind:** interface

```csharp
public interface IResponsePostProcessor
```


This interface defines a contract for post-processing a model's raw text before it is persisted to the database. Implementations should remove residual AI boilerplate (common opener/closer phrases) and apply a length cap derived from the current ConversationState. Markdown is deliberately preserved to keep Discord-style formatting intact. In the runtime flow, the controller streams raw deltas to the client and defers cleaning until the final saved message is produced.

## Remarks
The abstraction decouples content sanitation from transport and storage, enabling centralized, swap-friendly hygiene rules. By depending on ConversationState, implementations can adapt the maximum allowed length per conversation or user context, ensuring consistent policy without changing callers. It also provides a single point to evolve markdown-preserving cleanup strategies.

## Example
```csharp
public class SimpleResponsePostProcessor : IResponsePostProcessor
{
    public string Clean(string raw, ConversationState? state)
    {
        // Remove common AI boilerplate (opener/closer phrases)
        var cleaned = RawCleaner.StripAiBoilerplate(raw);
        // Cap length based on state or a sensible default
        int cap = state?.MaxSavedLength ?? 2048;
        if (cleaned.Length > cap)
            cleaned = cleaned.Substring(0, cap);
        return cleaned;
    }
}
```

## Notes
- Implementations must gracefully handle a null ConversationState, falling back to sensible defaults.
- Cleaning should be idempotent and predictable for the same input.
- Be cautious not to remove user-provided content or meaningful information while stripping boilerplate; preserve intent and markdown formatting.