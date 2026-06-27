# AgentOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentOptions.cs`  
> **Kind:** class

Configuration object that controls agent runtime behaviour related to tool-call iteration limits and conversation compaction. Use this when you need to tune how many tool-call iterations an agent may perform per user turn (to limit cost and runaway loops) and when to trigger rolling-summary compaction of conversation history to stay within an LLM provider's context window.

## Remarks
This POCO centralizes small-but-critical heuristics for safety and memory control: MaxIterations enforces a hard cap on how many tool-invocation iterations are allowed per user turn to avoid runaway computation and excessive cost; CompactThreshold and CompactKeepLast guide the rolling-summary process that trims conversation history as the estimated token usage approaches the model's context capacity (ContextWindowTokens). The class is intentionally minimal — it only holds tunable values; enforcement and token-estimation are performed by the agent runtime.

## Example
```csharp
// Create custom options (could be bound from configuration in an app)
var options = new AgentOptions
{
    MaxIterations = 10,         // allow a few more tool calls for complex tasks
    CompactThreshold = 0.75,    // start compacting when history reaches 75% of model context
    CompactKeepLast = 8         // keep last 8 messages verbatim for continuity
};

// Pass options to the agent builder/runtime (consumer-specific)
agentRuntime.Configure(options);
```

## Notes
- MaxIterations is a hard cap intended to limit cost and runaway loops — raising it increases risk and spend.
- CompactThreshold is a fraction (0.0–1.0) of the provider's ContextWindowTokens; its effect depends on the model's context size.
- CompactKeepLast preserves recent conversational continuity but increases retained token usage; set it low to save context, or higher to keep more short-term context.
- The class exposes no validation — consumers should validate these values if they must enforce additional constraints or bounds.