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

Reads a boolean stored in localStorage where the string '1' represents true. Returns the provided fallback when the key is missing (null/undefined) or when accessing localStorage throws (e.g. server-side rendering or storage access blocked). Use when your app stores boolean preferences as '1'/'0' strings and you want a safe, exception-free read with a default.

## Remarks
This helper normalizes a simple storage format and avoids propagating storage access errors. It treats any present value strictly: only the exact string '1' yields true; any other stored value (including '0' or 'true') yields false. The fallback is returned only when the key is absent (null/undefined) or an exception occurs while reading localStorage.

## Example
```typescript
// prefer dark mode by default if the key is missing or inaccessible
const darkMode = readBool('prefs.darkMode', true);

// explicit check for a user preference stored as '1'/'0'
if (readBool('betaFeatureEnabled', false)) {
  enableBetaFeature();
}
```

## Notes
- Only the exact string '1' is interpreted as true; other strings (e.g. 'true') are treated as false.
- Any exception thrown while accessing localStorage causes the function to return the fallback; this makes it safe to call in environments where localStorage may be unavailable.

---

## readLegacyHideReactDetails

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Returns true when a legacy user preference to hide React details is stored in localStorage (the stored value must be the string '1'); otherwise returns false. This function is safe to call in environments where localStorage may be unavailable or throw (it catches exceptions and falls back to false).

## Remarks
This wrapper exists for backward-compatibility: it reads an older preference key (LEGACY_HIDE_REACT_DETAILS_KEY) without propagating localStorage access errors to callers. It performs a strict string comparison against '1' and never mutates storage, so callers can use it to detect and apply legacy UI behavior conservatively.

## Example
```typescript
// Apply legacy preference if present
if (readLegacyHideReactDetails()) {
  // apply legacy behavior: hide React details in the UI
  hideReactDetailsInUI();
}
```

## Notes
- The function returns false both when the key is absent and when accessing localStorage throws (e.g., server-side rendering or privacy-restricted browsers), so callers cannot distinguish those cases.
- The checked value is the exact string '1' (case-sensitive).
- Relies on the global localStorage and the constant LEGACY_HIDE_REACT_DETAILS_KEY being defined in the module scope.

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


Returns a tuple [value, set] that binds a React component to a boolean user preference identified by key. The hook reads the initial value via readBool(key, fallback), writes updates with writeBool(key, next), and keeps the local state synchronized with both in-tab preference change events (PREFS_CHANGED_EVENT CustomEvent) and cross-tab storage events ('storage'). Use this hook when a component needs a reactive boolean preference value that stays consistent across the current tab and other tabs.

## Remarks
This hook centralizes reading, writing, and subscription concerns for a boolean preference: it obtains the persisted value, provides a setter that writes the preference, and listens for preference-change notifications so the UI remains in sync. It registers two listeners — a custom PREFS_CHANGED_EVENT for same-tab notifications and the built-in 'storage' event for cross-tab synchronization — and each listener filters by key so only relevant changes update the state. The setter performs an optimistic update (writes the new value and updates local state immediately); the listeners ensure eventual consistency across contexts.

## Example
```typescript
import React from 'react';

function DarkModeToggle() {
  const [darkMode, setDarkMode] = useBoolPref('darkMode', false);

  return (
    <label>
      <input
        type="checkbox"
        checked={darkMode}
        onChange={e => setDarkMode(e.target.checked)}
      />
      Dark mode
    </label>
  );
}
```

## Notes
- The initial value is read inside the useState initializer via readBool(key, fallback). If readBool accesses window or localStorage this will run during render — ensure readBool is safe for server-side rendering or only call the hook on the client.
- set performs an optimistic update: it calls writeBool(key, next) and immediately updates local state; event listeners will also refresh state when change events occur.
- Events without a specific key (e.g., a global prefs clear or a CustomEvent missing detail.key) will be treated as relevant and cause this hook to refresh its value.

---

## useHideThinking

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Provides a React boolean preference for whether the UI should hide "thinking"/react-details. Returns a tuple [value, setValue] where `value` is the current preference and `setValue` updates it. Use this hook from function components (or other hooks) when reading or toggling the user's "hide thinking" preference.

## Remarks
This is a small convenience wrapper around useBoolPref that pins the preference key (HIDE_THINKING_KEY) and uses readLegacyHideReactDetails() to derive the initial/default value. Centralizing the key and legacy-default logic here keeps migration and future key changes in one place.

