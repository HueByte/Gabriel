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


ContextStatsProps defines the props for the ContextStats UI, carrying the conversation identity and an optional refresh trigger. The conversationId identifies which conversation's context to display, while refreshKey, when provided and changed, signals the caller's intent to re-fetch the context stats (for example, after a chat turn to reflect updated context size).

## Remarks

By separating the identity (conversationId) from the refresh mechanism (refreshKey), this interface lets a parent component control refresh behavior without altering the data-fetching logic inside the ContextStats component. The refreshKey acts as a simple numeric signal that can be placed in a React state or effect dependency to re-run the fetch when the value changes.

## Example

```typescript
// Example usage of the interface
const exampleProps: ContextStatsProps = {
  conversationId: "convo-42",
  refreshKey: 1
};
```

## Notes

- refreshKey is optional; omit it if no explicit refresh trigger is needed.
- Increment refreshKey to trigger a re-fetch; avoid frequent unnecessary increments.

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


Category defines the shape of a category descriptor used by the Context Stats UI to group and display tokenized metrics. Each Category associates a machine-friendly key with a human-readable label, a color for UI styling, and a tokens function that computes a numeric value from a ContextMetricsResponse. This type is consumed by ContextStats.tsx to render per-category metrics such as system, project, memory, summary, tools, and conversation.

## Remarks
By centralizing label and color in this single type, the UI can render consistent category chips and allow the tokens function to be varied per category without changing rendering code. The tokens function is the only hook into the actual data for the category, enabling custom weighting or filtering logic to be defined per category while keeping the categories themselves as plain data.

## Example
```typescript
const exampleCategory: Category = {
  key: 'system',
  label: 'System',
  color: '#1e90ff',
  tokens: (m: ContextMetricsResponse) => 0
};
```

## Notes
- The tokens function should be deterministic and fast; avoid heavy computation during rendering.
- Ensure that every category provides a stable color to maintain visual consistency across renders.


---

## ContextStats
> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

```typescript
export function ContextStats(
```


ContextStats renders contextual statistics for a single conversation, identified by conversationId. It encapsulates the UI for metrics about the conversation context and accepts an optional refreshKey to force a data refresh from its parent.

## Remarks
ContextStats provides a reusable, consistent UI fragment for showing per-conversation metrics, decoupling data retrieval and presentation from page composition. It acts as a presentation bridge between the parent component and the app's data layer, and the refreshKey prop offers a controlled way to trigger a refresh of the displayed statistics without remounting the component.

## Notes
- refreshKey should be updated deliberately to trigger a refresh; frequent or unnecessary changes can lead to extra data requests.
- If conversationId changes, ensure you pass a valid and stable identifier to avoid showing stats for the wrong context.


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


Translates a ContextMetricsResponse into a fixed-size grid representation by distributing the tokens attributed to each category across a bounded number of cells. This is useful when the UI needs a compact, deterministic snapshot of how the context window is apportioned among categories, regardless of how many categories exist or how large the window is.

Given a ContextMetricsResponse, buildGridCells returns a fixed-length array whose length is GRID_CELLS; each element is either a Category key or null. It computes tokensPerCell from the context window, allocates at least one cell per non-zero category by rounding up, and fills cells in CATEGORIES order until the grid is full. The result is a stable, display-friendly mapping suitable for rendering in a grid or heatmap in ContextStats.tsx.

## Remarks
This function exists to decouple raw metric data from presentation concerns by mapping a variable-length token distribution into a fixed-size grid, enabling consistent rendering and easy comparisons across sessions. It preserves category ordering to ensure identical inputs yield identical outputs, and the rule that non-zero categories receive at least one cell guarantees visibility even when the window is large relative to GRID_CELLS. The trailing, unfilled cells act as a "free" bucket that absorbs rounding artefacts when there is more grid capacity than allocated categories.

## Notes
- Null cells indicate unallocated space; the UI should handle nulls gracefully.
- Rounding up per-category cell counts can over-allocate relative to the exact token distribution; changing GRID_CELLS or the context window size will alter the distribution.


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


Formats a numeric token count into a compact, human-friendly string for display in the UI. When you need to present token counts in a readable form, this helper centralizes the formatting logic so the ContextStats component—and any other UI surfaces sharing the same pattern—remain consistent. It converts large numbers into thousands with a trailing 'k' suffix, showing a decimal place for smaller thousands and dropping the decimal once the thousands reach three digits. For values under 1000, it returns the exact number as a string.

## Remarks
This abstraction isolates UI-specific number formatting from business logic, reducing duplication and ensuring a unified presentation style for token counts across the interface. By handling the 1.0k style case and the 100k+ rounding in one place, it prevents divergent representations in different parts of the UI and makes future adjustments to the display rules straightforward.

## Example
```typescript
formatTokens(12);      // "12"
formatTokens(12400);   // "12.4k"
formatTokens(123000);  // "123k"
formatTokens(1234567); // "1235k"
```

## Notes
- Values below 1000 are rendered as plain integers with no suffix. 
- For n >= 1000, the function uses a decimal form for k < 100 (e.g., 12.4k) and switches to a rounded integer form for k >= 100 (e.g., 123k, 1235k). This can yield surprising results like 1000k for 1,000,000; this is by design to keep the suffix consistent.
- The function does not apply locale-specific thousand separators or localized decimal marks; it uses a fixed dot as the decimal separator.

---