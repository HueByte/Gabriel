import { useEffect, type ReactNode } from 'react';
import { useAuth } from './AuthContext';
import { useRoute } from './useRoute';
import { LoginPage } from './LoginPage';
import { RegisterPage } from './RegisterPage';

// Wraps the rest of the app. Routes the user to login/register when
// unauthenticated, swaps to the app once /me confirms a session, and
// keeps the browser URL honest about where they are.
export function AuthGate({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const { path, navigate } = useRoute();

  // While authenticated, scrub auth routes from the URL so back-button doesn't
  // dump the user onto a login form that immediately bounces them back.
  useEffect(() => {
    if (user && (path === '/login' || path === '/register')) {
      navigate('/');
    }
  }, [user, path, navigate]);

  // While unauthenticated, force the URL onto an auth route so a deep link
  // doesn't 404 into a blank screen behind the gate.
  useEffect(() => {
    if (user === null && path !== '/login' && path !== '/register') {
      navigate('/login');
    }
  }, [user, path, navigate]);

  if (user === undefined) {
    return <div className="auth-loading">Loading…</div>;
  }

  if (user === null) {
    return path === '/register' ? <RegisterPage /> : <LoginPage />;
  }

  return <>{children}</>;
}
