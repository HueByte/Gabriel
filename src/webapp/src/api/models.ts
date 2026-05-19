// Client for /api/models — the model-selection surface.
//   GET  /api/models          — catalog + the user's current selection
//   PUT  /api/models/active   — persist a pick (or clear it with null/null)
//
// Hand-written for the same reasons sequence.ts is: we share the
// 401-refresh-and-retry path and we don't want the consumer to wait on
// openapi-typescript-codegen regenerating before the page can compile.

import { refreshSession, signalSessionExpired } from './authRefresh';

export interface ModelDto {
  provider: string;
  name: string;
  contextWindowTokens: number;
  inputPricePerMTokens: number;
  outputPricePerMTokens: number;
  cacheReadPricePerMTokens: number;
  cacheWritePricePerMTokens: number;
  isDefault: boolean;
  isSelected: boolean;
}

export interface SelectedModelDto {
  provider: string;
  name: string;
}

export interface ModelsResponse {
  availableModels: ModelDto[];
  selected: SelectedModelDto;
}

export interface SetActiveModelRequest {
  provider: string | null;
  name: string | null;
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

export async function fetchModels(signal?: AbortSignal): Promise<ModelsResponse> {
  const response = await withRefresh(
    () => fetch('/api/models', { credentials: 'include', signal }),
  );
  if (!response.ok) {
    throw new Error(`Models fetch failed: ${response.status} ${response.statusText}`);
  }
  return (await response.json()) as ModelsResponse;
}

export async function setActiveModel(
  request: SetActiveModelRequest,
  signal?: AbortSignal,
): Promise<ModelsResponse> {
  const response = await withRefresh(
    () => fetch('/api/models/active', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(request),
      signal,
    }),
  );
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(`Set active model failed: ${response.status} ${text}`);
  }
  return (await response.json()) as ModelsResponse;
}
