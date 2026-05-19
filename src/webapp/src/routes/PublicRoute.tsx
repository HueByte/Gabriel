import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

// Inverse of ProtectedRoute — gates routes (login/register) that should be
// inaccessible once authenticated. Sends already-signed-in users to /.
export function PublicRoute() {
  const { user } = useAuth();

  if (user === undefined) {
    return <div className="auth-loading">Loading…</div>;
  }
  if (user) {
    return <Navigate to="/" replace />;
  }
  return <Outlet />;
}
