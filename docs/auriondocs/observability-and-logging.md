# Observability and Diagnostics

> Logging enrichment and error handling to surface operational visibility and reliability.

Logging enrichment and structured error handling are used here to make operational behavior observable and diagnosable. The pieces below show how a small, targeted Serilog enricher, a pipeline extension to register it, a centralized HTTP exception translator, and a telemetry decorator for outbound web searches work together to produce consistent logs, route them for storage, and capture per-provider metrics.

## LogDateEnricher.cs
Adds a local-date LogDate property to log events.

The LogDateEnricher adds a "LogDate" property to each Serilog LogEvent using the local timestamp formatted as MM-dd-yyyy, enabling sinks that route or partition logs by a human-friendly date. Use this when you want Serilog.Sinks.Map or similar routing to place events into files or buckets grouped by the event's local date. See the implementation details and usage notes in [LogDateEnricher.cs](Code/src/api/Gabriel.API/Configuration/LogDateEnricher.cs.md).

## LogDateEnricher.cs
Extends Serilog with the LogDate enricher registration for pipelines.

This extension provides the wiring to register the LogDate enricher into a Serilog pipeline, making it easy to compose the enricher as part of host or application logging setup. By centralizing the registration logic developers can consistently ensure the LogDate property is present on all events without repeating configuration across bootstrapping code. Refer to the registration helper and how it integrates with Serilog in [LogDateEnricher.cs](Code/src/api/Gabriel.API/Configuration/LogDateEnricher.cs.md).

## GlobalExceptionHandler.cs
Converts exceptions into standardized HTTP ProblemDetails and logs them.

The GlobalExceptionHandler is the single point for translating exceptions thrown during request processing into standardized HTTP ProblemDetails responses and for logging those failures. It ensures responses conform to a consistent diagnostic structure for clients while also emitting logs that capture exception context; those logs inherit the application's enrichers (such as LogDate) so traces and failures can be correlated and routed. Implementation and behavior are documented in [GlobalExceptionHandler.cs](Code/src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs.md).

## InstrumentedWebSearch.cs
Wraps web-search calls with telemetry to capture per-provider metrics.

InstrumentedWebSearch decorates IWebSearch implementations to record outcomes and telemetry for every outbound web search: success, empty results, errors, latency, and query metadata. This decorator emits metrics and diagnostic events that let operators monitor each provider's reliability and performance independently, complementing the application logs produced by the Serilog pipeline. See the telemetry capture and recorded dimensions in [InstrumentedWebSearch.cs](Code/src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs.md).

Together these pieces form a lightweight observability pattern: logs are enriched with a consistent LogDate property (and registered via an extension) so events can be routed and stored deterministically; the GlobalExceptionHandler centralizes error-to-response translation and ensures those errors are logged in a standard form; and InstrumentedWebSearch provides fine-grained telemetry for external dependencies. The result is correlated, routable logs plus per-provider metrics that make troubleshooting and operational monitoring straightforward.

---
*Synthesised by Aurion on 2026-06-09 03:22:42 UTC*
