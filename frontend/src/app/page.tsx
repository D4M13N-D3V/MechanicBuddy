import { Navigation } from "./_components/landing/Navigation"
import { HeroSection, ServicesSection, AboutSection, TipsSection, ContactSection, Footer } from "./_components/landing/Sections"
import { LandingThemeProvider } from "@/_components/ThemeProvider"
import { IPublicLandingData } from "./home/settings/branding/model"

async function getLandingData(): Promise<IPublicLandingData | null> {
    try {
        const response = await fetch(`${process.env.API_URL}/api/publiclanding`, {
            cache: 'no-store',
        });

        if (!response.ok) {
            console.error('Failed to fetch landing data:', response.status);
            return null;
        }

        return response.json();
    } catch (error) {
        console.error('Error fetching landing data:', error);
        return null;
    }
}

export default async function Home() {
    const data = await getLandingData();

    // If no data, show a fallback or error state
    if (!data) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-slate-900 text-white">
                <div className="text-center">
                    <h1 className="text-2xl font-bold mb-4">Welcome</h1>
                    <p className="text-slate-400">Landing page content is being configured.</p>
                </div>
            </div>
        );
    }

    return (
        <LandingThemeProvider colors={data.branding.landingColors}>
            <Navigation data={data} />
            <main>
                <HeroSection data={data} />
                <ServicesSection data={data} />
                <AboutSection data={data} />
                <TipsSection data={data} />
                <ContactSection data={data} />
            </main>
            <Footer data={data} />
        </LandingThemeProvider>
    )
}
