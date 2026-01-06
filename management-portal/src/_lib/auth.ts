"use server";

import { cookies } from "next/headers";
import type { AdminUser, LoginCredentials, AuthSession } from "@/types";

const API_URL = process.env.MANAGEMENT_API_URL || "http://localhost:15568";
const SESSION_COOKIE_NAME = "admin_session";
const SESSION_DURATION = 24 * 60 * 60 * 1000; // 24 hours

/**
 * Login with email and password via Management API
 */
export async function login(credentials: LoginCredentials): Promise<{ success: boolean; error?: string }> {
  try {
    const response = await fetch(`${API_URL}/api/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        email: credentials.email,
        password: credentials.password,
      }),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: "Login failed" }));
      return { success: false, error: error.message || "Invalid credentials" };
    }

    const data = await response.json();

    const session: AuthSession = {
      user: {
        id: data.admin.id.toString(),
        email: data.admin.email,
        name: data.admin.name,
        role: data.admin.role,
        createdAt: new Date().toISOString(),
      },
      token: data.token,
      expiresAt: new Date(Date.now() + SESSION_DURATION).toISOString(),
    };

    // Set session cookie
    const cookieStore = await cookies();
    cookieStore.set(SESSION_COOKIE_NAME, JSON.stringify(session), {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      sameSite: "lax",
      maxAge: SESSION_DURATION / 1000,
      path: "/",
    });

    return { success: true };
  } catch (error) {
    console.error("Login error:", error);
    return { success: false, error: "Unable to connect to server" };
  }
}

/**
 * Logout and clear session
 */
export async function logout(): Promise<void> {
  const cookieStore = await cookies();
  cookieStore.delete(SESSION_COOKIE_NAME);
}

/**
 * Get current session
 */
export async function getSession(): Promise<AuthSession | null> {
  try {
    const cookieStore = await cookies();
    const sessionCookie = cookieStore.get(SESSION_COOKIE_NAME);

    if (!sessionCookie?.value) {
      return null;
    }

    const session: AuthSession = JSON.parse(sessionCookie.value);

    // Check if session is expired
    if (new Date(session.expiresAt) < new Date()) {
      await logout();
      return null;
    }

    return session;
  } catch (error) {
    console.error("Get session error:", error);
    return null;
  }
}

/**
 * Get the auth token for API calls
 */
export async function getAuthToken(): Promise<string | null> {
  const session = await getSession();
  return session?.token ?? null;
}

/**
 * Get current admin user
 */
export async function getCurrentUser(): Promise<AdminUser | null> {
  const session = await getSession();
  return session?.user ?? null;
}

/**
 * Check if user is authenticated
 */
export async function isAuthenticated(): Promise<boolean> {
  const session = await getSession();
  return session !== null;
}

/**
 * Require authentication (use in server components)
 */
export async function requireAuth(): Promise<AdminUser> {
  const user = await getCurrentUser();
  if (!user) {
    throw new Error("Unauthorized");
  }
  return user;
}
