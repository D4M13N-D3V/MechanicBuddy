# MechanicBuddy Management Portal

The admin dashboard for managing the MechanicBuddy SaaS platform. This Next.js application provides super admins with tools to manage tenants, monitor revenue, handle demo requests, and track platform analytics.

## Features

- **Dashboard Overview**: Real-time analytics and key metrics
- **Tenant Management**: View and manage all workshop tenants
- **Demo Requests**: Track and respond to demo inquiries
- **Billing & Revenue**: Monitor subscriptions and transactions
- **Secure Authentication**: Admin-only access with session management

## Tech Stack

- **Framework**: Next.js 15 with App Router
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **Icons**: Lucide React
- **Authentication**: Cookie-based sessions
- **Payment Processing**: Stripe (for subscription management)

## Getting Started

### Prerequisites

- Node.js 18+ and npm
- Management API running (see infrastructure setup)

### Installation

1. Install dependencies:
```bash
npm install
```

2. Copy environment variables:
```bash
cp .env.example .env
```

3. Update `.env` with your configuration:
```env
MANAGEMENT_API_URL=http://localhost:15568
NEXT_PUBLIC_MANAGEMENT_API_URL=http://localhost:15568
SESSION_SECRET=your-session-secret-key
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_...
STRIPE_SECRET_KEY=sk_test_...
```

### Development

Run the development server:
```bash
npm run dev
```

The portal will be available at [http://localhost:3026](http://localhost:3026)

### Production Build

```bash
npm run build
npm start
```

## Project Structure

```
management-portal/
├── src/
│   ├── app/                    # Next.js App Router pages
│   │   ├── (public)/          # Public-facing pages
│   │   │   ├── page.tsx       # Landing page
│   │   │   ├── demo/          # Demo request form
│   │   │   └── pricing/       # Pricing page
│   │   ├── (auth)/            # Authentication pages
│   │   │   └── login/         # Admin login
│   │   ├── dashboard/         # Admin dashboard
│   │   │   ├── page.tsx       # Overview/analytics
│   │   │   ├── tenants/       # Tenant management
│   │   │   ├── demos/         # Demo requests
│   │   │   └── billing/       # Revenue dashboard
│   │   └── api/               # API routes
│   ├── _components/           # React components
│   │   ├── ui/                # Base UI components
│   │   ├── dashboard/         # Dashboard components
│   │   └── forms/             # Form components
│   ├── _lib/                  # Utilities and APIs
│   │   ├── api.ts            # Management API client
│   │   ├── auth.ts           # Authentication
│   │   └── utils.ts          # Helper functions
│   └── types/                 # TypeScript types
└── public/                    # Static assets
```

## Authentication

### Demo Credentials

For development and testing:
- **Email**: admin@mechanicbuddy.com
- **Password**: admin123

### Production

In production, connect to the real Management API authentication endpoint by updating the `login` function in `src/_lib/auth.ts`.

## API Integration

The portal connects to the Management API for:
- Tenant CRUD operations
- Demo request management
- Billing and analytics data
- Subscription management via Stripe

Update `MANAGEMENT_API_URL` in `.env` to point to your Management API instance.

## Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `MANAGEMENT_API_URL` | Management API URL (server-side) | Yes |
| `NEXT_PUBLIC_MANAGEMENT_API_URL` | Management API URL (client-side) | Yes |
| `SESSION_SECRET` | Secret key for encrypting sessions | Yes |
| `NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY` | Stripe publishable key | No |
| `STRIPE_SECRET_KEY` | Stripe secret key | No |
| `STRIPE_WEBHOOK_SECRET` | Stripe webhook secret | No |

## Deployment

### Docker

Build the Docker image:
```bash
docker build -t mechanicbuddy-management-portal .
```

Run the container:
```bash
docker run -p 3026:3026 \
  -e MANAGEMENT_API_URL=http://management-api:15568 \
  -e SESSION_SECRET=your-secret \
  mechanicbuddy-management-portal
```

### Vercel

The portal is optimized for deployment on Vercel:

1. Push to GitHub
2. Import to Vercel
3. Configure environment variables
4. Deploy

## Development

### Adding New Pages

1. Create a new file in `src/app/dashboard/`
2. Add navigation link in `src/_components/dashboard/Sidebar.tsx`
3. Implement the page using server components

### Creating API Endpoints

Add new API functions in `src/_lib/api.ts`:

```typescript
export async function getNewData(): Promise<ApiResponse<DataType>> {
  return fetchApi<DataType>("/api/new-endpoint");
}
```

### Styling Guidelines

- Use Tailwind utility classes
- Follow the existing color scheme (primary-600, gray-900, etc.)
- Maintain responsive design (mobile-first)
- Use the UI components from `_components/ui/`

## License

This project is part of MechanicBuddy, forked from CarCare by rene98c.
