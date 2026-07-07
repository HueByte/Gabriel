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


Adds a LogDate property to every log event. The value is derived from the event's timestamp in local time and formatted as MM-dd-yyyy using invariant culture, then attached as a string property named `LogDate`. This enables downstream sinks (such as Serilog.Sinks.Map) to route each event to a file named after the date, since Serilog.Sinks.File is limited to its built-in yyyyMMdd rolling format.

## Remarks
This enricher centralizes the date extraction logic and exposes a single LogDate property per event, enabling date-based routing across sinks without embedding date logic in each sink configuration. It uses AddOrUpdateProperty to assign the date, overwriting any existing LogDate for the event, which keeps the value consistent with the current event; the class is sealed to avoid inheritance-related changes to its behavior.

## Notes
- The LogDate is derived from the event's LocalDateTime, so it reflects the host's time zone; for cross-location consistency, consider using UTC or a standardized time zone.
- The property name is hard-coded as `LogDate`; changing it would require modifying the enricher and recompiling.
- The enricher performs no shared-state mutation and is thread-safe with respect to Serilog's event pipeline.

---

## LogDateEnricherExtensions
> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

```csharp
public static class LogDateEnricherExtensions
```


Extends Serilog's enrichment configuration with a named helper that wires the LogDateEnricher into the enrichment pipeline. WithLogDate() is surfaced as Enrich.WithLogDate() so it can be referenced by name from appsettings.Serilog.Enrich, enabling configuration-driven enrichment without hard-coding the enricher type. It delegates to enrich.`With<LogDateEnricher>`() and returns the fluent LoggerConfiguration to allow further configuration via the standard Serilog chaining.

## Remarks
This extension encapsulates the concept of applying a log date field without leaking the concrete logic into consumer code. It provides a stable, discoverable entry point for enabling log-date enrichment from both code and configuration, and it participates in Serilog's established pattern of Enrich extensions, pairing the enrichment type with a name that can be used in settings.

## Notes
- Relies on a parameterless constructor or a resolvable factory for LogDateEnricher; otherwise registration may fail at runtime.
- Calling WithLogDate() multiple times may register duplicate enrichers; prefer a single invocation per logger configuration.
- If your configuration uses appsettings to enable enrichment, ensure the Serilog.Enrich path matches the recognized name (WithLogDate).

---