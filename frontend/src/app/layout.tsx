import { type Metadata } from 'next'
import { Inter, Lexend } from 'next/font/google'
import clsx from 'clsx'
import { config } from '@fortawesome/fontawesome-svg-core'
import '@fortawesome/fontawesome-svg-core/styles.css'
config.autoAddCss = false

import '@/_styles/tailwind.css'

const siteName = "3J's Auto Repairs"
const siteDescription = "Professional auto repair and maintenance in Greensboro, NC. Specializing in Chryslers, Chargers & Challengers. Oil changes, brake service, engine repair, transmission, diagnostics, and towing. Call (336) 689-8898."
const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://3jsautorepairs.com'

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    template: `%s | ${siteName}`,
    default: `${siteName} | Auto Repair Greensboro NC`,
  },
  description: siteDescription,
  keywords: [
    'auto repair Greensboro NC',
    'mechanic Greensboro',
    'Chrysler repair',
    'Dodge Charger repair',
    'Dodge Challenger repair',
    'Mopar specialist',
    'oil change Greensboro',
    'brake service',
    'engine repair',
    'transmission repair',
    'car diagnostics',
    'towing service Greensboro',
    '3Js Auto Repairs',
  ],
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
    title: `${siteName} | Professional Auto Repair in Greensboro, NC`,
    description: siteDescription,
    images: [
      {
        url: '/3js-logo.png',
        width: 636,
        height: 636,
        alt: `${siteName} Logo`,
      },
    ],
  },
  twitter: {
    card: 'summary_large_image',
    title: `${siteName} | Auto Repair Greensboro NC`,
    description: siteDescription,
    images: ['/3js-logo.png'],
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

 