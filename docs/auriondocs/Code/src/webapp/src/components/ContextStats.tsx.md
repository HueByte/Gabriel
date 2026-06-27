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

Props for the ContextStats component. Provide the conversationId for which context statistics should be displayed; optionally pass a numeric refreshKey that the parent increments to force the component to re-fetch or update its displayed metrics after a chat turn or other external change.

## Remarks
This interface keeps the component API minimal: conversationId identifies the conversation to inspect, while refreshKey serves only as a change signal (not as data to be displayed). Use refreshKey when the parent knows the underlying context changed and needs to force the child to refresh its data.

## Example
```typescript
// Parent component that increments a counter after each chat turn
function ChatContainer() {
  const [turn, setTurn] = useState(0);
  const conversationId = "conv-123";

  function onUserSend() {
    // send message...
    // then bump the refreshKey so ContextStats refetches
    setTurn(t => t + 1);
  }

  return (
    <>
      <ChatWindow conversationId={conversationId} onSend={onUserSend} />
      <ContextStats conversationId={conversationId} refreshKey={turn} />
    </>
  );
}
```

## Notes
- The numeric value of refreshKey is irrelevant except that it changes; incrementing an integer is the common pattern.
- If refreshKey is omitted, the component will not receive an explicit external refresh signal and may only update based on its own internal events or conversationId changes.
- Avoid passing a new value each render unless you want to force a refetch on every render (this will cause extra network/load work).

---

## Category

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** type

Represents a category wrapper object whose sole property, `key`, is restricted to one of six literal identifiers: 'system', 'project', 'memory', 'summary', 'tools', or 'conversation'. Use this when a component or function expects a category described as an object with a named `key` field instead of a bare string.

## Remarks
Using an object with a `key` property (rather than a plain string union type) makes the shape easy to extend later with additional metadata (labels, icons, counts) and serves as a simple discriminant when writing switches or type guards.

## Example
```typescript
const c1: Category = { key: 'system' };
function handleCategory(cat: Category) {
  switch (cat.key) {
    case 'system':
      // handle system
      break;
    case 'project':
      // handle project
      break;
    // ...other cases
  }
}
```

## Notes
- Category is not a plain string; checks must inspect the `key` property (e.g. `cat.key === 'memory'`).
- The allowed values are exact literals (lowercase); adding new categories requires updating the type definition and any switch/case handling.


---

## ContextStats

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

```typescript
export function ContextStats(
```


Renders context-related statistics for a specific conversation. Use this component in a conversation view or header when you need a compact display of per-conversation context metrics; the parent can force an update by changing the refreshKey prop.

## Remarks
This component accepts a conversation identifier and an optional numeric refreshKey (default 0). The refreshKey exists so a parent can trigger the component to refresh its displayed information without remounting it — update the numeric value (for example by incrementing) to request a refresh.

## Example
```typescript
// Basic usage in a conversation view
<ContextStats conversationId={conversation.id} />

// Force an update from a parent by changing refreshKey
const [refreshKey, setRefreshKey] = useState(0);
return (
  <>
    <button onClick={() => setRefreshKey(k => k + 1)}>Refresh stats</button>
    <ContextStats conversationId={conversation.id} refreshKey={refreshKey} />
  </>
);
```

## Notes
- If conversationId is absent or falsy the component may not display meaningful data; provide a valid identifier.
- refreshKey is a simple numeric token intended to signal "please refresh"; use a stable incrementing value rather than a new object each render.
- The exact update/fetch behavior is implemented inside the component; consumers should rely on refreshKey for explicit refresh requests rather than internal implementation details.

---

## buildGridCells

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

Creates a fixed-length array of GRID_CELLS entries that map contiguous grid positions to category keys (or null) based on the token counts in the provided ContextMetricsResponse. Use this when preparing a compact, visual representation of how different context categories occupy the model's token window (e.g., a small bar or grid where each cell represents a slice of the context window).

## Remarks
The function computes the effective context window (at least 1 token) and determines how many tokens each grid cell represents. It iterates CATEGORIES in order and assigns each category a contiguous block of cells sized proportionally to its token count, but always at least one cell for any category with >0 tokens. Because counts are rounded up, earlier categories can consume more cells and the loop stops once GRID_CELLS are filled; any remaining unassigned cells remain null (the "free" bucket).

## Example
```typescript
// Given some ContextMetricsResponse `metrics` (from the runtime) call:
const cells = buildGridCells(metrics);
// `cells` is an array of length GRID_CELLS where each entry is either a category key
// (Category['key']) or null for unassigned/free cells. You can then render these
// as a compact visualization (e.g. a horizontal grid where each cell is colored
// by its category key).
console.log(cells.length); // === GRID_CELLS
console.log(cells); // e.g. [ 'system', 'system', 'prompt', null, null, ... ]
```

## Notes
- The allocation is order-dependent: CATEGORIES earlier in the list have priority and may consume cells before later categories are considered.
- Non-zero token categories always receive at least one cell due to the Math.max(1, Math.ceil(...)) behavior; this can cause slight over-allocation which is absorbed by filling up to GRID_CELLS and leaving trailing nulls if any remain.
- The function protects against a zero context window by using Math.max(1, metrics.contextWindowTokens), so tokensPerCell is never Infinity or division-by-zero.

---

## formatTokens

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

Return a compact, human-friendly string for a token count: numbers below 1,000 are returned as-is, while values >= 1,000 are abbreviated with a "k" suffix. Use this when displaying token or item counts in a UI where a short, readable format is preferred over the full numeric value.

## Remarks
This function intentionally shows one decimal place for values in the thousands (e.g. "12.4k") to preserve some precision for smaller thousands, but drops the decimal for larger values (>= 100k) using whole-number rounding (e.g. "187k"). It is a small, presentation-focused helper — it does not localize number formatting or handle negative-abbreviation semantics.

## Example
```typescript
console.log(formatTokens(999));     // "999"
console.log(formatTokens(1000));    // "1.0k"
console.log(formatTokens(12400));   // "12.4k"
console.log(formatTokens(187000));  // "187k"
```

## Notes
- toFixed(1) always emits one decimal, so values like 1000 become "1.0k" (not "1k").
- Rounding of the decimal form can push values to the next magnitude: e.g., 99_999 -> k = 99.999 -> toFixed(1) -> "100.0k".
- The function does not perform localization (decimal separator is ".") and does not abbreviate negative numbers (negative values < 1000 are returned as their full string).

---