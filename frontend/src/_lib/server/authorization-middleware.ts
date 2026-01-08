
import { NextResponse, NextRequest } from 'next/server'
import { deleteSession, getJwt, getMustChangePassword } from '@/_lib/server/session'
export default async function authorizationMiddleware(request: NextRequest,response: NextResponse) {

   // 2. Check if the current route is protected or public
   const path = request.nextUrl.pathname
   const isProtectedRoute = path.startsWith('/home');
   const isChangePasswordRoute = path === '/auth/change-password';
    // 3. Decrypt the session from the cookie

   // logout if /home/logout is called and redirect to login page
  if (path.includes("/home/logout")) {
    await deleteSession();
    return NextResponse.redirect(new URL('/auth/login', request.url))
  }

  const jwt = await getJwt();
  // 4. Redirect to /login if the user is not authenticated
  if (isProtectedRoute && !jwt) {
    return NextResponse.redirect(new URL('/auth/login', request.nextUrl))
  }

  // Check if user must change password
  const mustChangePassword = await getMustChangePassword();

  // If user must change password and is not on the change password page
  if (jwt && mustChangePassword && !isChangePasswordRoute) {
    return NextResponse.redirect(new URL('/auth/change-password', request.nextUrl))
  }

  // If user doesn't need to change password but is on the change password page, redirect to home
  if (jwt && !mustChangePassword && isChangePasswordRoute) {
    return NextResponse.redirect(new URL('/home/work', request.nextUrl))
  }

  // 5. Redirect to /home if the user is authenticated (skip in development to allow viewing landing page)
  const isDevMode = process.env.NODE_ENV === 'development'
  const isLandingPage = path === '/'
  const isAuthRoute = path.startsWith('/auth/login');
  if (
    !isProtectedRoute && !isChangePasswordRoute && !isAuthRoute && jwt && !(isDevMode && isLandingPage)
  ) {
    return NextResponse.redirect(new URL('/home/work', request.nextUrl))
  }

  return response
}
 