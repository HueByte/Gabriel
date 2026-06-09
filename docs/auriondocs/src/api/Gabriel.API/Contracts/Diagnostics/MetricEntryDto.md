# MetricEntryDto.cs

> **Source:** `src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs`

## Contents

- [MetricEntriesResponse](#metricentriesresponse)
- [MetricEntryDto](#metricentrydto)

---

## MetricEntriesResponse

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs`  
> **Kind:** record

```csharp
// Wire shape of GET /diagnostics/metrics. Returns up to `limit` recent rows
// matching the requested system (exact match) or system prefix, newest first.
public sealed record MetricEntriesResponse(
    IReadOnlyList<MetricEntryDto> Entries,
    int Count)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `match` | `exact` | — |


Represents the wire response for GET /diagnostics/metrics: a list of metric rows and an associated integer count. Use this record when sending or receiving the diagnostics metrics payload from the API; Entries holds the returned MetricEntryDto items and Count is the server-provided numeric metadata.

## Remarks
This is a simple DTO that separates the payload (Entries) from summary metadata (Count). The API returns up to the requested `limit` most recent rows (newest first), so this shape makes it easy for callers to consume the returned slice of data while also observing the overall count the server reports.

## Example
```csharp
// Constructing a response with two metric entries and a reported count of 42
var entries = new List<MetricEntryDto>
{
    new MetricEntryDto(/* ... */),
    new MetricEntryDto(/* ... */)
};
var response = new MetricEntriesResponse(entries, 42);
```

## Notes
- Entries is an IReadOnlyList; do not rely on mutating it locally to affect server state.
- The returned Entries are ordered newest-first. Count may not equal Entries.Count when the server applied a limit to the results (Count reflects the server-reported number, not necessarily the number of items in this payload).

---

## MetricEntryDto

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs`  
> **Kind:** record

Represents a single row in the generic metric event log exposed by the API. Use this DTO when sending or receiving metric events across process or service boundaries where the metric payload itself is treated as opaque JSON and the receiving side will re-deserialize it according to the originating subsystem's schema.

## Remarks
This record intentionally carries the metric payload as a System.Text.Json.JsonElement so the transport remains schema-flexible: callers can inspect or deserialize the raw JSON into whatever concrete type they expect without the API needing to know those types. The record holds basic metadata (Id, System, CreatedAt) alongside the JSON payload to identify the source and time of the metric.

## Example
```csharp
using System;
using System.Text.Json;

// Example strongly-typed metric shape used by a subsystem
public record MyMetric(int Count, string Unit);

// Constructing a MetricEntryDto from a JSON string
var json = "{ \"Count\": 5, \"Unit\": \"items\" }";
var metricElement = JsonSerializer.Deserialize<JsonElement>(json);
var entry = new MetricEntryDto(Guid.NewGuid(), "orders", metricElement, DateTimeOffset.UtcNow);

// Re-deserializing the opaque JsonElement into a typed object
var typed = entry.Metric.Deserialize<MyMetric>();
Console.WriteLine($"Count = {typed.Count}, Unit = {typed.Unit}");

// Or getting the raw JSON text
var raw = entry.Metric.GetRawText();
Console.WriteLine(raw);
```

## Notes
- JsonElement can be backed by a JsonDocument; avoid creating a JsonElement from the RootElement of a short-lived/disposed JsonDocument because the element may reference released memory. Prefer JsonSerializer.`Deserialize<JsonElement>`(...) when creating standalone elements.
- Treat Metric as opaque JSON for transport; deserialize to a concrete type before attempting property access to avoid brittle runtime parsing code.
- The record is immutable; the JsonElement is a value type representing JSON data and should be considered read-only in this DTO.

---