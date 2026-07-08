# LLMModel

> **File:** `src/api/Gabriel.Core/Configuration/LLMModel.cs`  
> **Kind:** class

```csharp
public class LLMModel
```


Per-model metadata used by the application to represent a specific hosted LLM (identifier, context size, pricing and tool handling). Reach for this type when adding or configuring a provider model (different sizes / price points / capabilities) so the system can switch models via configuration rather than code and perform token accounting and cost estimates.

## Remarks
This class centralizes the model-specific settings that other parts of the system rely on: token-budgeting (ContextWindowTokens and CompactThreshold), cost accounting (per‑million-token prices), activation/selection (IsActive as the default bootstrap model) and tool-call behavior (ToolMode). Pricing values are stored as USD per million tokens to match vendor price-page units; callers multiply by usage/1_000_000 to compute costs. CompactThreshold is nullable so a global AgentOptions.CompactThreshold can provide a fallback when a model-specific override isn't supplied.

## Notes
- Prices (InputPricePerMTokens / OutputPricePerMTokens / CacheReadPricePerMTokens / CacheWritePricePerMTokens) are denominated per million tokens; a value of zero is treated as "free or unknown" by accounting code — validate non‑zero for active models if accurate cost-tracking is required.
- CompactThreshold is optional (double?). When null, code should fall back to AgentOptions.CompactThreshold; use the model override only when you need behavior different from the global agent setting.
- IsActive is a bootstrap/fallback default; user-level preferences (e.g. a per-user PreferredModel) override it at runtime. ToolMode defaults to Native; Emulated wraps providers via the GabrielToolBridge approach described in comments, and None disables tools for models that can't handle them.