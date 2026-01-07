'use client'

import { useEffect } from 'react'

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

export default function PortalThemeProvider({ colors, children }: PortalThemeProviderProps) {
    useEffect(() => {
        if (colors) {
            const root = document.documentElement
            root.style.setProperty('--portal-sidebar-bg', colors.sidebarBg)
            root.style.setProperty('--portal-sidebar-text', colors.sidebarText)
            root.style.setProperty('--portal-sidebar-active-bg', colors.sidebarActiveBg)
            root.style.setProperty('--portal-sidebar-active-text', colors.sidebarActiveText)
            root.style.setProperty('--portal-accent-color', colors.accentColor)
            root.style.setProperty('--portal-content-bg', colors.contentBg)
        }
    }, [colors])

    return <>{children}</>
}
