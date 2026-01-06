# Management Portal Setup Guide

This guide will help you set up and run the MechanicBuddy Management Portal.

## Quick Start

### 1. Install Dependencies

```bash
cd management-portal
npm install
```

### 2. Configure Environment Variables

Copy the example environment file:

```bash
cp .env.example .env
```

Edit `.env` with your configuration:

```env
# Management API
MANAGEMENT_API_URL=http://localhost:15568
NEXT_PUBLIC_MANAGEMENT_API_URL=http://localhost:15568

# Session
SESSION_SECRET=change-this-to-a-random-secret-key

# Stripe (optional, for subscription management)
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_...
STRIPE_SECRET_KEY=sk_test_...
STRIPE_WEBHOOK_SECRET=whsec_...
```

### 3. Run Development Server

```bash
npm run dev
```

The portal will be available at [http://localhost:3026](http://localhost:3026)

### 4. Login

Use the demo credentials:
- **Email**: admin@mechanicbuddy.com
- **Password**: admin123

## Project Structure

```
management-portal/
├── src/
│   ├── app/                          # Next.js App Router
│   │   ├── (public)/                # Public pages (no auth required)
│   │   │   ├── page.tsx             # Landing page
│   │   │   ├── demo/page.tsx        # Demo request form
│   │   │   ├── pricing/page.tsx     # Pricing page
│   │   │   └── layout.tsx           # Public layout with header/footer
│   │   ├── (auth)/                  # Authentication pages
│   │   │   ├── login/page.tsx       # Admin login
│   │   │   └── layout.tsx           # Auth layout
│   │   ├── dashboard/               # Admin dashboard (protected)
│   │   │   ├── page.tsx             # Overview with analytics
│   │   │   ├── tenants/             # Tenant management
│   │   │   │   ├── page.tsx         # List all tenants
│   │   │   │   └── [id]/page.tsx    # Tenant details
│   │   │   ├── demos/page.tsx       # Demo requests
│   │   │   ├── billing/page.tsx     # Revenue & billing
│   │   │   └── layout.tsx           # Dashboard layout (sidebar)
│   │   ├── api/
│   │   │   └── health/route.ts      # Health check endpoint
│   │   ├── layout.tsx               # Root layout
│   │   └── globals.css              # Global styles
│   ├── _components/
│   │   ├── ui/                      # Base UI components
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Badge.tsx
│   │   │   ├── Table.tsx
│   │   │   └── Textarea.tsx
│   │   ├── dashboard/               # Dashboard components
│   │   │   ├── Sidebar.tsx          # Navigation sidebar
│   │   │   ├── Header.tsx           # Top header with user menu
│   │   │   └── StatCard.tsx         # Metric card component
│   │   └── forms/
│   │       └── DemoRequestForm.tsx  # Demo request form
│   ├── _lib/
│   │   ├── api.ts                   # Management API client
│   │   ├── auth.ts                  # Authentication utilities
│   │   └── utils.ts                 # Helper functions
│   └── types/
│       └── index.ts                 # TypeScript type definitions
├── public/                          # Static assets
├── package.json                     # Dependencies
├── tsconfig.json                    # TypeScript config
├── tailwind.config.ts              # Tailwind CSS config
├── next.config.ts                  # Next.js config
├── Dockerfile                      # Docker build config
├── .env.example                    # Environment variables template
└── README.md                       # Documentation
```

## Key Features

### 1. Public Pages

**Landing Page** (`/`)
- Hero section with call-to-action
- Feature showcase
- Pricing preview
- Navigation to demo and pricing pages

**Pricing Page** (`/pricing`)
- Detailed pricing tiers (Free, Standard, Premium, Enterprise)
- Feature comparisons
- FAQ section

**Demo Request** (`/demo`)
- Contact form for demo inquiries
- Form validation
- Success confirmation

### 2. Authentication

**Login Page** (`/login`)
- Email/password authentication
- Session-based auth with httpOnly cookies
- Protected dashboard routes

### 3. Dashboard Pages

**Overview** (`/dashboard`)
- Key metrics (tenants, revenue, demos)
- MRR trend chart
- Recent tenant activity

**Tenants** (`/dashboard/tenants`)
- List all workshop tenants
- Filter by status and plan
- View tenant details
- Tenant management actions

**Tenant Details** (`/dashboard/tenants/[id]`)
- Comprehensive tenant information
- Usage statistics
- Activity timeline
- Actions (suspend, delete)

**Demo Requests** (`/dashboard/demos`)
- List all demo inquiries
- Filter by status (pending, contacted, converted, declined)
- Contact management
- Status updates

**Billing** (`/dashboard/billing`)
- Revenue analytics
- MRR tracking
- Transaction history
- Revenue by plan breakdown

### 4. Components

**UI Components** (`_components/ui/`)
- `Button`: Multiple variants (primary, secondary, outline, ghost, danger)
- `Input`: Form input with label and error states
- `Card`: Container with header, content, footer
- `Badge`: Status indicators
- `Table`: Data tables with responsive design
- `Textarea`: Multi-line text input

**Dashboard Components** (`_components/dashboard/`)
- `Sidebar`: Navigation with active states
- `Header`: User info and logout
- `StatCard`: Metric display with trend indicators

**Form Components** (`_components/forms/`)
- `DemoRequestForm`: Demo request with validation

## API Integration

### Management API Client

The portal includes a server-side API client (`src/_lib/api.ts`) with functions for:

- `getDashboardAnalytics()`: Fetch dashboard metrics
- `getTenants()`: List tenants with pagination
- `getTenant(id)`: Get tenant details
- `updateTenantStatus(id, status)`: Update tenant status
- `getDemoRequests()`: List demo requests
- `createDemoRequest(data)`: Submit demo request
- `updateDemoRequestStatus(id, status)`: Update demo status
- `getBillingTransactions()`: Get billing data

### Adding New API Endpoints

1. Define the type in `src/types/index.ts`
2. Add the API function in `src/_lib/api.ts`
3. Use the function in your page/component

Example:

```typescript
// types/index.ts
export interface NewData {
  id: string;
  name: string;
}

// _lib/api.ts
export async function getNewData(): Promise<ApiResponse<NewData>> {
  return fetchApi<NewData>("/api/new-endpoint");
}

// app/dashboard/page.tsx
import { getNewData } from "@/_lib/api";

export default async function Page() {
  const result = await getNewData();
  // Use result.data
}
```

## Authentication Flow

1. User navigates to `/login`
2. Submits email/password
3. `login()` function validates credentials (currently uses hardcoded demo credentials)
4. On success, creates encrypted session cookie
5. User redirected to `/dashboard`
6. Dashboard layout checks for valid session
7. If no session, redirects to `/login`

### Updating Authentication

To connect to real Management API authentication:

1. Update `src/_lib/auth.ts` `login()` function:

```typescript
export async function login(credentials: LoginCredentials) {
  const response = await fetch(`${process.env.MANAGEMENT_API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(credentials),
  });

  if (!response.ok) {
    return { success: false, error: "Invalid credentials" };
  }

  const data = await response.json();
  // Store session...
  return { success: true };
}
```

## Styling

The portal uses **Tailwind CSS** with a custom color scheme:

- **Primary**: Blue (`primary-600`)
- **Text**: Gray scale (`gray-900`, `gray-600`)
- **Success**: Green (`green-600`)
- **Warning**: Yellow (`yellow-600`)
- **Danger**: Red (`red-600`)

### Design System

- Font: System fonts (Arial, Helvetica, sans-serif)
- Spacing: Tailwind spacing scale
- Radius: Rounded corners (`rounded-lg`)
- Shadows: Subtle shadows (`shadow-sm`)

## Development Tips

### Hot Reload

Next.js provides fast refresh. Changes to files will automatically reload in the browser.

### Type Safety

The project uses TypeScript. Run type checking:

```bash
npm run type-check
```

### Linting

Run ESLint:

```bash
npm run lint
```

### Mock Data

Currently, all dashboard data uses mock data defined in the page files. Replace with API calls once the Management API is ready.

## Deployment

### Docker

Build and run with Docker:

```bash
docker build -t management-portal .
docker run -p 3026:3026 \
  -e MANAGEMENT_API_URL=http://api:15568 \
  -e SESSION_SECRET=your-secret \
  management-portal
```

### Vercel

1. Push to GitHub
2. Import to Vercel
3. Set environment variables
4. Deploy

### Production Checklist

- [ ] Update `SESSION_SECRET` to a strong random key
- [ ] Configure real Management API URL
- [ ] Set up Stripe keys (if using subscriptions)
- [ ] Update authentication to use real API
- [ ] Enable HTTPS
- [ ] Configure proper CORS
- [ ] Set up error monitoring
- [ ] Configure logging

## Troubleshooting

### Port Already in Use

If port 3026 is in use, change it:

```bash
npm run dev -- -p 3027
```

Or update `package.json` scripts.

### Session Not Persisting

Check that `SESSION_SECRET` is set in `.env`.

### API Errors

1. Verify `MANAGEMENT_API_URL` is correct
2. Check that Management API is running
3. Check browser console for errors
4. Verify CORS settings

## Next Steps

1. **Install dependencies**: `npm install`
2. **Configure environment**: Copy and edit `.env`
3. **Start development**: `npm run dev`
4. **Build Management API**: Connect to real backend
5. **Deploy**: Choose your hosting platform

## Support

For issues or questions:
- Check the README.md
- Review the code comments
- Inspect the Management API documentation