## Example
```typescript
function SettingsToggle() {
  const [hideThinking, setHideThinking] = useHideThinking();

  return (
    <label>
      <input
        type="checkbox"
        checked={hideThinking}
        onChange={e => setHideThinking(e.target.checked)}
      />
      Hide thinking UI
    </label>
  );
}
```

## Notes
- This is a React hook: call it only at the top level of function components or other hooks (do not call conditionally).
- The initial value comes from readLegacyHideReactDetails(), so existing users may see a migrated/legacy setting as the default.
- The setter expects a boolean (`(next: boolean) => void`).

---

## useHideToolCalls

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Returns a boolean preference and setter that control whether "tool calls" are hidden in the UI. Use this hook from a React function component when you need to read or update the global "hide tool calls" user preference (it mirrors useBoolPref but provides the specific key and legacy default used by the application).

## Remarks
This hook is a thin wrapper around useBoolPref that supplies the specific preference key (HIDE_TOOL_CALLS_KEY) and an initial value read from readLegacyHideReactDetails() for backwards compatibility. Call sites use this hook to get a consistent source of truth for the "hide tool calls" setting across the app without having to repeat the preference key or legacy fallback logic.

## Example
```typescript
import React from "react";

function ToolCallToggle() {
  const [hideToolCalls, setHideToolCalls] = useHideToolCalls();

  return (
    <label>
      <input
        type="checkbox"
        checked={hideToolCalls}
        onChange={e => setHideToolCalls(e.target.checked)}
      />
      Hide tool calls
    </label>
  );
}
```

## Notes
- This is a React hook: call it only from function components or other hooks (observe the Rules of Hooks).
- The setter expects a boolean value (e.g., setHideToolCalls(true)); it is not an event handler wrapper.
- The initial value may come from legacy stored preferences via readLegacyHideReactDetails(), so first-time behavior can differ from a hard-coded default.

---

## useHideToolResults

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Exposes the user's "hide tool results" preference as a React hook pair: a boolean value and a setter. Use this inside functional components that need to read or update whether tool results should be hidden; it persists the choice via the app's user-preferences mechanism and applies a legacy fallback when available.

## Remarks
This is a small wrapper around the generic useBoolPref hook, bound to the HIDE_TOOL_RESULTS_KEY and using readLegacyHideReactDetails() as the initial fallback. The wrapper centralizes the preference key and legacy-migration logic so callers only need to consume the value and update it. Because it is a React hook it must follow the usual rules of hooks (call at the top level of a component or another hook).

## Example
```typescript
function ToolPanel() {
  const [hideToolResults, setHideToolResults] = useHideToolResults();

  return (
    <div>
      <label>
        <input
          type="checkbox"
          checked={hideToolResults}
          onChange={e => setHideToolResults(e.target.checked)}
        />
        Hide tool results
      </label>

      {!hideToolResults && <ToolResults />}
    </div>
  );
}
```

## Notes
- Call this hook only from React function components or other hooks (top-level only).
- The setter expects an explicit boolean (true/false); it does not toggle automatically.
- The initial value may come from a legacy preference via readLegacyHideReactDetails(), so the first read can migrate older settings.

---

## writeBool

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Persist a boolean user preference under the given key in localStorage (stored as '1' for true and '0' for false) and dispatch a PREFS_CHANGED_EVENT so same-window listeners can react. Use this when you need a small, user-visible flag saved across reloads and you want to notify local listeners about the change; if localStorage is unavailable the function fails silently and the preference is only ephemeral.

## Remarks
This helper centralizes how boolean preferences are encoded and how changes are broadcast inside the page. It writes a compact string representation to localStorage and then emits a CustomEvent with detail { key } so other parts of the app can respond immediately. Errors from localStorage (privacy mode, quota, or being disabled) are swallowed so callers do not need to handle storage exceptions — the trade-off is that preferences may not persist in those environments.

## Example
```typescript
// Store a preference
writeBool('darkMode', true);

// Listen for preference changes in the same window
window.addEventListener(PREFS_CHANGED_EVENT, (ev: Event) => {
  const detail = (ev as CustomEvent<{ key: string }>).detail;
  console.log('preference changed:', detail.key);
  // read updated value if needed
  const value = localStorage.getItem(detail.key) === '1';
});
```

## Notes
- If localStorage.setItem throws (e.g. in privacy mode or when quota is exceeded) the error is caught and ignored; the preference will not be persisted.
- The CustomEvent is dispatched only on the current window; other tabs/windows will not receive that CustomEvent. Changes to localStorage still trigger the native "storage" event in other windows.
- Boolean values are encoded as the strings '1' (true) and '0' (false); code that reads the value must decode this representation (e.g. localStorage.getItem(key) === '1').


---