// Client for the Gabriel Sequence endpoints — the 64-frame, 16×16
// palette-indexed RGB representation of an entity's personality + current
// emotional state. Two scopes:
//   - GET /api/conversations/{id}/sequence — per-conversation, used for
//     standalone chats (chats in the Default project).
//   - GET /api/projects/{id}/sequence — project-shared, used for chats
//     inside a user-created project where every chat shares one avatar.
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

// Discriminated source so callers can't mix the scope up at runtime — the
// view component takes one of these and forwards it through to the fetch.
export type SequenceSource =
  | { kind: 'conversation'; conversationId: string }
  | { kind: 'project'; projectId: string };

function urlFor(source: SequenceSource): string {
  return source.kind === 'conversation'
    ? `/api/conversations/${encodeURIComponent(source.conversationId)}/sequence`
    : `/api/projects/${encodeURIComponent(source.projectId)}/sequence`;
}

function doFetch(source: SequenceSource, signal?: AbortSignal): Promise<Response> {
  return fetch(urlFor(source), { credentials: 'include', signal });
}

export async function fetchGabrielSequence(
  source: SequenceSource,
  signal?: AbortSignal,
): Promise<GabrielSequence> {
  let response = await doFetch(source, signal);

  if (response.status === 401) {
    const refreshed = await refreshSession();
    if (refreshed) response = await doFetch(source, signal);
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
