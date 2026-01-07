'use server'

import { httpPut, httpPost, httpDelete } from "@/_lib/server/query-api";
import { pushToast } from "@/_lib/server/pushToast";
import { redirect } from "next/navigation";
import { IBrandingOptions, IServiceItem, IStatItem, ITipItem, IAboutFeature } from "./model";

export async function updateBranding(formData: FormData) {
    // Handle logo file upload
    const logoFile = formData.get('logo') as File | null;
    let logoBase64: string | null = null;
    let logoMimeType: string | null = null;

    if (logoFile && logoFile.size > 0) {
        const buffer = await logoFile.arrayBuffer();
        logoBase64 = Buffer.from(buffer).toString('base64');
        logoMimeType = logoFile.type;
    }

    const body: IBrandingOptions = {
        logoBase64,
        logoMimeType,
        portalColors: {
            sidebarBg: formData.get('portalSidebarBg')?.toString() || '#111827',
            sidebarText: formData.get('portalSidebarText')?.toString() || '#9ca3af',
            sidebarActiveBg: formData.get('portalSidebarActiveBg')?.toString() || '#1f2937',
            sidebarActiveText: formData.get('portalSidebarActiveText')?.toString() || '#ffffff',
            accentColor: formData.get('portalAccentColor')?.toString() || '#4f46e5',
            contentBg: formData.get('portalContentBg')?.toString() || '#f9fafb',
        },
        landingColors: {
            primaryColor: formData.get('landingPrimaryColor')?.toString() || '#7c3aed',
            secondaryColor: formData.get('landingSecondaryColor')?.toString() || '#22c55e',
            accentColor: formData.get('landingAccentColor')?.toString() || '#5b21b6',
            headerBg: formData.get('landingHeaderBg')?.toString() || '#0f172a',
            footerBg: formData.get('landingFooterBg')?.toString() || '#0f172a',
        }
    };

    await httpPut({ url: 'branding', body });

    pushToast('Branding updated successfully!');
    redirect('/home/settings/branding');
}

export async function updateHero(formData: FormData) {
    // Handle background image upload
    const bgFile = formData.get('backgroundImage') as File | null;
    let backgroundImageBase64: string | null = null;
    let backgroundImageMimeType: string | null = null;

    if (bgFile && bgFile.size > 0) {
        const buffer = await bgFile.arrayBuffer();
        backgroundImageBase64 = Buffer.from(buffer).toString('base64');
        backgroundImageMimeType = bgFile.type;
    }

    const body = {
        companyName: formData.get('companyName')?.toString() || '',
        tagline: formData.get('tagline')?.toString() || null,
        subtitle: formData.get('subtitle')?.toString() || null,
        specialtyText: formData.get('specialtyText')?.toString() || null,
        ctaPrimaryText: formData.get('ctaPrimaryText')?.toString() || 'Our Services',
        ctaPrimaryLink: formData.get('ctaPrimaryLink')?.toString() || '#services',
        ctaSecondaryText: formData.get('ctaSecondaryText')?.toString() || 'Contact Us',
        ctaSecondaryLink: formData.get('ctaSecondaryLink')?.toString() || '#contact',
        backgroundImageBase64,
        backgroundImageMimeType,
    };

    await httpPut({ url: 'branding/hero', body });

    pushToast('Hero section updated successfully!');
    redirect('/home/settings/landing');
}

export async function updateAbout(formData: FormData) {
    const body = {
        sectionLabel: formData.get('sectionLabel')?.toString() || 'About Us',
        headline: formData.get('headline')?.toString() || '',
        description: formData.get('description')?.toString() || null,
        secondaryDescription: formData.get('secondaryDescription')?.toString() || null,
    };

    await httpPut({ url: 'branding/about', body });

    pushToast('About section updated successfully!');
    redirect('/home/settings/landing');
}

export async function updateTipsSection(formData: FormData) {
    const body = {
        isVisible: formData.get('isVisible') === 'on',
        sectionLabel: formData.get('sectionLabel')?.toString() || 'Expert Advice',
        headline: formData.get('headline')?.toString() || 'Auto Care Tips',
        description: formData.get('description')?.toString() || null,
    };

    await httpPut({ url: 'branding/tips-section', body });

    pushToast('Tips section updated successfully!');
    redirect('/home/settings/landing');
}

export async function updateFooter(formData: FormData) {
    const body = {
        companyDescription: formData.get('companyDescription')?.toString() || null,
        showQuickLinks: formData.get('showQuickLinks') === 'on',
        showContactInfo: formData.get('showContactInfo') === 'on',
        copyrightText: formData.get('copyrightText')?.toString() || null,
    };

    await httpPut({ url: 'branding/footer', body });

    pushToast('Footer updated successfully!');
    redirect('/home/settings/landing');
}

export async function updateContact(formData: FormData) {
    // Parse business hours from form
    const businessHours = [];
    const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

    for (const day of days) {
        const open = formData.get(`hours_${day}_open`)?.toString() || 'Closed';
        const close = formData.get(`hours_${day}_close`)?.toString() || 'Closed';
        businessHours.push({ day, open, close });
    }

    const body = {
        sectionLabel: formData.get('sectionLabel')?.toString() || 'Get In Touch',
        headline: formData.get('headline')?.toString() || 'Contact Us',
        description: formData.get('description')?.toString() || null,
        showTowing: formData.get('showTowing') === 'on',
        towingText: formData.get('towingText')?.toString() || 'Towing service available — call us!',
        businessHours,
    };

    await httpPut({ url: 'branding/contact', body });

    pushToast('Contact section updated successfully!');
    redirect('/home/settings/landing');
}

