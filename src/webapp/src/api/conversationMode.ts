// Client for PUT /api/conversations/{id}/mode — sets the per-conversation
// behaviour bias (chatty / elaborative / concise / tutor / critic). Hand-
// written to share the 401-refresh path with the other custom clients;
// the typed swagger client could host this too, but the dropdown is a tiny
// surface and the refresh helper isn't easily composable with the
// auto-generated axios stack.

import { refreshSession, signalSessionExpired } from './authRefresh';

export type GabrielMode = 'chatty' | 'elaborative' | 'concise' | 'tutor' | 'critic';

export const GABRIEL_MODES: GabrielMode[] = ['chatty', 'elaborative', 'concise', 'tutor', 'critic'];

// One-line description per mode, used as the dropdown's option subtitle so
// the user can pick without remembering what each one does.
export const MODE_DESCRIPTIONS: Record<GabrielMode, string> = {
  chatty: 'Default — natural conversation, mirrored register',
  elaborative: 'Longer artifacts, comments, named trade-offs',
  concise: 'Shortest correct answer, no preamble',
  tutor: 'Step-by-step, examples-first, explain the *why*',
  critic: 'Skeptical — finds flaws before validating',
};

async function withRefresh(doFetch: () => Promise<Response>): Promise<Response> {
  let response = await doFetch();
  if (response.status === 401) {
    const refreshed = await refreshSession();
    if (refreshed) response = await doFetch();
    if (response.status === 401) {
      signalSessionExpired();
      throw new Error('Session expired. Please sign in again.');
    }
  }
  return response;
}

// Pass `mode = null` to clear the bias and revert to Chatty.
export async function setConversationMode(
  conversationId: string,
  mode: GabrielMode | null,
  signal?: AbortSignal,
): Promise<void> {
  const response = await withRefresh(
    () => fetch(`/api/conversations/${encodeURIComponent(conversationId)}/mode`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ mode }),
      signal,
    }),
  );
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(`Set conversation mode failed: ${response.status} ${text}`);
  }
}
