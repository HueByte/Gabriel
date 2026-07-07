# LLMModel

> **File:** `src/api/Gabriel.Core/Configuration/LLMModel.cs`  
> **Kind:** class

```csharp
public class LLMModel
```


Represents per-model metadata for a hosted LLM. Use this type when adding or editing the models a provider exposes (size/price/context window/tool handling) or when reading model properties for token accounting, cost estimation and tool-routing.

## Remarks
This class is a compact configuration/DTO that lets the system treat multiple models from a single provider as configurable entries rather than hard-coded behavior. ApplicationUser.PreferredModel (a per-user override) and runtime discovery (GET /api/models) interact with these entries: IsActive is a bootstrap/default selection only, discovery returns all entries regardless of IsActive, and some settings (CompactThreshold) fall back to global AgentOptions when not set. The ContextWindowTokens and CompactThreshold are consumed by token-accounting and summarization logic; pricing fields feed cost-estimation and billing code.

## Example
```csharp
// Define a model in configuration
var model = new LLMModel
{
    Name = "grok-4.3",
    IsActive = true,
    ContextWindowTokens = 262_144,
    CompactThreshold = 0.18, // prefer to compact at ~18% of window
    InputPricePerMTokens = 0.30m,   // USD per 1,000,000 input tokens
    OutputPricePerMTokens = 0.40m,  // USD per 1,000,000 output tokens
    CacheReadPricePerMTokens = 0.05m,
    CacheWritePricePerMTokens = 0.10m,
    ToolMode = ToolMode.Native
};

// Rough per-call cost for 1,500 input tokens and 2,500 output tokens
decimal inputTokens = 1500m;
decimal outputTokens = 2500m;
decimal cost = (model.InputPricePerMTokens * (inputTokens / 1_000_000m))
             + (model.OutputPricePerMTokens * (outputTokens / 1_000_000m));

Console.WriteLine($"Estimated cost: ${cost:F6}");
```

## Notes
- Pricing fields are USD per million tokens. Multiply by (tokens / 1_000_000) to compute per-call cost; a value of 0 means "free" or "unknown" and is treated as free by accounting — validate non-zero for accurate cost tracking.
- CompactThreshold is nullable and, when null, the system falls back to AgentOptions.CompactThreshold; set it only when you need a model-specific override (e.g. to remain inside a cheaper vendor tier).
- IsActive is a default/bootstrap selection only. Per-user PreferredModel overrides runtime choice, and model discovery returns every configured model regardless of IsActive.
