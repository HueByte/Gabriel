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

Represents the props accepted by the ContextStats component: the required conversation identifier and an optional numeric refresh key that can be bumped to force the component to re-fetch or re-evaluate context-related statistics.

## Remarks
This interface is a small prop contract used to drive context-related UI updates. conversationId uniquely identifies which conversation's context is being inspected; refreshKey is an optional incrementing value used solely as a signal to trigger a refetch or recomputation (for example, after a chat turn completes).

## Example
```typescript
// Parent component that increments refreshKey after each message send
function ChatParent() {
  const [refreshKey, setRefreshKey] = React.useState(0);
  const conversationId = 'conv_123';

  function onSendMessage() {
    // send message logic...
    // bump refreshKey so ContextStats will re-fetch/update
    setRefreshKey(k => k + 1);
  }

  return (
    <>
      <ChatConversation id={conversationId} onSend={onSendMessage} />
      <ContextStats conversationId={conversationId} refreshKey={refreshKey} />
    </>
  );
}
```

## Notes
- refreshKey is optional; when provided it should change (e.g., increment) to signal an update. Reusing the same value will not trigger a refetch.
- conversationId is required and must uniquely identify the conversation whose context statistics are being displayed.

---

## Category

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** type

Represents a category object whose required `key` property must be one of the specific literal identifiers: 'system', 'project', 'memory', 'summary', 'tools', or 'conversation'. Use this type when you need a typed object that conveys one of the predefined category keys (rather than an unconstrained string).

## Remarks
This type constrains category identifiers to a small, explicit set of values so TypeScript can provide exhaustiveness checks, autocompletion, and safer branching logic in UI components and application logic. Modeling the category as an object with a `key` property makes it easy to extend later with additional metadata while keeping the allowed keys strictly typed.

## Example
```typescript
const c: Category = { key: 'project' };

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
- The type is an object with a `key` property, not a bare string union; use `obj.key` when you need the literal value.
- Adding or removing allowed keys requires updating this type so all usages remain type-safe.

---

## ContextStats

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

Renders contextual statistics for a single conversation and exposes a small refresh hook via props. Use this exported React functional component when you want a reusable UI piece that displays metrics or other status information tied to a specific conversation ID; the optional refreshKey lets a caller signal that the component should update its displayed data.

## Remarks
This is a lightweight presentation/data-fetch component: the conversationId prop selects which conversation to show stats for and refreshKey (default 0) is provided so callers can force an update cycle without changing the conversationId. It is intended to be used wherever conversation-level statistics are needed in the UI and is exported for reuse across pages or other components.

## Example
```typescript
import React from 'react'
import { ContextStats } from './components/ContextStats'

function ConversationHeader() {
  const conversationId = 'conv_123'
  const [tick, setTick] = React.useState(0)

  // increment tick to force ContextStats to refresh
  const refresh = () => setTick(t => t + 1)

  return (
    <div>
      <h2>Conversation</h2>
      <button onClick={refresh}>Refresh stats</button>
      <ContextStats conversationId={conversationId} refreshKey={tick} />
    </div>
  )
}
```

## Notes
- conversationId is required to identify which conversation's stats to show.
- refreshKey defaults to 0; callers can change it to prompt the component to refresh its data. The exact refresh semantics depend on the component implementation.

---

## buildGridCells

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

Builds a fixed-size array of GRID_CELLS entries that maps each grid position to a category key (or null) based on the proportional token counts from the provided ContextMetricsResponse. Use this when rendering a compact visual representation of how the context window is distributed across the known categories; the output is suitable for iterating when painting a grid or bar made of discrete cells.

## Remarks
This function converts continuous token counts into discrete cell allocations while guarding against tiny categories becoming invisible: any category with a positive token count is given at least one cell (via a ceiling allocation). Allocation proceeds in the order of the global CATEGORIES array and fills cells left-to-right until GRID_CELLS are exhausted; remaining cells (if any) stay null and represent unused/free capacity. The calculation uses Math.max(1, metrics.contextWindowTokens) so a zero or negative context window is treated as 1 to avoid divide-by-zero.

## Example
```typescript
// Assume GRID_CELLS = 10 and CATEGORIES = [ { key: 'system', tokens: m => m.system }, { key: 'prompt', tokens: m => m.prompt }, { key: 'response', tokens: m => m.response } ]
const metrics = { contextWindowTokens: 1000, system: 200, prompt: 300, response: 400 } as ContextMetricsResponse;
// tokensPerCell = 1000 / 10 = 100
// allocations: system -> ceil(200/100)=2 cells, prompt -> 3 cells, response -> 4 cells => total 9 cells; last cell remains null
const cells = buildGridCells(metrics);
// Example result: ['system','system','prompt','prompt','prompt','response','response','response','response', null]
```

## Notes
- The order of CATEGORIES determines which categories occupy earlier cells; reorder CATEGORIES to change visual placement.
- Categories with tokens <= 0 are skipped and do not consume cells.
- Because allocations are rounded up, the sum of allocated cells can slightly exceed the exact proportional expectation; the remaining null cells act as a trailing "free" bucket.
- The function depends on external constants (GRID_CELLS, CATEGORIES) and the Category shape (Category['key'] and tokens accessor); ensure those globals match expected types/semantics.

---

## formatTokens

> **File:** `src/webapp/src/components/ContextStats.tsx`  
> **Kind:** function

Formats a numeric token/count value into a compact, human-readable string. Values below 1000 are returned as-is (e.g. "999"); values of 1000 or more are shown with a "k" suffix. For thousands it shows one decimal place up to 99.9k (e.g. "12.4k") and switches to whole-number thousands for 100k and above (e.g. "187k").

## Remarks
This helper is intended for UI displays where space is limited and full numeric precision isn't required (for example, showing token counts or result counts in a status bar). The chosen thresholds prioritize readability: a single decimal gives useful granularity for smaller thousands while larger values are rounded to keep the string short.

## Example
```typescript
console.log(formatTokens(999));     // "999"
console.log(formatTokens(12400));   // "12.4k"
console.log(formatTokens(187000));  // "187k"
```

## Notes
- The function always uses the dot (.) as the decimal separator via toFixed(1); it does not localize formatting.
- Negative numbers are not specially handled (e.g. -1500 -> "-1.5k").
- Rounding behavior near the 100k cutoff follows JavaScript rounding: values where k >= 100 use Math.round(k), which may change display for numbers close to that boundary.

---