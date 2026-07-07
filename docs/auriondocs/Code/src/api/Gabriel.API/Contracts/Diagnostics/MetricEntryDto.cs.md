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
public sealed record MetricEntriesResponse(
    IReadOnlyList<MetricEntryDto> Entries,
    int Count)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Entries` | `IReadOnlyList<MetricEntryDto>` | — |
| `Count` | `int` | — |


Represents the response payload for the diagnostics metrics endpoint (GET /diagnostics/metrics). It conveys a set of recent metric entries for the requested system (matched exactly or by prefix) and a Count of how many entries are included, with the newest entries first.

## Remarks
MetricEntriesResponse is a minimal, transport-focused contract used by the diagnostics metrics API. As a sealed, immutable record, it provides a stable data container that can be serialized across the wire without side effects. It couples Entries (a read-only list of MetricEntryDto) with Count, offering both the actual entries and a quick indicator of how many items were returned for the request.

---

## MetricEntryDto
> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/MetricEntryDto.cs`  
> **Kind:** record

```csharp
public sealed record MetricEntryDto(
    Guid Id,
    string System,
    JsonElement Metric,
    DateTimeOffset CreatedAt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `Guid` | — |
| `System` | `string` | — |
| `Metric` | `JsonElement` | — |
| `CreatedAt` | `DateTimeOffset` | — |


MetricEntryDto represents a single row in the generic metric event log. As a sealed record, it provides a lightweight, immutable carrier for the identifying Id, the originating System, the raw Metric payload, and the CreatedAt timestamp. The Metric payload is stored as a JsonElement to keep the metric shape opaque and schema-agnostic, enabling consumers to re-deserialize into their own domain types or defer schema decisions to their caller.

## Remarks
This abstraction decouples the transport contract from the metric data shape, allowing diverse subsystems to emit metrics without forcing a shared schema. The record semantics offer value-based equality and convenient deconstruction, which simplifies mapping and projection of metric rows in consumer code.

## Example
```csharp
// Rehydrate the opaque metric payload into a known type when needed
MyMetricDto? payload = JsonSerializer.Deserialize<MyMetricDto>(entry.Metric.GetRawText());
```

## Notes
- JsonElement is a struct that references an underlying JsonDocument; preserve the underlying document (or copy the payload via GetRawText) if you plan to access the JSON beyond the immediate call.
- Deserialization requires a concrete target type that matches the payload shape; when in doubt, treat Metric as opaque and operate on GetRawText()/JsonElement APIs, or deserialize into a domain-specific DTO when a schema is known.

---