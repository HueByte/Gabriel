using Gabriel.Core.Configuration;
using Gabriel.Core.Repositories;
using Gabriel.Core.Services;
using Gabriel.Engine.Providers;
using Gabriel.Engine.Tools.Docs;
using Gabriel.Engine.Tools.Web;
using Gabriel.Infrastructure.Persistence;
using Gabriel.Infrastructure.Persistence.Repositories;
using Gabriel.Infrastructure.Projects;
using Gabriel.Infrastructure.Providers;
using Gabriel.Infrastructure.Tools.Docs;
using Gabriel.Infrastructure.Tools.Web;
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
        services.AddScoped<IProjectRepository, ProjectRepository>();

        // Project file storage (Phase 8). Options bound from Projects:Files,
        // disk-backed implementation persists under {Root}/{ProjectId:N}.
        services.Configure<ProjectFilesOptions>(config.GetSection(ProjectFilesOptions.SectionName));
        services.AddScoped<IProjectFileService, DiskProjectFileService>();

        AddChatProvider(services, config);
        AddWebSearch(services, config);
        AddWebFetch(services);
        AddDocsLookup(services, config);

        return services;
    }

    // Web page fetcher used by the web_fetch tool. A single HttpClient with a
    // sensible timeout and a normal browser UA - pages from major sites refuse
    // requests with a blank or scriptable-looking UA. Redirects allowed (the
    // SSRF guard runs against the FINAL destination via the request hooks).
    private static void AddWebFetch(IServiceCollection services)
    {
        services.AddHttpClient(HttpUrlFetcher.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        });

        services.AddSingleton<IUrlFetcher, HttpUrlFetcher>();
    }

    // Web search wiring. Default is DuckDuckGo - free, no API key, fine for
    // a single-user hobby deployment. Set Tools:Web:Active=brave to switch to
    // the Brave Search API (requires Tools:Web:Brave:ApiKey). Unknown values
    // log a warning and fall back to DDG so a typo doesn't break the tool.
    private static void AddWebSearch(IServiceCollection services, IConfiguration config)
    {
        var active = config["Tools:Web:Active"]?.Trim().ToLowerInvariant() ?? "ddg";

        switch (active)
        {
            case "brave":
                services.Configure<BraveSearchOptions>(config.GetSection(BraveSearchOptions.SectionName));
                services.AddHttpClient(BraveWebSearch.HttpClientName, (sp, client) =>
                {
                    var opts = sp.GetRequiredService<IOptions<BraveSearchOptions>>().Value;
                    client.BaseAddress = new Uri(opts.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
                    client.DefaultRequestHeaders.Add("X-Subscription-Token", opts.ApiKey);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });
                services.AddSingleton<IWebSearch, BraveWebSearch>();
                break;

            case "ddg":
            case "duckduckgo":
            default:
                services.AddHttpClient(DuckDuckGoWebSearch.HttpClientName, client =>
                {
                    client.BaseAddress = new Uri("https://html.duckduckgo.com/");
                    client.Timeout = TimeSpan.FromSeconds(15);
                    // DDG blocks blank/unfamiliar UAs. A normal browser UA passes
                    // their bot heuristics and gets a real HTML response.
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
                });
                services.AddSingleton<IWebSearch, DuckDuckGoWebSearch>();
                break;
        }
    }

    // GitHub-backed docs lookup. Two named HttpClients - one for the JSON API
    // (api.github.com, used by ListAsync) and one for the raw content host
    // (raw.githubusercontent.com, used by ReadAsync). Defaults to HueByte/PulsePixel
    // so the docs tool works out of the box.
    private static void AddDocsLookup(IServiceCollection services, IConfiguration config)
    {
        services.Configure<GitHubDocsOptions>(config.GetSection(GitHubDocsOptions.SectionName));

        services.AddHttpClient(GitHubDocsLookup.ApiHttpClientName, (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<GitHubDocsOptions>>().Value;
            client.BaseAddress = new Uri("https://api.github.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Add("User-Agent", "Gabriel-Docs-Lookup");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            if (!string.IsNullOrWhiteSpace(opts.Token))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {opts.Token}");
            }
        });

        services.AddHttpClient(GitHubDocsLookup.RawHttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("User-Agent", "Gabriel-Docs-Lookup");
        });

        services.AddSingleton<IDocsLookup, GitHubDocsLookup>();
    }

    private static void AddChatProvider(IServiceCollection services, IConfiguration config)
    {
        var active = config["Providers:Active"]?.ToLowerInvariant() ?? "mock";

        switch (active)
        {
            case "grok":
                // Read once at startup. The resilience pipeline's timeouts are
                // pipeline-level - no per-request live tuning, so a captured
                // value is correct here.
                var grokTimeout = TimeSpan.FromSeconds(
                    config.GetValue($"{GrokOptions.SectionName}:TimeoutSeconds", 900));

                // Bind + validate options once. Validation fires the first time
                // IOptions<GrokOptions>.Value is read (i.e. when the first
                // HttpClient is created), producing a clear OptionsValidationException
                // instead of an ad-hoc throw inside the HttpClient factory.
                services.ConfigureSection<GrokOptions>(config)
                    .Validate(
                        o => !string.IsNullOrWhiteSpace(o.ApiKey),
                        "Providers:Grok:ApiKey is required. Set via user-secrets or env var PROVIDERS__GROK__APIKEY.")
                    .Validate(
                        o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                        "Providers:Grok:BaseUrl must be a valid absolute URL ending with '/'.")
                    .Validate(
                        o => o.TimeoutSeconds > 0,
                        "Providers:Grok:TimeoutSeconds must be greater than zero.")
                    .Validate(
                        o => o.Models.Count(m => m.IsActive) == 1,
                        "Providers:Grok:Models must contain exactly one entry with IsActive=true.")
                    .Validate(
                        o => o.GetActiveModel() is { } m
                             && !string.IsNullOrWhiteSpace(m.Name)
                             && m.ContextWindowTokens > 0,
                        "Providers:Grok:Models active entry must have a non-empty Name and ContextWindowTokens > 0.");

                // DelegatingHandler resolved per HttpClient creation. Transient is
                // the required lifetime for handlers registered via AddHttpMessageHandler.
                services.AddTransient<GrokAuthHandler>();

                // Named HttpClient - consumed via IHttpClientFactory.CreateClient(name)
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
    // Retries stay at the default - a retry only fires before the response
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
