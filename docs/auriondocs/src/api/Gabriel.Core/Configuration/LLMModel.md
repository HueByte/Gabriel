# LLMModel

> **File:** `src/api/Gabriel.Core/Configuration/LLMModel.cs`  
> **Kind:** class

Represents configuration and per-model metadata for a hosted LLM. Use this when registering or configuring models that your application can send requests to — it captures the provider's wire-level model identifier, context window size, per‑token pricing, cache pricing, the compacting trigger, activation status, and how the model should handle tool calls.

## Remarks
This class is a lightweight configuration object used by provider adapters, cost/accounting logic, and token-management code. It exists so operators can add or swap models (different sizes, price points, or context budgets) via configuration rather than code changes. Runtime behaviour (which model is actually used for a user) is influenced by a per-user preference; IsActive is only the system default and does not filter the model discovery endpoint.

## Example
```csharp
var model = new LLMModel
{
    // must match what the provider API expects
    Name = "grok-4.3",
    IsActive = true,
    ContextWindowTokens = 262144,

    // null => fall back to AgentOptions.CompactThreshold
    CompactThreshold = 0.18,

    // USD per million tokens
    InputPricePerMTokens = 3.50M,
    OutputPricePerMTokens = 6.00M,

    // cache prices (0 means provider doesn't support cached billing)
    CacheReadPricePerMTokens = 0M,
    CacheWritePricePerMTokens = 0M,

    // how tool calls are handled for this model
    ToolMode = ToolMode.Emulated
};
```

## Notes
- A price of 0 for any per-MTokens field is treated as "free or unknown" by accounting; active model validation should ensure non-zero prices when accurate cost tracking is required.
- CompactThreshold is nullable; when null the token-compaction logic should use AgentOptions.CompactThreshold.
- Name is the provider's wire-level identifier (what you send in API requests), not a marketing or friendly name.
- IsActive is a default fallback only; GET /api/models returns every configured entry regardless of IsActive.
- ToolMode defaults to ToolMode.Native; set to Emulated to wrap provider calls with GabrielToolBridge or to None to disable tools for models that can't support them.
