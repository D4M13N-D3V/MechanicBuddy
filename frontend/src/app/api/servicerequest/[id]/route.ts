import { cookies } from "next/headers"
import { jwtVerify } from "jose"
import { NextRequest, NextResponse } from "next/server"

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

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  try {
    const jwt = await getJwtFromCookies()
    if (!jwt) {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 })
    }

    const { id } = await params
    
    const response = await fetch(`${process.env.API_URL}/api/servicerequest/${id}`, {
      method: "DELETE",
      headers: {
        "Authorization": `Bearer ${jwt}`
      }
    })

    if (!response.ok) {
      const text = await response.text()
      console.error("Backend error:", response.status, text)
      return NextResponse.json({ error: "Backend error" }, { status: response.status })
    }

    return NextResponse.json({ success: true })
  } catch (error) {
    console.error("Failed to delete:", error)
    return NextResponse.json({ error: "Failed to delete" }, { status: 500 })
  }
}
