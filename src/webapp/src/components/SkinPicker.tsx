import { useEffect, useState } from 'react';
import { HiOutlineArrowPath } from 'react-icons/hi2';
import { SequenceService, type SequenceCatalogResponse } from '../api/generated';
import { notifyError } from '../lib/notify';

// Reusable "skin picker" — pattern + palette selectors, plus an optional
// reroll button for the seed-derived dimensions. Used by Project Settings
// today; designed so a standalone-chat variant can drop it in later.
//
// Conventions:
//   - `null` for either dimension means "seed-derived" (the override is cleared).
//   - The component is uncontrolled-friendly: it owns nothing, just renders the
//     current state and fires onChange / onReroll up.
//   - The catalog is fetched once on mount and cached in-memory across re-renders.

interface SkinPickerProps {
  pattern: string | null;
  palette: string | null;
  onChange: (next: { pattern: string | null; palette: string | null }) => void;
  /** Optional reroll handler. When provided, a button is rendered next to the
   *  pickers. The skin overrides are NOT cleared by reroll — pattern/palette
   *  pins survive a reroll (reroll only changes seed-derived dimensions). */
  onReroll?: () => void;
  /** Disables interaction (e.g. while saving). */
  disabled?: boolean;
}

// Module-level cache so navigating between Project Settings pages doesn't
// re-fetch the static catalog every time. The catalog is process-stable; a
// fresh build would deploy a new client anyway.
let catalogCache: SequenceCatalogResponse | null = null;

export function SkinPicker({ pattern, palette, onChange, onReroll, disabled }: SkinPickerProps) {
  const [catalog, setCatalog] = useState<SequenceCatalogResponse | null>(catalogCache);

  useEffect(() => {
    if (catalogCache) return;
    let cancelled = false;
    SequenceService.getApiSequenceCatalog()
      .then(c => {
        if (cancelled) return;
        catalogCache = c;
        setCatalog(c);
      })
      .catch(e => { if (!cancelled) notifyError(e, 'Failed to load skin catalog.'); });
    return () => { cancelled = true; };
  }, []);

  const handlePattern = (value: string) => {
    onChange({ pattern: value === '' ? null : value, palette });
  };
  const handlePalette = (value: string) => {
    onChange({ pattern, palette: value === '' ? null : value });
  };

  return (
    <div className="skin-picker">
      <label className="settings-field">
        <span>Pattern</span>
        <select
          value={pattern ?? ''}
          onChange={e => handlePattern(e.target.value)}
          disabled={disabled || !catalog}
        >
          <option value="">Auto (seed-derived)</option>
          {catalog?.patterns.map(p => (
            <option key={p} value={p}>{p}</option>
          ))}
        </select>
      </label>
      <label className="settings-field">
        <span>Palette</span>
        <select
          value={palette ?? ''}
          onChange={e => handlePalette(e.target.value)}
          disabled={disabled || !catalog}
        >
          <option value="">Auto (seed-derived)</option>
          {catalog?.palettes.map(p => (
            <option key={p} value={p}>{p}</option>
          ))}
        </select>
      </label>
      {onReroll && (
        <button
          type="button"
          className="settings-secondary"
          onClick={onReroll}
          disabled={disabled}
          title="Reroll seed — pattern/palette pins survive"
        >
          <HiOutlineArrowPath aria-hidden="true" />
          <span>Reroll seed</span>
        </button>
      )}
    </div>
  );
}
