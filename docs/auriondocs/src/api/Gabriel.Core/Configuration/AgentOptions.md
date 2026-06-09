# AgentOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentOptions.cs`  
> **Kind:** class

Represents configuration settings that control conversational agent runtime behavior — when to compact conversation history and how many tool-call iterations are allowed per user turn. Reach for this type when tuning the agent's cost vs. context-fidelity tradeoffs or when binding agent-specific settings from configuration.

## Remarks
This POCO is the named configuration section for agent behavior (SectionName = "Agent") and is intended to be bound from application configuration (e.g., appsettings.json) via the IConfigSection pattern or the configuration binder. MaxIterations prevents runaway tool-call loops and excessive spend; CompactThreshold is a fraction of the provider's ContextWindowTokens used to decide when to trigger a rolling-summary compaction of history; CompactKeepLast preserves the most recent messages verbatim through compaction to retain short-term continuity.

## Example
```csharp
// appsettings.json
{
  "Agent": {
    "MaxIterations": 10,
    "CompactThreshold": 0.75,
    "CompactKeepLast": 8
  }
}

// Binding in code (Microsoft.Extensions.Configuration)
var agentOptions = configuration
    .GetSection(AgentOptions.SectionName)
    .Get<AgentOptions>();

// Or via options pattern
services.Configure<AgentOptions>(configuration.GetSection(AgentOptions.SectionName));
```

## Notes
- CompactThreshold is a fraction (0.0–1.0) of the provider's ContextWindowTokens — set it accordingly, not as an absolute token count.  
- CompactKeepLast counts messages (not tokens); increasing it raises short-term context fidelity but uses more tokens.  
- MaxIterations is a hard cap per user turn; raising it may increase cost and risk of loops.  
- These are simple configuration values (POCO); binding and lifecycle (reload-on-change, validation) depend on the host's configuration setup.