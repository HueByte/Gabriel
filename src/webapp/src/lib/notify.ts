import { toast } from 'react-toastify';
import { ApiError } from '../api/generated';

/** Extract a user-facing message from arbitrary error shapes (axios/ApiError, Error, unknown). */
export function formatError(e: unknown, fallback = 'Something went wrong.'): string {
  if (e instanceof ApiError) {
    const body = e.body as { detail?: string; title?: string } | undefined;
    return body?.detail ?? body?.title ?? e.message;
  }
  if (e instanceof Error) return e.message;
  return fallback;
}

export const notifyError = (e: unknown, fallback?: string) => toast.error(formatError(e, fallback));
export const notifySuccess = (msg: string) => toast.success(msg);
export const notifyInfo = (msg: string) => toast.info(msg);
