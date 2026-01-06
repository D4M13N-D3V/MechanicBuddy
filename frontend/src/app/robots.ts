import { MetadataRoute } from 'next'

export default function robots(): MetadataRoute.Robots {
  const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://3jsautorepairs.com'

  return {
    rules: [
      {
        userAgent: '*',
        allow: '/',
        disallow: ['/home/', '/auth/', '/print/', '/api/', '/error'],
      },
    ],
    sitemap: `${siteUrl}/sitemap.xml`,
  }
}
