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


MetricEntriesResponse is an immutable data carrier that models the payload returned by the GET /diagnostics/metrics endpoint. It exposes a read-only collection of MetricEntryDto items representing the most recent metric entries that match the requested system (either an exact match or a system prefix), up to the specified limit, with the newest entries first. The Count value indicates how many items are included in Entries for this response.

## Remarks
MetricEntriesResponse's sealed record design ensures value-based equality and predictable de/serialization across API boundaries. By separating the payload (Entries) from a tiny bit of metadata (Count), callers can iterate the entries while easily inspecting how many items were yielded in this page, which supports simple paging semantics and UI rendering.

## Notes
- Count equals Entries.Count in typical usage; if paging is applied, it may be less than the total matches.
- Entries should be treated as an immutable snapshot; do not mutate after creation.
- Ensure Entries is non-null to avoid null reference during enumeration.

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


Represents a single row in the generic metric event log. It carries the row's Id, the originating System, a raw JSON payload for the metric, and the timestamp when the row was Created. The Metric field is stored as an opaque JsonElement so callers can re-deserialize it according to the specific metric shape produced by the originating subsystem.

## Remarks
This abstraction provides a stable transport contract for metric entries across heterogeneous subsystems by decoupling callers from any single metric model. By preserving the Metric payload as JsonElement, consumers can adapt to evolving formats without changing this DTO. The sealed record and its value-based equality enforce immutability and reliable comparisons across diagnostic boundaries. CreatedAt supports ordering, auditing, and correlation of events.

## Notes
- JsonElement is a non-owning view into JSON; ensure the underlying JsonDocument remains alive for the lifetime of the MetricEntryDto, otherwise reading Metric may throw or yield invalid data.

---