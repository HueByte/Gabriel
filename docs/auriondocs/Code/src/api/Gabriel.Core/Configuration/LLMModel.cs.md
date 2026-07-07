# LLMModel

> **File:** `src/api/Gabriel.Core/Configuration/LLMModel.cs`  
> **Kind:** class

```csharp
public class LLMModel
```


Represents per-model configuration and pricing used by the application to talk to an LLM provider. Store the wire-level model identifier (the exact string sent to the provider), context window size, per-million-token pricing for input/output and optional cache read/write costs, plus how the model should handle tool calls. Use this type when adding or editing provider models in configuration so switching models can be a config change rather than a code change.

## Remarks
LLMModel centralizes the metadata needed by token-accounting, cost-estimation, model discovery, and runtime selection. IsActive acts as a bootstrap/default selection but per-user preferences (PreferredModel on ApplicationUser) override it at runtime; discovery endpoints return all configured models regardless of this flag. CompactThreshold is nullable so it can inherit a global fallback from AgentOptions when not set per-model. ToolMode controls whether tools are handled natively, emulated, or disabled for this specific model.

## Example
```csharp
// Create a model entry and compute the dollar cost for a call that used 12,345 tokens.
var model = new LLMModel
{
    Name = "grok-4.3",
    IsActive = true,
    ContextWindowTokens = 200_000,
    CompactThreshold = null, // fall back to AgentOptions if null
    InputPricePerMTokens = 0.50m, // $0.50 per 1,000,000 input tokens
    OutputPricePerMTokens = 0.75m,
    CacheReadPricePerMTokens = 0m,
    CacheWritePricePerMTokens = 0m,
    ToolMode = ToolMode.Native
};

var agentOptions = new AgentOptions { CompactThreshold = 0.18 };
// effective compact threshold uses model override when present, otherwise global
double effectiveCompact = model.CompactThreshold ?? agentOptions.CompactThreshold;

int tokensUsed = 12_345;
decimal inputCost = model.InputPricePerMTokens * tokensUsed / 1_000_000m;
decimal outputCost = model.OutputPricePerMTokens * tokensUsed / 1_000_000m;
decimal totalCost = inputCost + outputCost;
```

## Notes
- Pricing values are USD per million tokens; divide by 1_000_000 and multiply by actual tokens used to get per-call dollars.
- A zero price is treated as "free or unknown" by accounting; validate non-zero pricing for any model you mark as active if you require accurate cost tracking.
- If CompactThreshold is null the system should fall back to AgentOptions.CompactThreshold; set the per-model value only when you need to override global behavior (for example to keep a conversation inside a cheaper tier of a provider).
