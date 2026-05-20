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
using Microsoft.Extensions.Logging;
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
        services.AddScoped<IMemoryRepository, MemoryRepository>();

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

    // Docs lookup wiring. The model-facing IDocsLookup is a CompositeDocsLookup
    // that fans across two sources, in priority order:
    //
    //   1. LocalDocsLookup   - reads `docs/gabriel-self-docs/` on disk. These
    //                          pages are written specifically for the LLM to
    //                          consume and are the PRIMARY source of truth.
    //   2. GitHubDocsLookup  - reads the human-prose docs over GitHub raw.
    //                          Acts as fallback when a topic isn't covered in
    //                          the local LLM-native folder.
    //
    // The composite handles both ListAsync (union, primary entries first,
    // dedupe by path) and ReadAsync (try in order, first hit wins). A failing
    // source never poisons the others.
    private static void AddDocsLookup(IServiceCollection services, IConfiguration config)
    {
        // Local LLM-native source (primary).
        services.Configure<LocalDocsOptions>(config.GetSection(LocalDocsOptions.SectionName));
        services.AddSingleton<LocalDocsLookup>();

        // GitHub source (fallback). Two named HttpClients - one for the JSON
        // API (api.github.com, used by ListAsync), one for raw content
        // (raw.githubusercontent.com, used by ReadAsync). Defaults to
        // HueByte/Gabriel so the fallback works out of the box.
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

        services.AddSingleton<GitHubDocsLookup>();

        // Composite facade. Order of the IEnumerable<IDocsLookup> determines
        // priority - local first, GitHub second.
        services.AddSingleton<IDocsLookup>(sp => new CompositeDocsLookup(
            new IDocsLookup[]
            {
                sp.GetRequiredService<LocalDocsLookup>(),
                sp.GetRequiredService<GitHubDocsLookup>(),
            },
            sp.GetRequiredService<ILogger<CompositeDocsLookup>>()));
    }

    private static void AddChatProvider(IServiceCollection services, IConfiguration config)
    {
        // Mock is always registered — zero external deps, and acts as the
        // safety-net provider so AgentService never crashes with "no provider
        // available". It also gives dev a fallback in the UI picker.
        services.AddSingleton<IChatProvider, MockChatProvider>();

        // Grok wires up only when Providers:Grok exists in config with at
        // least one model. Each provider is its own named section bound via
        // the standard Options pipeline — secrets address the section by
        // name (PROVIDERS__GROK__APIKEY), independent of any ordering.
        var grokSection = config.GetSection(GrokOptions.SectionName);
        var grokModelsCount = grokSection.GetSection(nameof(LLMProviderOptions.Models)).GetChildren().Count();
        if (grokSection.Exists() && grokModelsCount > 0)
        {
            var grokBuilder = services.ConfigureSection<GrokOptions>(config)
                .Validate(
                    o => !string.IsNullOrWhiteSpace(o.ApiKey),
                    $"{GrokOptions.SectionName}:ApiKey is required. Set via env var PROVIDERS__GROK__APIKEY or Infisical.")
                .Validate(
                    o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _),
                    $"{GrokOptions.SectionName}:BaseUrl must be a valid absolute URL ending with '/'.")
                .Validate(
                    o => o.TimeoutSeconds > 0,
                    $"{GrokOptions.SectionName}:TimeoutSeconds must be greater than zero.")
                .Validate(
                    // At most one default — zero is allowed (means "no preferred
                    // default for this provider"); IModelCatalog handles a
                    // catalog-wide fallback if every provider declines.
                    o => o.Models.Count(m => m.IsActive) <= 1,
                    $"{GrokOptions.SectionName}:Models must contain at most one entry with IsActive=true.")
                .Validate(
                    o => o.Models.All(m => !string.IsNullOrWhiteSpace(m.Name) && m.ContextWindowTokens > 0),
                    $"{GrokOptions.SectionName}:Models entries must have a non-empty Name and ContextWindowTokens > 0.");

            // ValidateOnStart fires at host build, which the swagger codegen
            // pass also triggers. The codegen process doesn't have secrets
            // available, so the ApiKey validator would fail it. SKIP_DB_INIT
            // is the same flag the migration block uses for the same reason.
            if (Environment.GetEnvironmentVariable("SKIP_DB_INIT") != "true")
            {
                grokBuilder.ValidateOnStart();
            }

            // Read TimeoutSeconds once at startup for the resilience pipeline —
            // pipeline-level timeouts can't be tuned per request anyway, so a
            // captured value is correct here. Falls back to the default when
            // the key isn't set yet (validation above will catch that on host
            // start before any request reaches the pipeline).
            var grokTimeout = TimeSpan.FromSeconds(
                grokSection.GetValue(nameof(LLMProviderOptions.TimeoutSeconds), 900));

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
        }

        // Provider registry + model catalog — singletons that walk the
        // IEnumerable<IChatProvider> set above. Order of registration here
        // matters: these must come after every AddSingleton<IChatProvider>
        // call so the constructor's IEnumerable sees them all.
        services.AddSingleton<IChatProviderRegistry, ChatProviderRegistry>();
        services.AddSingleton<IModelCatalog, ModelCatalog>();
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
