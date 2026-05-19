using Gabriel.Core.Configuration;

namespace Gabriel.Engine.Providers;

// Concrete IModelCatalog. Builds its catalog once at construction by walking
// every registered IChatProvider's Models list — singletons all the way, so
// the iteration cost only happens on first resolve.
public sealed class ModelCatalog : IModelCatalog
{
    private readonly IReadOnlyList<AvailableModel> _models;
    private readonly ModelSelection _default;

    public ModelCatalog(IEnumerable<IChatProvider> providers)
    {
        var available = new List<AvailableModel>();
        AvailableModel? bootstrapDefault = null;

        foreach (var provider in providers)
        {
            foreach (var model in provider.Models)
            {
                var entry = new AvailableModel(
                    Provider: provider.Name,
                    Name: model.Name,
                    ContextWindowTokens: model.ContextWindowTokens,
                    CompactThreshold: model.CompactThreshold,
                    InputPricePerMTokens: model.InputPricePerMTokens,
                    OutputPricePerMTokens: model.OutputPricePerMTokens,
                    CacheReadPricePerMTokens: model.CacheReadPricePerMTokens,
                    CacheWritePricePerMTokens: model.CacheWritePricePerMTokens,
                    IsDefault: model.IsActive);

                available.Add(entry);
                if (model.IsActive && bootstrapDefault is null)
                {
                    bootstrapDefault = entry;
                }
            }
        }

        _models = available;

        // No IsActive declared anywhere — fall back to the first registered
        // model. Mock should always be present so this branch keeps the agent
        // booting even on a wholly-misconfigured deployment.
        bootstrapDefault ??= available.FirstOrDefault();

        if (bootstrapDefault is null)
        {
            throw new InvalidOperationException(
                "No chat models registered. Every IChatProvider returned an empty Models list. " +
                "Register Mock (always available) or configure at least one Providers[] entry.");
        }

        _default = new ModelSelection(
            bootstrapDefault.Provider,
            bootstrapDefault.Name,
            bootstrapDefault.ContextWindowTokens,
            bootstrapDefault.CompactThreshold);
    }

    public IReadOnlyList<AvailableModel> AvailableModels => _models;

    public ModelSelection Resolve(string? preferredProvider, string? preferredModel)
    {
        if (!string.IsNullOrWhiteSpace(preferredProvider) && !string.IsNullOrWhiteSpace(preferredModel))
        {
            var match = _models.FirstOrDefault(m =>
                string.Equals(m.Provider, preferredProvider, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(m.Name, preferredModel, StringComparison.Ordinal));

            if (match is not null)
            {
                return new ModelSelection(
                    match.Provider,
                    match.Name,
                    match.ContextWindowTokens,
                    match.CompactThreshold);
            }
            // Stale preference (model was removed from config) — silently fall
            // back to the default. The user will see the dropdown reflect the
            // actual current selection on next page load.
        }

        return _default;
    }
}
