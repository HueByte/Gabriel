// Client for GET /api/conversations/{id}/sequence — the Gabriel Sequence
// (64-frame, 16×16 palette-indexed RGB representation of the conversation's
// personality + current emotional state).
//
// Hand-written rather than depending on the openapi-typescript-codegen output
// so a) the consumer doesn't sit behind a codegen regeneration, and b) we get
// to share the 401-refresh-and-retry helper with streamChat.ts.

import { refreshSession, signalSessionExpired } from './authRefresh';

export interface GabrielSequenceMetadata {
  seed: number;
  generatedAt: string;       // ISO 8601
  stateSummary: string | null;
}

export interface GabrielSequence {
  version: number;
  palette: number[][];       // [r, g, b][]
  frames: number[][];        // 64 arrays of 256 palette indices each
  metadata: GabrielSequenceMetadata;
}

function doFetch(conversationId: string, signal?: AbortSignal): Promise<Response> {
  return fetch(
    `/api/conversations/${encodeURIComponent(conversationId)}/sequence`,
    { credentials: 'include', signal },
  );
}

export async function fetchGabrielSequence(
  conversationId: string,
  signal?: AbortSignal,
): Promise<GabrielSequence> {
  let response = await doFetch(conversationId, signal);

  if (response.status === 401) {
    const refreshed = await refreshSession();
    if (refreshed) response = await doFetch(conversationId, signal);
    if (response.status === 401) {
      signalSessionExpired();
      throw new Error('Session expired. Please sign in again.');
    }
  }

  if (!response.ok) {
    throw new Error(`Sequence fetch failed: ${response.status} ${response.statusText}`);
  }

  return (await response.json()) as GabrielSequence;
}
