namespace Gabriel.API.Contracts.Models;

// Wire shape of one entry in the /api/models response. Pricing is per-million
// tokens; cache fields stay 0 for providers that don't expose caching pricing.
public sealed record ModelDto(
    string Provider,
    string Name,
    int ContextWindowTokens,
    decimal InputPricePerMTokens,
    decimal OutputPricePerMTokens,
    decimal CacheReadPricePerMTokens,
    decimal CacheWritePricePerMTokens,
    bool IsDefault,
    bool IsSelected);

// GET /api/models response. AvailableModels is everything the catalog
// exposes; Selected echoes what the agent will currently use for this user
// (config default if they haven't picked yet).
public sealed record ModelsResponse(
    IReadOnlyList<ModelDto> AvailableModels,
    SelectedModelDto Selected);

public sealed record SelectedModelDto(string Provider, string Name);

// PUT /api/models/active body. Pass null/null to clear the preference and
// fall back to the config default.
public sealed record SetActiveModelRequest(string? Provider, string? Name);
