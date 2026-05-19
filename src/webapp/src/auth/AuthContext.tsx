import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';
import {
  ApiError,
  AuthService,
  type MeResponse,
} from '../api/generated';
import { SESSION_EXPIRED_EVENT } from '../api/authRefresh';

export interface AuthState {
  // undefined  → still resolving the initial /me call (avoid flicker)
  // null       → confirmed unauthenticated
  // MeResponse → authenticated user
  user: MeResponse | null | undefined;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthState | null>(null);

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>');
  return ctx;
}

function formatError(e: unknown, fallback: string): string {
  if (e instanceof ApiError) {
    const detail = (e.body as { detail?: string; title?: string } | undefined)?.detail;
    return detail ?? e.message ?? fallback;
  }
  if (e instanceof Error) return e.message;
  return fallback;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<MeResponse | null | undefined>(undefined);

  const refreshMe = useCallback(async () => {
    try {
      const me = await AuthService.getApiAuthMe();
      setUser(me);
    } catch {
      // 401 (or anything else) → treat as unauthenticated
      setUser(null);
    }
  }, []);

  // On boot, ask the server "who am I?". The HttpOnly cookies travel
  // automatically; we never read tokens client-side.
  useEffect(() => {
    void refreshMe();
  }, [refreshMe]);

  const login = useCallback(async (email: string, password: string) => {
    try {
      await AuthService.postApiAuthLogin({ requestBody: { email, password } });
    } catch (e) {
      throw new Error(formatError(e, 'Login failed.'));
    }
    await refreshMe();
  }, [refreshMe]);

  const register = useCallback(async (email: string, password: string) => {
    try {
      await AuthService.postApiAuthRegister({ requestBody: { email, password } });
    } catch (e) {
      throw new Error(formatError(e, 'Registration failed.'));
    }
    await refreshMe();
  }, [refreshMe]);

  const logout = useCallback(async () => {
    try {
      await AuthService.postApiAuthLogout();
    } catch {
      // Even if the server call fails, clear local state — the cookies might be
      // gone already and forcing the user to keep retrying logout is worse than
      // a tiny window where the server still thinks they're logged in.
    }
    setUser(null);
  }, []);

  // Interceptor (axios) or SSE client fires this when a 401 → refresh attempt
  // fails. Run the full logout flow so the server-side refresh family is
  // revoked and the browser cookies are cleared — matches the manual sign-out
  // path. The logout endpoint is anonymous and idempotent, so a stale or
  // missing refresh cookie is fine.
  useEffect(() => {
    const handler = () => { void logout(); };
    window.addEventListener(SESSION_EXPIRED_EVENT, handler);
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, handler);
  }, [logout]);

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
