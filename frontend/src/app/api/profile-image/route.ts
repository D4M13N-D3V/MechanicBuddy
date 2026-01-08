import { NextResponse } from 'next/server';
import { cookies } from 'next/headers';

const API_URL = process.env.API_URL || 'http://localhost:15567';

/**
 * Security: Fetches profile image using httpOnly JWT cookie
 * This prevents exposing the JWT in URLs
 */
export async function GET() {
  try {
    const cookieStore = await cookies();
    const jwt = cookieStore.get('jwt')?.value;

    if (!jwt) {
      // Return a default/empty image if not authenticated
      return new NextResponse(null, { status: 401 });
    }

    // Fetch profile picture from backend using the JWT
    const response = await fetch(`${API_URL}/api/users/profilepicture/${jwt}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${jwt}`,
      },
    });

    if (!response.ok) {
      return new NextResponse(null, { status: response.status });
    }

    const imageData = await response.arrayBuffer();
    const contentType = response.headers.get('content-type') || 'image/jpeg';

    return new NextResponse(imageData, {
      status: 200,
      headers: {
        'Content-Type': contentType,
        'Cache-Control': 'private, max-age=3600', // Cache for 1 hour
      },
    });
  } catch (error) {
    console.error('Error fetching profile image:', error);
    return new NextResponse(null, { status: 500 });
  }
}
