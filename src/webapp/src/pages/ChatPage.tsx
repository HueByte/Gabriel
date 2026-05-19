import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { HiOutlineArrowPath } from 'react-icons/hi2';
import { Chat } from '../components/Chat';
import { GabrielSequenceView } from '../components/GabrielSequenceView';
import { useMainLayout } from '../layouts/MainLayout';
import { ConversationsService, type ConversationResponse } from '../api/generated';
import { notifyError } from '../lib/notify';
import { paletteForSeed, paletteVarsFromStops, type RGB } from '../pulse/palettes';
import type { GabrielSequence } from '../api/sequence';

const ACTIVE_CONVERSATION_KEY = 'gabriel.activeConversationId';

// Placeholder seed used only during the brief gap before the conversation
// metadata loads. The avatar swaps to the conversation's real seed as soon
// as Chat fires onConversationLoaded.
const FALLBACK_AVATAR_SEED = 1;

function persistActiveConversation(id: string | null) {
  try {
    if (id) localStorage.setItem(ACTIVE_CONVERSATION_KEY, id);
    else localStorage.removeItem(ACTIVE_CONVERSATION_KEY);
  } catch {
    // ignore
  }
}

export function ChatPage() {
  const { conversationId = '' } = useParams<{ conversationId: string }>();
  const navigate = useNavigate();
  const { bumpSidebar } = useMainLayout();

  const [avatarSeed, setAvatarSeed] = useState<number>(FALLBACK_AVATAR_SEED);
  const [sequenceRefresh, setSequenceRefresh] = useState(0);
  const [thinking, setThinking] = useState(false);
  // Server-driven palette pulled from the latest GabrielSequence response.
  // Falls back to the seed-derived pulse palette before the first sequence
  // fetch resolves.
  const [sequenceStops, setSequenceStops] = useState<readonly RGB[] | null>(null);

  // Remember the active conversation so IndexPage can resume here on next visit.
  useEffect(() => {
    persistActiveConversation(conversationId || null);
  }, [conversationId]);

  // When the active conversation changes (via URL), clear the previous server
  // palette so we fall back to seed-derived colors until the new sequence
  // resolves. Without this the old conversation's palette would briefly bleed
  // into the new one's UI on switch.
  useEffect(() => {
    setSequenceStops(null);
  }, [conversationId]);

  const bumpSequence = useCallback(() => setSequenceRefresh(n => n + 1), []);

  // Convenience for "a chat turn just completed" — both the sidebar list
  // (re-sort by updatedAt) and the Gabriel Sequence (Live State just changed)
  // are affected.
  const onTurnComplete = useCallback(() => {
    bumpSidebar();
    bumpSequence();
  }, [bumpSidebar, bumpSequence]);

  // Chat fires this once it loads the conversation's metadata — lets us pick
  // up the avatar seed without doing a duplicate fetch.
  const handleConversationLoaded = useCallback((conv: ConversationResponse) => {
    setAvatarSeed(conv.avatarSeed);
  }, []);

  // GabrielSequenceView fires this once per successful sequence fetch. The
  // sequence's palette is the canonical visual identity now that the avatar
  // is server-driven.
  const handleSequenceLoaded = useCallback((seq: GabrielSequence) => {
    const stops: RGB[] = seq.palette.map(c => [c[0], c[1], c[2]] as const);
    setSequenceStops(stops);
  }, []);

  const rerollAvatar = useCallback(async () => {
    if (!conversationId) return;
    try {
      const conv = await ConversationsService.postApiConversationsAvatarReroll({ id: conversationId });
      setAvatarSeed(conv.avatarSeed);
      // Reroll changes the seed → the sequence's palette + pattern change.
      bumpSidebar();
      bumpSequence();
    } catch (e: unknown) {
      notifyError(e);
    }
  }, [conversationId, bumpSidebar, bumpSequence]);

  // If the conversation comes back 404 (e.g. deleted on another tab), Chat
  // surfaces an error toast. We also bounce the user back to /, which will
  // either find another stored conv or create a fresh one.
  const handleConversationMissing = useCallback(() => {
    persistActiveConversation(null);
    navigate('/', { replace: true });
  }, [navigate]);

  // Active palette stops, in priority order:
  //   1) The server-driven Gabriel Sequence palette — canonical visual
  //      identity once the avatar finishes its first fetch.
  //   2) The seed-derived pulse palette — fallback during the gap.
  const activeStops = useMemo<readonly RGB[]>(
    () => sequenceStops ?? paletteForSeed(avatarSeed).stops,
    [sequenceStops, avatarSeed],
  );
  const paletteVars = useMemo(
    () => paletteVarsFromStops(activeStops) as React.CSSProperties,
    [activeStops],
  );

  if (!conversationId) {
    // useParams' default guarantees a string but the router should never let
    // us reach this branch — defensive guard so TS knows conversationId is
    // non-empty downstream.
    return null;
  }

  return (
    <div style={paletteVars} className="palette-scope">
      <div className={`avatar-wrap${thinking ? ' thinking' : ''}`}>
        <GabrielSequenceView
          // Remount on conversation switch so the animation timer resets.
          key={conversationId}
          conversationId={conversationId}
          refreshKey={sequenceRefresh}
          onSequenceLoaded={handleSequenceLoaded}
        />
        <button
          type="button"
          className="reroll"
          onClick={() => void rerollAvatar()}
          aria-label="Reroll avatar"
          title="Reroll avatar"
        >
          <HiOutlineArrowPath aria-hidden="true" />
        </button>
      </div>

      <Chat
        // Remount on conversation switch so internal history state resets
        // cleanly instead of trying to reconcile across two histories.
        key={conversationId}
        conversationId={conversationId}
        avatarSeed={avatarSeed}
        paletteStops={sequenceStops}
        onMessageSent={onTurnComplete}
        onBusyChange={setThinking}
        onConversationLoaded={handleConversationLoaded}
        onConversationMissing={handleConversationMissing}
      />
    </div>
  );
}
