using Gabriel.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gabriel.Core;

// Domain wiring only. The agent stack (LLM providers, tools, personality, ReAct
// loop) lives in Gabriel.Engine and is registered via AddEngineServices.
public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        return services;
    }
}
