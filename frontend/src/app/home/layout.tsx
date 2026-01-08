'use server'


import { cookies } from 'next/headers';
import Nav from './_components/layout/Nav'
import NavDialog from './_components/layout/NavDialog'
import ToastMessages from '@/_components/ToastMessages'
import PortalThemeProvider from '@/_components/PortalThemeProvider'
import { redirect } from 'next/navigation';
import { jwtDecode } from 'jwt-decode';
import { httpGet } from '@/_lib/server/query-api';
import { escapeColorForScript } from '@/_lib/colorValidator';

interface CustomJwtPayload {
    FullName?: string;
}

interface IBrandingOptions {
    logoBase64: string | null
    logoMimeType: string | null
    portalColors: {
        sidebarBg: string
        sidebarText: string
        sidebarActiveBg: string
        sidebarActiveText: string
        accentColor: string
        contentBg: string
    }
}

async function getBranding(): Promise<IBrandingOptions | null> {
    try {
        const response = await httpGet('branding');
        return await response.json() as IBrandingOptions;
    } catch {
        return null;
    }
}

export default async function Layout({ children }: { children: React.ReactNode }) {

    const jwt = (await cookies()).get('jwt')?.value;

    if(!jwt) {
        redirect('/home/logout');
    }

    // Decode the JWT to get the claims
    const decodedToken = jwtDecode<CustomJwtPayload>(jwt);
    const fullName = decodedToken.FullName || ''; // Extract the FullName claim

    // If there's no full name in the token, you might want to redirect or handle it
    if(!fullName) {
        redirect('/home/logout');
    }

    // Fetch branding data
    const branding = await getBranding();

    // Use proxy path for profile picture to avoid NEXT_PUBLIC_API_URL build-time issues
    const imageUrl = `/backend-api/users/profilepicture/${jwt}`

    // Generate inline CSS for immediate color application (prevents flash)
    // Security: Validate and escape all color values to prevent CSS injection
    const portalColors = branding?.portalColors;
    const safeColors = portalColors ? {
        sidebarBg: escapeColorForScript(portalColors.sidebarBg) || '#111827',
        sidebarText: escapeColorForScript(portalColors.sidebarText) || '#ffffff',
        sidebarActiveBg: escapeColorForScript(portalColors.sidebarActiveBg) || '#1f2937',
        sidebarActiveText: escapeColorForScript(portalColors.sidebarActiveText) || '#ffffff',
        accentColor: escapeColorForScript(portalColors.accentColor) || '#3b82f6',
        contentBg: escapeColorForScript(portalColors.contentBg) || '#f9fafb',
    } : null;

    const themeStyles = safeColors ? `
        :root {
            --portal-sidebar-bg: ${safeColors.sidebarBg};
            --portal-sidebar-text: ${safeColors.sidebarText};
            --portal-sidebar-active-bg: ${safeColors.sidebarActiveBg};
            --portal-sidebar-active-text: ${safeColors.sidebarActiveText};
            --portal-accent: ${safeColors.accentColor};
            --portal-content-bg: ${safeColors.contentBg};
        }
    ` : '';

    return (
        <>
            {/* Inline styles to prevent color flash on page load */}
            {themeStyles && <style dangerouslySetInnerHTML={{ __html: themeStyles }} />}
            <PortalThemeProvider colors={branding?.portalColors || null}>
                {/* <Timeout></Timeout> */}
                <ToastMessages></ToastMessages>
                <div>
                    {/* Static sidebar for desktop */}
                    <div className="hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:w-62 lg:flex-col">
                        {/* Sidebar component, swap this element with another sidebar if you like */}
                        <div className="flex grow flex-col gap-y-5 overflow-y-auto px-6" style={{ backgroundColor: 'var(--portal-sidebar-bg, #111827)' }}>
                          <Nav imageUrl={imageUrl} fullName={fullName} onSmallScreen={false}></Nav>
                        </div>
                    </div>
                     <NavDialog imageUrl={imageUrl} fullName={fullName}></NavDialog>
                    <main style={{ backgroundColor: 'var(--portal-content-bg, #f9fafb)' }}>
                        {children}
                    </main>
                  </div>
            </PortalThemeProvider>
        </>
    )
}
