"use server";

import { cookies } from "next/headers";
import type { AuthSession } from "@/types";

const API_URL = process.env.MANAGEMENT_API_URL || "http://localhost:15568";
const SESSION_COOKIE_NAME = "admin_session";
const SESSION_DURATION = 24 * 60 * 60 * 1000; // 24 hours

interface SignupData {
  email: string;
  password: string;
  name: string;
  companyName: string;
}

/**
 * Sign up for a new account with auto-provisioned free tenant
 */
export async function signup(data: SignupData): Promise<{ success: boolean; error?: string }> {
  try {
    const response = await fetch(`${API_URL}/api/signup`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        email: data.email,
        password: data.password,
        name: data.name,
        companyName: data.companyName,
      }),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: "Signup failed" }));
      return { success: false, error: error.message || "Signup failed" };
    }

    const responseData = await response.json();

    const session: AuthSession = {
      user: {
        id: responseData.user.id.toString(),
        email: responseData.user.email,
        name: responseData.user.name,
        role: responseData.user.role,
        createdAt: new Date().toISOString(),
      },
      token: responseData.token,
      expiresAt: new Date(Date.now() + SESSION_DURATION).toISOString(),
    };

    // Set session cookie for auto-login
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
    console.error("Signup error:", error);
    return { success: false, error: "Unable to connect to server" };
  }
}
