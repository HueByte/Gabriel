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


Reads a boolean from localStorage under the given key and returns it as a boolean. If the key is missing or localStorage access fails, it returns the provided fallback. The stored value is true only when the string is exactly "1"; any other value yields false.

## Remarks
This helper centralizes the common pattern of reading a boolean preference from localStorage while guarding against storage access errors. By encoding a simple convention ("1" means true), callers can store booleans compactly and read them reliably without repeating parsing logic. It also avoids crashing the app if storage is unavailable, thanks to the try/catch.

## Example
```typescript
const prefersCompact = readBool('compactMode', false);
```

## Notes
- Only the string '1' maps to true; '0', 'true', or any other value will be treated as false.
- If localStorage is blocked or unavailable (e.g., in some privacy modes), this function returns the fallback instead of throwing.

---

## readLegacyHideReactDetails
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
function readLegacyHideReactDetails(): boolean
```

**Returns:** `boolean`


Reads the LEGACY_HIDE_REACT_DETAILS_KEY flag from localStorage and returns true if its value is '1'; otherwise, it returns false. If localStorage access fails (for example, in environments where storage is unavailable), it safely falls back to false. Use this function to determine whether to hide React-related UI details in the legacy path rather than performing storage checks inline.

## Remarks
By centralizing the storage access, this function isolates the legacy UI toggle from the rest of the codebase. The try/catch pattern ensures resilience against environments without localStorage, private modes, or blocked storage, providing a deterministic default behavior. It relies on the LEGACY_HIDE_REACT_DETAILS_KEY; any change to the key affects every caller.

## Example
```typescript
if (readLegacyHideReactDetails()) {
  // Hide React-specific details for legacy configurations
} else {
  // Render React details as usual
}
```

## Notes
- The function uses strict equality to '1' to enable the flag.
- If the key is missing or its value is not '1', the function returns false.
- This function swallows storage errors; do not rely on it to detect storage availability.

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


useBoolPref binds a boolean user preference to React state. It reads the initial value with readBool(key, fallback) and keeps the value in sync when the preference changes, either within the tab (via PREFS_CHANGED_EVENT) or across tabs (via the storage event). It returns a tuple [value, set], where value is the current boolean and set updates the preference with writeBool and applies an optimistic UI update.

## Remarks
This hook centralizes the boilerplate of reading and observing a boolean preference, decoupling UI components from the underlying storage and event-wiring. It ensures a consistent, reactive view of a named preference across the app and tabs, coordinating with a unified event stream so changes propagate predictably to all listeners.

## Example
```typescript
function ThemeToggle() {
  const [darkMode, setDarkMode] = useBoolPref('darkMode', false);
  return (
    <button onClick={() => setDarkMode(!darkMode)}>
      {darkMode ? 'Disable' : 'Enable'} dark mode
    </button>
  );
}
```

## Notes
- The hook listens for changes in two channels: a custom in-tab event (PREFS_CHANGED_EVENT) and the browser storage event. If you update the same key from elsewhere, the hook will refresh automatically.
- The setter performs an optimistic update: it writes the new value and immediately updates local state, with the expectation that the change event will propagate to keep all listeners in sync.

---

## useHideThinking
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideThinking(): [boolean, (next: boolean) => void]
```


Returns a two-element tuple: the current boolean indicating whether the thinking indicator should be hidden, and a setter to update that preference. It wraps a generic boolean-preference hook via useBoolPref(HIDE_THINKING_KEY, readLegacyHideReactDetails()). Use this hook when you want to read or persist the user's preference for hiding the thinking state in the UI, rather than accessing the preference directly.

## Remarks
This hook centralizes a specific user preference for the UI's thinking indicator, preventing duplication of the underlying key and default logic across components. It also preserves backward compatibility by using readLegacyHideReactDetails() as the default when the preference has not yet been set, ensuring consistent behavior across both legacy and newer code paths.

## Notes
- The setter accepts a boolean value; it does not toggle automatically. Pass true to hide or false to show.
- The default value is sourced from readLegacyHideReactDetails(), which provides backward-compatible behavior for older React details state.

---

