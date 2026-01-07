import { NextResponse } from 'next/server';

export async function GET() {
    try {
        const response = await fetch(`${process.env.API_URL}/api/publiclanding`, {
            cache: 'no-store', // TODO: Add caching strategy for production
        });

        if (!response.ok) {
            return NextResponse.json(
                { error: 'Failed to fetch landing page data' },
                { status: response.status }
            );
        }

        const data = await response.json();
        return NextResponse.json(data);
    } catch (error) {
        console.error('Error fetching public landing data:', error);
        return NextResponse.json(
            { error: 'Internal server error' },
            { status: 500 }
        );
    }
}
