import { cookies } from "next/headers"
import { jwtVerify } from "jose"
import { NextRequest, NextResponse } from "next/server"

const secretKey = process.env.SESSION_SECRET
const encodedKey = new TextEncoder().encode(secretKey)

async function getJwtFromCookies() {
  const cookieStore = await cookies()
  const allCookies = cookieStore.getAll()
  console.log("All cookies:", allCookies.map(c => c.name))
  
  const session = cookieStore.get("session")?.value
  console.log("Session cookie present:", !!session)
  console.log("Session cookie length:", session?.length || 0)
  
  if (!session) return null
  
  try {
    const { payload } = await jwtVerify(session, encodedKey, { algorithms: ["HS256"] })
    console.log("JWT decoded successfully, has apiRootJwt:", !!payload.apiRootJwt)
    return payload.apiRootJwt as string
  } catch (err) {
    console.error("JWT decode error:", err)
    return null
  }
}

export async function PUT(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  console.log("PUT /api/servicerequest/[id]/status called")
  console.log("Request headers:", Object.fromEntries(request.headers.entries()))
  
  try {
    const jwt = await getJwtFromCookies()
    if (!jwt) {
      console.error("No JWT available")
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 })
    }

    const { id } = await params
    const body = await request.json()
    console.log("Updating status for:", id, "to:", body.status)
    
    const backendUrl = `${process.env.API_URL}/api/servicerequest/${id}/status`
    console.log("Backend URL:", backendUrl)
    
    const response = await fetch(backendUrl, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${jwt}`
      },
      body: JSON.stringify(body)
    })

    console.log("Backend response status:", response.status)

    if (!response.ok) {
      const text = await response.text()
      console.error("Backend error:", response.status, text)
      return NextResponse.json({ error: "Backend error" }, { status: response.status })
    }

    return NextResponse.json({ success: true })
  } catch (error) {
    console.error("Failed to update status:", error)
    return NextResponse.json({ error: "Failed to update status" }, { status: 500 })
  }
}
