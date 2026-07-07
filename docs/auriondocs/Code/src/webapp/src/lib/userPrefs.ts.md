# userPrefs.ts

> **Source:** `src/webapp/src/lib/userPrefs.ts`

## Contents

- [readBool](#readbool)
- [readLegacyHideReactDetails](#readlegacyhidereactdetails)
- [useBoolPref](#useboolpref)
- [useHideThinking](#usehidethinking)
- [useHideToolCalls](#usehidetoolcalls)
- [useHideToolResults](#usehidetoolresults)
- [writeBool](#writebool)

---

## readBool
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
function readBool(key: string, fallback: boolean): boolean
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `key` | `string` | — |
| `fallback` | `boolean` | — |

**Returns:** `boolean`


Reads a boolean value stored under a given key from localStorage and returns a fallback when the key is missing or the read fails. It treats the string '1' as true and any other non-null value as false; if an error occurs during access to localStorage, the provided fallback is returned.

## Remarks
Centralizes the common pattern of persisting boolean flags in localStorage while handling environments where localStorage is unavailable. By returning the fallback on missing keys or read errors, it prevents propagation of exceptions to call sites. The implementation uses a strict comparison to '1' to avoid ambiguous truthy values coming from user input or storage. This function is best used when you want a simple, robust read of a boolean preference with a safe default.

## Example
```typescript
// Example usage: read a feature flag from localStorage
const showNewDashboard = readBool('showNewDashboard', false);
```

## Notes
- The function never throws; errors are swallowed and replaced with the provided fallback.
- It only considers '1' as true; other values (e.g., '0', 'true', 'false') are treated as false.


---

## readLegacyHideReactDetails
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
function readLegacyHideReactDetails(): boolean
```

**Returns:** `boolean`


Reads a legacy user preference from localStorage to determine whether React details should be hidden. It reads the value stored under LEGACY_HIDE_REACT_DETAILS_KEY and returns true only if that value is exactly the string '1'; for any other value or if the item is absent, it returns false. The storage access is wrapped in a try/catch; if any error occurs (for example when localStorage is unavailable or access is blocked), the function gracefully returns false. This tiny helper centralizes the legacy preference read, so UI code can rely on a single source of truth for this flag.

## Remarks
Why this abstraction exists: it isolates the legacy storage key and its interpretation from the rest of the UI logic, allowing storage strategy or key naming to evolve without sprinkling localStorage calls everywhere. It also defines a safe, conservative default: if the flag can't be read, we opt to show React details rather than silently hiding them.

## Notes
- Returns true only when the stored value is exactly '1'; any other value yields false.
- The function is defensive: any storage access error will cause a false return rather than throwing.
- Relies on LEGACY_HIDE_REACT_DETAILS_KEY being defined elsewhere; if that symbol isn't present, this function cannot read a meaningful value.

---

## useBoolPref
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useBoolPref(key: string, fallback: boolean): [boolean, (next: boolean) => void]
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `key` | `string` | — |
| `fallback` | `boolean` | — |


useBoolPref is a React hook that exposes a boolean user preference stored under a string key. It initializes its value by reading the persisted boolean with readBool(key, fallback). The hook subscribes to two event channels (PREFS_CHANGED_EVENT and the browser storage event) to refresh its value whenever the underlying preference changes, ensuring all components reading the same key stay in sync across tabs. The returned pair [value, set] lets a component read the current preference and update it; the setter writes the new value with writeBool and then updates local state optimistically, so the UI responds immediately while the global change propagates through the event system.

## Remarks

This abstraction centralizes boolean preferences in a key-based store and provides reactivity across components. It decouples UI code from direct storage access and relies on a lightweight event-based invalidation mechanism to keep all consumers aligned. The optimistic update pattern (set immediately after write) gives a snappy UI while deferring final coherence to the shared event stream.

## Notes

- This hook relies on a browser-like environment (window, storage events) and may not work in SSR or non-browser runtimes.
- It uses an extra event PREFS_CHANGED_EVENT to refresh values; ensure all changes to the same key emit this event so all tabs stay synchronized.
- There is no explicit error handling around writeBool; exceptions would bubble up to the caller if write fails.

---

## useHideThinking
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideThinking(): [boolean, (next: boolean) => void]
```


useHideThinking is a React hook that exposes a persistent boolean preference for hiding thinking indicators as a tuple [hideThinking, setHideThinking]. It delegates to useBoolPref with the key HIDE_THINKING_KEY and an initial value derived from readLegacyHideReactDetails(), so components can read and update the user's preference without directly managing storage or migration logic.

## Remarks
By centralizing the preference behind this hook, the app ensures a consistent initial state when migrating from legacy behavior and a single, shared API for all consumers. It hides the implementation details of how the preference is stored, enabling future changes to storage or key naming without touching every consumer.

## Example
```typescript
const [hideThinking, setHideThinking] = useHideThinking();
// Use the flag to conditionally render thinking indicators
if (hideThinking) {
  // render without thinking indicators
} else {
  // render with thinking indicators
}

// Persist a user choice to hide thinking indicators
setHideThinking(true);
```

## Notes
- The initial value uses readLegacyHideReactDetails(), which may reflect legacy behavior; changes to that function may affect the initial state for new sessions.
- Toggling via setHideThinking updates the persistent preference through useBoolPref, so the value persists across reloads.

---

## useHideToolCalls
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideToolCalls(): [boolean, (next: boolean) => void]
```


Returns a persistent UI preference indicating whether tool call details should be hidden, paired with a setter to update that preference. It uses useBoolPref with the key HIDE_TOOL_CALLS_KEY and defaults to readLegacyHideReactDetails() for backward compatibility. Use this hook when you want to conditionally render or suppress tool-call information across the app without managing persistence yourself.

## Remarks
This hook centralizes the hide-tool-calls preference, shielding components from the storage key and legacy-default logic. By returning a stable pair, it makes it easy to toggle the visibility of tool-call details and have that choice persist across sessions. The default comes from readLegacyHideReactDetails(), which preserves existing user expectations during migrations and can be evolved without touching callers.

## Notes
- Be mindful that the default value is derived from readLegacyHideReactDetails(); in test or non-browser environments where persistence is unavailable, provide a mock or override as needed.

---

## useHideToolResults
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideToolResults(): [boolean, (next: boolean) => void]
```


useHideToolResults is a small, reusable hook that exposes a boolean flag indicating whether tool results should be hidden and a setter to update that flag. It derives its initial value from a boolean preference stored under HIDE_TOOL_RESULTS_KEY, falling back to a legacy default via readLegacyHideReactDetails(). Components use this hook to read and persist the user's preference for hiding tool results, avoiding manual reads or writes to the underlying preference store.

## Remarks
By encapsulating this preference in a dedicated hook, the UI remains declarative and easy to reuse across components. It delegates to the shared preference system (useBoolPref) to ensure consistent behavior with other user-facing toggles, and the legacy default path preserves existing user settings when the new preference key has no value.

## Example
```typescript
const [hideToolResults, setHideToolResults] = useHideToolResults();

// Example: toggle the preference in response to a UI action
setHideToolResults(!hideToolResults);
```

## Notes
- The setter in the returned tuple expects a boolean value (next: boolean) and is not a functional updater; you must compute the next value before calling it.
- readLegacyHideReactDetails provides the initial default when there is no stored value for HIDE_TOOL_RESULTS_KEY, aiding backward compatibility.
- All mutations feed through useBoolPref, so changes persist across sessions and are reflected consistently wherever the hook is used.

---

## writeBool
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
function writeBool(key: string, value: boolean): void
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `key` | `string` | — |
| `value` | `boolean` | — |

**Returns:** `void`


Writes a boolean preference to localStorage under the given key, persisting true as '1' and false as '0', and emits a PREFS_CHANGED_EVENT to notify listeners about the change. If localStorage is unavailable (privacy mode, quota restrictions), the write is skipped and the app continues to function with ephemeral settings.

## Remarks
Centralizes boolean-flag persistence and decouples UI logic from storage. The emitted event enables reactive updates without polling storage, and the empty catch keeps the UI responsive in environments where storage is disabled—at the cost of persistence in those cases.

## Notes
- localStorage may be unavailable in some contexts; the change is ephemeral when that happens.
- The function swallows errors; if persistence is essential, provide a fallback mechanism or additional instrumentation.

---