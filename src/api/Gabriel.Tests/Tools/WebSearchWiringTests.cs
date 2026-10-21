using Gabriel.Engine;
using Gabriel.Engine.Tools.Web;
using Gabriel.Infrastructure;
using Gabriel.Infrastructure.Tools.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gabriel.Tests.Tools;

// What Tools:Web:Active actually wires up. Resolution only - no searches run,
// so nothing touches the network or the metrics database.
public class WebSearchWiringTests
{
    private static IWebSearch Resolve(string active)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Tools:Web:Active"] = active })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEngineServices(config);
        services.AddInfrastructure(config);
        return services.BuildServiceProvider().GetRequiredService<IWebSearch>();
    }

    [Theory]
    [InlineData("searxng")]
    [InlineData("ddg")]
    // Aliases collapse before de-duplication: still one provider, not two.
    [InlineData("ddg,duckduckgo")]
    // Unknown keys fall back to DuckDuckGo rather than leaving the tool unwired.
    [InlineData("nonsense")]
    public void Single_provider_is_wrapped_directly(string active)
        => Assert.IsType<InstrumentedWebSearch>(Resolve(active));

    [Theory]
    [InlineData("searxng,ddg")]
    [InlineData("SearXNG, brave")]
    public void Several_providers_go_through_the_composite(string active)
        => Assert.IsType<CompositeWebSearch>(Resolve(active));
}
