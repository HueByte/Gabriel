# ContextStats.tsx

> **Source:** `src/webapp/src/components/ContextStats.tsx`

## Contents

- [ContextStatsProps](#contextstatsprops)
- [Category](#category)
- [ContextStats](#contextstats)
- [buildGridCells](#buildgridcells)
- [formatTokens](#formattokens)

---

## ContextStatsProps
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** interface

```typescript
interface ContextStatsProps
```


ContextStatsProps defines the shape of the props expected by the ContextStats UI component. It requires conversationId to identify the conversation whose statistics are displayed and exposes an optional refreshKey that, when incremented, triggers a refresh of the context statistics to reflect the latest state.

## Remarks
ContextStatsProps is a small, focused data contract that separates rendering concerns from data refresh logic. Using the refreshKey avoids introducing a dedicated refresh callback in the component, instead leveraging React's dependency tracking to re-run data fetches whenever the key changes. This makes it easier to synchronize the UI with chat events without mutating internal component state.

## Example
```typescript
<ContextStats conversationId="conv-123" refreshKey={refreshCounter} />
```

## Notes
- refreshKey is optional; omit it when you don't need an explicit refresh trigger.
- Changing refreshKey to a new value will trigger a refresh; repeated values do not.
- conversationId should identify the exact conversation; passing a different ID will reflect a different dataset.


---

## Category
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** type alias

```typescript
type Category =
```


Category is a type describing a small object that carries a single restricted field named key. The key is a union of six string literals: 'system', 'project', 'memory', 'summary', 'tools', and 'conversation'. Use this type when tagging items in the UI context (as in ContextStats) to ensure only one of the predefined categories is assigned, catching typos at compile time instead of at runtime.

## Remarks
By modeling the category as a literal-union on a property, code that consumes Category can perform exhaustive checks by switch on value, and editors get better autocomplete. It serves as a lightweight abstraction that groups related contextual items under a finite set of labels. If new categories are needed, they must be added to the union and all dependent logic should be updated accordingly.

## Notes
- This type defines only the key property; if your data shape includes additional fields for Category-tagged items, they must be added to the type or wrapped with an interface that extends Category.

---

## ContextStats
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

```typescript
export function ContextStats(
```


ContextStats is a UI helper that renders contextual statistics for a given conversation. It takes a conversationId to determine which conversation's metrics to display and an optional refreshKey that can be used to re-fetch or recompute the statistics when the surrounding context changes (for example, when the user performs an action that would alter the data being shown). Use this component when you need to surface a compact, contextual snapshot of a conversation's state—such as message counts, last activity, participant activity, or other derived metrics—without embedding the statistical logic directly in the page or parent component. This abstraction centralizes the logic for computing and presenting context-specific metrics, making it easier to maintain consistency across the UI and to reuse the same statistics rendering in multiple places where a conversation context is shown. If you need the exact fields shown or the data fetching strategy, refer to the implementation details within ContextStats in the file src/webapp/src/components/ContextStats.tsx.

---

## buildGridCells
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

```typescript
function buildGridCells(metrics: ContextMetricsResponse): (Category['key'] | null)[]
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `metrics` | [`ContextMetricsResponse`](../../../api/Gabriel.API/Contracts/Conversations/ContextMetricsResponse.cs.md) | — |


Computes a fixed-length grid representation of category token distribution from a ContextMetricsResponse. It splits the context window into GRID_CELLS segments and allocates consecutive cells to each category based on its token count, ensuring every non-empty category gets at least one cell. The result is an array of GRID_CELLS elements where each entry is either the corresponding Category['key'] or null, suitable for compact visual summaries in the UI; if capacity runs out, later categories are truncated.

## Remarks
This abstraction centralizes the logic for mapping token-based category weights into a fixed-size grid, decoupling layout from metric computation and enabling consistent rendering across the UI. It relies on the ordering of CATEGORIES and their tokens(...) calculations to determine allocation, and uses a trailing "free" bucket behavior when the grid cannot accommodate all categories.

## Notes
- If all categories report zero tokens, the returned grid consists entirely of nulls.
- Non-zero categories are guaranteed at least one cell, which can cause slight over-allocation due to rounding up.
- When GRID_CELLS is insufficient for all positive-token categories, later categories are not represented in the result.

---

## formatTokens
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

```typescript
function formatTokens(n: number): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `n` | `number` | — |

**Returns:** `string`


Formats a non-negative numeric token count into a concise, human-readable string using a trailing 'k' to denote thousands. For values below 1000 it returns the plain number as a string. For 1000 and above it converts to thousands, showing one decimal place when the thousands value is less than 100 (e.g., 12400 -> '12.4k'), and dropping the decimal for 100000 and above (e.g., 123000 -> '123k'). This behavior is implemented by dividing by 1000 to get k, then conditionally rounding or fixing one decimal depending on k. The helper is intended for UI scenarios (such as ContextStats) where large counts should be compact without obscuring the magnitude.

---