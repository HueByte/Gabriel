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


ContextStatsProps defines the shape of the props expected by the ContextStats UI component. It requires a conversationId string that identifies the current conversation and optionally accepts a refreshKey number that, when provided and changed, triggers a refetch or re-render to reflect updated context size.

## Remarks
ContextStatsProps isolates identity data (conversationId) from refresh semantics (refreshKey), enabling callers to signal updates without mutating the identity. It is intended for use by a UI component that renders context statistics for a chat conversation and should be supplied by a parent component that manages the conversation state and its refresh lifecycle.

## Example
```typescript
// Example usage: constructing props for a ContextStats component
const propsNoRefresh: ContextStatsProps = {
  conversationId: "conv-1234"
};

const propsWithRefresh: ContextStatsProps = {
  conversationId: "conv-1234",
  refreshKey: 1
};
```

## Notes
- The refreshKey is optional; omit it if you do not need the refresh signaling.
- Treat refreshKey as a signal to trigger a refresh; changing it should produce a new value to activate the update.
- This interface is a small descriptor of props and does not perform data fetching itself; it simply communicates identity and refresh semantics to the consuming UI.

---

## Category
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** type alias

```typescript
type Category = {
  key: 'system' | 'project' | 'memory' | 'summary' | 'tools' | 'conversation';
  label: string;
  color: string;
  tokens: (m: ContextMetricsResponse) => number;
};
```


Category is a TypeScript type that encapsulates a UI-facing category for contextual metrics used by ContextStats.tsx. Each category carries a fixed key (one of system, project, memory, summary, tools, conversation), a human-friendly label, a color string, and a tokens function that, given a ContextMetricsResponse, returns the numeric value that category contributes to the overall metrics. This abstraction enables the UI to render categories consistently—mapping semantic meaning (the key) to presentation (label and color) and to the actual metric extraction logic (tokens).

## Remarks
This type centralizes the metadata and extraction logic for metric categories, facilitating consistent rendering and easy extension. It helps decouple the presentation from the data extraction strategy, allowing new categories to be added without scattering UI logic across the codebase. The tokens function provides a pluggable hook for category-specific metric calculations.

## Notes
- Be mindful that tokens relies on the shape of ContextMetricsResponse; changes to ContextMetricsResponse may require updating the corresponding tokens implementations.
- If you introduce a new category key, ensure the label, color, and tokens mapping stay in sync across the UI and any enumeration that consumes the keys.

---

## ContextStats
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

```typescript
export function ContextStats(
```


ContextStats is a React function component that renders statistics related to the contextual aspects of a specific conversation. It accepts a conversationId and an optional refreshKey (defaulting to 0); changing refreshKey prompts a refresh of the statistics without remounting the component.

## Remarks

By encapsulating context statistics into its own component, this symbol isolates data-fetching and presentation concerns from surrounding UI, making it reusable and easier to test. The refreshKey prop provides a lightweight mechanism for parent components to trigger a refresh in response to external events without reconstructing the component tree.

## Notes

- The actual implementation may fetch data from an API; ensure you handle loading and error states in the UI that hosts this component.
- If refreshKey changes frequently, consider memoization or batching to avoid unnecessary re-fetches.
- The snippet shows only the two props visible in the signature; the real component may accept additional props or rely on context.

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


The function builds a fixed-size grid that represents how the tokens in a contextual window are distributed across predefined categories. It takes a ContextMetricsResponse, derives a token window, and divides that window into GRID_CELLS equal parts, then assigns each non-empty category a consecutive block of cells proportional to the category’s token count. Each category with tokens > 0 gets at least one cell, ensuring visibility in the grid; the resulting array is length GRID_CELLS and contains either a category key or null for unassigned cells. This abstraction is useful for rendering compact visualizations (such as a grid heatmap) of how much context space each category consumes, independent of the raw metrics calculation.

## Remarks
This abstraction translates raw contextual metrics into a deterministic, UI-friendly grid representation. It uses a simple linear scaling (tokens per cell) with ceil rounding to guarantee visibility for any non-zero token count, accepting modest over-allocation in exchange for a stable layout. The trailing null entries effectively form a neutral “free” bucket for any rounding slack, and categories with zero tokens contribute no cells.

## Notes
- The allocation is order-sensitive: categories are filled in the sequence defined by CATEGORIES, so changing that order changes the resulting grid.
- If GRID_CELLS is small or one category consumes most of the window, later categories may not appear in the grid, since processing stops once the grid is full.


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


Converts a numeric token count into a compact, human-friendly string that uses a trailing 'k' to denote thousands. For values below 1000 it returns the plain integer as a string; for 1000 and above it expresses the value in thousands by dividing by 1000. If the resulting thousands value is 100 or more, it is rounded to a whole number and suffixed with 'k' (e.g., 125000 -> "125k"). Otherwise, it is shown with one decimal place (e.g., 12400 -> "12.4k"). The boundary around 100k yields a subtle nuance: 99,999 would format as "100.0k" due to the single-decimal rule before the integer-branch.

## Remarks

Centralizes token-count formatting so user interfaces display counts consistently without duplicating logic across components. It encapsulates the decision of when to show a decimal versus a whole-number thousands value, making future tweaks to formatting localized to this function.

## Example

```typescript
formatTokens(950);    // "950"
formatTokens(1500);   // "1.5k"
formatTokens(125000); // "125k"
formatTokens(99999);  // "100.0k" (edge-case boundary demonstrates the decimal-before-integer rule)
```

## Notes

- Be aware of the 100k boundary behavior: values just below 100000 may still render with a decimal if the thousands portion is less than 100, which can look inconsistent if you expect strict 100k+ formatting. Consider post-processing if strict bounds are required for a given UI.


---