using Gabriel.Core.Configuration;
using Gabriel.Core.Repositories;
using Gabriel.Core.Services;
using Gabriel.Engine.Providers;
using Gabriel.Engine.Services;
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
        services.AddScoped<IMetricRepository, MetricRepository>();

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

    // Web search wiring. `Tools:Web:Active` is a comma-separated list of
    // provider keys (one or many). Examples:
    //   "ddg"                 -> DuckDuckGo only (default; free, no key).
    //   "brave"               -> Brave only (requires Tools:Web:Brave:ApiKey).
    //   "tavily"              -> Tavily only (requires Tools:Web:Tavily:ApiKey).
    //   "tavily,brave,ddg"    -> all three, parallel-queried + merged by
    //                            CompositeWebSearch. Cross-provider hits rank
    //                            first. Unknown keys are dropped with a warn.
    // The order in the list doesn't affect ranking - merging is rank-aware
    // already. With one provider the composite is bypassed and we register
    // the bare implementation as IWebSearch.
    private static void AddWebSearch(IServiceCollection services, IConfiguration config)
    {
        // Per-provider call recording is handled by InstrumentedWebSearch (a
        // decorator below), writing into the generic IMetricRecorder. Reads
        // come from IMetricRepository, queried by the /diagnostics/web-search
        // endpoint. Both are already registered by Engine + the EF wiring
        // above; nothing to add here besides the decorator.

        var raw = config["Tools:Web:Active"]?.Trim() ?? "ddg";
        var requested = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToLowerInvariant())
            .Distinct()
            .ToList();

        // Map each known key to a registration delegate that adds the named
        // HttpClient + the concrete singleton, and returns the concrete type
        // + display name so we can wrap it in InstrumentedWebSearch when
        // building the final IWebSearch.
        var registered = new List<(Type ConcreteType, string DisplayName)>(requested.Count);
        foreach (var key in requested)
        {
            switch (key)
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
                    services.AddSingleton<BraveWebSearch>();
                    registered.Add((typeof(BraveWebSearch), "Brave"));
                    break;

                case "tavily":
                    services.Configure<TavilySearchOptions>(config.GetSection(TavilySearchOptions.SectionName));
                    services.AddHttpClient(TavilyWebSearch.HttpClientName, (sp, client) =>
                    {
                        var opts = sp.GetRequiredService<IOptions<TavilySearchOptions>>().Value;
                        client.BaseAddress = new Uri(opts.BaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                    });
                    services.AddSingleton<TavilyWebSearch>();
                    registered.Add((typeof(TavilyWebSearch), "Tavily"));
                    break;

                case "ddg":
                case "duckduckgo":
                    ConfigureDdgHttpClient(services);
                    services.AddSingleton<DuckDuckGoWebSearch>();
                    registered.Add((typeof(DuckDuckGoWebSearch), "DuckDuckGo"));
                    break;

                default:
                    // Don't crash on a typo - just skip. Empty `registered`
                    // is handled by the fallback below.
                    break;
            }
        }

        // Fallback: bad config entirely (no recognized keys) -> DDG, so the
        // tool keeps working instead of throwing at first call.
        if (registered.Count == 0)
        {
            ConfigureDdgHttpClient(services);
            services.AddSingleton<DuckDuckGoWebSearch>();
            registered.Add((typeof(DuckDuckGoWebSearch), "DuckDuckGo"));
        }

        if (registered.Count == 1)
        {
            // Single-provider path: wrap in the metrics decorator so the
            // diagnostics endpoint still tells us when this provider stops
            // working. No composite overhead, no merge logic in the hot path.
            var (concreteType, name) = registered[0];
            services.AddSingleton<IWebSearch>(sp =>
            {
                var inner = (IWebSearch)sp.GetRequiredService(concreteType);
                var recorder = sp.GetRequiredService<IMetricRecorder>();
                return new InstrumentedWebSearch(inner, recorder, name);
            });
        }
        else
        {
            // Multi-provider path: wrap each provider in InstrumentedWebSearch,
            // then hand the wrapped instances to the composite. The composite
            // sees the decorators - so its per-provider error catch still
            // works, and the metric event log records every per-provider call
            // before the composite's merge ever runs.
            services.AddSingleton<IWebSearch>(sp =>
            {
                var recorder = sp.GetRequiredService<IMetricRecorder>();
                var instances = registered
                    .Select(r => (IWebSearch)new InstrumentedWebSearch(
                        (IWebSearch)sp.GetRequiredService(r.ConcreteType),
                        recorder,
                        r.DisplayName))
                    .ToList();
                var logger = sp.GetRequiredService<ILogger<CompositeWebSearch>>();
                return new CompositeWebSearch(instances, logger);
            });
        }
    }

    // Registers the named HttpClient for DuckDuckGoWebSearch. Pulled out so
    // the active-providers path and the empty-config fallback share one
    // source of truth - both want the same bot-resistant header set and
    // the same automatic gzip/deflate handling (DDG compresses responses
    // for browsers; without AutomaticDecompression we'd read raw bytes
    // and the HTML parser would silently fail).
    private static void ConfigureDdgHttpClient(IServiceCollection services)
    {
        services.AddHttpClient(DuckDuckGoWebSearch.HttpClientName, client =>
        {
            // Endpoint URLs are absolute in DuckDuckGoWebSearch - the html/
            // and lite/ subdomains differ, so we can't share one BaseAddress.
            client.Timeout = TimeSpan.FromSeconds(15);
            // Full browser-like header set. DDG flags requests that look
            // synthetic (UA-only) more aggressively than ones carrying the
            // sticks a real Chrome navigation has - Accept, Accept-Language,
            // and the Sec-Fetch-* family in particular. Adding these lifts
            // the request out of the "obvious scraper" bucket. Cookies
            // aren't required for the html/ + lite/ endpoints.
            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add(
                "Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Without this, sending Accept-Encoding (implicit when the server
            // negotiates it via vary headers) means we receive gzipped bytes
            // and ReadAsStringAsync hands back gibberish that doesn't parse.
            AutomaticDecompression = System.Net.DecompressionMethods.All,
        });
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
