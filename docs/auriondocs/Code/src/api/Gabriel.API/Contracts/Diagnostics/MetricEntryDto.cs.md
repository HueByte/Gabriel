# MetricEntryDto.cs

> **Source:** `src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs`

## Contents

- [MetricEntriesResponse](#metricentriesresponse)
- [MetricEntryDto](#metricentrydto)

---

## MetricEntriesResponse

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs`  
> **Kind:** record

Represents the wire response for the GET /diagnostics/metrics endpoint. Contains the returned metric rows (Entries) and an integer Count associated with the response. Entries contains up to the requested limit of most-recent rows that match the requested system (exact match or prefix), ordered newest first.

## Remarks
This record is a compact DTO used at the API boundary to return metric data. It groups the set of MetricEntryDto objects together with a numeric count so clients can consume both the payload and a simple numeric summary in a single response.

## Example
```csharp
// Server-side: build and return up to `limit` most-recent metric entries
var entries = new List<MetricEntryDto>
{
    // ... populate MetricEntryDto instances ...
};
var response = new MetricEntriesResponse(entries, entries.Count);
return Ok(response);
```

## Notes
- Entries are ordered newest-first and will contain at most the configured `limit` items; the list may be shorter when fewer matching rows exist.
- The semantic meaning of Count is not fully specified in source comments — callers should confirm whether it represents the number of entries returned (Entries.Count) or the total number of matching rows across all pages for this query.

---

## MetricEntryDto

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs`  
> **Kind:** record

A simple, immutable data transfer object that represents one row in the generic metric event log. Use this record when transporting or persisting a single metric event emitted by a subsystem; the Metric property carries the metric payload as raw JSON so the originating subsystem can define its own schema.

## Remarks
This type exists to provide a stable wire shape for metric events across different subsystems. It treats the metric payload as an opaque JSON value (JsonElement) rather than a typed object so producers and consumers can agree on their own JSON schemas without introducing a shared CLR type. Equality and immutability come from being a C# record, making it suitable for use in logging, persistence, and message passing scenarios.

## Example
```csharp
using System.Text.Json;

// Constructing a MetricEntryDto from a JSON string
var json = "{ \"count\": 42, \"unit\": \"requests\" }";
using var doc = JsonDocument.Parse(json);
var entry = new MetricEntryDto(
    Id: Guid.NewGuid(),
    System: "orders-service",
    Metric: doc.RootElement,
    CreatedAt: DateTimeOffset.UtcNow);

// Consuming the metric payload (read as JsonElement)
if (entry.Metric.TryGetProperty("count", out var countProp))
{
    int count = countProp.GetInt32();
    // ...
}
```

## Notes
- JsonElement can be tied to the lifetime of a JsonDocument when obtained via JsonDocument.Parse; do not dispose the JsonDocument while the JsonElement is still in use. If you need a long-lived, independent representation, consider storing the raw JSON string alongside or serializing the JsonElement immediately.
- The Metric property is intentionally opaque — consumers must know the expected JSON schema for a given producer/System value before attempting to deserialize into a typed object.
- The record is immutable and its equality semantics include all properties (Id, System, Metric, CreatedAt).

---