'use client'

import { Container } from "@/_components/layout/Container"
import Link from "next/link"
import { useState } from "react"
import { PhoneIcon, MapPinIcon, ClockIcon } from "@heroicons/react/24/outline"
import { IPublicLandingData } from "@/app/home/settings/branding/model"
import SocialIcons from "./SocialIcons"

interface NavigationProps {
    data: IPublicLandingData;
}

export function Navigation({ data }: NavigationProps) {
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
    const { branding, content, companyInfo } = data;
    const headerSocialLinks = (content.socialLinks || []).filter(l => l.isActive && l.showInHeader).sort((a, b) => a.sortOrder - b.sortOrder);

    const navLinks = [
        { name: "Services", href: "#services" },
        { name: "About Us", href: "#about" },
        ...(content.tipsSection.isVisible && content.sectionVisibility?.tipsVisible ? [{ name: "Auto Tips", href: "#tips" }] : []),
        ...(content.sectionVisibility?.galleryVisible && content.galleryPhotos?.length > 0 ? [{ name: "Gallery", href: "#gallery" }] : []),
        { name: "Contact", href: "#contact" },
    ]

    return (
        <header className="bg-landing-header-bg text-white">
            <div className="py-2 bg-landing-primary">
                <Container>
                    <div className="flex flex-wrap justify-between items-center text-sm">
                        <div className="flex items-center gap-6">
                            {companyInfo.phone && (
                                <a href={`tel:${companyInfo.phone}`} className="flex items-center gap-2 hover:text-slate-200 transition-colors">
                                    <PhoneIcon className="h-4 w-4" />
                                    <span>{companyInfo.phone}</span>
                                </a>
                            )}
                            {companyInfo.address && (
                                <span className="hidden sm:flex items-center gap-2">
                                    <MapPinIcon className="h-4 w-4" />
                                    <span>{companyInfo.address}</span>
                                </span>
                            )}
                        </div>
                        <div className="flex items-center gap-4">
                            {content.contact.businessHours.length > 0 && (
                                <span className="hidden md:flex items-center gap-2">
                                    <ClockIcon className="h-4 w-4" />
                                    <span>
                                        {content.contact.businessHours
                                            .filter(h => h.open !== 'Closed')
                                            .slice(0, 2)
                                            .map(h => `${h.day.slice(0, 3)}: ${h.open} - ${h.close}`)
                                            .join(' | ')}
                                    </span>
                                </span>
                            )}
                            {headerSocialLinks.length > 0 && (
                                <SocialIcons links={headerSocialLinks} className="hidden sm:flex" />
                            )}
                        </div>
                    </div>
                </Container>
            </div>

            <Container>
                <nav className="relative flex justify-between items-center py-4">
                    <Link href="/" className="flex items-center gap-3">
                        {branding.logoBase64 ? (
                            <img
                                src={`data:${branding.logoMimeType};base64,${branding.logoBase64}`}
                                alt={content.hero.companyName}
                                className="h-10 w-auto"
                            />
                        ) : (
                            <span className="text-xl font-bold tracking-tight">{content.hero.companyName}</span>
                        )}
                    </Link>

                    <div className="hidden md:flex items-center gap-6">
                        {navLinks.map((link) => (
                            <a
                                key={link.name}
                                href={link.href}
                                className="text-sm font-medium transition-colors hover:opacity-80 px-2"
                            >
                                {link.name}
                            </a>
                        ))}
                        <Link
                            href="/auth/login"
                            className="ml-2 px-5 py-2.5 rounded-lg text-sm font-semibold transition-all shadow-lg hover:opacity-90 bg-landing-secondary"
                        >
                            Mechanic Portal
                        </Link>
                    </div>

                    {/* Mobile menu button */}
                    <button
                        onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                        className="md:hidden p-2 hover:bg-slate-800 rounded-lg transition-colors"
                        aria-label="Toggle menu"
                    >
                        {mobileMenuOpen ? (
                            <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        ) : (
                            <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                            </svg>
                        )}
                    </button>

                    {/* Mobile menu panel */}
                    {mobileMenuOpen && (
                        <>
                            <div
                                className="fixed inset-0 z-40 bg-black/50"
                                onClick={() => setMobileMenuOpen(false)}
                            />
                            <div className="absolute top-full left-0 right-0 z-50 mt-2 mx-4 bg-slate-800 rounded-xl shadow-xl p-4">
                                <div className="flex flex-col gap-2">
                                    {navLinks.map((link) => (
                                        <a
                                            key={link.name}
                                            href={link.href}
                                            onClick={() => setMobileMenuOpen(false)}
                                            className="text-base font-medium py-3 px-4 rounded-lg hover:bg-slate-700 transition-colors"
                                        >
                                            {link.name}
                                        </a>
                                    ))}
                                    <Link
                                        href="/auth/login"
                                        onClick={() => setMobileMenuOpen(false)}
                                        className="mt-2 px-5 py-3 rounded-lg text-base font-semibold text-center transition-all shadow-lg hover:opacity-90 bg-landing-secondary"
                                    >
                                        Mechanic Portal
                                    </Link>
                                </div>
                            </div>
                        </>
                    )}
                </nav>
            </Container>
        </header>
    )
}
