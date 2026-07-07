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


Adds a LogDate property to every Serilog log event by formatting the event's timestamp as MM-dd-yyyy and attaching it to the event so downstream sinks or routing rules can target a date-based file name. This enables routing through Serilog.Sinks.Map to place each event into a file named for the day, bypassing Serilog.Sinks.File's built-in yyyyMMdd rolling pattern.

## Remarks

Stateless enrichment: LogDateEnricher has no internal state and is safe to reuse across threads. It centralizes the date-formatting concern so any sink or routing that consumes the LogDate property can operate independently of how the date was computed. Using LocalDateTime ensures the date reflects the log event's local time; if you require a different time zone, adjust the timestamp source or format accordingly. The property name "LogDate" is conventional for downstream routing, so consider potential collisions with other enrichers that might emit the same property.

## Notes

- Potential collision: If a log event already has a property named "LogDate", AddOrUpdateProperty will overwrite it. Ensure this name is not used by other enrichers or sinks in your pipeline.
- Performance: The operation is lightweight (one string formatting per event) and incurs negligible overhead in typical logging rates.
- Time zone behavior: The enricher uses LocalDateTime from the event timestamp; for a different temporal interpretation, adjust the source of the date or the format as needed.

---

## LogDateEnricherExtensions
> **File:** `src/api/Gabriel.API/Configuration/LogDateEnricher.cs`  
> **Kind:** class

```csharp
public static class LogDateEnricherExtensions
```


Provides a strongly-typed extension method, WithLogDate, on Serilog's LoggerEnrichmentConfiguration that wires the LogDateEnricher into the logging pipeline. This allows you to enable the log-date enrichment by name (e.g., in appsettings Serilog.Enrich) while preserving fluent chaining by returning a LoggerConfiguration.

## Remarks

This method acts as a small façade that binds the concrete enricher type (LogDateEnricher) to Serilog's enrichment pipeline under a friendly name. By using WithLogDate rather than referencing LogDateEnricher directly, you gain a stable public API surface that can survive internal refactors. It relies on Serilog's generic Enrichment `With<T>` mechanism to instantiate the enrichment when configured, keeping configuration concerns decoupled from implementation details.

## Notes

- This is a thin wrapper around Serilog's Enrichment `With<T>` and does not modify enrichment behavior by itself; it simply exposes a named surface for the LogDateEnricher.
- If you configure enrichments via appsettings, reference the name "WithLogDate" (as surfaced by this extension) in Serilog.Enrich to enable the date-enrichment without code changes.


---