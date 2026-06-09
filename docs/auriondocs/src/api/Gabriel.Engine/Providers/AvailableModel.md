# AvailableModel

> **File:** `src/api/Gabriel.Engine/Providers/AvailableModel.cs`  
> **Kind:** record

Represents a single model option shown to users (for example in a UI dropdown). Instances are a flattened view of models across providers and carry identifying information (provider and model name), capacity (context window), optional heuristics (CompactThreshold), pricing for different kinds of token operations, a flag marking the config-declared bootstrap choice (IsDefault), and the intended ToolMode.

## Remarks
This record is a provider-agnostic, read-only summary used by cataloging and selection code. IModelCatalog builds a list of AvailableModel entries by iterating every registered IChatProvider's Models; the result is convenient for displaying choices, comparing cost/capacity across providers, and making selection decisions (including honoring a single config-declared default). The fields expose the data most callers need without requiring provider-specific types.

## Notes
- Prices are expressed per MTokens (the field names include "PerMTokens"); callers must convert these values to the token unit they use when computing actual costs.
- CompactThreshold is nullable — a null value means "no compaction hint" and should be treated accordingly by callers that implement compaction logic.
- IsDefault is a catalog-level hint (the codebase expects at most one default entry); this type does not enforce uniqueness.
- As a C# record, AvailableModel is immutable after creation.