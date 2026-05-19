// Client for /api/memories. The settings UI uses this directly; the chat
// "Remember this" button also goes through Save() with a pre-filled body.
//
// Hand-written rather than depending on openapi-typescript-codegen so the
// 401-refresh-and-retry path stays shared with the rest of the client.

import { refreshSession, signalSessionExpired } from './authRefresh';

export type MemoryType = 'user' | 'feedback' | 'project' | 'reference';

export interface MemoryDto {
  id: string;
  projectId: string | null;   // null = user-scope
  type: MemoryType;
  name: string;
  description: string;
  body: string;
  createdAt: string;          // ISO 8601
  updatedAt: string;
}

export interface SaveMemoryRequest {
  projectId: string | null;
  type: MemoryType;
  name: string;
  description: string;
  body: string;
}

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

// Three scopes the caller can ask for:
//   { kind: 'user' }              — user-scope only (projectId omitted)
//   { kind: 'project', projectId } — that one project's memories
//   { kind: 'all', projectId? }   — what the agent sees: user-scope merged with
//                                    the project's entries (projectId optional)
export type MemoryScope =
  | { kind: 'user' }
  | { kind: 'project'; projectId: string }
  | { kind: 'all'; projectId?: string };

function urlFor(scope: MemoryScope): string {
  const params = new URLSearchParams();
  if (scope.kind === 'all') {
    params.set('scope', 'all');
    if (scope.projectId) params.set('projectId', scope.projectId);
  } else if (scope.kind === 'project') {
    params.set('projectId', scope.projectId);
  }
  const qs = params.toString();
  return qs ? `/api/memories?${qs}` : '/api/memories';
}

export async function listMemories(scope: MemoryScope, signal?: AbortSignal): Promise<MemoryDto[]> {
  const response = await withRefresh(
    () => fetch(urlFor(scope), { credentials: 'include', signal }),
  );
  if (!response.ok) {
    throw new Error(`Memories fetch failed: ${response.status} ${response.statusText}`);
  }
  return (await response.json()) as MemoryDto[];
}

export async function saveMemory(
  request: SaveMemoryRequest,
  signal?: AbortSignal,
): Promise<MemoryDto> {
  const response = await withRefresh(
    () => fetch('/api/memories', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(request),
      signal,
    }),
  );
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(`Save memory failed: ${response.status} ${text}`);
  }
  return (await response.json()) as MemoryDto;
}

export async function deleteMemory(id: string, signal?: AbortSignal): Promise<void> {
  const response = await withRefresh(
    () => fetch(`/api/memories/${encodeURIComponent(id)}`, {
      method: 'DELETE',
      credentials: 'include',
      signal,
    }),
  );
  if (!response.ok && response.status !== 404) {
    throw new Error(`Delete memory failed: ${response.status} ${response.statusText}`);
  }
}
