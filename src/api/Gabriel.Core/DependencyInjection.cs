using Gabriel.Core.Services;
using Gabriel.Core.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gabriel.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AgentOptions>(config.GetSection(AgentOptions.SectionName));

        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IToolRegistry, ToolRegistry>();
        services.AddSingleton<ITokenEstimator, NaiveTokenEstimator>();

        // Tool registrations. Each ITool added here is automatically discovered
        // by ToolRegistry via the IEnumerable<ITool> constructor injection.
        services.AddScoped<ITool, GetCurrentTimeTool>();

        return services;
    }
}
