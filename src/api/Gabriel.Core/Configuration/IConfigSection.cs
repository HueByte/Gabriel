using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gabriel.Core.Configuration;

// Contract every options POCO bound from appsettings should implement so the
// section name lives next to the type (one source of truth, one rename to
// update). The static-abstract member is the C# 11 feature that lets a
// generic extension method read it without instantiating the type.
public interface IConfigSection<TSelf> where TSelf : class, IConfigSection<TSelf>
{
    static abstract string SectionName { get; }
}

public static class ConfigSectionExtensions
{
    // Bind appsettings -> options using the type's declared SectionName.
    // Caller still gets back the OptionsBuilder, so per-options validators
    // (e.g. .Validate(o => ...)) chain naturally.
    public static OptionsBuilder<TOptions> ConfigureSection<TOptions>(
        this IServiceCollection services,
        IConfiguration config)
        where TOptions : class, IConfigSection<TOptions>
    {
        return services
            .AddOptions<TOptions>()
            .Bind(config.GetSection(TOptions.SectionName));
    }
}
