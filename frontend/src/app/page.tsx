import { Navigation } from "./_components/landing/Navigation"
import { HeroSection, ServicesSection, AboutSection, TipsSection, GallerySection, ContactSection, Footer } from "./_components/landing/Sections"
import { LandingThemeProvider } from "@/_components/ThemeProvider"
import { sanitizeColor } from "@/_lib/colorValidator"
import { IPublicLandingData } from "./home/settings/branding/model"
import { headers } from "next/headers"

// Extract tenant ID from hostname for multi-tenant routing
async function getTenantIdFromHost(): Promise<string | null> {
    const headersList = await headers();
    const host = headersList.get('host');
    if (!host) return null;

    const parts = host.split('.');
    if (parts.length >= 2) {
        const tenantId = parts[0];
        // Skip common subdomains that aren't tenant IDs
        if (tenantId && tenantId !== 'www' && tenantId !== 'api' && tenantId !== 'localhost') {
            return tenantId;
        }
    }
    return null;
}

async function getLandingData(): Promise<IPublicLandingData | null> {
    try {
        const requestHeaders: HeadersInit = {
            'Content-Type': 'application/json',
        };

        // Add tenant ID header for multi-tenant routing
        const tenantId = await getTenantIdFromHost();
        if (tenantId) {
            requestHeaders['X-Tenant-ID'] = tenantId;
        }

        const response = await fetch(`${process.env.API_URL}/api/publiclanding`, {
            cache: 'no-store',
            headers: requestHeaders,
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
                    <p className="text-slate-400 mb-6">Landing page content is being configured.</p>
                    <a
                        href="/auth/login"
                        className="inline-flex items-center px-6 py-3 bg-purple-600 hover:bg-purple-700 text-white font-medium rounded-lg transition-colors"
                    >
                        Sign In
                    </a>
                </div>
            </div>
        );
    }

    // Generate inline CSS for immediate color application (prevents flash).
    // SECURITY: these colors come from tenant branding and are injected into a
    // <style> block, so every value MUST be validated. sanitizeColor() returns
    // the value only if it is a well-formed color, otherwise undefined — this
    // prevents breaking out of the <style> with markup/script (stored XSS).
    const landingColors = data.branding.landingColors;
    const cssVars: Array<[string, string | undefined]> = landingColors ? [
        ["--landing-primary", sanitizeColor(landingColors.primaryColor)],
        ["--landing-secondary", sanitizeColor(landingColors.secondaryColor)],
        ["--landing-accent", sanitizeColor(landingColors.accentColor)],
        ["--landing-header-bg", sanitizeColor(landingColors.headerBg)],
        ["--landing-footer-bg", sanitizeColor(landingColors.footerBg)],
    ] : [];
    const cssBody = cssVars
        .filter((entry): entry is [string, string] => Boolean(entry[1]))
        .map(([name, value]) => `${name}: ${value};`)
        .join("\n            ");
    const themeStyles = cssBody ? `
        :root {
            ${cssBody}
        }
    ` : '';

    return (
        <>
            {/* Inline styles to prevent color flash on page load */}
            {themeStyles && <style dangerouslySetInnerHTML={{ __html: themeStyles }} />}
            <LandingThemeProvider colors={data.branding.landingColors}>
                <Navigation data={data} />
                <main>
                    <HeroSection data={data} />
                    <ServicesSection data={data} />
                    <AboutSection data={data} />
                    <TipsSection data={data} />
                    <GallerySection data={data} />
                    <ContactSection data={data} />
                </main>
                <Footer data={data} />
            </LandingThemeProvider>
        </>
    )
}
