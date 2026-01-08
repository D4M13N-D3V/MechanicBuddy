import { type Metadata } from 'next'
import { Inter, Lexend } from 'next/font/google'
import clsx from 'clsx'
import { config } from '@fortawesome/fontawesome-svg-core'
import '@fortawesome/fontawesome-svg-core/styles.css'
config.autoAddCss = false

import '@/_styles/tailwind.css'
import { IPublicLandingData } from './home/settings/branding/model'

// Default fallback values
const defaultSiteName = 'MechanicBuddy'
const defaultDescription = 'Professional auto repair and maintenance services.'
const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3025'
const apiUrl = process.env.API_URL || 'http://localhost:15567'

async function getTenantData(): Promise<IPublicLandingData | null> {
  try {
    const response = await fetch(`${apiUrl}/api/publiclanding`, {
      next: { revalidate: 60 }, // Cache for 60 seconds
    })
    if (!response.ok) return null
    return response.json()
  } catch {
    return null
  }
}

export async function generateMetadata(): Promise<Metadata> {
  const data = await getTenantData()

  // Extract tenant info or use defaults
  const siteName = data?.companyInfo?.name || data?.content?.hero?.companyName || defaultSiteName
  const tagline = data?.content?.hero?.tagline
  const subtitle = data?.content?.hero?.subtitle
  const aboutDescription = data?.content?.about?.description
  const footerDescription = data?.content?.footer?.companyDescription
  const phone = data?.companyInfo?.phone
  const address = data?.companyInfo?.address

  // Build description from available content
  let siteDescription = defaultDescription
  if (tagline || subtitle || aboutDescription || footerDescription) {
    const parts = [tagline, subtitle, aboutDescription, footerDescription]
      .filter(Boolean)
      .slice(0, 2) // Use first 2 available pieces
    siteDescription = parts.join(' - ') || defaultDescription
  }
  // Append contact info if available
  if (phone) {
    siteDescription += ` Call ${phone}.`
  }

  // Build keywords from company info and services
  const keywords: string[] = [
    'auto repair',
    'mechanic',
    'car service',
    'vehicle maintenance',
    siteName,
  ]
  if (address) {
    // Extract city/location from address for local SEO
    const addressParts = address.split(',').map(p => p.trim())
    if (addressParts.length > 1) {
      keywords.push(`auto repair ${addressParts[1]}`)
      keywords.push(`mechanic ${addressParts[1]}`)
    }
  }
  // Add services to keywords if available
  if (data?.content?.services) {
    data.content.services
      .filter(s => s.isActive)
      .slice(0, 5)
      .forEach(s => keywords.push(s.title.toLowerCase()))
  }

  // Logo URL - use the public API endpoint
  const logoUrl = `${siteUrl}/backend-api/branding/logo`

  return {
    metadataBase: new URL(siteUrl),
    title: {
      template: `%s | ${siteName}`,
      default: siteName,
    },
    description: siteDescription,
    keywords,
    authors: [{ name: siteName }],
    creator: siteName,
    publisher: siteName,
    formatDetection: {
      telephone: true,
      address: true,
    },
    openGraph: {
      type: 'website',
      locale: 'en_US',
      url: siteUrl,
      siteName: siteName,
      title: tagline ? `${siteName} | ${tagline}` : siteName,
      description: siteDescription,
      images: [
        {
          url: logoUrl,
          width: 636,
          height: 636,
          alt: `${siteName} Logo`,
        },
      ],
    },
    twitter: {
      card: 'summary_large_image',
      title: tagline ? `${siteName} | ${tagline}` : siteName,
      description: siteDescription,
      images: [logoUrl],
    },
    icons: {
      icon: '/icon.ico',
      apple: '/apple-touch-icon.png',
    },
    manifest: '/manifest.json',
    robots: {
      index: true,
      follow: true,
      googleBot: {
        index: true,
        follow: true,
        'max-video-preview': -1,
        'max-image-preview': 'large',
        'max-snippet': -1,
      },
    },
    alternates: {
      canonical: siteUrl,
    },
    category: 'automotive',
  }
}

const inter = Inter({
  subsets: ['latin'],
  display: 'swap',
  variable: '--font-inter',
})

const lexend = Lexend({
  subsets: ['latin'],
  display: 'swap',
  variable: '--font-lexend',
})

export default function DefaultLayout({
  children,
}: {
  children: React.ReactNode
}) {
 
  return (
    <html className={clsx(
      'h-full xl:bg-gray-50  ',
      inter.variable,
      lexend.variable,
    )}>
      <head>
        <link rel="icon" href="/icon.ico" sizes="any" />
        <link rel="apple-touch-icon" href="/apple-touch-icon.png" />
      </head>
      <body className=" h-full ">
     
        {children}
      </body>
    </html>
  )
}

 