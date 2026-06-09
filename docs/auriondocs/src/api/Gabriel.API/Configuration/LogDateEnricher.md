# LogDateEnricher.cs

> **Source:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`

## Contents

- [LogDateEnricher](#logdateenricher)
- [LogDateEnricherExtensions](#logdateenricherextensions)

---

## LogDateEnricher

> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

Adds a "LogDate" property to each Serilog LogEvent using the local timestamp formatted as MM-dd-yyyy. Use this enricher when you need Serilog.Sinks.Map to route events into files named by a human-readable date (for example "03-25-2026.log"), because Serilog.Sinks.File only supports its built-in yyyyMMdd rolling format.

## Remarks
This enricher injects a single property, "LogDate", computed from LogEvent.Timestamp.LocalDateTime and created with CultureInfo.InvariantCulture. It is intended as a lightweight plumbing piece that enables the Map sink to select a file name pattern based on a formatted date rather than relying on the File sink's limited rolling-period formats. The class is sealed and uses Serilog's ILogEventEnricher pattern so it can be registered directly on the LoggerConfiguration.

## Example
```csharp
// Register the enricher and route events into per-date files using the Map sink.
var logger = new LoggerConfiguration()
    .Enrich.With(new LogDateEnricher())
    .WriteTo.Map("LogDate", (logDate, writeTo) =>
        writeTo.File($"logs/{logDate}.log"))
    .CreateLogger();

logger.Information("This event will be written to logs/<MM-dd-yyyy>.log");
```

## Notes
- The enricher uses the event's LocalDateTime (local time zone). If you need UTC-based filenames, modify the implementation to use Timestamp.UtcDateTime instead.
- The date format is "MM-dd-yyyy" (e.g. 03-25-2026). The Map sink configuration must use the same property name and value format to route correctly.
- AddOrUpdateProperty is used, so if a "LogDate" property already exists on an event it will be replaced with this enricher's value.

---

## LogDateEnricherExtensions

> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

Adds the LogDateEnricher to Serilog's enrichment pipeline and exposes it as Enrich.WithLogDate so the enricher can be referenced by name from Serilog configuration (for example appsettings). Use this when you want the LogDateEnricher to be available both from code and from configuration-based enrichment.

## Remarks
This static extension is a thin wrapper that registers the LogDateEnricher type with Serilog's enrichment configuration. Exposing the enricher as Enrich.WithLogDate enables named lookup from configuration files (the same naming convention Serilog uses for built-in enrichers) without requiring callers to reference the enricher type directly.

## Example
```csharp
// Programmatic usage
var logger = new LoggerConfiguration()
    .Enrich.WithLogDate()
    .WriteTo.Console()
    .CreateLogger();

// When using appsettings.json, the enricher can be referenced by name
// (configuration example not shown here — reference "Enrich:With" -> "LogDate").
```

## Notes
- The method simply registers the LogDateEnricher type; Serilog will instantiate it via its usual mechanisms, so LogDateEnricher must have a public parameterless constructor to be created successfully.
- This extension does not provide configuration or parameters for the enricher; configure behavior on the enricher type itself if needed.

---