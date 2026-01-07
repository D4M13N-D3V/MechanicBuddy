'use client';

import { useEffect } from 'react';

interface PortalColors {
  sidebarBg?: string;
  sidebarText?: string;
  sidebarActiveBg?: string;
  sidebarActiveText?: string;
  accentColor?: string;
  contentBg?: string;
}

interface LandingColors {
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  headerBg?: string;
  footerBg?: string;
}

interface PortalThemeProviderProps {
  colors?: PortalColors;
  children: React.ReactNode;
}

interface LandingThemeProviderProps {
  colors?: LandingColors;
  children: React.ReactNode;
}

/**
 * PortalThemeProvider - Applies custom colors to the admin portal
 * Wraps the admin layout to inject CSS variables at runtime
 */
export function PortalThemeProvider({ colors, children }: PortalThemeProviderProps) {
  useEffect(() => {
    if (colors) {
      const root = document.documentElement;

      if (colors.sidebarBg) {
        root.style.setProperty('--portal-sidebar-bg', colors.sidebarBg);
      }
      if (colors.sidebarText) {
        root.style.setProperty('--portal-sidebar-text', colors.sidebarText);
      }
      if (colors.sidebarActiveBg) {
        root.style.setProperty('--portal-sidebar-active-bg', colors.sidebarActiveBg);
      }
      if (colors.sidebarActiveText) {
        root.style.setProperty('--portal-sidebar-active-text', colors.sidebarActiveText);
      }
      if (colors.accentColor) {
        root.style.setProperty('--portal-accent', colors.accentColor);
      }
      if (colors.contentBg) {
        root.style.setProperty('--portal-content-bg', colors.contentBg);
      }
    }
  }, [colors]);

  return <>{children}</>;
}

/**
 * LandingThemeProvider - Applies custom colors to the landing page
 * Wraps the landing page to inject CSS variables at runtime
 */
export function LandingThemeProvider({ colors, children }: LandingThemeProviderProps) {
  useEffect(() => {
    if (colors) {
      const root = document.documentElement;

      if (colors.primaryColor) {
        root.style.setProperty('--landing-primary', colors.primaryColor);
      }
      if (colors.secondaryColor) {
        root.style.setProperty('--landing-secondary', colors.secondaryColor);
      }
      if (colors.accentColor) {
        root.style.setProperty('--landing-accent', colors.accentColor);
      }
      if (colors.headerBg) {
        root.style.setProperty('--landing-header-bg', colors.headerBg);
      }
      if (colors.footerBg) {
        root.style.setProperty('--landing-footer-bg', colors.footerBg);
      }
    }
  }, [colors]);

  return <>{children}</>;
}

/**
 * ThemeScript - Inline script to prevent flash of default colors
 * Include this in the document head for immediate color application
 */
export function ThemeScript({ portalColors, landingColors }: {
  portalColors?: PortalColors;
  landingColors?: LandingColors;
}) {
  const script = `
    (function() {
      var root = document.documentElement;
      ${portalColors?.sidebarBg ? `root.style.setProperty('--portal-sidebar-bg', '${portalColors.sidebarBg}');` : ''}
      ${portalColors?.sidebarText ? `root.style.setProperty('--portal-sidebar-text', '${portalColors.sidebarText}');` : ''}
      ${portalColors?.sidebarActiveBg ? `root.style.setProperty('--portal-sidebar-active-bg', '${portalColors.sidebarActiveBg}');` : ''}
      ${portalColors?.sidebarActiveText ? `root.style.setProperty('--portal-sidebar-active-text', '${portalColors.sidebarActiveText}');` : ''}
      ${portalColors?.accentColor ? `root.style.setProperty('--portal-accent', '${portalColors.accentColor}');` : ''}
      ${portalColors?.contentBg ? `root.style.setProperty('--portal-content-bg', '${portalColors.contentBg}');` : ''}
      ${landingColors?.primaryColor ? `root.style.setProperty('--landing-primary', '${landingColors.primaryColor}');` : ''}
      ${landingColors?.secondaryColor ? `root.style.setProperty('--landing-secondary', '${landingColors.secondaryColor}');` : ''}
      ${landingColors?.accentColor ? `root.style.setProperty('--landing-accent', '${landingColors.accentColor}');` : ''}
      ${landingColors?.headerBg ? `root.style.setProperty('--landing-header-bg', '${landingColors.headerBg}');` : ''}
      ${landingColors?.footerBg ? `root.style.setProperty('--landing-footer-bg', '${landingColors.footerBg}');` : ''}
    })();
  `;

  return <script dangerouslySetInnerHTML={{ __html: script }} />;
}
