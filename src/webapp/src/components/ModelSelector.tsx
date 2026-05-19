import { useCallback, useEffect, useMemo, useState } from 'react';
import { fetchModels, setActiveModel, type ModelDto, type ModelsResponse } from '../api/models';

// Catalog-backed dropdown that drives ApplicationUser.PreferredModel via
// /api/models. The selection takes effect on the *next* chat turn — the
// agent re-resolves IModelCatalog every call. We refetch after each PUT so
// the UI shows the canonical state (the server may have normalised empty
// strings to nulls, for example).
//
// "Default" handling: an empty value in the <select> means "clear my pick
// and fall back to the config default". The user always sees the actual
// resolved selection in the dropdown.
export function ModelSelector() {
  const [data, setData] = useState<ModelsResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const ac = new AbortController();
    fetchModels(ac.signal)
      .then(setData)
      .catch((e: unknown) => {
        if ((e as Error).name !== 'AbortError') {
          setError((e as Error).message);
        }
      });
    return () => ac.abort();
  }, []);

  // Optimistic-ish update: we re-fetch the canonical state from the PUT
  // response so the dropdown reflects what the server actually stored.
  const onPick = useCallback(async (value: string) => {
    setSaving(true);
    setError(null);
    try {
      const [provider, name] = value === '' ? [null, null] : value.split('::');
      const next = await setActiveModel({ provider, name });
      setData(next);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSaving(false);
    }
  }, []);

  const selectedKey = useMemo(() => {
    if (!data) return '';
    return `${data.selected.provider}::${data.selected.name}`;
  }, [data]);

  if (error && !data) {
    return <div className="error">Failed to load models: {error}</div>;
  }
  if (!data) {
    return <div className="settings-loading">Loading models…</div>;
  }

  return (
    <div className="model-selector">
      <label className="model-selector-label" htmlFor="model-selector-picker">
        Active model
      </label>
      <select
        id="model-selector-picker"
        className="model-selector-picker"
        value={selectedKey}
        onChange={e => void onPick(e.target.value)}
        disabled={saving}
      >
        {data.availableModels.map(m => (
          <option key={`${m.provider}::${m.name}`} value={`${m.provider}::${m.name}`}>
            {labelFor(m)}
          </option>
        ))}
      </select>
      <p className="settings-hint">
        Changes apply on your next message. The default option is the one
        marked in <code>appsettings.json</code>{' '}
        (<code>Providers:&lt;name&gt;:Models[].IsActive</code>).
      </p>
      {error && <div className="error">{error}</div>}
    </div>
  );
}

function labelFor(m: ModelDto): string {
  const tag = m.isDefault ? ' (default)' : '';
  const window = (m.contextWindowTokens / 1000).toFixed(0);
  return `${m.provider} / ${m.name} — ${window}k ctx${tag}`;
}
