// Coalesced refresh-on-401 helper, shared between the axios interceptor and the
// hand-rolled SSE client. The /api/auth/refresh POST lives here exclusively so
// concurrent 401s on different transports never fire multiple refresh requests.

export const SESSION_EXPIRED_EVENT = 'gabriel:session-expired';

let refreshing: Promise<boolean> | null = null;

async function postRefresh(): Promise<boolean> {
  try {
    const res = await fetch('/api/auth/refresh', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      // Empty body — webapp sends the refresh token via cookie; the server falls
      // back to body for external clients but ignores it for us.
      body: '{}',
    });
    return res.ok;
  } catch {
    return false;
  }
}

// Returns whether the refresh succeeded. Multiple concurrent callers share one
// in-flight request — the first invocation kicks off the fetch; the rest await
// the same promise. Cleared when the promise settles so a future expiry can
// trigger a fresh attempt.
export function refreshSession(): Promise<boolean> {
  refreshing ??= postRefresh().finally(() => { refreshing = null; });
  return refreshing;
}

// Notify the rest of the app that the session is unrecoverable. AuthContext
// listens and tears down local state + invokes /api/auth/logout to clear the
// server-side refresh family + browser cookies.
export function signalSessionExpired(): void {
  window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT));
}
