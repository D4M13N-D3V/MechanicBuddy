import { type Metadata } from 'next'
import { Inter, Lexend } from 'next/font/google'
import clsx from 'clsx'
import { config } from '@fortawesome/fontawesome-svg-core'
import '@fortawesome/fontawesome-svg-core/styles.css'
config.autoAddCss = false

import '@/_styles/tailwind.css'

const siteName = 'MechanicBuddy'
const siteDescription = 'Workshop management made simple. MechanicBuddy is the all-in-one solution for auto repair shops to track work orders, manage clients and vehicles, handle inventory, and generate professional invoices.'
const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3025'

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    template: `%s | ${siteName}`,
    default: `${siteName} - Workshop Management for Auto Repair Shops`,
  },
  description: siteDescription,
  keywords: [
    'workshop management',
    'auto repair software',
    'mechanic software',
    'garage management',
    'work order tracking',
    'vehicle service',
    'invoicing software',
    'inventory management',
    'MechanicBuddy',
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
    title: `${siteName} - Workshop Management Made Simple`,
    description: siteDescription,
  },
  twitter: {
    card: 'summary_large_image',
    title: `${siteName} - Workshop Management Made Simple`,
    description: siteDescription,
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
  category: 'software',
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

 