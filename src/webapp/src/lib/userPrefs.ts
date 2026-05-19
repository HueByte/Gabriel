// User-scoped client preferences persisted in localStorage. No server round-
// trips today — kept here so any component can read/write the same value, and
// a future migration to server-side preferences only has to swap this module.
//
// All values are tiny strings ('1' / '0'); using JSON.stringify everywhere is
// overkill and complicates the storage-event listener below.

import { useEffect, useState } from 'react';

// Per-kind ReAct visibility toggles. Replaced the single boolean
// `hideReactDetails` so users can keep e.g. tool calls visible while hiding
// the model's chain-of-thought (or any other combination). One key per kind
// keeps the storage shape trivial and the storage-event listener (below)
// simple — toggling one doesn't perturb the others.
const HIDE_THINKING_KEY = 'gabriel.prefs.hideThinking';
const HIDE_TOOL_CALLS_KEY = 'gabriel.prefs.hideToolCalls';
const HIDE_TOOL_RESULTS_KEY = 'gabriel.prefs.hideToolResults';

// Legacy single-boolean key (pre-split). Migrated on first read of any of
// the new keys: if the new key is absent but the legacy key was `1`, treat
// the new key as `1` so we don't surprise users who had toggled the old
// flag on. We DON'T write back the legacy value — once any of the new keys
// is touched, the legacy entry can be cleaned up via removeItem.
const LEGACY_HIDE_REACT_DETAILS_KEY = 'gabriel.prefs.hideReactDetails';

// Storage-event channel for cross-component sync within a single tab.
// localStorage's native 'storage' event only fires across tabs, so we
// dispatch a custom event whenever we write — same shape, single hook.
const PREFS_CHANGED_EVENT = 'gabriel:prefs-changed';

function readBool(key: string, fallback: boolean): boolean {
  try {
    const v = localStorage.getItem(key);
    if (v == null) return fallback;
    return v === '1';
  } catch {
    return fallback;
  }
}

function writeBool(key: string, value: boolean): void {
  try {
    localStorage.setItem(key, value ? '1' : '0');
    window.dispatchEvent(new CustomEvent(PREFS_CHANGED_EVENT, { detail: { key } }));
  } catch {
    // localStorage may be disabled (privacy mode, quota). The preference is
    // ephemeral in that case, but the app still works.
  }
}

// React hook for any boolean preference. Re-renders when the value changes
// in this tab (via writeBool) or in another tab (via the native 'storage'
// event). Generic enough that future toggles slot in by passing a new key.
export function useBoolPref(key: string, fallback: boolean): [boolean, (next: boolean) => void] {
  const [value, setValue] = useState(() => readBool(key, fallback));

  useEffect(() => {
    const refresh = (e: Event) => {
      // CustomEvent in-tab: only refresh if the change is for our key.
      if (e instanceof CustomEvent) {
        const detail = e.detail as { key?: string } | undefined;
        if (detail?.key && detail.key !== key) return;
      } else if (e instanceof StorageEvent) {
        if (e.key && e.key !== key) return;
      }
      setValue(readBool(key, fallback));
    };
    window.addEventListener(PREFS_CHANGED_EVENT, refresh);
    window.addEventListener('storage', refresh);
    return () => {
      window.removeEventListener(PREFS_CHANGED_EVENT, refresh);
      window.removeEventListener('storage', refresh);
    };
  }, [key, fallback]);

  const set = (next: boolean) => {
    writeBool(key, next);
    setValue(next);  // optimistic — event listener would set this too
  };

  return [value, set];
}

// Reads the legacy single-boolean once on module load, used only as the
// fallback default for the new per-kind toggles. After the user touches any
// of the new toggles, the legacy key becomes irrelevant.
function readLegacyHideReactDetails(): boolean {
  try {
    return localStorage.getItem(LEGACY_HIDE_REACT_DETAILS_KEY) === '1';
  } catch {
    return false;
  }
}

// Per-kind toggles. Each independently hides one category of ReAct
// scaffolding from the chat transcript:
//   - thinking  → `thought` + `reasoning` entries (model's chain-of-thought,
//                 pre-tool reasoning text and dedicated reasoning_content)
//   - tool calls → `toolCall` entries (the action badge + tool name/args)
//   - tool results → `toolResult` entries (the observation badge + payload)
//
// Each defaults to the legacy single-flag value so users who previously
// set "hide ReAct details" keep that behavior across all three.
export function useHideThinking(): [boolean, (next: boolean) => void] {
  return useBoolPref(HIDE_THINKING_KEY, readLegacyHideReactDetails());
}
export function useHideToolCalls(): [boolean, (next: boolean) => void] {
  return useBoolPref(HIDE_TOOL_CALLS_KEY, readLegacyHideReactDetails());
}
export function useHideToolResults(): [boolean, (next: boolean) => void] {
  return useBoolPref(HIDE_TOOL_RESULTS_KEY, readLegacyHideReactDetails());
}
