# Administrator Guide

This guide covers system administration tasks including user management, company settings, branding, and configuration.

## Accessing Admin Features

Administrator features are found under **Settings** in the sidebar. Only users with admin privileges can access these sections.

---

## User Management

Manage who can access MechanicBuddy and their permissions.

### Viewing Users

1. Go to **Settings > Users**
2. See list of all user accounts

**User Information:**
| Field | Description |
|-------|-------------|
| Username | Login name |
| Email | User's email address |
| Employee | Linked employee record |
| Status | Active or Disabled |

### Creating Users

1. Click **+ Add User**
2. Fill in the form:

| Field | Required | Description |
|-------|----------|-------------|
| Username | Yes | Unique login name |
| Email | Yes | For notifications and password reset |
| Employee | No | Link to employee record |

3. Click **Create**

The new user receives:
- Auto-generated temporary password
- Email notification (if email configured)
- Must change password on first login

!!! tip "Employee Link"
    Linking a user to an employee allows tracking who created work orders, issued invoices, etc.

### Resetting Passwords

If a user forgets their password:

1. Go to **Settings > Users**
2. Find the user
3. Click **Reset Password**
4. A new temporary password is generated
5. Share with user securely

The user must change password on next login.

### Disabling Users

To prevent a user from logging in:

1. Find the user in the list
2. Click **Disable**
3. Confirm the action

Disabled users:
- Cannot log in
- Remain in the system for audit purposes
- Can be re-enabled later

### Re-enabling Users

1. Find the disabled user
2. Click **Enable**
3. User can log in again

---

## Company Settings

Configure your company information that appears on invoices and estimates.

### Company Requisites

Go to **Settings > Company** to edit:

| Field | Description |
|-------|-------------|
| Company Name | Your business name |
| Address | Business address |
| Phone | Contact phone number |
| Email | Contact email address |
| Registration Number | Business registration |
| Tax ID | VAT/Tax identification |
| Bank Account | For bank transfer payments |

### Updating Company Info

1. Edit the relevant fields
2. Click **Save**
3. Changes apply to new documents immediately

!!! note "Existing Documents"
    Already-issued invoices retain their original company info for legal compliance.

---

## Pricing Settings

Configure how prices and taxes work.

### Tax/VAT Settings

Go to **Settings > Pricing**:

| Setting | Description |
|---------|-------------|
| VAT Rate | Percentage (e.g., 20 for 20%) |
| Surcharge | Optional markup percentage |

### Document Text

Customize text on estimates and invoices:

| Field | Description |
|-------|-------------|
| Disclaimer | Legal text at bottom of documents |
| Signature Line | Text for signature area |

### Email Templates

Customize email content:

| Template | Used For |
|----------|----------|
| Invoice Email Content | Body text when sending invoices |
| Estimate Email Content | Body text when sending estimates |

**Template Variables:**
- `{CustomerName}` - Client's name
- `{DocumentNumber}` - Invoice/Estimate number
- `{Amount}` - Total amount

Example:
```
Dear {CustomerName},

Please find attached invoice #{DocumentNumber} for {Amount}.

Thank you for your business!
```

---

## Branding

Customize the look of MechanicBuddy to match your brand.

### Logo

Go to **Settings > Branding**:

1. Click **Upload Logo**
2. Select your logo file (PNG, JPG, or SVG)
3. Logo appears on:
   - Application header
   - Invoices and estimates
   - Landing page

**Recommended Specifications:**
- Format: PNG with transparency
- Size: 200x60 pixels minimum
- File size: Under 500KB

### Portal Colors

Customize the application appearance:

| Setting | Description |
|---------|-------------|
| Sidebar Color | Navigation sidebar background |
| Accent Color | Buttons, links, highlights |
| Content Background | Main content area background |

**How to Set Colors:**
1. Click the color picker
2. Choose your color
3. Preview updates immediately
4. Click **Save** to apply

### Landing Page Colors

If using the public landing page:

| Setting | Description |
|---------|-------------|
| Primary Color | Main brand color |
| Secondary Color | Supporting color |
| Accent Color | Call-to-action buttons |
| Header Color | Top navigation background |
| Footer Color | Footer background |

---

## Landing Page Configuration

MechanicBuddy includes a customizable public landing page for your workshop.

### Enabling Landing Page

The landing page is available at your root URL (e.g., `https://yourworkshop.com`).

### Section Visibility

Control which sections appear:

Go to **Settings > Landing > Visibility**:

| Section | Toggle |
|---------|--------|
| Hero | Main banner |
| Services | Service offerings |
| About | About your workshop |
| Statistics | Achievement numbers |
| Tips | Maintenance tips |
| Gallery | Photo gallery |
| Contact | Contact information |
| Footer | Page footer |

### Hero Section

The top banner of your landing page:

Go to **Settings > Landing > Hero**:

