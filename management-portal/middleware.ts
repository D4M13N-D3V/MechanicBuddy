import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { verifySession } from "@/_lib/session-crypto";

const SESSION_COOKIE_NAME = "admin_session";

// Routes that don't require authentication
const publicRoutes = ["/login", "/register", "/forgot-password", "/demo", "/pricing", "/"];

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Allow public routes
  if (publicRoutes.some((route) => pathname === route || pathname.startsWith("/api/public"))) {
    return NextResponse.next();
  }

  // Allow static files and Next.js internals
  if (
    pathname.startsWith("/_next") ||
    pathname.startsWith("/favicon") ||
    pathname.includes(".")
  ) {
    return NextResponse.next();
  }

  // Check for session cookie
  const sessionCookie = request.cookies.get(SESSION_COOKIE_NAME);

  if (!sessionCookie?.value) {
    // No session - redirect to login
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("redirect", pathname);
    return NextResponse.redirect(loginUrl);
  }

  // Verify the signature before trusting the session contents.
  const session = await verifySession(sessionCookie.value);
  if (!session || new Date(session.expiresAt) < new Date()) {
    // Missing/forged/expired session - clear cookie and redirect
    const response = NextResponse.redirect(new URL("/login", request.url));
    response.cookies.delete(SESSION_COOKIE_NAME);
    return response;
  }

  // Valid, unexpired session - allow request
  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Match all request paths except:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public files (public folder)
     */
    "/((?!_next/static|_next/image|favicon.ico|.*\\..*|api/public).*)",
  ],
};
