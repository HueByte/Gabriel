using Gabriel.Core.Configuration;
using Gabriel.Engine.Personality;
using Gabriel.Engine.Personality.Prompts;
using Gabriel.Engine.Sequence;
using Gabriel.Engine.Services;
using Gabriel.Engine.Tools;
using Gabriel.Engine.Tools.Docs;
using Gabriel.Engine.Tools.Calc;
using Gabriel.Engine.Tools.Codecs;
using Gabriel.Engine.Tools.Files;
using Gabriel.Engine.Tools.Memory;
using Gabriel.Engine.Tools.Numbers;
using Gabriel.Engine.Tools.Projects;
using Gabriel.Engine.Tools.Web;
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
        services.Configure<AgentToolsOptions>(config.GetSection(AgentToolsOptions.SectionName));

        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IToolRegistry, ToolRegistry>();
        services.AddSingleton<ITokenEstimator, NaiveTokenEstimator>();

        // Generic metric event log. Singleton write surface (bridges to the
        // scoped IMetricRepository via IServiceScopeFactory on each call).
        // Subsystems record via IMetricRecorder; the diagnostics endpoint
        // reads via IMetricRepository directly.
        services.AddSingleton<IMetricRecorder, MetricRecorder>();

        // Per-request tool execution context. AgentService.Set populates it
        // once per turn; project-scoped tools read from it.
        services.AddScoped<IToolExecutionContext, ToolExecutionContext>();

        // Personality stack - all four are pure / config-driven, so singleton.
        // The prompt registry holds every named persona fragment; the system
        // prompt builder pulls from it on every turn.
        services.AddSingleton<IPromptRegistry, PromptRegistry>();
        services.AddSingleton<IConversationStateUpdater, HeuristicConversationStateUpdater>();
        services.AddSingleton<ISystemPromptBuilder, GabrielSystemPromptBuilder>();
        services.AddSingleton<IResponsePostProcessor, ResponsePostProcessor>();

        // Gabriel Sequence (Phase 10) - stateless generator + scoped service that
        // loads the conversation to resolve seed + state.
        services.AddSingleton<IGabrielSequenceGenerator, GabrielSequenceGenerator>();
        services.AddScoped<IGabrielSequenceService, GabrielSequenceService>();

        // Tool registrations. Each ITool added here is automatically discovered
        // by ToolRegistry via the IEnumerable<ITool> constructor injection.
        // Tools that depend on Infrastructure-side providers (IWebSearch,
        // IDocsLookup) are registered here but their providers come from
        // Gabriel.Infrastructure.DependencyInjection.AddInfrastructure.
        services.AddScoped<ITool, GetCurrentTimeTool>();
        services.AddScoped<ITool, CalculateTool>();
        services.AddScoped<ITool, BaseConvertTool>();
        services.AddScoped<ITool, Base64Tool>();
        services.AddScoped<ITool, WebSearchTool>();
        services.AddScoped<ITool, WebFetchTool>();
        services.AddScoped<ITool, DocsListTool>();
        services.AddScoped<ITool, DocsReadTool>();
        services.AddScoped<ITool, ListProjectFilesTool>();
        services.AddScoped<ITool, ReadProjectFileTool>();

        // Memory tools — Claude-style. Scope is "user" (cross-project) or
        // "project" (this conversation's project only).
        services.AddScoped<ITool, MemorySaveTool>();
        services.AddScoped<ITool, MemoryListTool>();
        services.AddScoped<ITool, MemoryRemoveTool>();

        // Filesystem tools (Phase 12). Path resolution is shared so the same
        // host-vs-project hardening applies to every file tool.
        services.AddScoped<IAgentPathResolver, AgentPathResolver>();
        services.AddScoped<ITool, FileInfoTool>();
        services.AddScoped<ITool, ListDirTool>();
        services.AddScoped<ITool, FindTool>();
        services.AddScoped<ITool, GrepTool>();

        return services;
    }
}