| Field | Description |
|-------|-------------|
| Company Name | Displayed prominently |
| Tagline | Short catchy phrase |
| Subtitle | Additional description |
| Specialty Text | What you specialize in |
| Primary CTA | Main button text and link |
| Secondary CTA | Secondary button text and link |
| Background Image | Banner background |

**Example:**
- Company Name: "Mike's Auto Shop"
- Tagline: "Quality Service You Can Trust"
- Specialty Text: "Specializing in Japanese vehicles"

### Services Section

List the services you offer:

Go to **Settings > Landing > Services**:

1. Click **+ Add Service**
2. Fill in:
   - Icon (choose from icon library)
   - Title (e.g., "Oil Change")
   - Description (brief explanation)
3. Drag to reorder services
4. Toggle visibility for each

### About Section

Tell your story:

Go to **Settings > Landing > About**:

| Field | Description |
|-------|-------------|
| Section Label | Small text above headline |
| Headline | Main heading |
| Description | Your workshop story |
| Features | Bullet points of key features |

### Statistics

Show impressive numbers:

Go to **Settings > Landing > Statistics**:

Add stats like:
- "15+ Years Experience"
- "5000+ Vehicles Serviced"
- "98% Customer Satisfaction"

### Tips Section

Share maintenance advice:

Go to **Settings > Landing > Tips**:

Add helpful tips like:
- "Check tire pressure monthly"
- "Change oil every 5,000 miles"
- "Inspect brakes annually"

### Gallery

Showcase your workshop:

Go to **Settings > Landing > Gallery**:

1. Click **+ Add Photo**
2. Upload image
3. Add caption (optional)
4. Drag to reorder

**Photo Recommendations:**
- High quality images
- Show clean workshop
- Include staff working
- Feature completed work

### Contact Section

Help customers reach you:

Go to **Settings > Landing > Contact**:

| Field | Description |
|-------|-------------|
| Section Label | "Get In Touch" etc. |
| Headline | Contact section title |
| Description | Invitation to contact |
| Business Hours | Operating schedule (JSON format) |
| Show Towing | Toggle towing service message |
| Towing Text | Towing service details |

### Footer

Bottom of the page:

Go to **Settings > Landing > Footer**:

| Field | Description |
|-------|-------------|
| Company Description | Brief about text |
| Show Quick Links | Toggle navigation links |
| Show Contact Info | Toggle contact details |
| Copyright Text | Copyright notice |

### Social Media Links

Connect your social profiles:

Go to **Settings > Landing > Social**:

1. Click **+ Add Social Link**
2. Select platform (Facebook, Instagram, etc.)
3. Enter your profile URL
4. Choose where to display (header, footer, or both)

---

## Storage Locations

Organize your inventory by location.

### Managing Storages

Go to **Settings > Storages**:

1. Click **+ Add Storage**
2. Enter:
   - Name (e.g., "Shelf A", "Back Room")
   - Address (optional)
   - Description (optional)
3. Click **Save**

### Assigning Parts to Storages

When adding or editing spare parts:
1. Select the storage location from dropdown
2. Parts are organized by location in reports

---

## Employees

Manage your staff records.

### Employee List

Go to **Settings > Employees** to see all staff.

### Adding Employees

1. Click **+ Add Employee**
2. Enter:
   - First Name
   - Last Name
   - Phone
   - Email
   - Profession (e.g., "Mechanic", "Service Advisor")
   - Description (optional notes)
3. Click **Save**

### Employee Uses

Employees appear in:
- User account linking
- Mechanic assignment on services
- Work order starter/completer tracking
- Invoice issuer records

---

## System Maintenance

### Data Backup

!!! warning "Important"
    Regular backups protect your data. Contact your system administrator for backup procedures.

### Performance

If the system feels slow:
1. Clear browser cache
2. Check internet connection
3. Contact system administrator

### Updates

System updates are managed by your IT administrator. Features may be added or improved with updates.

---

## Troubleshooting

### User Can't Log In

1. Check username is correct (case-sensitive)
2. Verify user is not disabled
3. Try password reset
4. Check for account lockout

### Changes Not Saving

1. Check internet connection
2. Look for error messages
3. Try refreshing the page
4. Clear browser cache

### Documents Look Wrong

1. Verify company settings are complete
2. Check branding/logo configuration
3. Preview before sending to customers

### Email Not Sending

1. Verify SMTP settings (contact IT admin)
2. Check recipient email is valid
3. Look for error messages
4. Try sending test email

---

## Best Practices

### Security

- [ ] Use strong passwords
- [ ] Disable unused accounts
- [ ] Review user list regularly
- [ ] Don't share admin credentials

### Data Quality

- [ ] Keep company info current
- [ ] Update pricing as needed
- [ ] Maintain employee records
- [ ] Clean up test data

### Customer Experience

- [ ] Use professional logo
- [ ] Write clear email templates
- [ ] Keep landing page updated
- [ ] Respond to service requests promptly
