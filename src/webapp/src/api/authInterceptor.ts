import axios from 'axios';

// Axios interceptor for the generated openapi-typescript-codegen client.
// On 401 it tries /api/auth/refresh once (cookies travel automatically because
// the API is same-origin via the Vite proxy) and retries the original call.
// Concurrent 401s queue on a single refresh attempt so we don't fire N parallel
// refreshes.

const AUTH_PATHS = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh', '/api/auth/logout'];

// Dispatched when the refresh attempt fails — AuthContext listens and clears state.
export const SESSION_EXPIRED_EVENT = 'gabriel:session-expired';

let refreshing: Promise<boolean> | null = null;

async function tryRefresh(): Promise<boolean> {
  try {
    const res = await fetch('/api/auth/refresh', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      // Empty body — webapp sends the refresh token via cookie. Server falls back
      // to body for external clients but ignores it for us.
      body: '{}',
    });
    return res.ok;
  } catch {
    return false;
  }
}

export function installAuthInterceptor() {
  axios.interceptors.response.use(
    response => response,
    async error => {
      const original = error.config;
      const status = error.response?.status;
      const url: string = original?.url ?? '';

      // Don't try to refresh on auth endpoints themselves — that would loop.
      const isAuthCall = AUTH_PATHS.some(p => url.startsWith(p));

      if (status !== 401 || original?._retried || isAuthCall) {
        return Promise.reject(error);
      }

      original._retried = true;

      // Coalesce concurrent 401s onto a single refresh request.
      refreshing ??= tryRefresh().finally(() => { refreshing = null; });
      const ok = await refreshing;

      if (!ok) {
        window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT));
        return Promise.reject(error);
      }

      return axios(original);
    },
  );
}
