export const SOCIAL_PLATFORMS = [
    { value: 'facebook', label: 'Facebook', icon: 'facebook' },
    { value: 'instagram', label: 'Instagram', icon: 'instagram' },
    { value: 'twitter', label: 'X (Twitter)', icon: 'twitter' },
    { value: 'youtube', label: 'YouTube', icon: 'youtube' },
    { value: 'tiktok', label: 'TikTok', icon: 'tiktok' },
    { value: 'linkedin', label: 'LinkedIn', icon: 'linkedin' },
    { value: 'yelp', label: 'Yelp', icon: 'yelp' },
    { value: 'google', label: 'Google Business', icon: 'google' },
    { value: 'custom', label: 'Custom Link', icon: 'link' },
] as const;

export type SocialPlatform = typeof SOCIAL_PLATFORMS[number]['value'];