## useHideToolCalls
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideToolCalls(): [boolean, (next: boolean) => void]
```


useHideToolCalls is a React hook that exposes a boolean preference for hiding tool calls in the UI. It delegates to useBoolPref with the key HIDE_TOOL_CALLS_KEY and a default value sourced from readLegacyHideReactDetails(), returning a tuple [boolean, (next: boolean) => void] that components can read and update to reflect the user's preference.

## Remarks
This hook centralizes access to the 'hide tool calls' preference, so components don't need to know how the value is stored or persisted. The default value comes from readLegacyHideReactDetails(), ensuring backward compatibility while the app migrates to newer preference mechanisms. It sits at the boundary between user preferences and UI rendering, enabling consistent behavior across the app while keeping storage details out of the UI layer.

## Example
```typescript
const [hideToolCalls, setHideToolCalls] = useHideToolCalls();
// Use hideToolCalls to conditionally render tool call details
if (hideToolCalls) {
  // suppress tool-call related UI
}
setHideToolCalls(true); // persist the choice to hide tool calls
```

## Notes
- The setter accepts a boolean and updates the underlying preference store via useBoolPref.
- The default is read from legacy behavior via readLegacyHideReactDetails(), so existing users see familiar UI choices on first load.

---

## useHideToolResults
> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

```typescript
export function useHideToolResults(): [boolean, (next: boolean) => void]
```


UseHideToolResults provides a small, reusable React hook for honoring a user preference to hide or show tool results. It returns a tuple [boolean, (next: boolean) => void] that components can read to determine whether to hide the results and to update that preference in response to user actions. The hook relies on the HIDE_TOOL_RESULTS_KEY for storage and uses readLegacyHideReactDetails as the default when no preference has yet been stored.

## Remarks
By centralizing this preference behind a single hook, you avoid duplicating the storage key logic across several components. It also helps migrate existing UI that followed the legacy behavior by encapsulating the default in readLegacyHideReactDetails, while new code can rely on the shared preference. This hook follows the same pattern as other preference hooks, providing a consistent, composable API for UI decisions tied to user settings.

## Example
```ts
// Example: toggle the visibility of tool results
import React from 'react';

function ToolResultsToggle() {
  const [hide, setHide] = useHideToolResults();

  return (
    <div>
      <button onClick={() => setHide(!hide)}>
        {hide ? 'Show Tool Results' : 'Hide Tool Results'}
      </button>

      {!hide && (
        <div className="tool-results">
          {/* render tool results here */}
        </div>
      )}
    </div>
  );
}
```

## Notes
- The setter signature is (next: boolean) => void; to toggle, compute the desired next value and pass it directly (e.g., setHide(!hide)). There is no updater-function form.
- Initial value depends on readLegacyHideReactDetails; during migration, the default may reflect legacy behavior until the stored preference is updated.
- The hook uses a shared storage key (HIDE_TOOL_RESULTS_KEY); keep that key consistent across all consumers to avoid diverging behavior.

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


The function persists a boolean preference by storing it in localStorage as an encoded string ('1' for true, '0' for false) under the provided key, and then broadcasts a change notification to the rest of the application. If the write succeeds, a CustomEvent named by PREFS_CHANGED_EVENT is dispatched with the key included in the event detail, allowing listeners to react immediately. If localStorage is unavailable (for example in privacy mode or due to quota limits), the operation is swallowed silently, and the app continues to function with ephemeral state.

## Remarks
This helper abstracts the common pattern of persisting a boolean preference and notifying dependents in one place. By coupling the persistence with an event broadcast, components can stay in sync without requiring direct references to the storage mechanism. The silent fallback ensures robustness in restricted environments, preventing a hard failure if persistence is blocked, while signaling to developers that the change could not be persisted.

## Example
```typescript
writeBool('darkMode', true);
```

## Notes
- If localStorage is disabled or quota-exceeded, no error is surfaced and no event is dispatched.
- Values are stored as '1' or '0'; retrieval logic should interpret these strings back into booleans.
- The function returns void and does not surface success/failure to the caller; callers should rely on the event or storage state if needed.

---