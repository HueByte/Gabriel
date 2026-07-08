# LogDateEnricher.cs

> **Source:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`

## Contents

- [LogDateEnricher](#logdateenricher)
- [LogDateEnricherExtensions](#logdateenricherextensions)

---

## LogDateEnricher
> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

```csharp
public sealed class LogDateEnricher : ILogEventEnricher
```


LogDateEnricher enriches every Serilog log event with a LogDate property, enabling downstream sinks to route logs by date. It derives the date from the event's timestamp in local time, formats it as MM-dd-yyyy using invariant culture, and adds or updates the property so sinks like Serilog.Sinks.Map can dispatch events to date-named files.

## Remarks
Encapsulating the date-formatting logic in an ILogEventEnricher keeps routing concerns separate from business logic and makes the behavior easy to reuse across different log sources. Using LocalDateTime respects the host's time zone for the reported date, and AddOrUpdateProperty ensures the property can be consistently overwritten by subsequent enrichers without creating duplicates. The pattern is specifically intended for Map-based routing where the property value drives the target file name; if you aren't using Map, the enrichment has no routing effect.

## Notes
- Time zone caveat: The date is derived from the host's local time; in multi-region deployments you may want to standardize on a single time zone or switch to a UTC-based date to ensure consistent routing.
- Property naming: The property is named 'LogDate'. If other parts of the system rely on a different property name, adjust accordingly or harmonize across enrichers.
- Format mutability: The MM-dd-yyyy pattern is fixed here; changing it affects the downstream Map routing and file-naming conventions.

---

## LogDateEnricherExtensions
> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

```csharp
public static class LogDateEnricherExtensions
```


Adds a strongly-typed extension method WithLogDate on LoggerEnrichmentConfiguration that wires the LogDateEnricher into Serilog's enrichment pipeline. It lets you register the date-enrichment either in code via Enrich.WithLogDate() or by name in appsettings Serilog.Enrich, without needing to reference the concrete enricher type directly.

## Remarks

By hiding the concrete LogDateEnricher behind a named extension, this symbol provides a stable, discoverable entry point for configuration. It participates in Serilog's extension-based enrichment flow and keeps the codebase decoupled from the enricher's implementation. Because it is a thin wrapper over Serilog's Enrich.`With<T>`(), it remains fully compatible with the standard Serilog configuration pipeline.

## Notes

- Ensure the extension's namespace is imported wherever you configure Serilog; extension methods require the appropriate using directive to be visible.
- This is a light wrapper around Enrich.`With<LogDateEnricher>`(); verify that LogDateEnricher is available at runtime to avoid enrichment failures.


---