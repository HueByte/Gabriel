namespace Gabriel.Engine.Providers;

// Maps a provider name to the concrete IChatProvider instance. We register
// every provider as singleton IChatProvider; the registry is what lets
// AgentService pick "the one named 'grok'" at request time rather than
// taking a fixed dependency.
public interface IChatProviderRegistry
{
    IChatProvider Resolve(string providerName);
    IReadOnlyCollection<string> AvailableProviders { get; }
}

public sealed class ChatProviderRegistry : IChatProviderRegistry
{
    private readonly Dictionary<string, IChatProvider> _byName;

    public ChatProviderRegistry(IEnumerable<IChatProvider> providers)
    {
        _byName = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IChatProvider Resolve(string providerName)
    {
        if (_byName.TryGetValue(providerName, out var p))
        {
            return p;
        }

        throw new InvalidOperationException(
            $"No chat provider registered with name '{providerName}'. " +
            $"Available: {string.Join(", ", _byName.Keys)}.");
    }

    public IReadOnlyCollection<string> AvailableProviders => _byName.Keys;
}
