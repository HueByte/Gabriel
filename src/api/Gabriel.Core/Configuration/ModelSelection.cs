namespace Gabriel.Core.Configuration;

// Resolved per-turn model choice: the provider name + the wire-level model
// identifier + the context window + the optional per-model compact override.
// Built by IModelCatalog.Resolve from a user's PreferredProvider /
// PreferredModel (with a config-driven fallback) and threaded through the
// agent loop so the provider call, the compact heuristic, and the metrics
// endpoint all agree on which model is in play.
public sealed record ModelSelection(
    string Provider,
    string Name,
    int ContextWindowTokens,
    double? CompactThreshold);
