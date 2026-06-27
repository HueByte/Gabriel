# LogDateEnricher.cs

> **Source:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`

## Contents

- [LogDateEnricher](#logdateenricher)
- [LogDateEnricherExtensions](#logdateenricherextensions)

---

## LogDateEnricher

> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

Adds a "LogDate" property to every Serilog LogEvent containing the event's local date formatted as MM-dd-yyyy. Use this enricher when you want to route or write logs to files named with an arbitrary date pattern (for example via Serilog.Sinks.Map), since Serilog.Sinks.File only supports its built-in yyyyMMdd rolling-period format.

## Remarks
This enricher produces a simple, deterministic string property (named "LogDate") that downstream sinks can use for routing or filename composition. It's intentionally minimal and stateless: it formats the LogEvent.Timestamp using the event's LocalDateTime and CultureInfo.InvariantCulture so the resulting value is consistent across environments. The typical scenario is pairing this enricher with Serilog.Sinks.Map to create files whose names follow a custom date pattern.

## Example
```csharp
// Register the enricher and use Map to route events into files named by LogDate
var logger = new LoggerConfiguration()
    .Enrich.With(new LogDateEnricher())
    .WriteTo.Map("LogDate", (logDate, wt) => wt.File($"logs/{logDate}.log"))
    .CreateLogger();

logger.Information("This event will be routed into a file named like 06-08-2026.log");
```

## Notes
- The enricher uses logEvent.Timestamp.LocalDateTime (server local timezone). If you require UTC-based filenames, replace with UtcDateTime or provide a UTC-based enricher.
- The property name is exactly "LogDate"; Map or other sinks must reference this name to route correctly.
- The date is formatted with CultureInfo.InvariantCulture and the MM-dd-yyyy pattern; that pattern does not sort lexicographically for chronological ordering (use yyyy-MM-dd if lexicographic sort of filenames is needed).

---

## LogDateEnricherExtensions

> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

Exposes a Serilog enrichment extension that registers the LogDateEnricher. Use this when configuring Serilog (programmatically or via configuration) to add the LogDateEnricher into the logger pipeline; the method is surfaced as Enrich.WithLogDate so it can also be referenced by name from appsettings.Serilog.Enrich.

## Remarks
This static adapter exists to connect the concrete LogDateEnricher type with Serilog's extension-based enrichment API. By providing WithLogDate as an extension on LoggerEnrichmentConfiguration the enricher can be added fluently in code and can be discovered by Serilog's configuration system when enrichment entries are specified by name.

## Example
```csharp
// Programmatic registration
var logger = new LoggerConfiguration()
    .Enrich.WithLogDate()
    .WriteTo.Console()
    .CreateLogger();

// Configuration-based registration (appsettings.json)
// "Serilog": {
//   "Enrich": [ "WithLogDate" ]
// }
```

## Notes
- Ensure the assembly containing this extension (and LogDateEnricher) is referenced when relying on configuration-based enrichment; Serilog must be able to find the extension by name at runtime.
- The method is a thin factory: it registers LogDateEnricher and returns the LoggerConfiguration for chaining; there are no additional options exposed here.
- This extension is stateless and safe to call multiple times, but calling it repeatedly will register multiple instances of the enricher unless Serilog deduplicates enrichers at pipeline construction.

---