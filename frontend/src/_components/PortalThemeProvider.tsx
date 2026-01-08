'use client'

import { useEffect } from 'react'
import { isValidColor } from '@/_lib/colorValidator'

interface PortalColors {
    sidebarBg: string
    sidebarText: string
    sidebarActiveBg: string
    sidebarActiveText: string
    accentColor: string
    contentBg: string
}

interface PortalThemeProviderProps {
    colors: PortalColors | null
    children: React.ReactNode
}

/**
 * Security: Safely set a CSS custom property only if the value is a valid color
 */
function safeSetProperty(root: HTMLElement, property: string, value: string | undefined, fallback: string) {
    if (value && isValidColor(value)) {
        root.style.setProperty(property, value)
    } else {
        root.style.setProperty(property, fallback)
    }
}

export default function PortalThemeProvider({ colors, children }: PortalThemeProviderProps) {
    useEffect(() => {
        if (colors) {
            const root = document.documentElement
            // Security: Validate all color values before applying to prevent CSS injection
            safeSetProperty(root, '--portal-sidebar-bg', colors.sidebarBg, '#111827')
            safeSetProperty(root, '--portal-sidebar-text', colors.sidebarText, '#ffffff')
            safeSetProperty(root, '--portal-sidebar-active-bg', colors.sidebarActiveBg, '#1f2937')
            safeSetProperty(root, '--portal-sidebar-active-text', colors.sidebarActiveText, '#ffffff')
            safeSetProperty(root, '--portal-accent', colors.accentColor, '#3b82f6')
            safeSetProperty(root, '--portal-content-bg', colors.contentBg, '#f9fafb')
        }
    }, [colors])

    return <>{children}</>
}