// Services CRUD
export async function createService(formData: FormData) {
    const body: Partial<IServiceItem> = {
        iconName: formData.get('iconName')?.toString() || 'WrenchIcon',
        title: formData.get('title')?.toString() || '',
        description: formData.get('description')?.toString() || '',
        usePrimaryColor: formData.get('usePrimaryColor') === 'on',
        isActive: formData.get('isActive') === 'on',
    };

    await httpPost({ url: 'branding/services', body });

    pushToast('Service created successfully!');
    redirect('/home/settings/landing/services');
}

export async function updateService(formData: FormData) {
    const id = formData.get('id')?.toString();
    const body: Partial<IServiceItem> = {
        iconName: formData.get('iconName')?.toString() || 'WrenchIcon',
        title: formData.get('title')?.toString() || '',
        description: formData.get('description')?.toString() || '',
        usePrimaryColor: formData.get('usePrimaryColor') === 'on',
        isActive: formData.get('isActive') === 'on',
    };

    await httpPut({ url: `branding/services/${id}`, body });

    pushToast('Service updated successfully!');
    redirect('/home/settings/landing/services');
}

export async function deleteService(formData: FormData) {
    const id = formData.get('id')?.toString();

    await httpDelete({ url: `branding/services/${id}`, body: {} });

    pushToast('Service deleted successfully!');
    redirect('/home/settings/landing/services');
}

export async function reorderServices(formData: FormData) {
    const orderJson = formData.get('order')?.toString();
    if (orderJson) {
        const order = JSON.parse(orderJson);
        await httpPut({ url: 'branding/services/reorder', body: { order } });
    }
    redirect('/home/settings/landing/services');
}

// Stats CRUD
export async function createStat(formData: FormData) {
    const body: Partial<IStatItem> = {
        value: formData.get('value')?.toString() || '',
        label: formData.get('label')?.toString() || '',
    };

    await httpPost({ url: 'branding/stats', body });

    pushToast('Stat created successfully!');
    redirect('/home/settings/landing/stats');
}

export async function updateStat(formData: FormData) {
    const id = formData.get('id')?.toString();
    const body: Partial<IStatItem> = {
        value: formData.get('value')?.toString() || '',
        label: formData.get('label')?.toString() || '',
    };

    await httpPut({ url: `branding/stats/${id}`, body });

    pushToast('Stat updated successfully!');
    redirect('/home/settings/landing/stats');
}

export async function deleteStat(formData: FormData) {
    const id = formData.get('id')?.toString();

    await httpDelete({ url: `branding/stats/${id}`, body: {} });

    pushToast('Stat deleted successfully!');
    redirect('/home/settings/landing/stats');
}

export async function reorderStats(formData: FormData) {
    const orderJson = formData.get('order')?.toString();
    if (orderJson) {
        const order = JSON.parse(orderJson);
        await httpPut({ url: 'branding/stats/reorder', body: { order } });
    }
    redirect('/home/settings/landing/stats');
}

// Tips CRUD
export async function createTip(formData: FormData) {
    const body: Partial<ITipItem> = {
        title: formData.get('title')?.toString() || '',
        description: formData.get('description')?.toString() || '',
        isActive: formData.get('isActive') === 'on',
    };

    await httpPost({ url: 'branding/tips', body });

    pushToast('Tip created successfully!');
    redirect('/home/settings/landing/tips');
}

export async function updateTip(formData: FormData) {
    const id = formData.get('id')?.toString();
    const body: Partial<ITipItem> = {
        title: formData.get('title')?.toString() || '',
        description: formData.get('description')?.toString() || '',
        isActive: formData.get('isActive') === 'on',
    };

    await httpPut({ url: `branding/tips/${id}`, body });

    pushToast('Tip updated successfully!');
    redirect('/home/settings/landing/tips');
}

export async function deleteTip(formData: FormData) {
    const id = formData.get('id')?.toString();

    await httpDelete({ url: `branding/tips/${id}`, body: {} });

    pushToast('Tip deleted successfully!');
    redirect('/home/settings/landing/tips');
}

export async function reorderTips(formData: FormData) {
    const orderJson = formData.get('order')?.toString();
    if (orderJson) {
        const order = JSON.parse(orderJson);
        await httpPut({ url: 'branding/tips/reorder', body: { order } });
    }
    redirect('/home/settings/landing/tips');
}

// About Features CRUD
export async function createAboutFeature(formData: FormData) {
    const body: Partial<IAboutFeature> = {
        text: formData.get('text')?.toString() || '',
    };

    await httpPost({ url: 'branding/about/features', body });

    pushToast('Feature created successfully!');
    redirect('/home/settings/landing/about');
}

export async function updateAboutFeature(formData: FormData) {
    const id = formData.get('id')?.toString();
    const body: Partial<IAboutFeature> = {
        text: formData.get('text')?.toString() || '',
    };

    await httpPut({ url: `branding/about/features/${id}`, body });

    pushToast('Feature updated successfully!');
    redirect('/home/settings/landing/about');
}

export async function deleteAboutFeature(formData: FormData) {
    const id = formData.get('id')?.toString();

    await httpDelete({ url: `branding/about/features/${id}`, body: {} });

    pushToast('Feature deleted successfully!');
    redirect('/home/settings/landing/about');
}

export async function reorderAboutFeatures(formData: FormData) {
    const orderJson = formData.get('order')?.toString();
    if (orderJson) {
        const order = JSON.parse(orderJson);
        await httpPut({ url: 'branding/about/features/reorder', body: { order } });
    }
    redirect('/home/settings/landing/about');
}
