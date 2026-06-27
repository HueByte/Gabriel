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

Read a boolean preference from localStorage and fall back to a provided default when the key is missing or when storage access fails. The function treats the string '1' as true; any other stored value is considered false.

## Remarks
This helper centralizes the parsing of boolean preferences stored as strings and the error handling around localStorage access (for example, when storage is unavailable in private browsing or due to security settings). Use it whenever you need a safe, non-throwing read of a boolean user preference with a sensible default.

## Example
```typescript
// store a value elsewhere in your app
localStorage.setItem('showHints', '1');

// read it safely with a fallback
const showHints = readBool('showHints', false); // true

// missing key -> fallback
const enableBeta = readBool('enableBeta', false); // false
```

## Notes
- Only the exact string '1' is treated as true. Strings like 'true', 'True', or '0' are not considered true.
- If the key is not present in localStorage or an exception occurs while accessing localStorage, the provided fallback is returned.
- The function does not trim or normalize stored values; any leading/trailing whitespace will prevent a match with '1'.

---

## readLegacyHideReactDetails

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Read whether the legacy "hide React details" user preference is set in localStorage and return it as a boolean. Use this helper when initializing user preferences or migrating legacy settings; it safely handles environments where localStorage access may throw and treats any non-'1' value as disabled.

## Remarks
This function exists as a small compatibility shim to read an older preference key (LEGACY_HIDE_REACT_DETAILS_KEY) stored as the string '1' for enabled. It intentionally does not attempt to migrate or write new preferences — its only responsibility is to detect the old setting in a safe, synchronous way during startup or preference loading.

## Example
```typescript
const hideReactDetails = readLegacyHideReactDetails();
if (hideReactDetails) {
  // initialize UI with React details hidden
}
```

## Notes
- Accessing localStorage can throw (e.g., in some privacy modes or non-browser runtimes); this function catches exceptions and returns false in that case.
- Only the exact string '1' is considered enabled; any other value (including 'true' or '0') yields false.
- This function is read-only and synchronous; it does not persist or migrate the legacy value.

---

## useBoolPref

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Synchronizes a boolean user preference (stored externally) with React state and keeps it up-to-date across the current tab and other browser tabs/windows. Provide the preference key and a fallback value; the hook returns the current boolean value and a setter that writes the new value and updates local state optimistically.

## Remarks
This hook bridges local React state with an external preference store via readBool/writeBool and listens for two events to stay in sync: a custom PREFS_CHANGED_EVENT (used for same-tab notifications) and the browser 'storage' event (used for cross-tab notifications). It performs an optimistic local update when you call the setter — the event listeners will refresh the state if the underlying store changes elsewhere.

## Example
```typescript
function ThemeToggle() {
  const [darkMode, setDarkMode] = useBoolPref('ui.darkMode', false);

  return (
    <label>
      Dark mode
      <input
        type="checkbox"
        checked={darkMode}
        onChange={(e) => setDarkMode(e.target.checked)}
      />
    </label>
  );
}
```

## Notes
- The browser 'storage' event only fires in other tabs/windows; the hook relies on PREFS_CHANGED_EVENT for same-tab notifications — the custom event should include a detail.key when emitted so the listener can ignore unrelated changes.
- The setter performs an optimistic update (setValue immediately). If writeBool fails or is asynchronous and reverts, this hook does not roll back the optimistic state.
- Changing the key (or the fallback) will re-read and re-subscribe; keep keys stable (e.g., memoize) to avoid unnecessary re-subscriptions.
- This hook assumes readBool/writeBool and PREFS_CHANGED_EVENT behave consistently (writeBool updates the underlying store and code that writes emits PREFS_CHANGED_EVENT when needed); if those are not implemented, other components may not observe updates as expected.


---

## useHideThinking

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Returns a React hook that exposes the user's "hide thinking" preference as a boolean and a setter function. Use this inside function components to read or update the HIDE_THINKING_KEY preference without dealing with the preference key or legacy-default logic directly; the hook delegates storage and reactivity to useBoolPref and supplies a fallback default from readLegacyHideReactDetails().

## Remarks
This is a small convenience wrapper that centralizes the preference key and legacy-default handling for the "hide thinking" option. Components can import and call this hook to get a ready-to-use [value, setValue] pair instead of repeating the preference key or legacy migration logic.

