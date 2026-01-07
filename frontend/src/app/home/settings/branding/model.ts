// Portal Colors
export interface IPortalColors {
  sidebarBg: string;
  sidebarText: string;
  sidebarActiveBg: string;
  sidebarActiveText: string;
  accentColor: string;
  contentBg: string;
}

// Landing Colors
export interface ILandingColors {
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  headerBg: string;
  footerBg: string;
}

// Full Branding Options
export interface IBrandingOptions {
  logoBase64: string | null;
  logoMimeType: string | null;
  portalColors: IPortalColors;
  landingColors: ILandingColors;
}

// Hero Section
export interface IHeroOptions {
  companyName: string;
  tagline: string | null;
  subtitle: string | null;
  specialtyText: string | null;
  ctaPrimaryText: string;
  ctaPrimaryLink: string;
  ctaSecondaryText: string;
  ctaSecondaryLink: string;
  backgroundImageBase64: string | null;
  backgroundImageMimeType: string | null;
}

// Service Item
export interface IServiceItem {
  id?: string;
  iconName: string;
  title: string;
  description: string;
  usePrimaryColor: boolean;
  sortOrder: number;
  isActive: boolean;
}

// About Feature
export interface IAboutFeature {
  id?: string;
  text: string;
  sortOrder: number;
}

// About Section
export interface IAboutOptions {
  sectionLabel: string;
  headline: string;
  description: string | null;
  secondaryDescription: string | null;
  features: IAboutFeature[];
}

// Stat Item
export interface IStatItem {
  id?: string;
  value: string;
  label: string;
  sortOrder: number;
}

// Tips Section Settings
export interface ITipsSectionOptions {
  isVisible: boolean;
  sectionLabel: string;
  headline: string;
  description: string | null;
}

// Tip Item
export interface ITipItem {
  id?: string;
  title: string;
  description: string;
  sortOrder: number;
  isActive: boolean;
}

// Footer Settings
export interface IFooterOptions {
  companyDescription: string | null;
  showQuickLinks: boolean;
  showContactInfo: boolean;
  copyrightText: string | null;
}

// Business Hours Entry
export interface IBusinessHoursEntry {
  day: string;
  open: string;
  close: string;
}

// Contact Section
export interface IContactOptions {
  sectionLabel: string;
  headline: string;
  description: string | null;
  showTowing: boolean;
  towingText: string;
  businessHours: IBusinessHoursEntry[];
}

// Full Landing Content Options
export interface ILandingContentOptions {
  hero: IHeroOptions;
  services: IServiceItem[];
  about: IAboutOptions;
  stats: IStatItem[];
  tipsSection: ITipsSectionOptions;
  tips: ITipItem[];
  footer: IFooterOptions;
  contact: IContactOptions;
}

// Company Info (from requisites)
export interface ICompanyInfo {
  name: string;
  phone: string;
  address: string;
  email: string;
  bankAccount: string;
  regNr: string;
  kmkr: string;
}

// Public Landing Page Data
export interface IPublicLandingData {
  branding: IBrandingOptions;
  content: ILandingContentOptions;
  companyInfo: ICompanyInfo;
}
