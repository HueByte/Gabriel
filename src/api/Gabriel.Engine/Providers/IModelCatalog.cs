using Gabriel.Core.Configuration;

namespace Gabriel.Engine.Providers;

// Read-only view of every model exposed by every registered IChatProvider,
// plus the resolver the agent uses to map a user's preference into a
// concrete ModelSelection. Stays an interface so tests and future
// implementations (e.g. dynamic model discovery) can swap in.
public interface IModelCatalog
{
    IReadOnlyList<AvailableModel> AvailableModels { get; }

    // Returns the resolved selection for the user. If both preference fields
    // match a registered model, that wins. Otherwise falls back to the
    // config-declared default (the IsActive=true model). If no IsActive entry
    // exists, the first registered model is used as a last resort.
    ModelSelection Resolve(string? preferredProvider, string? preferredModel);
}
