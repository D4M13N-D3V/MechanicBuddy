import type { AuthSession } from "@/types";

// HMAC-signed session cookie. Uses the Web Crypto API so it works in both the
// Edge middleware and Node server actions, with no extra dependency.
//
// Format: base64url(payloadJson) + "." + base64url(hmacSha256(payloadJson))
// The signature is verified before the payload is trusted, so a client cannot
// forge role/expiry by editing the cookie.

const encoder = new TextEncoder();
const decoder = new TextDecoder();

function bytesToBase64Url(bytes: Uint8Array): string {
  let binary = "";
  for (let i = 0; i < bytes.length; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function base64UrlToBytes(value: string): Uint8Array {
  let normalized = value.replace(/-/g, "+").replace(/_/g, "/");
  while (normalized.length % 4 !== 0) {
    normalized += "=";
  }
  const binary = atob(normalized);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
}

async function getKey(): Promise<CryptoKey> {
  const secret = process.env.SESSION_SECRET;
  if (!secret) {
    throw new Error("SESSION_SECRET is not configured");
  }
  return crypto.subtle.importKey(
    "raw",
    encoder.encode(secret),
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign", "verify"],
  );
}

/** Returns a signed, tamper-evident cookie value for the given session. */
export async function signSession(session: AuthSession): Promise<string> {
  const payloadBytes = encoder.encode(JSON.stringify(session));
  const key = await getKey();
  const signature = new Uint8Array(await crypto.subtle.sign("HMAC", key, payloadBytes));
  return `${bytesToBase64Url(payloadBytes)}.${bytesToBase64Url(signature)}`;
}

/** Verifies the signed cookie and returns the session, or null if invalid. */
export async function verifySession(value: string | undefined | null): Promise<AuthSession | null> {
  if (!value) return null;
  const dot = value.indexOf(".");
  if (dot <= 0 || dot === value.length - 1) return null;

  const payloadB64 = value.slice(0, dot);
  const signatureB64 = value.slice(dot + 1);

  try {
    const payloadBytes = base64UrlToBytes(payloadB64);
    const signatureBytes = base64UrlToBytes(signatureB64);
    const key = await getKey();
    const valid = await crypto.subtle.verify("HMAC", key, signatureBytes, payloadBytes);
    if (!valid) return null;
    return JSON.parse(decoder.decode(payloadBytes)) as AuthSession;
  } catch {
    return null;
  }
}
