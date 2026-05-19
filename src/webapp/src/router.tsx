import { Component, Suspense, lazy, type ErrorInfo, type ReactNode } from 'react';
import { createBrowserRouter } from 'react-router-dom';
import { AuthLayout } from './layouts/AuthLayout';
import { MainLayout } from './layouts/MainLayout';
import { ProtectedRoute } from './routes/ProtectedRoute';
import { PublicRoute } from './routes/PublicRoute';

// Pages — lazy loaded. Named-export wrapping is required because React.lazy
// expects a module with a `default` export.
const IndexPage = lazy(() =>
  import('./pages/IndexPage').then(m => ({ default: m.IndexPage })),
);
const ChatPage = lazy(() =>
  import('./pages/ChatPage').then(m => ({ default: m.ChatPage })),
);
const DiagnosticsPage = lazy(() =>
  import('./pages/DiagnosticsPage').then(m => ({ default: m.DiagnosticsPage })),
);
const LoginPage = lazy(() =>
  import('./pages/LoginPage').then(m => ({ default: m.LoginPage })),
);
const RegisterPage = lazy(() =>
  import('./pages/RegisterPage').then(m => ({ default: m.RegisterPage })),
);

function PageLoader() {
  return <div className="auth-loading">Loading…</div>;
}

// Shown when a lazy chunk fails to load. Almost always means a new build was
// deployed mid-session and the old chunk URL is now 404 — a reload fixes it.
function ChunkErrorFallback() {
  const handleReload = () => window.location.reload();
  return (
    <div className="auth-loading" style={{ flexDirection: 'column', gap: 12 }}>
      <p>Failed to load page. A new version may be available.</p>
      <button type="button" className="auth-submit" onClick={handleReload}>
        Reload
      </button>
    </div>
  );
}

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback: ReactNode;
}
interface ErrorBoundaryState {
  hasError: boolean;
}

class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }
  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }
  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    // Single out the "stale chunk after deploy" case for clearer logs — every
    // other error is logged with the React tree info that React.captureOwner
    // gives us in errorInfo.
    if (error.message.includes('Failed to fetch dynamically imported module')) {
      console.error('Chunk loading error — new version may be available:', error.message);
    } else {
      console.error('Page error:', error, errorInfo);
    }
  }
  render(): ReactNode {
    if (this.state.hasError) return this.props.fallback;
    return this.props.children;
  }
}

function LazyPage({ children }: { children: ReactNode }) {
  return (
    <Suspense fallback={<PageLoader />}>
      <ErrorBoundary fallback={<ChunkErrorFallback />}>
        {children}
      </ErrorBoundary>
    </Suspense>
  );
}

export const router = createBrowserRouter([
  // Auth pages — redirect to / when already signed in.
  {
    element: <PublicRoute />,
    children: [
      {
        element: <AuthLayout />,
        children: [
          {
            path: '/login',
            element: <LazyPage><LoginPage /></LazyPage>,
          },
          {
            path: '/register',
            element: <LazyPage><RegisterPage /></LazyPage>,
          },
        ],
      },
    ],
  },
  // App pages — require authentication.
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <MainLayout />,
        children: [
          {
            index: true,
            element: <LazyPage><IndexPage /></LazyPage>,
          },
          {
            path: '/c/:conversationId',
            element: <LazyPage><ChatPage /></LazyPage>,
          },
          {
            path: '/c/:conversationId/diagnostics',
            element: <LazyPage><DiagnosticsPage /></LazyPage>,
          },
        ],
      },
    ],
  },
]);
