import { Container } from "@/_components/layout/Container"
import Link from "next/link"
import {
    WrenchScrewdriverIcon,
    PhoneIcon,
    MapPinIcon,
    ClockIcon,
    CheckCircleIcon,
    LightBulbIcon,
    CogIcon,
    TruckIcon,
    PhotoIcon,
} from "@heroicons/react/24/outline"
import { IPublicLandingData, IServiceItem, IGalleryPhotoItem } from "@/app/home/settings/branding/model"
import { ServiceRequestForm } from "./ServiceRequestForm"
import SocialIcons from "./SocialIcons"

// Dynamic icon mapping
const iconMap: { [key: string]: React.ElementType } = {
    WrenchScrewdriverIcon,
    WrenchIcon: WrenchScrewdriverIcon,
    CogIcon,
    TruckIcon,
    PhoneIcon,
    LightBulbIcon,
    CheckCircleIcon,
}

function getIcon(iconName: string): React.ElementType {
    return iconMap[iconName] || CogIcon
}

export function HeroSection({ data }: { data: IPublicLandingData }) {
    const { content } = data;
    const hero = content.hero;
    const sectionVisibility = content.sectionVisibility;

    if (!sectionVisibility?.heroVisible) return null;

    return (
        <section
            className="relative text-white py-20 lg:py-28"
            style={{
                background: hero.backgroundImageBase64
                    ? undefined
                    : `linear-gradient(135deg, #1a1a2e 0%, var(--landing-accent, #5b21b6) 50%, #1a1a2e 100%)`
            }}
        >
            {hero.backgroundImageBase64 && (
                <div
                    className="absolute inset-0 bg-cover bg-center opacity-15"
                    style={{ backgroundImage: `url(data:${hero.backgroundImageMimeType};base64,${hero.backgroundImageBase64})` }}
                />
            )}
            <div className="absolute inset-0 bg-gradient-to-b from-black/50 to-black/70" />

            <Container className="relative z-10">
                <div className="max-w-4xl mx-auto text-center">
                    <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold mb-6 leading-tight tracking-tight">
                        {hero.tagline || `Professional Auto Repair You Can Trust`}
                    </h1>
                    {hero.subtitle && (
                        <p className="text-lg md:text-xl text-slate-300 mb-4 leading-relaxed max-w-3xl mx-auto">
                            {hero.subtitle}
                        </p>
                    )}
                    {hero.specialtyText && (
                        <p className="text-base md:text-lg text-slate-400 mb-8 leading-relaxed max-w-2xl mx-auto">
                            <span className="font-semibold text-landing-secondary">{hero.specialtyText}</span>
                        </p>
                    )}
                    <div className="flex flex-wrap justify-center gap-4">
                        <a
                            href={hero.ctaPrimaryLink}
                            className="px-6 py-3 rounded-lg text-base font-semibold transition-all shadow-xl inline-flex items-center gap-2 hover:opacity-90 bg-landing-primary"
                        >
                            <WrenchScrewdriverIcon className="h-5 w-5" />
                            {hero.ctaPrimaryText}
                        </a>
                        <a
                            href={hero.ctaSecondaryLink}
                            className="px-6 py-3 rounded-lg text-base font-semibold transition-all shadow-xl inline-flex items-center gap-2 hover:opacity-90 bg-landing-secondary"
                        >
                            <PhoneIcon className="h-5 w-5" />
                            {hero.ctaSecondaryText}
                        </a>
                    </div>
                </div>
            </Container>
        </section>
    )
}

function ServiceCard({ service }: { service: IServiceItem }) {
    const Icon = getIcon(service.iconName)
    return (
        <div className="bg-white rounded-2xl p-6 shadow-lg hover:shadow-xl transition-all duration-300 group">
            <div
                className={`w-14 h-14 rounded-xl flex items-center justify-center mb-4 shadow-md group-hover:scale-105 transition-transform duration-300 ${service.usePrimaryColor ? 'bg-landing-primary' : 'bg-landing-secondary'
                    }`}
            >
                <Icon className="h-7 w-7 text-white" />
            </div>
            <h3 className="text-lg font-bold text-slate-900 mb-2">{service.title}</h3>
            <p className="text-slate-600 text-sm leading-relaxed">{service.description}</p>
        </div>
    )
}

