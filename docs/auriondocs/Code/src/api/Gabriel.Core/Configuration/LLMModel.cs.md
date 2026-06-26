# LLMModel

> **File:** `src/api/Gabriel.Core/Configuration/LLMModel.cs`  
> **Kind:** class

Represents configuration and runtime metadata for a single LLM offered by a provider — including the provider-facing model identifier, pricing (USD per million tokens), context window size, compaction trigger, caching prices, and how the model handles tool calls. Use this when adding or editing models in configuration, when selecting a default model for new users, or when performing token-accounting and cost-estimation.

## Remarks
This class centralizes the per-model information the system needs to make runtime decisions (which model to pick), perform token accounting (when to trim or summarize), and estimate usage cost. IsActive is a deployment-level default; a per-user PreferredModel (on ApplicationUser) can override it at runtime. Pricing values are expressed per million tokens to match vendor pricing pages and must be scaled by usage/1_000_000 when computing per-call costs.

## Example
```csharp
var model = new LLMModel
{
    Name = "grok-4.3",
    IsActive = true,
    ContextWindowTokens = 200_000,
    CompactThreshold = 0.18, // keep history inside cheaper pricing tier
    InputPricePerMTokens = 0.50m,
    OutputPricePerMTokens = 0.60m,
    CacheReadPricePerMTokens = 0m,
    CacheWritePricePerMTokens = 1.0m,
    ToolMode = ToolMode.Native
};

// Estimate cost for 15k input tokens and 5k output tokens
decimal inputCost = model.InputPricePerMTokens * (15_000m / 1_000_000m);
decimal outputCost = model.OutputPricePerMTokens * (5_000m / 1_000_000m);
```

## Notes
- CompactThreshold is nullable; when null the system falls back to AgentOptions.CompactThreshold.
- A price of 0 means "free" or "unknown"; accounting treats unknown as free — validate non-zero prices for accurate cost-tracking of active models.
- IsActive is a fallback/default only; per-user preferences may select a different model at runtime.
- Cache read/write prices are per-million-tokens; leave at 0 for providers that do not support caching.
- ToolMode defaults to Native. Emulated mode wraps the provider in GabrielToolBridge (injecting tool docs and parsing tool-call markers); None disables tool support for that model.