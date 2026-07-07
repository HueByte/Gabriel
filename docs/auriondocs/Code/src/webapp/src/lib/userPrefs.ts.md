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


Reads a boolean value from localStorage by key and returns a safe boolean. If the key is missing or an error occurs during access, it returns the provided fallback. When a value is present, only the string '1' is treated as true; any other non-null value yields false. This keeps callers from dealing with storage errors and type coercion, providing a predictable boolean result for feature flags or user preferences backed by localStorage.

## Remarks
This helper encapsulates the common booleans-from-localStorage pattern behind a stable contract: you always get a boolean, with a safe default. It isolates the '1' sentinel convention for true, so higher-level code can treat the result as a feature flag or preference without caring about storage details. It also gracefully handles environments where localStorage is unavailable by falling back to the provided default.

## Example
```typescript
const verboseLogging = readBool('prefs.verboseLogging', false);
```

## Notes
- Only '1' is interpreted as true; everything else yields false, including 'true' or 'yes'.
- Access is wrapped in try/catch; in restricted environments the fallback is returned silently.
- It never mutates storage; it only reads.

---

## readLegacyHideReactDetails
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
function readLegacyHideReactDetails(): boolean
```

**Returns:** `boolean`


Reads the legacy user preference determining whether React details should be hidden in the UI. It fetches LEGACY_HIDE_REACT_DETAILS_KEY from localStorage and returns true only when the stored string equals '1'. If the key is missing, the value is not '1', or any storage access error occurs, the function returns false. This encapsulation makes it safe to query the preference from places that render UI, without repeating storage handling or guarding against storage unavailability.

## Remarks
This function centralizes the legacy-format flag (string '1') and provides a stable default of false when the preference can't be read. It helps keep UI logic simple by abstracting away localStorage access and error handling.

## Example
```typescript
// Most common usage: conditionally render elements based on the legacy preference
if (readLegacyHideReactDetails()) {
  // hide advanced React details in the UI
} else {
  // show React details
}
```

## Notes
- Access to localStorage can throw in private/incognito modes or in certain environments; the function catches errors and returns false.
- Only a value of '1' is treated as true; any other value (including 'true', 'yes', or '0') yields false.
- LEGACY_HIDE_REACT_DETAILS_KEY must be defined consistently; a mismatch would default to false.

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


UseBoolPref is a React hook that reads and writes a boolean preference by key, returning [value, set]. Use it when you need a simple, hook-based API to track a boolean user preference and have the UI reflect changes across tabs or other parts of the app.

## Remarks
The hook encapsulates storage read/write and subscribes to global events so the component stays in sync with the underlying preference. It performs an optimistic update on set, updating local state immediately and relying on the event system to reconcile with the stored value. It also filters change events to refresh only when the relevant key is affected, avoiding unnecessary re-renders for unrelated keys.

## Example
```typescript
// Example usage of useBoolPref
const [dark, setDark] = useBoolPref('ui.darkMode', false);

console.log('Dark mode enabled:', dark);
setDark(!dark);
```

## Notes
- No error propagation: writeBool errors are not surfaced; the UI may reflect the requested value even if the underlying storage fails, until a refresh occurs.
- Key stability matters: changing the key or fallback will reinitialize the hook; ensure keys are stable for the component lifecycle.

---

## useHideThinking
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideThinking(): [boolean, (next: boolean) => void]
```


useHideThinking returns a tuple [boolean, (next: boolean) => void] representing the user's preference for hiding the 'thinking' indicator and a setter to update it. It delegates to useBoolPref with the HIDE_THINKING_KEY and a default from readLegacyHideReactDetails(), so the preference is persisted across sessions and remains backward-compatible with legacy behavior; use it in components that render or suppress the thinking indicator based on this preference.

## Remarks
This hook encapsulates a centralized user preference for hiding the thinking indicator. By delegating to useBoolPref and readLegacyHideReactDetails, it provides a single source of truth and shields UI components from storage details. It fits into a family of useX hooks that manage named boolean preferences across the app, promoting consistency and easier testing.

## Example
```typescript
const [hideThinking, setHideThinking] = useHideThinking();
// hideThinking === true -> do not render the thinking indicator

if (!hideThinking) {
  // render thinking indicator here
}

// Update the user preference
setHideThinking(true);
```

