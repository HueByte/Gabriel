import { useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { ConversationsService } from '../api/generated';
import { notifyError } from '../lib/notify';

const ACTIVE_CONVERSATION_KEY = 'gabriel.activeConversationId';

function loadLastConversation(): string | null {
  try { return localStorage.getItem(ACTIVE_CONVERSATION_KEY); } catch { return null; }
}

// Boot page for "/". Picks up where the user left off - restoring the
// stored last-active conversation if one exists, otherwise creating a fresh
// one. Either way we end up at /c/{id} so the URL is the source of truth
// from that point on. Strict-mode-safe via the ranBootRef sentinel: the
// effect can run twice in dev, but we only ever start one redirect.
export function IndexPage() {
  const navigate = useNavigate();
  const ranBootRef = useRef(false);

  useEffect(() => {
    if (ranBootRef.current) return;
    ranBootRef.current = true;

    const last = loadLastConversation();
    if (last) {
      navigate(`/c/${encodeURIComponent(last)}`, { replace: true });
      return;
    }

    // Title intentionally null - backend defaults to the conversation's GUID
    // so each new chat has a unique, distinguishable identifier out of the
    // box. The user (or a future auto-titler) can rename via PATCH.
    ConversationsService.postApiConversations({ requestBody: { title: null } })
      .then(conv => {
        navigate(`/c/${encodeURIComponent(conv.id)}`, { replace: true });
      })
      .catch(notifyError);
  }, [navigate]);

  return <div className="auth-loading">Loading…</div>;
}
