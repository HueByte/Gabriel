namespace Gabriel.Engine.Providers;

// One model the user can pick from in the UI dropdown. Flattened across
// providers — IModelCatalog walks every registered IChatProvider's Models
// list to build this. IsDefault marks the config-declared bootstrap choice
// (one entry across the whole catalog).
public sealed record AvailableModel(
    string Provider,
    string Name,
    int ContextWindowTokens,
    double? CompactThreshold,
    decimal InputPricePerMTokens,
    decimal OutputPricePerMTokens,
    decimal CacheReadPricePerMTokens,
    decimal CacheWritePricePerMTokens,
    bool IsDefault);
