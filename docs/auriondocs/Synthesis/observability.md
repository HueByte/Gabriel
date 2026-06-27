# Observability and telemetry

> Logging enrichment for timestamps and a lightweight metrics surface to record telemetry events. The topic covers log enrichment extensions and metric recording.

Logging enrichment for timestamps and a lightweight metrics surface are provided as small, focused primitives: a Serilog enricher that attaches a local date to every log event, plus a minimal interface and a JSON-serializing implementation for recording telemetry metrics to a repository. These pieces are intended to be low-overhead building blocks you can register and call from long-lived engine components to get consistent log date fields and simple metric emission.

## LogDateEnricher.cs
Adds a Local date to Serilog LogEvents.

This source implements the actual Serilog enricher that appends a "LogDate" property to each LogEvent. The property uses the local date formatted as MM-dd-yyyy so logs can be routed or written to files keyed by date. See the implementation for the exact formatting and the enrichment logic: [LogDateEnricher.cs](Code/src/api/Gabriel.API/Configuration/LogDateEnricher.cs.md).

## LogDateEnricher.cs
Registers the LogDateEnricher via Serilog enrichment extensions.

This part of the same file exposes an extension method to register the LogDate enricher with Serilog configuration. Use this extension when wiring up the logging pipeline so the enricher is applied consistently across the application without having to instantiate it manually: [LogDateEnricher.cs](Code/src/api/Gabriel.API/Configuration/LogDateEnricher.cs.md).

## IMetricRecorder.cs
Defines the interface for recording telemetry metrics.

This interface defines the lightweight abstraction engine subsystems should depend on to emit telemetry-style metric events. Consumers call into this interface to record named metrics; implementations are responsible for how those metrics are persisted or transmitted. Refer to the interface contract and intended usage here: [IMetricRecorder.cs](Code/src/api/Gabriel.Engine/Services/IMetricRecorder.cs.md).

## MetricRecorder.cs
Implements serialization of metrics to a repository for telemetry.

This implementation serializes metric objects to JSON and writes them to an IMetricRepository. It's intended for record-and-forget telemetry from long-lived or singleton components: callers emit metrics through the recorder and it handles converting and persisting them. See the concrete behavior and serialization details in the implementation: [MetricRecorder.cs](Code/src/api/Gabriel.Engine/Services/MetricRecorder.cs.md).

The pieces work together as a minimal observability surface: logging enrichment and metric recording are separate concerns but complementary. Application startup registers the LogDate enricher so every Serilog event contains a stable "LogDate" field for routing or file naming, while engine components depend on IMetricRecorder to emit telemetry; MetricRecorder performs JSON serialization and hands metrics off to an IMetricRepository for storage or further processing. Together they provide consistent log metadata plus a simple, testable path for emitting and persisting telemetry events.

---
*Synthesised by Aurion on 2026-06-08 22:34:58 UTC*