## Notes
- The setter takes a boolean value; it is not a functional updater. Pass true or false directly.
- The default value is sourced from readLegacyHideReactDetails(); migrations or changes to the legacy path may require updates to the stored key for future defaults.

---

## useHideToolCalls
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideToolCalls(): [boolean, (next: boolean) => void]
```


This React hook exposes a user preference for hiding tool-call details in the UI and provides a setter to update that preference. It returns a two-item tuple: [boolean, (next: boolean) => void], where the boolean indicates whether tool calls should be hidden and the function updates that value. The implementation delegates to useBoolPref with the key HIDE_TOOL_CALLS_KEY and uses readLegacyHideReactDetails() as the default when no value is stored, ensuring backward-compatible behavior.

## Remarks
This abstraction centralizes the concept of hiding tool calls behind a single, reusable hook, ensuring all components observe the same preference and react to changes consistently. It decouples UI logic from the exact persistence mechanism and preserves a sensible default derived from legacy settings. By encapsulating the preference, developers can toggle visibility without duplicating state management or storage concerns across multiple components.

## Example
```typescript
const [hideToolCalls, setHideToolCalls] = useHideToolCalls();

function ToolCallsToggle() {
  return (
    <button onClick={() => setHideToolCalls(!hideToolCalls)}>
      {hideToolCalls ? 'Show Tool Calls' : 'Hide Tool Calls'}
    </button>
  );
}

// Usage in a component
return (
  <div>
    <ToolCallsToggle />
    {!hideToolCalls && <ToolCallsPanel />}
  </div>
);
```

## Notes
- The setter updates a persistent user preference; always use the provided setter rather than mutating the boolean value directly.
- The default value is derived from readLegacyHideReactDetails(), so behavior may reflect legacy app settings unless updated via the setter.
- This hook is intended for cross-component UI control of tool-call visibility, not for ephemeral local state.

---

## useHideToolResults
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideToolResults(): [boolean, (next: boolean) => void]
```


The hook returns a tuple [boolean, (next: boolean) => void] where the boolean indicates whether tool results should be hidden and the function updates that preference. It wraps a generic boolean-preference caller (useBoolPref) with the specific key HIDE_TOOL_RESULTS_KEY and a default derived from readLegacyHideReactDetails(), enabling centralized, consistent control of this UI preference across the app.

## Remarks
This domain-specific hook centralizes the "hide tool results" preference, ensuring consistent behavior across components and future refactors. By wiring into the shared boolean-preference mechanism, it also preserves backward-compatible defaults via readLegacyHideReactDetails(), reducing migration risk. Using this hook improves testability and readability by removing direct reads/writes to storage in UI components and clarifying the intent of toggling the UI for tool results.

## Example
```typescript
const [hide, setHide] = useHideToolResults();
// Typical usage: bind to a checkbox and conditionally render tool results
return (
  <div>
    <label>
      <input
        type="checkbox"
        checked={hide}
        onChange={(e) => setHide(e.target.checked)}
      />
      Hide tool results
    </label>
    {!hide && <ToolResults />}
  </div>
);
```

## Notes
- The setter accepts a boolean representing the next value; to toggle, compute the next value (e.g., setHide(!hide)).
- The default value is derived from readLegacyHideReactDetails(), so be aware of any legacy behavior that may influence initial state.

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


Persists a boolean preference in localStorage under the provided key by encoding true as '1' and false as '0', then notifies listeners of the change by dispatching a CustomEvent with the PREFS_CHANGED_EVENT type and the key included in the event detail. If localStorage is unavailable (for example in privacy modes or when storage quota is exceeded), the write is skipped and no event is dispatched; the app remains functional and the preference is ephemeral in that case.

## Remarks

Consolidates persistence and change notification for user preferences. The emitted event lets decoupled components refresh state in response to updates without querying storage directly.

## Example

```typescript
writeBool('darkMode', true);
```

## Notes

- Silent catch hides storage failures; no exception is thrown and no event is dispatched in that path.
- Relies on a globally defined PREFS_CHANGED_EVENT; ensure it exists and that listeners subscribe to it.

---