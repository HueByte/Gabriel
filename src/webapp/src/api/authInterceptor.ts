import axios from 'axios';
import { refreshSession, signalSessionExpired } from './authRefresh';

// Axios interceptor for the generated openapi-typescript-codegen client.
// On 401 it tries /api/auth/refresh once (cookies travel automatically because
// the API is same-origin via the Vite proxy) and retries the original call.
// Concurrent 401s queue on a single refresh attempt — see authRefresh.ts.
//
// SSE (streamChat) intentionally bypasses this interceptor since it uses raw
// fetch — but it shares the same refreshSession() so a single refresh promise
// covers both transports.

const AUTH_PATHS = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh', '/api/auth/logout'];

// Re-exported so existing imports keep working. New code should pull this from
// './authRefresh' directly.
export { SESSION_EXPIRED_EVENT } from './authRefresh';

export function installAuthInterceptor() {
  axios.interceptors.response.use(
    response => response,
    async error => {
      const original = error.config;
      const status = error.response?.status;
      const url: string = original?.url ?? '';

      // Don't try to refresh on auth endpoints themselves — that would loop.
      const isAuthCall = AUTH_PATHS.some(p => url.startsWith(p));

      if (status !== 401 || isAuthCall) {
        return Promise.reject(error);
      }

      // Already attempted a refresh+retry for this request and still got 401 —
      // the session is unrecoverable. Signal so AuthContext clears local state
      // and revokes server-side (logout flow). Falling through silently would
      // log the user out via AuthContext.refreshMe's catch but skip the proper
      // server-side cleanup.
      if (original?._retried) {
        signalSessionExpired();
        return Promise.reject(error);
      }

      original._retried = true;

      const ok = await refreshSession();

      if (!ok) {
        // Refresh itself failed — refresh token expired/revoked/etc. Only at
        // this point do we treat the session as dead. Matches the user's mental
        // model: logout happens only when BOTH the access JWT and the refresh
        // token are unusable.
        signalSessionExpired();
        return Promise.reject(error);
      }

      return axios(original);
    },
  );
}
