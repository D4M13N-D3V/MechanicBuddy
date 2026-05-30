import type { NextRequest } from 'next/server';

/**
 * Same-origin check for CSRF protection on state-changing requests.
 *
 * These routes authenticate from the auto-sent httpOnly session cookie, so a
 * cross-site form/fetch could otherwise drive a state-changing request. We
 * require the Origin (or, as a fallback, Referer) to match the request host.
 * A state-changing request with no Origin/Referer is rejected.
 */
export function isSameOrigin(request: NextRequest): boolean {
  const host = request.headers.get('host');
  if (!host) return false;

  const origin = request.headers.get('origin');
  if (origin) {
    try {
      return new URL(origin).host === host;
    } catch {
      return false;
    }
  }

  const referer = request.headers.get('referer');
  if (referer) {
    try {
      return new URL(referer).host === host;
    } catch {
      return false;
    }
  }

  // No Origin/Referer on a state-changing request -> reject.
  return false;
}

/** Methods that do not change state and so don't need a CSRF check. */
export function isSafeMethod(method: string): boolean {
  return method === 'GET' || method === 'HEAD' || method === 'OPTIONS';
}
