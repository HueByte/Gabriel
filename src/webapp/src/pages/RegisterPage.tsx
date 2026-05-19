import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  HiOutlineArrowPath,
  HiOutlineEnvelope,
  HiOutlineExclamationCircle,
  HiOutlineEye,
  HiOutlineEyeSlash,
  HiOutlineLockClosed,
  HiOutlineSparkles,
  HiOutlineUserPlus,
} from 'react-icons/hi2';
import { Avatar } from '../components/Avatar';
import { useAuth } from '../auth/AuthContext';
import { randomSeed } from '../pulse/rng';

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [seed, setSeed] = useState<number>(() => randomSeed());
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Reroll the avatar on every visit so the register screen feels alive
  // instead of static. The seed is purely visual; the real avatar seed for
  // the new account is assigned server-side at registration.
  useEffect(() => {
    setSeed(randomSeed());
  }, []);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (busy) return;
    setBusy(true);
    setError(null);
    try {
      await register(email, password);
      navigate('/', { replace: true });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Registration failed.');
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
          <h1 className="auth-title">Create account</h1>
          <p className="auth-subtitle">Spin up a fresh Gabriel of your own.</p>
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
              autoComplete="new-password"
              placeholder="At least 6 characters"
              value={password}
              onChange={e => setPassword(e.target.value)}
              disabled={busy}
              required
              minLength={6}
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
              <span>Creating…</span>
            </>
          ) : (
            <>
              <span>Create account</span>
              <HiOutlineUserPlus aria-hidden="true" />
            </>
          )}
        </button>

        <div className="auth-switch">
          <span>Already have an account?</span>
          <button type="button" className="auth-link" onClick={() => navigate('/login')}>
            Sign in
          </button>
        </div>
      </form>
    </div>
  );
}
