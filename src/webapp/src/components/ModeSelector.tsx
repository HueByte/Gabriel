import { useCallback, useState } from 'react';
import {
  GABRIEL_MODES,
  MODE_DESCRIPTIONS,
  setConversationMode,
  type GabrielMode,
} from '../api/conversationMode';

interface ModeSelectorProps {
  conversationId: string;
  // Current mode for this conversation. Null = default (treated as Chatty).
  // Initial value comes from ConversationResponse.mode on load.
  value: GabrielMode | null;
  onChanged: (next: GabrielMode | null) => void;
  disabled?: boolean;
}

// Compact per-conversation mode picker. Lives next to the composer so a
// switch is one click away — different conversations end up needing
// different shapes (chat vs code session vs review), so a chat-level
// override is the primary control.
//
// Null value means "default" (= chatty); we render that explicitly in the
// dropdown so the user can always tell what mode is in effect.
export function ModeSelector({ conversationId, value, onChanged, disabled }: ModeSelectorProps) {
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onPick = useCallback(async (raw: string) => {
    const next: GabrielMode = raw as GabrielMode;
    setSaving(true);
    setError(null);
    try {
      await setConversationMode(conversationId, next);
      onChanged(next);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSaving(false);
    }
  }, [conversationId, onChanged]);

  // value === null is shown as "chatty" since that's the effective default.
  const effective: GabrielMode = value ?? 'chatty';

  return (
    <div className="mode-selector" title={MODE_DESCRIPTIONS[effective]}>
      <label className="mode-selector-label" htmlFor="mode-selector-picker">
        mode
      </label>
      <select
        id="mode-selector-picker"
        className="mode-selector-picker"
        value={effective}
        onChange={e => void onPick(e.target.value)}
        disabled={disabled || saving}
      >
        {GABRIEL_MODES.map(m => (
          <option key={m} value={m}>{m}</option>
        ))}
      </select>
      {error && <span className="mode-selector-error" role="alert">{error}</span>}
    </div>
  );
}
