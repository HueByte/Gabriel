using Microsoft.Extensions.Configuration;

namespace Gabriel.API.Configuration;

public class InfisicalConfigurationSource : IConfigurationSource
{
    private readonly InfisicalOptions _opts;

    public InfisicalConfigurationSource(InfisicalOptions opts)
    {
        _opts = opts;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new InfisicalConfigurationProvider(_opts);
}
