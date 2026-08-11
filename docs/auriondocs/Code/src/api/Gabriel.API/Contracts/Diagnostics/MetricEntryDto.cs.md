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


Represents the response payload for the diagnostics metrics endpoint. It carries two values: Entries, a read-only list of MetricEntryDto objects containing the most recent metric entries that match the requested system (exact match or system prefix), ordered newest first; and Count, the number of entries included in this response.

## Remarks
By expressing the response as a dedicated record, this symbol decouples transport concerns from the domain model and provides a stable wire format for clients and tests. The paired Entries and Count support paging: clients can render a page of results while knowing how many items were actually returned. The structure is future-proof for evolving the payload without touching the endpoint contract, since additional metadata can be added to this record later without breaking existing consumers.

## Notes
- The exact meaning of Count (whether it's the number of returned entries or the total matches) is not explicit in the snippet; consult the API contract to confirm semantics.

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


MetricEntryDto is a lightweight, immutable data transfer object that represents one row in the generic metric event log. The Metric field holds the raw JSON payload as a JsonElement, preserving the original shape and letting callers deserialize it later according to the schema agreed by the originating subsystem. The Id uniquely identifies the entry, System denotes the source subsystem, and CreatedAt records when the entry was created.

## Remarks
MetricEntryDto exists to boundary-cross the API surface from varying metric schemas. By using a sealed record, it gains immutable, value-based equality semantics suitable for transport across boundaries. Storing the payload as JsonElement provides flexibility for callers to interpret the metric on their own terms while avoiding premature deserialization. If you need a typed view, deserialize the JsonElement at the call site.

## Notes
- Treat the Metric payload as opaque; do not mutate it, and deserialize only when you know the target shape (e.g., via GetRawText() + JsonSerializer).
- The property named System may collide with common namespaces in consuming code; prefer explicit references to the DTO when accessing it.

---