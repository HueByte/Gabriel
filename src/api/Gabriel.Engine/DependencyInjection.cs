using Gabriel.Engine.Personality;
using Gabriel.Engine.Sequence;
using Gabriel.Engine.Services;
using Gabriel.Engine.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gabriel.Engine;

// All AI / LLM / ReAct wiring. Providers themselves (Grok, Mock) are registered
// from Gabriel.Infrastructure since they own the HTTP/transport concerns, but
// every interface they implement and every service that consumes them is bound
// here.
public static class DependencyInjection
{
    public static IServiceCollection AddEngineServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AgentOptions>(config.GetSection(AgentOptions.SectionName));
        services.Configure<PersonalityOptions>(config.GetSection(PersonalityOptions.SectionName));

        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IToolRegistry, ToolRegistry>();
        services.AddSingleton<ITokenEstimator, NaiveTokenEstimator>();

        // Personality stack — all three are pure / config-driven, so singleton.
        services.AddSingleton<IConversationStateUpdater, HeuristicConversationStateUpdater>();
        services.AddSingleton<ISystemPromptBuilder, GabrielSystemPromptBuilder>();
        services.AddSingleton<IResponsePostProcessor, ResponsePostProcessor>();

        // Gabriel Sequence (Phase 10) — stateless generator + scoped service that
        // loads the conversation to resolve seed + state.
        services.AddSingleton<IGabrielSequenceGenerator, GabrielSequenceGenerator>();
        services.AddScoped<IGabrielSequenceService, GabrielSequenceService>();

        // Tool registrations. Each ITool added here is automatically discovered
        // by ToolRegistry via the IEnumerable<ITool> constructor injection.
        services.AddScoped<ITool, GetCurrentTimeTool>();

        return services;
    }
}
