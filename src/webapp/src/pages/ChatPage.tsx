import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { HiOutlineArrowPath } from 'react-icons/hi2';
import { Chat } from '../components/Chat';
import { ContextStats } from '../components/ContextStats';
import { GabrielSequenceView } from '../components/GabrielSequenceView';
import { ConversationsService, type ConversationResponse } from '../api/generated';
import { notifyError } from '../lib/notify';
import { paletteForSeed, paletteVarsFromStops, type RGB } from '../pulse/palettes';
import type { GabrielSequence, SequenceSource } from '../api/sequence';

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

  const [avatarSeed, setAvatarSeed] = useState<number>(FALLBACK_AVATAR_SEED);
  // The conversation's parent project (null until metadata loads). When
  // !isDefault, every chat in the project shares one sequence keyed on the
  // project id; otherwise the chat falls back to its own conversation-keyed
  // sequence (standalone behavior).
  const [projectId, setProjectId] = useState<string | null>(null);
  const [projectIsDefault, setProjectIsDefault] = useState<boolean>(true);
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

  // Convenience for "a chat turn just completed". Only the Gabriel Sequence
  // (Live State + token stats) is refreshed - the sidebar list intentionally
  // stays put. Re-sorting by updatedAt isn't worth the cost of refetching the
  // entire conversation list (and the projects list, via the same refresh
  // signal) on every turn. The active conversation is already highlighted, so
  // stale position-in-list is harmless until next mount.
  const onTurnComplete = useCallback(() => {
    bumpSequence();
  }, [bumpSequence]);

  // Chat fires this once it loads the conversation's metadata - lets us pick
  // up the avatar seed without doing a duplicate fetch. Effective seed +
  // project context drive both the rendered sequence and the Pulse palette
  // fallback. When the chat is in a real (non-default) project, the avatar
  // shown is the project's shared one; standalone chats keep their own.
  const handleConversationLoaded = useCallback((conv: ConversationResponse) => {
    const isDefault = conv.projectIsDefault ?? true;
    setProjectIsDefault(isDefault);
    setProjectId(conv.projectId ?? null);
    setAvatarSeed(conv.effectiveAvatarSeed ?? conv.avatarSeed);
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
      // No sidebar bump - the conversation list doesn't display avatars, so
      // refetching it on every reroll is wasted work.
      bumpSequence();
    } catch (e: unknown) {
      notifyError(e);
    }
  }, [conversationId, bumpSequence]);

  // If the conversation comes back 404 (e.g. deleted on another tab), Chat
  // surfaces an error toast. We also bounce the user back to /, which will
  // either find another stored conv or create a fresh one.
  const handleConversationMissing = useCallback(() => {
    persistActiveConversation(null);
    navigate('/', { replace: true });
  }, [navigate]);

  // Active palette stops, in priority order:
  //   1) The server-driven Gabriel Sequence palette - canonical visual
  //      identity once the avatar finishes its first fetch.
  //   2) The seed-derived pulse palette - fallback during the gap.
  const activeStops = useMemo<readonly RGB[]>(
    () => sequenceStops ?? paletteForSeed(avatarSeed).stops,
    [sequenceStops, avatarSeed],
  );
  const paletteVars = useMemo(
    () => paletteVarsFromStops(activeStops) as React.CSSProperties,
    [activeStops],
  );

  // The sequence source flips per chat: non-default project → project's
  // shared sequence; otherwise this chat's own. Built as a memoized object so
  // the view's effect only re-fires when the underlying id/kind actually
  // changes, not on every render.
  const sequenceSource = useMemo<SequenceSource>(
    () => !projectIsDefault && projectId
      ? { kind: 'project', projectId }
      : { kind: 'conversation', conversationId },
    [projectIsDefault, projectId, conversationId],
  );

  if (!conversationId) {
    // useParams' default guarantees a string but the router should never let
    // us reach this branch - defensive guard so TS knows conversationId is
    // non-empty downstream.
    return null;
  }

  return (
    <div style={paletteVars} className="palette-scope">
      <div className={`avatar-wrap${thinking ? ' thinking' : ''}`}>
        <GabrielSequenceView
          // Remount on source switch so the animation timer resets when the
          // user navigates between a standalone chat and a project chat.
          key={`${sequenceSource.kind}:${sequenceSource.kind === 'conversation' ? sequenceSource.conversationId : sequenceSource.projectId}`}
          source={sequenceSource}
          refreshKey={sequenceRefresh}
          onSequenceLoaded={handleSequenceLoaded}
        />
        {/* Reroll is only meaningful for standalone (Default-project) chats -
            for real projects the avatar is the project's shared identity and
            rerolling lives on the Project Settings page (changing it here
            would silently re-skin every chat in the project). */}
        {projectIsDefault && (
          <button
            type="button"
            className="reroll"
            onClick={() => void rerollAvatar()}
            aria-label="Reroll avatar"
            title="Reroll avatar"
          >
            <HiOutlineArrowPath aria-hidden="true" />
          </button>
        )}
      </div>

      {/* Context-window usage strip - shares sequenceRefresh so it updates
          after every chat turn alongside the Gabriel Sequence Live State. */}
      <ContextStats
        conversationId={conversationId}
        refreshKey={sequenceRefresh}
      />

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