export function ServicesSection({ data }: { data: IPublicLandingData }) {
    const { sectionVisibility } = data.content;
    const services = data.content.services.filter(s => s.isActive).sort((a, b) => a.sortOrder - b.sortOrder)

    if (!sectionVisibility?.servicesVisible) return null;
    if (services.length === 0) return null

    return (
        <section id="services" className="py-16 bg-slate-100">
            <Container>
                <div className="text-center mb-12">
                    <span className="text-sm font-bold uppercase tracking-wider mb-2 block text-landing-primary">What We Offer</span>
                    <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">Our Services</h2>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {services.map((service) => (
                        <ServiceCard key={service.id} service={service} />
                    ))}
                </div>
            </Container>
        </section>
    )
}

export function AboutSection({ data }: { data: IPublicLandingData }) {
    const { about, stats, sectionVisibility } = data.content
    const sortedStats = stats.sort((a, b) => a.sortOrder - b.sortOrder)

    if (!sectionVisibility?.aboutVisible) return null;

    return (
        <section id="about" className="py-16 bg-slate-900 text-white">
            <Container>
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
                    <div>
                        <span className="text-sm font-bold uppercase tracking-wider mb-2 block text-landing-secondary">{about.sectionLabel}</span>
                        <h2 className="text-3xl md:text-4xl font-bold mb-6 leading-tight">{about.headline}</h2>
                        {about.description && (
                            <p className="text-slate-300 mb-4 text-base leading-relaxed">
                                {about.description}
                            </p>
                        )}
                        {about.secondaryDescription && (
                            <p className="text-slate-400 mb-8 text-sm leading-relaxed">
                                {about.secondaryDescription}
                            </p>
                        )}
                        {about.features.length > 0 && (
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                                {about.features.sort((a, b) => a.sortOrder - b.sortOrder).map((feature, index) => (
                                    <div key={feature.id} className="flex items-center gap-2 p-2 bg-slate-800/50 rounded-lg">
                                        <CheckCircleIcon className={`h-5 w-5 flex-shrink-0 ${index % 2 === 0 ? 'text-landing-primary' : 'text-landing-secondary'}`} />
                                        <span className="text-slate-200 text-sm font-medium">{feature.text}</span>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                    {sortedStats.length > 0 && (
                        <div className="relative">
                            <div className="rounded-2xl p-8 text-center shadow-2xl bg-gradient-to-br from-landing-primary to-landing-accent">
                                {sortedStats.map((stat, index) => (
                                    <div key={stat.id} className={index > 0 ? "mt-6 pt-6 border-t border-white/20" : ""}>
                                        <div className="text-4xl md:text-6xl font-bold mb-2">{stat.value}</div>
                                        <div className="text-base md:text-lg font-medium opacity-90">{stat.label}</div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            </Container>
        </section>
    )
}

export function TipsSection({ data }: { data: IPublicLandingData }) {
    const { tipsSection, tips, sectionVisibility } = data.content

    if (!sectionVisibility?.tipsVisible) return null
    if (!tipsSection.isVisible) return null

    const activeTips = tips.filter(t => t.isActive).sort((a, b) => a.sortOrder - b.sortOrder)

    if (activeTips.length === 0) return null

    return (
        <section id="tips" className="py-16 bg-white">
            <Container>
                <div className="text-center mb-12">
                    <div className="inline-flex items-center justify-center w-14 h-14 rounded-full mb-4 bg-purple-100">
                        <LightBulbIcon className="h-7 w-7 text-landing-primary" />
                    </div>
                    <span className="text-sm font-bold uppercase tracking-wider mb-2 block text-landing-secondary">{tipsSection.sectionLabel}</span>
                    <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">{tipsSection.headline}</h2>
                    {tipsSection.description && (
                        <p className="text-base text-slate-600 max-w-2xl mx-auto leading-relaxed">
                            {tipsSection.description}
                        </p>
                    )}
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {activeTips.map((tip, index) => (
                        <div key={tip.id} className="bg-slate-50 rounded-2xl p-6 shadow-md hover:shadow-lg transition-all duration-300">
                            <div className={`w-10 h-10 rounded-full flex items-center justify-center mb-4 text-white font-bold text-sm ${index % 2 === 0 ? 'bg-landing-primary' : 'bg-landing-secondary'}`}>
                                {index + 1}
                            </div>
                            <h3 className="text-base font-bold text-slate-900 mb-2">{tip.title}</h3>
                            <p className="text-slate-600 text-sm leading-relaxed">{tip.description}</p>
                        </div>
                    ))}
                </div>
            </Container>
        </section>
    )
}

function GalleryPhoto({ photo }: { photo: IGalleryPhotoItem }) {
    const imageUrl = photo.id ? `/api/branding/gallery-photos/${photo.id}/image` : null

    if (!imageUrl) return null

    return (
        <div className="relative group overflow-hidden rounded-xl bg-slate-200 aspect-[4/3]">
            <img
                src={imageUrl}
                alt={photo.caption || 'Gallery photo'}
                className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
            />
            {photo.caption && (
                <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/70 to-transparent p-4">
                    <p className="text-white text-sm font-medium">{photo.caption}</p>
                </div>
            )}
        </div>
    )
}

export function GallerySection({ data }: { data: IPublicLandingData }) {
    const { gallerySection, galleryPhotos, sectionVisibility } = data.content

    if (!sectionVisibility?.galleryVisible) return null

    const activePhotos = (galleryPhotos || []).filter(p => p.isActive).sort((a, b) => a.sortOrder - b.sortOrder)

    if (activePhotos.length === 0) return null

    return (
        <section id="gallery" className="py-16 bg-white">
            <Container>
                <div className="text-center mb-12">
                    <div className="inline-flex items-center justify-center w-14 h-14 rounded-full mb-4 bg-purple-100">
                        <PhotoIcon className="h-7 w-7 text-landing-primary" />
                    </div>
                    <span className="text-sm font-bold uppercase tracking-wider mb-2 block text-landing-secondary">
                        {gallerySection?.sectionLabel || 'Our Work'}
                    </span>
                    <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
                        {gallerySection?.headline || 'Photo Gallery'}
                    </h2>
                    {gallerySection?.description && (
                        <p className="text-base text-slate-600 max-w-2xl mx-auto leading-relaxed">
                            {gallerySection.description}
                        </p>
                    )}
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {activePhotos.map((photo) => (
                        <GalleryPhoto key={photo.id} photo={photo} />
                    ))}
                </div>
            </Container>
        </section>
    )
}

export function ContactSection({ data }: { data: IPublicLandingData }) {
    const { contact, services, sectionVisibility } = data.content
    const { companyInfo } = data

    if (!sectionVisibility?.contactVisible) return null

    return (
        <section id="contact" className="py-16 bg-slate-100">
            <Container>
                <div className="text-center mb-12">
                    <span className="text-sm font-bold uppercase tracking-wider mb-2 block text-landing-primary">{contact.sectionLabel}</span>
                    <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">{contact.headline}</h2>
                    {contact.description && (
                        <p className="text-base text-slate-600 max-w-2xl mx-auto leading-relaxed">
                            {contact.description}
                        </p>
                    )}
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                    <ServiceRequestForm services={services} />

                    <div className="bg-slate-900 text-white rounded-2xl p-8 shadow-xl">
                        <h3 className="text-xl font-bold mb-8">Contact Info</h3>
                        <div className="space-y-6">
                            {companyInfo.address && (
                                <div className="flex items-start gap-4">
                                    <div className="p-3 rounded-xl bg-landing-primary">
                                        <MapPinIcon className="h-5 w-5" />
                                    </div>
                                    <div>
                                        <h4 className="font-semibold text-base mb-1">Address</h4>
                                        <p className="text-slate-300 text-sm leading-relaxed">{companyInfo.address}</p>
                                    </div>
                                </div>
                            )}
                            {companyInfo.phone && (
                                <div className="flex items-start gap-4">
                                    <div className="p-3 rounded-xl bg-landing-secondary">
                                        <PhoneIcon className="h-5 w-5" />
                                    </div>
                                    <div>
                                        <h4 className="font-semibold text-base mb-1">Phone</h4>
                                        <a href={`tel:${companyInfo.phone}`} className="text-slate-300 hover:text-white transition-colors">{companyInfo.phone}</a>
                                    </div>
                                </div>
                            )}
                            {contact.showTowing && (
                                <div className="flex items-start gap-4">
                                    <div className="p-3 rounded-xl bg-landing-primary">
                                        <TruckIcon className="h-5 w-5" />
                                    </div>
                                    <div>
                                        <h4 className="font-semibold text-base mb-1">Towing</h4>
                                        <p className="text-slate-300 text-sm">{contact.towingText}</p>
                                    </div>
                                </div>
                            )}
                            {contact.businessHours.length > 0 && (
                                <div className="flex items-start gap-4">
                                    <div className="p-3 rounded-xl bg-landing-secondary">
                                        <ClockIcon className="h-5 w-5" />
                                    </div>
                                    <div>
                                        <h4 className="font-semibold text-base mb-1">Hours</h4>
                                        <div className="text-slate-300 text-sm space-y-0.5">
                                            {contact.businessHours.map((hours) => (
                                                <p key={hours.day}>
                                                    {hours.day}: {hours.open === 'Closed' ? 'Closed' : `${hours.open} - ${hours.close}`}
                                                </p>
                                            ))}
                                        </div>
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </Container>
        </section>
    )
}

export function Footer({ data }: { data: IPublicLandingData }) {
    const { footer, hero, tipsSection, socialLinks } = data.content
    const { companyInfo } = data
    const footerSocialLinks = (socialLinks || []).filter(l => l.isActive && l.showInFooter).sort((a, b) => a.sortOrder - b.sortOrder)

    return (
        <footer className="bg-landing-footer-bg text-white">
            <Container>
                <div className="py-12 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                    <div className="lg:col-span-2">
                        <div className="flex items-center gap-3 mb-4">
                            <span className="text-lg font-bold tracking-tight">{hero.companyName}</span>
                        </div>
                        {footer.companyDescription && (
                            <p className="text-slate-400 text-sm leading-relaxed max-w-md mb-4">
                                {footer.companyDescription}
                            </p>
                        )}
                        {footerSocialLinks.length > 0 && (
                            <SocialIcons links={footerSocialLinks} />
                        )}
                    </div>
                    {footer.showQuickLinks && (
                        <div>
                            <h4 className="font-bold text-base mb-4">Quick Links</h4>
                            <ul className="space-y-2 text-slate-400 text-sm">
                                <li><a href="#services" className="hover:text-white transition-colors">Services</a></li>
                                <li><a href="#about" className="hover:text-white transition-colors">About Us</a></li>
                                {tipsSection.isVisible && (
                                    <li><a href="#tips" className="hover:text-white transition-colors">Auto Tips</a></li>
                                )}
                                <li><a href="#contact" className="hover:text-white transition-colors">Contact</a></li>
                                <li><Link href="/auth/login" className="hover:text-white transition-colors">Mechanic Portal</Link></li>
                            </ul>
                        </div>
                    )}
                    {footer.showContactInfo && (
                        <div>
                            <h4 className="font-bold text-base mb-4">Contact Info</h4>
                            <ul className="space-y-2 text-slate-400 text-sm">
                                {companyInfo.address && (
                                    <li className="flex items-start gap-2">
                                        <MapPinIcon className="h-4 w-4 flex-shrink-0 mt-0.5" />
                                        <span>{companyInfo.address}</span>
                                    </li>
                                )}
                                {companyInfo.phone && (
                                    <li>
                                        <a href={`tel:${companyInfo.phone}`} className="flex items-center gap-2 hover:text-white transition-colors">
                                            <PhoneIcon className="h-4 w-4 flex-shrink-0" />
                                            <span>{companyInfo.phone}</span>
                                        </a>
                                    </li>
                                )}
                            </ul>
                        </div>
                    )}
                </div>
                <div className="border-t border-slate-800 py-6 text-center text-slate-500 text-sm">
                    <p>{footer.copyrightText || `© ${new Date().getFullYear()} ${hero.companyName}. All rights reserved.`}</p>
                </div>
            </Container>
        </footer>
    )
}
