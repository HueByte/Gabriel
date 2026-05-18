import { useState, type FormEvent } from 'react';
import { useAuth } from './AuthContext';
import { useRoute } from './useRoute';

export function RegisterPage() {
  const { register } = useAuth();
  const { navigate } = useRoute();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (busy) return;
    setBusy(true);
    setError(null);
    try {
      await register(email, password);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Registration failed.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="auth-screen">
      <form className="auth-card" onSubmit={onSubmit}>
        <h1 className="auth-title">Create account</h1>
        <label className="auth-field">
          <span>Email</span>
          <input
            type="email"
            autoComplete="email"
            value={email}
            onChange={e => setEmail(e.target.value)}
            disabled={busy}
            required
          />
        </label>
        <label className="auth-field">
          <span>Password</span>
          <input
            type="password"
            autoComplete="new-password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            disabled={busy}
            required
            minLength={6}
          />
        </label>
        {error && <div className="auth-error">{error}</div>}
        <button type="submit" className="auth-submit" disabled={busy || !email || !password}>
          {busy ? 'Creating…' : 'Create account'}
        </button>
        <div className="auth-switch">
          Already have an account?{' '}
          <button type="button" className="auth-link" onClick={() => navigate('/login')}>
            Sign in
          </button>
        </div>
      </form>
    </div>
  );
}
