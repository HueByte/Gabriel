using Microsoft.Extensions.Configuration;

namespace Gabriel.API.Configuration;

public static class InfisicalExtensions
{
    // Canonical Options-pattern signature - caller supplies an Action<TOptions>
    // (the same shape ASP.NET Core uses for AddDbContext, AddSwaggerGen, etc).
    public static IConfigurationBuilder AddInfisical(
        this IConfigurationBuilder builder,
        Action<InfisicalOptions> configure)
    {
        var opts = new InfisicalOptions();
        configure(opts);
        builder.Add(new InfisicalConfigurationSource(opts));
        return builder;
    }
}
