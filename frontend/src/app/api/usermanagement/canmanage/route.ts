import { cookies } from "next/headers"
import { jwtVerify } from "jose"
import { NextResponse } from "next/server"

const secretKey = process.env.SESSION_SECRET
const encodedKey = new TextEncoder().encode(secretKey)

async function getJwtFromCookies() {
  const session = (await cookies()).get("session")?.value
  if (!session) return null

  try {
    const { payload } = await jwtVerify(session, encodedKey, { algorithms: ["HS256"] })
    return payload.apiRootJwt as string
  } catch {
    return null
  }
}

export async function GET() {
  try {
    const jwt = await getJwtFromCookies()
    if (!jwt) {
      return NextResponse.json({ canManageUsers: false }, { status: 200 })
    }

    const response = await fetch(`${process.env.API_URL}/api/usermanagement/canmanage`, {
      method: "GET",
      headers: {
        "Authorization": `Bearer ${jwt}`,
        "Content-Type": "application/json"
      }
    })

    if (!response.ok) {
      console.error("Backend error checking canmanage:", response.status)
      return NextResponse.json({ canManageUsers: false }, { status: 200 })
    }

    const data = await response.json()
    return NextResponse.json(data)
  } catch (error) {
    console.error("Failed to check user management permissions:", error)
    return NextResponse.json({ canManageUsers: false }, { status: 200 })
  }
}
