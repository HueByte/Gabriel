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
      // The server reads the refresh token from the HttpOnly cookie when present
      // and falls back to body.refreshToken otherwise (external clients). We
      // MUST include the field even with an empty value: the controller's
      // [FromBody] RefreshTokenRequest has a non-nullable string member, and
      // [ApiController]'s auto-validation rejects `{}` as a 400 before the
      // cookie-fallback code in the action runs. With the field present (any
      // value), validation passes and the action picks up the cookie.
      body: JSON.stringify({ refreshToken: '' }),
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
