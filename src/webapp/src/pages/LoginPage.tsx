import { useEffect, useState, type FormEvent } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  HiOutlineArrowPath,
  HiOutlineArrowRightOnRectangle,
  HiOutlineEnvelope,
  HiOutlineExclamationCircle,
  HiOutlineEye,
  HiOutlineEyeSlash,
  HiOutlineLockClosed,
  HiOutlineSparkles,
} from 'react-icons/hi2';
import { Avatar } from '../components/Avatar';
import { useAuth } from '../auth/AuthContext';
import { randomSeed } from '../pulse/rng';

interface LocationState {
  from?: { pathname?: string };
}

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  // If ProtectedRoute redirected us here, it stashed the originally-requested
  // URL so we can bounce the user back after a successful login. Falls back
  // to "/" otherwise.
  const from = (location.state as LocationState | null)?.from?.pathname ?? '/';

  const [seed, setSeed] = useState<number>(() => randomSeed());
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Reroll the avatar on every visit so the login screen feels alive instead
  // of static. The seed is purely visual; nothing persists from it.
  useEffect(() => {
    setSeed(randomSeed());
  }, []);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (busy) return;
    setBusy(true);
    setError(null);
    try {
      await login(email, password);
      navigate(from, { replace: true });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Login failed.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="auth-screen">
      <form className="auth-card" onSubmit={onSubmit}>
        <div className="auth-avatar">
          <Avatar seed={seed} />
          <button
            type="button"
            className="auth-avatar-reroll"
            onClick={() => setSeed(randomSeed())}
            disabled={busy}
            aria-label="Reroll avatar"
            title="Reroll avatar"
          >
            <HiOutlineArrowPath aria-hidden="true" />
          </button>
        </div>

        <div className="auth-heading">
          <h1 className="auth-title">Welcome back</h1>
          <p className="auth-subtitle">Sign in to wake your Gabriel.</p>
        </div>

        <label className="auth-field">
          <span>Email</span>
          <div className="auth-input">
            <HiOutlineEnvelope aria-hidden="true" />
            <input
              type="email"
              autoComplete="email"
              placeholder="you@somewhere.dev"
              value={email}
              onChange={e => setEmail(e.target.value)}
              disabled={busy}
              required
            />
          </div>
        </label>
        <label className="auth-field">
          <span>Password</span>
          <div className="auth-input">
            <HiOutlineLockClosed aria-hidden="true" />
            <input
              type={showPassword ? 'text' : 'password'}
              autoComplete="current-password"
              placeholder="••••••••"
              value={password}
              onChange={e => setPassword(e.target.value)}
              disabled={busy}
              required
            />
            <button
              type="button"
              className="auth-input-toggle"
              onClick={() => setShowPassword(v => !v)}
              aria-label={showPassword ? 'Hide password' : 'Show password'}
              title={showPassword ? 'Hide password' : 'Show password'}
              tabIndex={-1}
            >
              {showPassword
                ? <HiOutlineEyeSlash aria-hidden="true" />
                : <HiOutlineEye aria-hidden="true" />}
            </button>
          </div>
        </label>

        {error && (
          <div className="auth-error" role="alert">
            <HiOutlineExclamationCircle aria-hidden="true" />
            <span>{error}</span>
          </div>
        )}

        <button type="submit" className="auth-submit" disabled={busy || !email || !password}>
          {busy ? (
            <>
              <HiOutlineSparkles aria-hidden="true" className="auth-submit-spin" />
              <span>Signing in…</span>
            </>
          ) : (
            <>
              <span>Sign in</span>
              <HiOutlineArrowRightOnRectangle aria-hidden="true" />
            </>
          )}
        </button>

        <div className="auth-switch">
          <span>No account?</span>
          <button type="button" className="auth-link" onClick={() => navigate('/register')}>
            Create one
          </button>
        </div>
      </form>
    </div>
  );
}
