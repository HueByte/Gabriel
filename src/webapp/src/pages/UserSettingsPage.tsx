import { useNavigate } from 'react-router-dom';
import { HiOutlineArrowLeft, HiOutlineArrowRightOnRectangle } from 'react-icons/hi2';
import { useAuth } from '../auth/AuthContext';
import { useHideThinking, useHideToolCalls, useHideToolResults } from '../lib/userPrefs';

// User-scoped settings: identity readout + sign-out. Intentionally minimal -
// password change, email change, and theme controls will land here once the
// API endpoints exist. The page reuses .settings-* styles shared with the
// project settings page so both feel like one surface.
export function UserSettingsPage() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [hideThinking, setHideThinking] = useHideThinking();
  const [hideToolCalls, setHideToolCalls] = useHideToolCalls();
  const [hideToolResults, setHideToolResults] = useHideToolResults();

  return (
    <div className="settings palette-scope">
      <div className="settings-head">
        <button type="button" className="diagnostics-back" onClick={() => navigate(-1)}>
          <HiOutlineArrowLeft aria-hidden="true" />
          <span>Back</span>
        </button>
        <h1 className="settings-title">Settings</h1>
      </div>

      <section className="settings-section">
        <h2 className="settings-section-title">Account</h2>
        {user === undefined && (
          <div className="settings-loading">Loading…</div>
        )}
        {user && (
          <dl className="settings-kv">
            <dt>Email</dt>
            <dd>{user.email}</dd>
            <dt>User ID</dt>
            <dd className="settings-mono">{user.id}</dd>
          </dl>
        )}
        {user === null && (
          <div className="error">Not signed in.</div>
        )}
      </section>

      <section className="settings-section">
        <h2 className="settings-section-title">Chat display</h2>
        <p className="settings-hint">
          The ReAct agent loop emits intermediate steps that Gabriel uses to
          reason through a request. Toggle each kind independently - final
          answers always remain visible.
        </p>
        <label className="settings-toggle">
          <input
            type="checkbox"
            checked={hideThinking}
            onChange={e => setHideThinking(e.target.checked)}
          />
          <span>Hide thinking <span className="settings-faint">(chain-of-thought + pre-tool reasoning)</span></span>
        </label>
        <label className="settings-toggle">
          <input
            type="checkbox"
            checked={hideToolCalls}
            onChange={e => setHideToolCalls(e.target.checked)}
          />
          <span>Hide tool calls <span className="settings-faint">(the `action` step - tool name + arguments)</span></span>
        </label>
        <label className="settings-toggle">
          <input
            type="checkbox"
            checked={hideToolResults}
            onChange={e => setHideToolResults(e.target.checked)}
          />
          <span>Hide tool results <span className="settings-faint">(the `observation` step - collapsed output)</span></span>
        </label>
      </section>

      <section className="settings-section">
        <h2 className="settings-section-title">Session</h2>
        <button
          type="button"
          className="settings-danger"
          onClick={() => void logout()}
        >
          <HiOutlineArrowRightOnRectangle aria-hidden="true" />
          <span>Sign out</span>
        </button>
      </section>
    </div>
  );
}
