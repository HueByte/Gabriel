// User-scoped client preferences persisted in localStorage. No server round-
// trips today — kept here so any component can read/write the same value, and
// a future migration to server-side preferences only has to swap this module.
//
// All values are tiny strings ('1' / '0'); using JSON.stringify everywhere is
// overkill and complicates the storage-event listener below.

import { useEffect, useState } from 'react';

const HIDE_REACT_DETAILS_KEY = 'gabriel.prefs.hideReactDetails';

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

// "Hide ReAct details" — when on, the chat hides thought / reasoning /
// tool-call / tool-result entries and shows only the final assistant text.
// Useful when the user wants a clean transcript without the agent's
// scaffolding visible.
export function useHideReactDetails(): [boolean, (next: boolean) => void] {
  return useBoolPref(HIDE_REACT_DETAILS_KEY, false);
}
