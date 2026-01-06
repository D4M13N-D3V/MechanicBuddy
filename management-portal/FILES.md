# MechanicBuddy Management Portal - File Structure

This document lists all files created for the Management Portal.

## Configuration Files

```
├── package.json                    # Dependencies and scripts
├── tsconfig.json                   # TypeScript configuration
├── next.config.ts                  # Next.js configuration
├── tailwind.config.ts              # Tailwind CSS configuration
├── postcss.config.mjs              # PostCSS configuration
├── eslint.config.mjs               # ESLint configuration
├── .env.example                    # Environment variables template
├── .gitignore                      # Git ignore rules
├── .dockerignore                   # Docker ignore rules
├── Dockerfile                      # Docker build configuration
├── README.md                       # Main documentation
├── SETUP.md                        # Setup guide
└── FILES.md                        # This file
```

## Application Structure

### Root Layout & Styles

```
src/app/
├── layout.tsx                      # Root HTML layout
└── globals.css                     # Global styles with Tailwind directives
```

### Public Pages (No Authentication Required)

```
src/app/(public)/
├── layout.tsx                      # Public layout with header/footer
├── page.tsx                        # Landing page with hero, features, pricing preview
├── demo/
│   └── page.tsx                   # Demo request form page
└── pricing/
    └── page.tsx                   # Detailed pricing page with FAQ
```

### Authentication Pages

```
src/app/(auth)/
├── layout.tsx                      # Auth layout (minimal, just logo)
└── login/
    └── page.tsx                   # Admin login page
```

### Dashboard Pages (Protected)

```
src/app/dashboard/
├── layout.tsx                      # Dashboard layout with sidebar & header
├── page.tsx                        # Overview page with analytics
├── tenants/
│   ├── page.tsx                   # Tenants list page
│   └── [id]/
│       └── page.tsx               # Tenant details page
├── demos/
│   └── page.tsx                   # Demo requests management
└── billing/
    └── page.tsx                   # Revenue & billing dashboard
```

### API Routes

```
src/app/api/
└── health/
    └── route.ts                   # Health check endpoint
```

## Components

### UI Components (Base Components)

```
src/_components/ui/
├── Button.tsx                      # Button with variants (primary, secondary, outline, ghost, danger)
├── Input.tsx                       # Text input with label and error states
├── Textarea.tsx                    # Multi-line text input
├── Card.tsx                        # Card container with header, content, footer
├── Badge.tsx                       # Status badge with color variants
└── Table.tsx                       # Data table with header, body, footer
```

### Dashboard Components

```
src/_components/dashboard/
├── Sidebar.tsx                     # Navigation sidebar with menu items
├── Header.tsx                      # Top header with user info and logout
└── StatCard.tsx                    # Metric card with icon and trend
```

### Form Components

```
src/_components/forms/
└── DemoRequestForm.tsx             # Demo request form with validation
```

## Libraries & Utilities

### Core Libraries

```
src/_lib/
├── api.ts                          # Management API client functions
├── auth.ts                         # Authentication utilities (login, logout, session)
└── utils.ts                        # Helper functions (formatting, validation)
```

### Type Definitions

```
src/types/
└── index.ts                        # TypeScript type definitions
                                    # - Tenant, DemoRequest, PricingTier
                                    # - DashboardAnalytics, BillingTransaction
                                    # - AdminUser, AuthSession
                                    # - ApiResponse, PaginatedResponse
```

## File Count Summary

- **Configuration**: 12 files
- **Pages**: 11 files (3 public, 1 auth, 6 dashboard, 1 API)
- **Components**: 10 files (6 UI, 3 dashboard, 1 form)
- **Libraries**: 3 files
- **Types**: 1 file
- **Documentation**: 3 files

**Total: 40 files**

## Key Features by File

### Public Pages

**Landing Page** (`(public)/page.tsx`)
- Hero section with CTA buttons
- Features grid (4 main features)
- Pricing preview (3 tiers)
- Footer with company info

**Pricing Page** (`(public)/pricing/page.tsx`)
- 4 pricing tiers (Free, Standard, Premium, Enterprise)
- Feature lists for each tier
- FAQ section with common questions
- Conversion-focused CTAs

**Demo Page** (`(public)/demo/page.tsx`)
- Contact form (email, company, message)
- Form validation
- Success state after submission

### Auth Pages

**Login Page** (`(auth)/login/page.tsx`)
- Email/password form
- Error handling
- Demo credentials display
- Redirect to dashboard on success

### Dashboard Pages

**Overview** (`dashboard/page.tsx`)
- 4 stat cards (tenants, active, MRR, demos)
- MRR trend chart (5 months)
- Recent tenants table
- Real-time metrics

**Tenants List** (`dashboard/tenants/page.tsx`)
- Searchable tenant list
- Columns: company, subdomain, plan, status, mechanics, storage, last active
- Badge indicators for status/plan
- Link to tenant details

**Tenant Details** (`dashboard/tenants/[id]/page.tsx`)
- 4 stat cards (revenue, work orders, invoices, users)
- Tenant information card
- Recent activity timeline
- Action buttons (suspend, delete)

**Demo Requests** (`dashboard/demos/page.tsx`)
- Demo request list with status badges
- Email links for quick contact
- Action buttons (contact, decline, convert)
- Pending count indicator

**Billing** (`dashboard/billing/page.tsx`)
- 4 revenue stat cards
- Revenue by plan breakdown
- Transaction history table
- Conversion rate metrics

### Components

**Sidebar** (`_components/dashboard/Sidebar.tsx`)
- Navigation menu with icons
- Active state highlighting
- Logo and version info

**Header** (`_components/dashboard/Header.tsx`)
- User avatar with initials
- User name and email
- Logout button

**StatCard** (`_components/dashboard/StatCard.tsx`)
- Metric title and value
- Icon display
- Optional trend indicator

**DemoRequestForm** (`_components/forms/DemoRequestForm.tsx`)
- Form validation
- Error states
- Success confirmation
- API integration

### Libraries

**API Client** (`_lib/api.ts`)
- Server-side fetch wrapper
- Type-safe API functions
- Error handling
- Functions for:
  - Dashboard analytics
  - Tenant CRUD
  - Demo requests
  - Billing data

**Auth** (`_lib/auth.ts`)
- Session management
- Cookie-based auth
- Login/logout functions
- Protected route helpers

**Utils** (`_lib/utils.ts`)
- Class name merging (cn)
- Currency formatting
- Date formatting
- Relative time formatting
- Validation helpers

## Technology Stack

- **Framework**: Next.js 15 with App Router
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **Icons**: Lucide React
- **State**: React Server Components
- **Authentication**: Cookie-based sessions
- **API**: Server-side fetch with type safety

## Next Steps

1. **Install dependencies**: `npm install`
2. **Configure environment**: Set up `.env` file
3. **Run development**: `npm run dev`
4. **Test pages**: Navigate through all routes
5. **Connect API**: Replace mock data with real API calls
6. **Deploy**: Build and deploy to production

All files are ready for development and deployment!
