-- Branding and Landing Page Customization Tables
-- All tables in tenant_config schema for per-tenant customization

-- Branding settings (logo and colors)
CREATE TABLE IF NOT EXISTS tenant_config.branding (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    -- Logo stored as blob
    logo BYTEA,
    logo_mime_type VARCHAR(50),

    -- Portal Colors (admin dashboard)
    portal_sidebar_bg VARCHAR(7) DEFAULT '#111827',
    portal_sidebar_text VARCHAR(7) DEFAULT '#9ca3af',
    portal_sidebar_active_bg VARCHAR(7) DEFAULT '#1f2937',
    portal_sidebar_active_text VARCHAR(7) DEFAULT '#ffffff',
    portal_accent_color VARCHAR(7) DEFAULT '#4f46e5',
    portal_content_bg VARCHAR(7) DEFAULT '#f9fafb',

    -- Landing Page Colors
    landing_primary_color VARCHAR(7) DEFAULT '#7c3aed',
    landing_secondary_color VARCHAR(7) DEFAULT '#22c55e',
    landing_accent_color VARCHAR(7) DEFAULT '#5b21b6',
    landing_header_bg VARCHAR(7) DEFAULT '#0f172a',
    landing_footer_bg VARCHAR(7) DEFAULT '#0f172a',

    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page hero section
CREATE TABLE IF NOT EXISTS tenant_config.landing_hero (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_name VARCHAR(200) NOT NULL,
    tagline VARCHAR(500),
    subtitle VARCHAR(500),
    specialty_text VARCHAR(300),
    cta_primary_text VARCHAR(50) DEFAULT 'Our Services',
    cta_primary_link VARCHAR(200) DEFAULT '#services',
    cta_secondary_text VARCHAR(50) DEFAULT 'Contact Us',
    cta_secondary_link VARCHAR(200) DEFAULT '#contact',
    background_image BYTEA,
    background_image_mime_type VARCHAR(50),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page services (CRUD with ordering)
CREATE TABLE IF NOT EXISTS tenant_config.landing_service (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    icon_name VARCHAR(50) NOT NULL,
    title VARCHAR(100) NOT NULL,
    description VARCHAR(500) NOT NULL,
    use_primary_color BOOLEAN DEFAULT true,
    sort_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page about section
CREATE TABLE IF NOT EXISTS tenant_config.landing_about (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    section_label VARCHAR(50) DEFAULT 'About Us',
    headline VARCHAR(200) NOT NULL,
    description TEXT,
    secondary_description TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page about section feature checkmarks
CREATE TABLE IF NOT EXISTS tenant_config.landing_about_feature (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    about_id UUID NOT NULL REFERENCES tenant_config.landing_about(id) ON DELETE CASCADE,
    text VARCHAR(100) NOT NULL,
    sort_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page stats (editable numbers)
CREATE TABLE IF NOT EXISTS tenant_config.landing_stat (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    value VARCHAR(20) NOT NULL,
    label VARCHAR(100) NOT NULL,
    sort_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page tips section settings
CREATE TABLE IF NOT EXISTS tenant_config.landing_tips_section (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    is_visible BOOLEAN DEFAULT true,
    section_label VARCHAR(50) DEFAULT 'Expert Advice',
    headline VARCHAR(100) DEFAULT 'Auto Care Tips',
    description VARCHAR(300),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page individual tips (CRUD with ordering)
CREATE TABLE IF NOT EXISTS tenant_config.landing_tip (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(100) NOT NULL,
    description VARCHAR(500) NOT NULL,
    sort_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page footer settings
CREATE TABLE IF NOT EXISTS tenant_config.landing_footer (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_description TEXT,
    show_quick_links BOOLEAN DEFAULT true,
    show_contact_info BOOLEAN DEFAULT true,
    copyright_text VARCHAR(200),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Landing page contact section (required - always visible)
CREATE TABLE IF NOT EXISTS tenant_config.landing_contact (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    section_label VARCHAR(50) DEFAULT 'Get In Touch',
    headline VARCHAR(100) DEFAULT 'Contact Us',
    description VARCHAR(300),
    show_towing BOOLEAN DEFAULT false,
    towing_text VARCHAR(200) DEFAULT 'Towing service available — call us!',
    -- Business hours stored as JSON for flexibility
    business_hours TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_landing_service_sort ON tenant_config.landing_service(sort_order);
CREATE INDEX IF NOT EXISTS idx_landing_service_active ON tenant_config.landing_service(is_active);
CREATE INDEX IF NOT EXISTS idx_landing_about_feature_about ON tenant_config.landing_about_feature(about_id);
CREATE INDEX IF NOT EXISTS idx_landing_about_feature_sort ON tenant_config.landing_about_feature(sort_order);
CREATE INDEX IF NOT EXISTS idx_landing_stat_sort ON tenant_config.landing_stat(sort_order);
CREATE INDEX IF NOT EXISTS idx_landing_tip_sort ON tenant_config.landing_tip(sort_order);
CREATE INDEX IF NOT EXISTS idx_landing_tip_active ON tenant_config.landing_tip(is_active);

-- Insert default branding record
INSERT INTO tenant_config.branding (id)
VALUES ('a1b2c3d4-e5f6-7890-abcd-ef1234567890');

-- Insert default hero content
INSERT INTO tenant_config.landing_hero (id, company_name, tagline, subtitle, specialty_text)
VALUES (
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    'Your Auto Shop',
    'Professional Auto Repair & Maintenance',
    'Quality service you can trust for all your automotive needs.',
    'Serving Your Community'
);

-- Insert default about section
INSERT INTO tenant_config.landing_about (id, headline, description, secondary_description)
VALUES (
    'c3d4e5f6-a7b8-9012-cdef-123456789012',
    'Your Trusted Auto Repair Experts',
    'We are dedicated to providing honest, reliable service to our community.',
    'Our technicians deliver skilled diagnostics, preventive maintenance, and quality repairs for all makes and models.'
);

-- Insert default about features
INSERT INTO tenant_config.landing_about_feature (id, about_id, text, sort_order) VALUES
('d4e5f6a7-b8c9-0123-def0-234567890123', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Certified Technicians', 0),
('e5f6a7b8-c9d0-1234-ef01-345678901234', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Quality Parts & Materials', 1),
('f6a7b8c9-d0e1-2345-f012-456789012345', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Transparent Pricing', 2),
('a7b8c9d0-e1f2-3456-0123-567890123456', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Customer Satisfaction', 3);

-- Insert default stats
INSERT INTO tenant_config.landing_stat (id, value, label, sort_order) VALUES
('b8c9d0e1-f2a3-4567-1234-678901234567', '10+', 'Years Experience', 0),
('c9d0e1f2-a3b4-5678-2345-789012345678', '5,000+', 'Satisfied Customers', 1);

-- Insert default tips section
INSERT INTO tenant_config.landing_tips_section (id, description)
VALUES (
    'd0e1f2a3-b4c5-6789-3456-890123456789',
    'Keep your vehicle in top shape with these helpful maintenance tips from our experts.'
);

-- Insert default tips
INSERT INTO tenant_config.landing_tip (id, title, description, sort_order) VALUES
('e1f2a3b4-c5d6-7890-4567-901234567890', 'Check Your Oil Regularly', 'Check your oil level at least once a month. Low oil can cause serious engine damage.', 0),
('f2a3b4c5-d6e7-8901-5678-012345678901', 'Monitor Tire Pressure', 'Proper tire pressure improves fuel economy and extends tire life. Check monthly.', 1),
('a3b4c5d6-e7f8-9012-6789-123456789012', 'Listen to Your Brakes', 'Squealing or grinding sounds indicate worn brake pads. Don''t ignore warning signs.', 2),
('b4c5d6e7-f8a9-0123-7890-234567890123', 'Keep Up with Fluid Changes', 'Transmission fluid, coolant, and brake fluid all need periodic replacement.', 3),
('c5d6e7f8-a9b0-1234-8901-345678901234', 'Watch Your Warning Lights', 'If your check engine light comes on, get it diagnosed promptly.', 4),
('d6e7f8a9-b0c1-2345-9012-456789012345', 'Replace Wipers & Filters', 'Change wiper blades every 6-12 months and air filters every 12,000-15,000 miles.', 5);

-- Insert default footer
INSERT INTO tenant_config.landing_footer (id, company_description, copyright_text)
VALUES (
    'e7f8a9b0-c1d2-3456-0123-567890123456',
    'Professional auto repair and maintenance services you can trust.',
    NULL
);

-- Insert default contact section (uses data from tenant_config.requisites for phone/address)
INSERT INTO tenant_config.landing_contact (id, description, business_hours)
VALUES (
    'f8a9b0c1-d2e3-4567-1234-678901234567',
    'Have questions or need to schedule service? Fill out the form or give us a call!',
    '[{"day": "Monday", "open": "8:00 AM", "close": "6:00 PM"}, {"day": "Tuesday", "open": "8:00 AM", "close": "6:00 PM"}, {"day": "Wednesday", "open": "8:00 AM", "close": "6:00 PM"}, {"day": "Thursday", "open": "8:00 AM", "close": "6:00 PM"}, {"day": "Friday", "open": "8:00 AM", "close": "6:00 PM"}, {"day": "Saturday", "open": "9:00 AM", "close": "3:00 PM"}, {"day": "Sunday", "open": "Closed", "close": "Closed"}]'
);

-- Insert default services
INSERT INTO tenant_config.landing_service (id, icon_name, title, description, use_primary_color, sort_order) VALUES
('01234567-89ab-cdef-0123-456789abcdef', 'Cog6ToothIcon', 'General Maintenance', 'Regular maintenance services to keep your vehicle running smoothly.', true, 0),
('12345678-9abc-def0-1234-56789abcdef0', 'WrenchScrewdriverIcon', 'Oil Change', 'Regular oil changes to keep your engine running smoothly. We use quality oils and filters for all makes and models.', false, 1),
('23456789-abcd-ef01-2345-6789abcdef01', 'WrenchIcon', 'Brake Service', 'Complete brake inspections, pad replacements, rotor resurfacing, and brake fluid flushes for your safety.', true, 2),
('3456789a-bcde-f012-3456-789abcdef012', 'CpuChipIcon', 'Engine Repair', 'From minor tune-ups to major engine overhauls, our certified technicians handle it all.', false, 3),
('456789ab-cdef-0123-4567-89abcdef0123', 'CogIcon', 'Transmission', 'Transmission fluid changes, repairs, and rebuilds. We keep your vehicle shifting smoothly.', true, 4),
('56789abc-def0-1234-5678-9abcdef01234', 'CircleStackIcon', 'Tire Service', 'Tire rotations, balancing, and new tire installations. Keep your ride smooth and safe.', false, 5),
('6789abcd-ef01-2345-6789-abcdef012345', 'BeakerIcon', 'Diagnostics', 'State-of-the-art diagnostic equipment to quickly identify and resolve any vehicle issues.', true, 6),
('789abcde-f012-3456-789a-bcdef0123456', 'TruckIcon', 'Towing Service', 'Stranded? We offer reliable towing services to get your vehicle to our shop safely.', false, 7);
