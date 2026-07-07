# API configuration and logging

> Configurations and logging enhancements that shape how the API is bootstrapped and observed, including route prefixes and log enrichment.

Configurations and logging code here shape how the API boots and how it is observed: a convention centralizes the base API path, an Infisical-backed configuration provider pulls secrets into IConfiguration at bootstrap, an extension wires that provider into the ConfigurationBuilder, and a Serilog enricher stamps events with a date field so sinks can route logs by day. Read these short notes to understand what each file actually implements, what types and methods they expose, and how they hand off responsibilities during startup and logging pipeline configuration.

## GlobalRoutePrefixConvention.cs
Configures a global route prefix for API endpoints.

The [GlobalRoutePrefixConvention](../Code/src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs.md) class implements ASP.NET Core's IApplicationModelConvention and runs during application model construction to ensure every controller route is prefixed with a single base path (for example "api"). Concretely, it iterates controllers and their selectors and either assigns the prefix when a controller has no explicit route or combines the prefix with an existing route via AttributeRouteModel.CombineAttributeRouteModel, so the prefix layers under controller- and action-level Route attributes rather than replacing them. Typical use is adding an instance to services.AddControllers(options => options.Conventions.Add(new GlobalRoutePrefixConvention("api"))); the convention is orthogonal to configuration and logging code in this topic and collaborates with other MVC conventions by manipulating the application model during startup.

## InfisicalConfigurationProvider.cs
Provides Infisical-based configuration for the API.

The [InfisicalConfigurationProvider](../Code/src/api/Gabriel.API/Configuration/InfisicalConfigurationProvider.cs.md) derives from ConfigurationProvider and performs the work of authenticating to a self-hosted Infisical instance, fetching workspace/environment secrets during configuration building (bootstrap), and merging them into the IConfiguration key/value graph. Because it runs before DI is available the provider uses a short-lived HttpClient and writes failures to Console.Error rather than ILogger; it stores secrets in a case-insensitive dictionary and translates Infisical keys using "__" into IConfiguration paths with colons (e.g. PROVIDERS__GROK__APIKEY → Providers:Grok:ApiKey), enabling later IOptions binding and direct IConfiguration lookups. Startup continues if the provider cannot fetch secrets, but missing keys may surface as runtime errors when consumed.

## InfisicalExtensions.cs
Extends configuration with Infisical helpers.

The [InfisicalExtensions](../Code/src/api/Gabriel.API/Configuration/InfisicalExtensions.cs.md) static class exposes an AddInfisical extension that follows the ASP.NET Core/Options pattern: callers pass an Action<InfisicalOptions> to configure options and the method registers an Infisical configuration source with the IConfigurationBuilder, returning the same builder for fluent chaining. This extension is the registration surface that causes the Infisical configuration source/provider to run in the bootstrap phase described above; calling AddInfisical multiple times adds multiple sources (ordering matters), and callers must ensure required InfisicalOptions properties are supplied to avoid runtime configuration gaps.

## LogDateEnricher.cs
Enriches Serilog events with timestamps.

The file defines a sealed [LogDateEnricher](../Code/src/api/Gabriel.API/Configuration/LogDateEnricher.cs.md) that implements Serilog's ILogEventEnricher and attaches a string property named LogDate to every log event. The value is derived from the event's LocalDateTime formatted as MM-dd-yyyy (invariant culture) and is added via AddOrUpdateProperty so it overwrites any existing LogDate for the event; the implementation is thread-safe and stateless. The same file also exposes [LogDateEnricherExtensions](../Code/src/api/Gabriel.API/Configuration/LogDateEnricher.cs.md), adding a WithLogDate() helper so callers can call Enrich.WithLogDate() or reference the enricher by name from appsettings.Serilog.Enrich, enabling configuration-driven enrichment and downstream routing (for example with Serilog.Sinks.Map) to files named by date.

How the pieces fit

These files split responsibilities by lifecycle phase: the Infisical extension and provider participate in configuration building (bootstrap) to hydrate IConfiguration with remote secrets that later feed IOptions and runtime components; the global route convention runs when MVC builds its application model to enforce a consistent base path for all routes; and the log enricher hooks into Serilog's enrichment pipeline so sinks can route or file logs by a standardized LogDate property. Together they represent three orthogonal startup concerns—configuration bootstrap, MVC model shaping, and logging enrichment—each exposed through a small, testable surface that is registered during application startup.

- InfisicalExtensions registers the configuration source; that source/provider populates IConfiguration at bootstrap and enables later binding.
- GlobalRoutePrefixConvention modifies the MVC application model when AddControllers is invoked, layering the base path onto existing Route attributes.
- LogDateEnricher (and its WithLogDate extension) is wired into Serilog's Enrich pipeline so configuration or code can enable date-based routing of log events.

---
*Synthesised by Aurion on 2026-07-07 18:10:03 UTC*
