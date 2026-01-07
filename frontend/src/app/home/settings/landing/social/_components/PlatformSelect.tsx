'use client'

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
];

interface PlatformSelectProps {
    defaultValue?: string;
    name?: string;
    onChange?: (value: string) => void;
}

export default function PlatformSelect({ defaultValue = 'facebook', name = 'platform', onChange }: PlatformSelectProps) {
    return (
        <select
            id={name}
            name={name}
            defaultValue={defaultValue}
            onChange={(e) => onChange?.(e.target.value)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
        >
            {SOCIAL_PLATFORMS.map((platform) => (
                <option key={platform.value} value={platform.value}>
                    {platform.label}
                </option>
            ))}
        </select>
    );
}