## Example
```typescript
function SettingsRow() {
  const [hideThinking, setHideThinking] = useHideThinking();

  return (
    <label>
      <input
        type="checkbox"
        checked={hideThinking}
        onChange={e => setHideThinking(e.target.checked)}
      />
      Hide "thinking" details
    </label>
  );
}
```

## Notes
- This is a React hook: call it only from function components or other hooks and follow the Rules of Hooks.
- The hook calls readLegacyHideReactDetails() when invoked to compute the fallback default; avoid relying on side effects from that call.
- Persistence and reactivity (e.g., storing the preference and causing updates) are handled by useBoolPref, which this wrapper delegates to.

---

## useHideToolCalls

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Returns a React hook tuple [hidden, setHidden] that reads and updates the user's "hide tool calls" preference. Call this from a component to read whether tool-call details should be hidden and to toggle that preference; the hook delegates to the shared useBoolPref implementation and seeds its initial value from the legacy readLegacyHideReactDetails() to preserve older user settings.

## Remarks
This small wrapper gives a semantic API for a specific user preference instead of callers using the raw preference key. It centralizes migration logic (using readLegacyHideReactDetails) so components do not need to know about legacy storage details and so the migration path is consistent across the app. The hook itself relies on useBoolPref for persistence and change subscriptions.

## Example
```typescript
import React from "react";
import { useHideToolCalls } from "./userPrefs";

function ToolCallToggle() {
  const [hidden, setHidden] = useHideToolCalls();

  return (
    <label>
      <input
        type="checkbox"
        checked={hidden}
        onChange={e => setHidden(e.target.checked)}
      />
      Hide tool calls
    </label>
  );
}
```

## Notes
- This is a React hook: call only from function components or other hooks and not conditionally.
- The initial value may come from a legacy preference via readLegacyHideReactDetails(); after updating the value it will be persisted via the current preference mechanism used by useBoolPref.
- Toggling this value updates a persisted user preference and may affect UI across components that read the same preference.

---

## useHideToolResults

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Returns a React hook-backed boolean preference and setter that controls whether the application's "tool results" UI is hidden. Use this from function components or other hooks to read the current preference and update it; the hook provides a migration path by seeding the preference from an older/legacy setting when no explicit value has been stored.

## Remarks
This hook is a thin wrapper around the shared boolean-preference mechanism (useBoolPref) and centralizes the preference key (HIDE_TOOL_RESULTS_KEY) and legacy migration (readLegacyHideReactDetails()). The legacy reader is used only to provide the initial default so existing users keep their previous choice; once the preference is stored by the modern system, the legacy value is no longer consulted.

## Example
```typescript
import React from 'react';
import { useHideToolResults } from './lib/userPrefs';

function ToolResultsToggle() {
  const [hideToolResults, setHideToolResults] = useHideToolResults();

  return (
    <label>
      <input
        type="checkbox"
        checked={hideToolResults}
        onChange={e => setHideToolResults(e.target.checked)}
      />
      Hide tool results
    </label>
  );
}
```

## Notes
- This is a React hook: call it only from function components or other hooks (top-level call rules apply).
- The second tuple element is a setter with signature (next: boolean) => void; using it updates the underlying preference.
- The legacy value provided by readLegacyHideReactDetails() is used only as the initial default when no current preference exists.

---

## writeBool

> **File:** `src/webapp/src/lib/userPrefs.ts`  
> **Kind:** function

Persist a boolean user preference and notify the app of the change.

writeBool stores the given boolean under the provided key in window.localStorage as the string '1' (true) or '0' (false), then dispatches a CustomEvent named by PREFS_CHANGED_EVENT with detail { key } so other parts of the app can react.

## Remarks
This helper centralizes two common concerns: a compact on-disk representation for booleans and a simple publish/subscribe notification when a preference changes. It intentionally swallows exceptions from localStorage (e.g. privacy mode or quota issues) so callers need not handle storage failures; when storage is unavailable the preference is effectively ephemeral.

## Example
```typescript
// enable a feature and notify listeners
writeBool('enableExperimentalUI', true);

// disable a feature
writeBool('showHints', false);
```

## Notes
- If localStorage is disabled or quota is exceeded the function catches errors and returns silently — no persistence is guaranteed and no error is thrown.
- The stored value is the literal string '1' or '0', not JSON; read routines must expect that format.
- The dispatched event uses the PREFS_CHANGED_EVENT constant and includes only { key } in detail; listeners should read the current value from storage (or fall back) rather than relying on the event payload to contain the new value.
- There is no success/failure feedback from this function; callers cannot distinguish a failed write from a successful one.

---