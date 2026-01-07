-- Gallery Section and Social Media Links Tables
-- Add photo gallery, social media links, and section visibility controls

-- Section visibility controls for all landing page sections
CREATE TABLE IF NOT EXISTS tenant_config.landing_section_visibility (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    hero_visible BOOLEAN DEFAULT true,
    services_visible BOOLEAN DEFAULT true,
    about_visible BOOLEAN DEFAULT true,
    stats_visible BOOLEAN DEFAULT true,
    tips_visible BOOLEAN DEFAULT true,
    gallery_visible BOOLEAN DEFAULT true,
    contact_visible BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Gallery section settings
CREATE TABLE IF NOT EXISTS tenant_config.landing_gallery_section (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    section_label VARCHAR(50) DEFAULT 'Our Work',
    headline VARCHAR(200) DEFAULT 'Photo Gallery',
    description VARCHAR(500),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Individual gallery photos
CREATE TABLE IF NOT EXISTS tenant_config.landing_gallery_photo (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    image BYTEA NOT NULL,
    image_mime_type VARCHAR(50) NOT NULL,
    caption VARCHAR(200),
    sort_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Social media links
CREATE TABLE IF NOT EXISTS tenant_config.landing_social_link (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    platform VARCHAR(50) NOT NULL, -- 'facebook', 'instagram', 'twitter', 'youtube', 'tiktok', 'linkedin', 'custom'
    url VARCHAR(500) NOT NULL,
    display_name VARCHAR(100), -- For custom links
    icon_name VARCHAR(50), -- Custom icon name (heroicon)
    sort_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    show_in_header BOOLEAN DEFAULT true,
    show_in_footer BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_landing_gallery_photo_sort ON tenant_config.landing_gallery_photo(sort_order);
CREATE INDEX IF NOT EXISTS idx_landing_gallery_photo_active ON tenant_config.landing_gallery_photo(is_active);
CREATE INDEX IF NOT EXISTS idx_landing_social_link_sort ON tenant_config.landing_social_link(sort_order);
CREATE INDEX IF NOT EXISTS idx_landing_social_link_active ON tenant_config.landing_social_link(is_active);
CREATE INDEX IF NOT EXISTS idx_landing_social_link_header ON tenant_config.landing_social_link(show_in_header);
CREATE INDEX IF NOT EXISTS idx_landing_social_link_footer ON tenant_config.landing_social_link(show_in_footer);

-- Insert default section visibility (all sections visible by default)
INSERT INTO tenant_config.landing_section_visibility (id)
VALUES ('a1234567-89ab-cdef-0123-456789abcde1');

-- Insert default gallery section settings
INSERT INTO tenant_config.landing_gallery_section (id, description)
VALUES (
    'b1234567-89ab-cdef-0123-456789abcde2',
    'Take a look at some of our recent work and satisfied customers.'
);
