import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

// Renders the nested route when the user is authenticated; otherwise bounces
// to /login. The `state.from` payload lets LoginPage send the user back to the
// URL they originally requested after a successful sign-in.
export function ProtectedRoute() {
  const { user } = useAuth();
  const location = useLocation();

  if (user === undefined) {
    return <div className="auth-loading">Loading…</div>;
  }
  if (user === null) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }
  return <Outlet />;
}
