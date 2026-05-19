using Gabriel.Core.Repositories;
using Gabriel.Engine.Providers;
using Gabriel.Infrastructure.Persistence;
using Gabriel.Infrastructure.Persistence.Repositories;
using Gabriel.Infrastructure.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace Gabriel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? "Data Source=gabriel.db";

        services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        AddChatProvider(services, config);

        return services;
    }

    private static void AddChatProvider(IServiceCollection services, IConfiguration config)
    {
        var active = config["Providers:Active"]?.ToLowerInvariant() ?? "mock";

        switch (active)
        {
            case "grok":
                // Read once at startup. The resilience pipeline's timeouts are
                // pipeline-level — no per-request live tuning, so a captured
                // value is correct here.
                var grokTimeout = TimeSpan.FromSeconds(
                    config.GetValue($"{GrokOptions.SectionName}:TimeoutSeconds", 900));

                // Bind + validate options once. Validation fires the first time
                // IOptions<GrokOptions>.Value is read (i.e. when the first
                // HttpClient is created), producing a clear OptionsValidationException
                // instead of an ad-hoc throw inside the HttpClient factory.
                services.AddOptions<GrokOptions>()
                    .Bind(config.GetSection(GrokOptions.SectionName))
                    .Validate(
                        o => !string.IsNullOrWhiteSpace(o.ApiKey),
                        "Providers:Grok:ApiKey is required. Set via user-secrets or env var PROVIDERS__GROK__APIKEY.")
                    .Validate(
                        o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                        "Providers:Grok:BaseUrl must be a valid absolute URL ending with '/'.")
                    .Validate(
                        o => o.TimeoutSeconds > 0,
                        "Providers:Grok:TimeoutSeconds must be greater than zero.");

                // DelegatingHandler resolved per HttpClient creation. Transient is
                // the required lifetime for handlers registered via AddHttpMessageHandler.
                services.AddTransient<GrokAuthHandler>();

                // Named HttpClient — consumed via IHttpClientFactory.CreateClient(name)
                // inside GrokChatProvider. The Bearer header is applied by the
                // DelegatingHandler rather than DefaultRequestHeaders so a future
                // key rotation flows through without recycling the client.
                // HttpClient.Timeout is left infinite so the resilience pipeline
                // (added below) is the only timeout authority.
                services.AddHttpClient(GrokChatProvider.HttpClientName, (sp, client) =>
                    {
                        var opts = sp.GetRequiredService<IOptions<GrokOptions>>().Value;
                        client.BaseAddress = new Uri(opts.BaseUrl);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    })
                    .AddHttpMessageHandler<GrokAuthHandler>()
                    .AddStandardResilienceHandler(opts => ConfigureGrokResilience(opts, grokTimeout));

                services.AddSingleton<IChatProvider, GrokChatProvider>();
                break;

            case "mock":
            default:
                services.AddSingleton<IChatProvider, MockChatProvider>();
                break;
        }
    }

    // Standard resilience pipeline tuned for SSE chat streams. The defaults
    // (30s total / 10s per-attempt) would terminate any non-trivial generation
    // mid-stream, so both timeouts are driven by Providers:Grok:TimeoutSeconds.
    // Retries stay at the default — a retry only fires before the response
    // stream starts, so network/DNS/initial-5xx failures benefit, and once
    // tokens are flowing the pipeline is out of the picture for the duration
    // of that attempt. Circuit-breaker sampling is widened to satisfy the
    // framework's SamplingDuration >= 2 * AttemptTimeout validation rule.
    private static void ConfigureGrokResilience(HttpStandardResilienceOptions opts, TimeSpan totalTimeout)
    {
        opts.TotalRequestTimeout.Timeout = totalTimeout;
        opts.AttemptTimeout.Timeout = totalTimeout;
        opts.CircuitBreaker.SamplingDuration = TimeSpan.FromTicks(totalTimeout.Ticks * 2);
    }
}
